using ESD_EDI_BE.Services.Common;

namespace ESD_EDI_BE.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtService _jwtService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = await _jwtService.ValidateTokenAsync(token);
            var userRole = _jwtService.GetRolenameFromToken(token);

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userRole))
            {
                // attach user to context on successful jwt validation
                context.Items["UserId"] = userId;
                context.Items["UserRole"] = userRole;
            }

            await _next(context);
        }
    }
}
