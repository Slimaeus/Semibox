using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Semibox.Identity.Infrastructure.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = Guid.NewGuid().ToString();
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                Log.ForContext("CorrelationId", correlationId)
                    .Fatal(exception, "An unhandled exception occurred during the request processing. Correlation ID: {CorrelationId}", correlationId);

                var statusCode = (int)HttpStatusCode.InternalServerError;

                var error = new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Internal Server Error",
                    Status = statusCode,
                    Instance = context.Request.Path,
                    Extensions =
                    {
                        ["correlationId"] = correlationId
                    }
                };
                if (_environment.IsDevelopment())
                {
                    var stackTrace = exception.StackTrace
                    .Split("\r\n")
                    .Select(e => e.TrimStart());
                    error.Detail = exception.Message;
                    error.Extensions.Add("stackTrace", stackTrace);
                }

                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = statusCode;

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(error, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
