using System.Diagnostics;

namespace Paulov.Tarkov.WebServer.DOTNET.Middleware;

/// <summary>
/// Middleware that logs HTTP request and response details, including method, path, status code, and elapsed
/// time.
/// </summary>
/// <remarks>This middleware logs information about incoming HTTP requests and their corresponding
/// responses to the console and debug output. It logs the request method and path at the start of processing,
/// and the response status code and elapsed time after processing. WebSocket requests are bypassed and not
/// logged.</remarks>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Middleware that logs details about incoming HTTP requests and their responses.
    /// </summary>
    /// <remarks>This middleware captures information about each HTTP request and response,
    /// which can be used for debugging, monitoring, or auditing purposes. Ensure that this middleware is added
    /// to the pipeline in the correct order to avoid missing important request or response details.</remarks>
    /// <param name="next">The next middleware in the request pipeline. Cannot be null.</param>
    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes an incoming HTTP request, logs request and response details, and forwards the request to the
    /// next middleware in the pipeline.
    /// </summary>
    /// <remarks>If the request is a WebSocket request, the method forwards the request to the
    /// next middleware without logging. For non-WebSocket requests, the method logs the HTTP method, request
    /// path, response status code, and the elapsed time for processing the request.</remarks>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();

        string logMessage = $"REQ {context.Request.Method} {context.Request.Path}";
        Console.WriteLine(logMessage);
        Debug.WriteLine(logMessage);

        await _next(context);

        sw.Stop();

        logMessage = $"{context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {sw.Elapsed.TotalMilliseconds}ms";
        Console.WriteLine(logMessage);
        Debug.WriteLine(logMessage);
    }
}