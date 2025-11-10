using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LenguajesVisualesII.Api.Services;
using LenguajesVisualesII.Api.Models;

namespace LenguajesVisualesII.Api.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Resolvemos ILogService por petición (scoped) con inyección en el método InvokeAsync
    public async Task InvokeAsync(HttpContext context, ILogService logService)
    {
        context.Request.EnableBuffering();
        string? bodyText = null;
        if (context.Request.ContentLength is > 0)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            bodyText = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await logService.SaveAsync(new LogApi
            {
                TipoLog = "Error",
                RequestBody = bodyText,
                UrlEndpoint = $"{context.Request.Path}",
                MetodoHttp = context.Request.Method,
                DireccionIp = context.Connection.RemoteIpAddress?.ToString(),
                Detalle = ex.ToString()
            });

            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Internal Server Error");
        }
        finally
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await logService.SaveAsync(new LogApi
            {
                TipoLog = "Info",
                RequestBody = bodyText,
                ResponseBody = responseText,
                UrlEndpoint = $"{context.Request.Path}",
                MetodoHttp = context.Request.Method,
                DireccionIp = context.Connection.RemoteIpAddress?.ToString()
            });

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}
