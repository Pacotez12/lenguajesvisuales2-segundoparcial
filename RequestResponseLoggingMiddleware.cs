// Middleware/RequestResponseLoggingMiddleware.cs
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LenguajesVisualesII.Api.Services;
using LenguajesVisualesII.Api.Models;
using Microsoft.AspNetCore.Hosting;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Added IWebHostEnvironment to be able to show details in Development
    public async Task Invoke(HttpContext context, ILogService logService, IWebHostEnvironment env)
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

        Exception? handledException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            handledException = ex;
            _logger.LogError(ex, "Unhandled exception in pipeline.");

            // Intent: try to save the log, but DO NOT let logging failures hide the original exception
            try
            {
                var log = new LogApi
                {
                    RequestBody = bodyText,
                    UrlEndpoint = context.Request.Path,
                    MetodoHttp = context.Request.Method,
                    Detalle = ex.ToString()
                };
                await logService.SaveAsync(log);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error while saving request/response log. This should not break the response flow.");
                // swallow — avoid rethrowing
            }

            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain; charset=utf-8";

            if (env.IsDevelopment())
            {
                // Exponer detalles en Development para depuración
                await context.Response.WriteAsync(ex.ToString());
            }
            else
            {
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
        finally
        {
            // Leer la respuesta (si existe) y guardar log de respuesta — con protección
            try
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                try
                {
                    var logResponse = new LogApi
                    {
                        RequestBody = bodyText,
                        ResponseBody = responseText,
                        UrlEndpoint = context.Request.Path,
                        MetodoHttp = context.Request.Method,
                        Detalle = handledException?.ToString()
                    };
                    await logService.SaveAsync(logResponse);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Error while saving response log in finally.");
                    // swallow
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading response body in logging middleware.");
            }

            // copiar el body al stream original
            try
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying response body to original stream.");
                // si esto falla, el framework puede manejarlo; no re-lanzamos.
                context.Response.Body = originalBodyStream;
            }
        }
    }
}