using FootballManager.Application.Interfaces.Services;
using FootballManager.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Headers;
using System.Text;

namespace FootballManager.UI.Middlewares
{
    public class AdminAuthorizationHandler : ActionFilterAttribute, IActionFilter
    {
        private AuthService _authServices;

        public async override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (string.IsNullOrEmpty(request.Headers["Authorization"]))
            {
                filterContext.Result = new UnauthorizedObjectResult("Unauthorized");
                return;
            }

            var authHeader = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter ?? "")).Split(':');
            var username = credentials.FirstOrDefault();
            var password = credentials.LastOrDefault();

            _authServices = (AuthService)filterContext.HttpContext.RequestServices.GetService(typeof(IAuthService));
            var result = await _authServices.VerifyAdminAccess(username, password);

            if (!result)
                filterContext.Result = new UnauthorizedObjectResult("Unauthorized");
        }
    }
}
