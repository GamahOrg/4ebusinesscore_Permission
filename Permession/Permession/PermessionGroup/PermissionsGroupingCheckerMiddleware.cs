using _4EPlatform.PermissionsGroupingChecker.Dto;
using _4EPlatform.PermissionsGroupingChecker.Dto.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace _4EPlatform.PermissionsGroupingChecker
{
    public class PermissionsGroupingCheckerMiddleware : IMiddleware
    {
        private readonly PermissionGroupingService.PermissionGroupingServiceClient _permissionServiceClient;
        private readonly IConfiguration _configuration;
        public PermissionsGroupingCheckerMiddleware(PermissionGroupingService.PermissionGroupingServiceClient permissionServiceClient
            , IConfiguration configuration)
        {
            _permissionServiceClient = permissionServiceClient;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Protocol == "HTTP/2")
            {

                await next.Invoke(context);
                return;
            }
            string? url = context.Request.Path.Value.ToString();
            if (url.Contains("wwwroot"))
            {

                await next.Invoke(context);
                return;
            }
            var ActionName = context.Request.RouteValues["action"].ToString();
            var ControllerName = context.Request.RouteValues["controller"].ToString();

            var actions = _configuration.GetSection("ControllerAndActionSkip").Get<List<ControllerAndActions>>() ?? new List<ControllerAndActions>();
            var controllers = _configuration.GetSection("ControllerSkip").Get<List<string>>() ?? new List<string>();
            if (actions.Any(n => n.ControllerName.ToLower().Contains(ControllerName.ToLower())
            && n.ActionName.Any(x => x.ToLower().Contains(ActionName.ToLower())))
                || controllers.Any(n => ControllerName.ToLower().Contains(n.ToLower()))
                || url.Contains("swagger")
                || url.Contains("favicon.ico")
                || url.Contains(".well-known/openid-configuration")
                || url.Contains("connect/token")
                )
            {
                await next.Invoke(context);
                return;
            }

            string verb = context.Request.Method.ToString();

            string UserId = context.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var workspceKey = context.Request.Headers["workspaceKey"].ToString() == "" ? "0" : context.Request.Headers["workspaceKey"].ToString();

            if(ActionName.Contains("PostUserRole") && workspceKey != "80")
            {
                context.Response.StatusCode = 403;
                // Set response body
                await context.Response.WriteAsync("Unauthorized. You are not authorized to access this resource.");
            }

            var responseGrpc = await _permissionServiceClient.GetUserPermissionGroupingAsync(
                new GetUserPermissionGroupingRequest { UserId = UserId, WorkspaceKey = int.Parse(workspceKey) });
            if ((bool)responseGrpc.IsSystemAdmin)
            {
                await next.Invoke(context);
                return;
            }
            if (responseGrpc.GroupingType != (int)VerbPermissionEnum.NoPermession)
            {
                if (responseGrpc.GroupingType == (int)VerbPermissionEnum.Write)
                {
                    await next.Invoke(context);
                }
                else if (responseGrpc.GroupingType == (int)VerbPermissionEnum.Read && verb == "GET")
                {
                    await next.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = 403;
                    // Set response body
                    await context.Response.WriteAsync("Unauthorized. You are not authorized to access this resource.");
                }
            }
            else
            {
                context.Response.StatusCode = 403;
                // Set response body
                await context.Response.WriteAsync("Unauthorized. You are not authorized to access this resource.");
            }
        }
    }
}