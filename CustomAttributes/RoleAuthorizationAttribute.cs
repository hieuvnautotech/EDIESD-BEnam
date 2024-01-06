using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using ESD_EDI_BE.Cache;
using ESD_EDI_BE.Extensions;
using ESD_EDI_BE.Models.Dtos.Common;
using ESD_EDI_BE.Services.Cache;

namespace ESD_EDI_BE.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly HashSet<string> _roleHash = new();
        public RoleAuthorizationAttribute(params string[] roles)
        {
            if (roles.Length > 0)
            {
                foreach (var item in roles)
                {
                    if (!_roleHash.Add(item))
                    {
                        continue;
                    }
                }
            }
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAll] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAllAttribute>().Any();
            if (allowAnonymous) return;

            // authorization
            var userId = context.HttpContext.Items["UserId"]?.ToString();
            var userRole = context.HttpContext.Items["UserRole"]?.ToString();
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {

                context.Result = new JsonResult(new ResponseModel<UserDto>
                {
                    ResponseMessage = StaticReturnValue.LOST_AUTHORIZATION,
                    HttpResponseCode = 401,
                    Data = null
                })
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
            else
            {
                if (!_roleHash.Any())
                {
                    return;
                }
                else
                {
                    string[] roleArray = userRole.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var _cache = context.HttpContext.RequestServices.GetService(typeof(ISysCacheService)) as ISysCacheService;
                    var availableTokens = await _cache.GetAvailableTokensFromRedis();

                    var roleHash = new HashSet<string>();
                    foreach (var role in roleArray)
                    {
                        if (!roleHash.Add(role))
                        {
                            continue;
                        }
                    }

                    if (!roleHash.Any(_roleHash.Contains) || !availableTokens.Contains(token))
                    {
                        SetUnauthorizedResponse(context, !availableTokens.Contains(token));
                    }
                    else
                    {

                        // Continue with normal flow when both conditions are satisfied
                        return;
                    }
                }
            }
        }

        private static void SetUnauthorizedResponse(AuthorizationFilterContext context, bool invalidAccessToken)
        {
            var responseMessage = invalidAccessToken ? StaticReturnValue.INVALID_ACCESS_TOKEN : StaticReturnValue.UNAUTHORIZED;

            context.Result = new JsonResult(new ResponseModel<dynamic>
            {
                ResponseMessage = responseMessage,
                HttpResponseCode = 401,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
