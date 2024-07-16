using FootballManager.Application.Models.Responses;
using Newtonsoft.Json;
using System.Net;

namespace FootballManager.UI.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await ExceptionHandlerAsync(context, exception);
            }
        }

        private async Task ExceptionHandlerAsync(HttpContext context, Exception exception)
        {
            int httpCode;
            string message = "An exception was thrown.";

            switch (exception)
            {
                default:
                    httpCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            _logger.LogError($"Following exception occured: HttpCode-{httpCode} ErrorMessage: {exception.Message}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = httpCode;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(BaseResponse<NoContent>.Fail($"{message}", httpCode)));
        }
    }
}
