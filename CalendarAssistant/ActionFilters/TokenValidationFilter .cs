using CalendarAssistant.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CalendarAssistant.ActionFilters
{
    public class TokenValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine("OnActionExecuting..");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var clientId = configuration!["GoogleCalendarSettings:ClientId"];

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isTokenValid = ValidateIdToken(token, clientId).GetAwaiter().GetResult();

            if (!isTokenValid.IsRequestSucceeded)
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        }

        private async Task<RequestResponse> ValidateIdToken(string idToken, string? clientId)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { clientId }
                };

                string token = idToken.Replace("Bearer ", "");

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                if (payload != null)
                {
                    return new RequestResponse
                    {
                        IsRequestSucceeded = true,
                        Message = "Valid Token"
                    };
                }
                else
                {
                    return new RequestResponse
                    {
                        IsRequestSucceeded = true,
                        Message = "Valid Token"
                    };
                }
            }
            catch (Exception ex)
            {
                return new RequestResponse
                {
                    IsRequestSucceeded = false,
                    Message = ex.Message
                };
            }
        }
    }
}
