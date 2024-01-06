using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ESD_EDI_BE.DbAccess;
using ESD_EDI_BE.Models.Dtos.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static ESD_EDI_BE.Extensions.ServiceExtensions;
using ESD_EDI_BE.Services.Cache;
using Newtonsoft.Json.Linq;
using ESD_EDI_BE.Extensions;

namespace ESD_EDI_BE.Services.Common
{
    public interface IJwtService
    {
        Task<AuthorizationResponse> GetTokenAsync(UserDto model);
        Task<string?> ValidateTokenAsync(string token);
        long GetUserIdFromToken(string token);
        string GetUserNameFromToken(string token);
        string? GetRolenameFromToken(string token);

        Task<string> GetUserRoleCodeListByUserId(long useId);
        Task<bool?> CheckRoleChanged(string token);
    }

    //[ScopedRegistration]
    [SingletonRegistration]
    public class JwtService : IJwtService
    {

        // private readonly IMemoryCache _memoryCache;
        private readonly ISysCacheService _sysCacheService;
        private readonly ISqlDataAccess _sqlDataAccess;

        public JwtService(
            // IMemoryCache memoryCache, 
            ISysCacheService sysCacheService
            , ISqlDataAccess sqlDataAccess
            )
        {
            // _memoryCache = memoryCache;
            _sysCacheService = sysCacheService;
            _sqlDataAccess = sqlDataAccess;
        }

        public async Task<AuthorizationResponse> GetTokenAsync(UserDto model)
        {
            var token = await GenerateToken(model);
            var refreshToken = await GenerateRefreshToken();
            return new AuthorizationResponse { accessToken = token, refreshToken = refreshToken };
        }

        private async Task<string?> GenerateToken(UserDto model)
        {
            //List<string> roles = new();
            //if (model.RoleNames.Any())
            //{
            //    foreach (var item in model.RoleNames)
            //    {
            //        roles.Add(item);
            //    }

            //    int claimsLength = roles.Count + 1;
            //    var claims = new Claim[claimsLength];

            //    claims[0] = new Claim(ClaimTypes.NameIdentifier, model.UserId.ToString());

            //    for (int i = 1; i < claimsLength; i++)
            //    {
            //        claims[i] = new Claim(ClaimTypes.Role, roles[i - 1]);
            //    }

            //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConnectionString.SECRET));

            //    var tokenHandler = new JwtSecurityTokenHandler();
            //    var descriptor = new SecurityTokenDescriptor()
            //    {
            //        Subject = new ClaimsIdentity(claims),
            //        Expires = DateTime.UtcNow.AddMinutes(90),
            //        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature),
            //        Audience = ConnectionString.AUDIENCE,
            //        Issuer = ConnectionString.ISSUER
            //    };

            //    //var cres = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //    //var descriptor = new JwtSecurityToken(
            //    //    claims: claims,
            //    //    expires: DateTime.UtcNow.AddDays(1),
            //    //    signingCredentials: cres
            //    //    );

            //    var token = tokenHandler.CreateToken(descriptor);
            //    return await Task.FromResult(tokenHandler.WriteToken(token)); 
            //}
            var claims = new Claim[3];
            claims[0] = new Claim(ClaimTypes.NameIdentifier, model.userId.ToString());
            claims[1] = new Claim(ClaimTypes.Name, model.userName);
            claims[2] = new Claim(ClaimTypes.Role, model.RoleCodeList);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConnectionString.SECRET));

            var tokenHandler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(480),
                //Expires = DateTime.UtcNow.AddSeconds(20),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature),
                Audience = ConnectionString.AUDIENCE,
                Issuer = ConnectionString.ISSUER
            };

            var token = tokenHandler.CreateToken(descriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

        private static async Task<string> GenerateRefreshToken()
        {
            var byteArr = new byte[64];
            using var cryptoProvider = RandomNumberGenerator.Create();
            cryptoProvider.GetBytes(byteArr);
            return await Task.FromResult(Convert.ToBase64String(byteArr));
        }

        public string GetUserNameFromToken(string token)
        {
            var jwtToken = GetInfoFromToken(token);
            if (jwtToken != null)
            {
                var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
                return userName ?? string.Empty;
            }
            return string.Empty;
        }

        public string? GetRolenameFromToken(string token)
        {
            var jwtToken = GetInfoFromToken(token);
            if (jwtToken != null)
            {
                var roleName = jwtToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                return roleName ?? string.Empty;
            }
            return string.Empty;
        }

        public async Task<string?> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return string.Empty;
            }

            var availableTokens = await _sysCacheService.GetAvailableTokensFromRedis();
            if (!availableTokens.Contains(token))
            {
                return CommonConst.NON_EXIST_TOKEN;
            }

            var jwtToken = GetInfoFromToken(token);
            var userId = string.Empty;
            if (jwtToken != null)
            {
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;

            }
            return userId;
        }

        public long GetUserIdFromToken(string token)
        {
            var jwtToken = GetInfoFromToken(token);
            if (jwtToken != null)
            {
                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
                return !string.IsNullOrEmpty(userId) ? long.Parse(userId) : 0;
            }
            return 0;
        }

        public async Task<bool?> CheckRoleChanged(string token)
        {
            var userId = GetUserIdFromToken(token);
            var userRole = GetRolenameFromToken(token);

            if (userId == 0)
            {
                return false;
            }

            var data = await GetUserRoleCodeListByUserId(userId);

            return userRole != data.Replace(" ", "");
        }

        private static JwtSecurityToken? GetInfoFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConnectionString.SECRET));
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateIssuer = true,
                    ValidIssuer = ConnectionString.ISSUER,

                    ValidateAudience = true,
                    ValidAudience = ConnectionString.AUDIENCE,

                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return (JwtSecurityToken)validatedToken;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetUserRoleCodeListByUserId(long userId)
        {
            var query = @"SELECT STUFF((
                                SELECT ', ' + r.roleCode
                                FROM [sysTbl_User_Role] ur
                                JOIN [sysTbl_Role] r ON r.roleId = ur.roleId
                                WHERE ur.userId = u.userId AND ur.isActived = 1
                                FOR XML PATH('')
                            ), 1, 2, '') AS userRole
                        FROM dbo.sysTbl_User u
                        WHERE u.[userId] = @userId;";

            var param = new DynamicParameters();
            param.Add("@userId", userId);

            var data = await _sqlDataAccess.LoadDataUsingRawQuery<string>(query, param);
            return data.FirstOrDefault();
        }
    }
}
