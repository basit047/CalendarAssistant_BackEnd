using CalendarAssistant.Services;
using System.Security.Claims;

namespace CalendarAssistant.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserMiddleware(RequestDelegate next,
            IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var user = _httpContextAccessor?.HttpContext?.User;

            if (user?.Identity != null && user.Identity.IsAuthenticated!)
            {
                ClaimsIdentity userIdentity = (ClaimsIdentity)user.Identity;
                int userId = Convert.ToInt32(userIdentity?.FindFirst("UserId")?.Value.ToString());
                string? userName = userIdentity?.FindFirst("UserName")?.Value?.ToString();
                string? userEmail = userIdentity?.FindFirst("UserEmail")?.Value?.ToString();

                var userService = httpContext.RequestServices.GetRequiredService<IUserService>();
                userService.SetUserDetails(userName, userId, userEmail);
            }

            await _next(httpContext);
        }
    }
}
