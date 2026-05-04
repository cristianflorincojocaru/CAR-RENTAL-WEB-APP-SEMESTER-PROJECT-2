using System.Net;
using System.Text.Json;

namespace CarRental.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (status, title, detail) = exception switch
        {
            KeyNotFoundException e => (HttpStatusCode.NotFound, "Resource Not Found", e.Message),
            UnauthorizedAccessException e => (HttpStatusCode.Unauthorized, "Unauthorized", e.Message),
            InvalidOperationException e when e.Message.Contains("locked") => (HttpStatusCode.Locked, "Account Locked", e.Message),
            InvalidOperationException e when e.Message.Contains("overlap") || e.Message.Contains("available") =>
                (HttpStatusCode.Conflict, "Resource Conflict", e.Message),
            InvalidOperationException e when e.Message.Contains("already") =>
                (HttpStatusCode.Conflict, "Duplicate Resource", e.Message),
            InvalidOperationException e => (HttpStatusCode.BadRequest, "Invalid Operation", e.Message),
            ArgumentException e => (HttpStatusCode.BadRequest, "Invalid Argument", e.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)status;

        var response = new
        {
            status = (int)status,
            title,
            detail,
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}
