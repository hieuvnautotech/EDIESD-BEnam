using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using ESD_EDI_BE.Cache;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Hubs;
using ESD_EDI_BE.Middlewares;
using ESD_EDI_BE.Services.Cache;
using ESD_EDI_BE.Services.Common;
using ESD_EDI_BE.SubscribeTableDependencies;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using ESD_EDI_BE.Services.FileWatcher;
using ESD_EDI_BE.RabbitMQ;
using ESD_EDI_BE.Connection;
using ESD_EDI_BE.Models.Dtos;
using MiniExcelLibs;
using ESD_EDI_BE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Hangfire;
using Hangfire.Dashboard;
using ESD_EDI_BE.CustomAttributes;
using ESD_EDI_BE.Hangfire.Services;
using ESD_EDI_BE.Hangfire.Database;
using ESD_EDI_BE.ElasticSearch.Services;
using ESD_EDI_BE.ElasticSearch.Host;
// this is a test push

IEnumerable<Type> GetControllerTypes()
{
    var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

    return controllerTypes;
}


var builder = WebApplication.CreateBuilder(args);
////
////Add services to the container.
//builder.Services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
//builder.Services.AddScoped<IPersonService, PersonService>();
//builder.Services.AddTransient<IUserInfoService, UserInfoService>();


////ConnectionString
var connectionString = builder.Configuration.GetSection("ConnectionStrings")["SQLConnectionString"];

////Auto adding services to the container.
builder.Services.RegisterServices(builder.Configuration);

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsApi",
//        //builder => builder.WithOrigins("http://localhost:3002", "http://s-wms.autonsi.com")
//        builder => builder.AllowAnyOrigin()
//            .AllowAnyHeader()
//            .AllowAnyMethod());
//});

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
})
//.AddFluentValidation(options =>
//            {
//                // Validate child properties and root collection elements
//                options.ImplicitlyValidateChildProperties = true;
//                options.ImplicitlyValidateRootCollectionElements = true;

//                // Automatic registration of validators in assembly
//                options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
//            })
;

builder.Services.AddHttpClient("expoToken", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://fcm.googleapis.com");
    //httpClient.DefaultRequestHeaders.Accept.Clear();

    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", string.Format("key={0}", "AAAALMwU6Us:APA91bHpHsvqkiPm7TWEC9sJ9WolAvF1Ihjdg-g7OoQhxklfYKr0VvAdQpKjHBNbZKCXeEnZrwfUoT9CrI6c5gauhdjU690daS1GzS7lgrMCbPRY9bY06NRXK4PetGE2LzFgsnAchQi6"));
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

/// <summary>
/// Config Authorization for Swagger
/// </summary>
builder.Services.AddSwaggerGen(options =>
{
    //options.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT", Version = "v1" });
    options.EnableAnnotations();

    var securitySchema = new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement {
        { securitySchema, new[] { "Bearer" } }
    };

    options.AddSecurityRequirement(securityRequirement);

    int index = 1;
    foreach (var controller in GetControllerTypes())
    {
        var controllerName = controller.Name.Replace("Controller", "");
        options.SwaggerDoc(controllerName.ToLower(), new OpenApiInfo { Title = $"{index}. {controllerName}", Version = "v1" });
        index++;
    }
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
        {
            return false;
        }

        var controllerName = methodInfo.DeclaringType.Name.Replace("Controller", "");
        var groupName = controllerName.ToLower();

        if (docName == groupName)
        {
            return true;
        }

        return false;
    });

    options.TagActionsBy(api => api.RelativePath.Split("/")[1]);
});

/// <summary>
/// Add Authentication
/// </summary>
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    //string key = builder.Configuration.GetSection("Jwt:Key").Value;
    //opt.TokenValidationParameters = new TokenValidationParameters
    //{
    //    //ValidateLifetime = true,

    //    ValidateIssuer = true,
    //    ValidIssuer = ConnectionString.ISSUER,

    //    ValidateAudience = true,
    //    ValidAudience = ConnectionString.AUDIENCE,

    //    ValidateIssuerSigningKey = true,
    //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConnectionString.SECRET)),
    //    ClockSkew = TimeSpan.Zero
    //};
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/signalr"))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.Configure<ConnectionModel>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<WatcherDirectory>(builder.Configuration.GetSection("WatcherFolder"));
builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.Configure<AutonsiRabbitMqConfiguration>(builder.Configuration.GetSection("AutonsiRabbitMQ"));

builder.Services.AddMemoryCache();
builder.Services.AddHostedService<InitializeCacheService>();
builder.Services.AddHostedService<InitializeElasticSearchService>();
// builder.Services.AddHostedService<FileWatcherService>();
builder.Services.AddHostedService<ConsumerHostedService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SignalRHub>();

/**
* RabbitMQ
*/
builder.Services.AddSingleton<IConsumerService>(provider => new ConsumerService(
    provider.GetRequiredService<IRabbitMqService>()
    , provider.GetRequiredService<SignalRHub>()
    , new List<string> {
        CommonConst.QUEUE_MESSAGE
        , CommonConst.QUEUE_PPORTAL_QUAL02_INFO
    }
    , provider.GetRequiredService<ESD_DBContext>()
));

builder.Services.AddSingleton<IAutonsiConsumerService>(provider => new AutonsiConsumerService(
    provider.GetRequiredService<IWebHostEnvironment>()
    , provider.GetRequiredService<IRabbitMqService>()
    , new List<string> {
        CommonConst.AUTONSI_QUEUE_MACHINE
        , CommonConst.AUTONSI_QUEUE_MENU
        , CommonConst.AUTONSI_QUEUE_MENU_PERMISSION
        , CommonConst.AUTONSI_QUEUE_DOCUMENT
    }
    // , provider.GetRequiredService<ESD_DBContext>()
    // , provider.GetRequiredService<ISqlDataAccess>()
));

builder.Services.AddSingleton<ICache, RedisCache>();

// builder.Services.AddSingleton<ISysCacheService, SysCacheService>();
// builder.Services.AddSingleton<SubscribeRoleMenuPermissionTableDependency>();
// builder.Services.AddSingleton<SubscribeAppTableDependency>();
// builder.Services.AddSingleton<SubscribeMenuTableDependency>();

builder.Services.AddDbContext<ESD_DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ESD_ConnectionStr")), ServiceLifetime.Singleton);
builder.Services.AddDbContext<HangfireAutonsiContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("AUTONSI_ConnectionStr")), ServiceLifetime.Singleton);

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});

builder.Services.AddHangfire(configuration => configuration
       .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();


var app = builder.Build();


app.UseMiddleware<JwtMiddleware>();

////config CORS
//app.UseCors("CorsApi");
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("access-token", "refresh-token", "content-type"));


app.UseRouting();

//var path = Path.Combine(app.Environment.ContentRootPath, "Upload");
//if (!Directory.Exists(path))
//{
//    Directory.CreateDirectory(path);
//}

//app.UseStaticFiles(new StaticFileOptions
//{
//    RequestPath = "/VersionApp",
//    FileProvider = new PhysicalFileProvider(path)
//});

var options = new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
};
app.UseHangfireDashboard("/hangfire", options);

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapHub<NotificationHub>("/notification");//d�ng trong function firebase push notification
    endpoints.MapHub<SignalRHub>("/signalr");
});

// Configure the HTTP request pipeline.
app.UseSwagger();

//if (app.Environment.IsProduction())
//{
//    app.UseSwaggerUI(c =>
//    {
//        c.DefaultModelsExpandDepth(-1); // Disable swagger schemas at bottom
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jwt v1");
//        c.RoutePrefix = string.Empty;
//        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
//    });
//}
//else
//{
// app.UseSwaggerUI(c =>
//     {
//         c.DefaultModelsExpandDepth(-1); // Disable swagger schemas at bottom
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jwt v1");
//         c.RoutePrefix = "swagger";

//         c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
//     });
//}
app.UseSwaggerUI(c =>
{
    c.DefaultModelsExpandDepth(-1); // Disable swagger schemas at bottom
                                    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jwt v1");
    c.RoutePrefix = "swagger";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    int index = 1;
    foreach (var controller in GetControllerTypes())
    {
        var controllerName = controller.Name.Replace("Controller", "");
        c.SwaggerEndpoint($"/swagger/{controllerName.ToLower()}/swagger.json", $"{index}. {controllerName}");
        index++;
    }
});

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".apk"] = "application/vnd.android.package-archive";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// app.UseSqlTableDependency<SubscribeRoleMenuPermissionTableDependency>(connectionString);
// app.UseSqlTableDependency<SubscribeAppTableDependency>(connectionString);
// app.UseSqlTableDependency<SubscribeMenuTableDependency>(connectionString);

app.MapControllers();

if (app.Environment.EnvironmentName != CommonConst.DEVELOPMENT)
{
    RecurringJob.AddOrUpdate<IHangfireService>(
   "delete-expired-tokens-job",
   service => service.DeleteExpiredTokens()
   , "*/15 * * * *" // At every 15th minute.
                    //    , "0 0 * * 1-7" // At 00:00 on every day-of-week from Monday through Sunday.
);
}

if (app.Environment.EnvironmentName == CommonConst.PRODUCTION)
{
    Console.WriteLine("Môi trường thực thi: " + app.Environment.EnvironmentName);
    RecurringJob.AddOrUpdate<IHangfireService>(
    "update-user-manual-job",
    service => service.UpdateDocumentFromAutonsi()
    , "*/30 * * * *" // At every 30 minute.
    //    , "0 0 * * 1-7" // At 00:00 on every day-of-week from Monday through Sunday.
    );
}


app.Run();
