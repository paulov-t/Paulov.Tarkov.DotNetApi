using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;

namespace Paulov.Tarkov.WebServer.DOTNET.Middleware;

public class WebsocketMiddleware
{
    private static readonly ConcurrentDictionary<string, WebSocket> WebSockets = new();
    private static readonly ArrayPool<byte> WebsocketBufferPool = ArrayPool<byte>.Create();
    private readonly RequestDelegate _next;

    public WebsocketMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // You can find useful information on WebSockets in .NET here https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0
        if (!context.WebSockets.IsWebSocketRequest || !context.Request.Path.HasValue)
        {
            await _next(context);
            return;
        }
        
        string path = context.Request.Path.Value;
        string sessionID = path.Substring(path.LastIndexOf('/') + 1);
        if (string.IsNullOrEmpty(sessionID))
        {
            await _next(context);
            return;
        }
        
        JObject defaultNotificationPing = new()
        {
            ["type"] = "Ping",
            ["eventId"] = "ping"
        };
        
        Debug.WriteLine($"WebSocket: request received for {sessionID}");
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Debug.WriteLine($"WebSocket: request accepted for {sessionID}");

        //TODO: Judge whether using a buffer pool is worth it in this application
        string notificationPingString = defaultNotificationPing.ToJson();
        byte[] buffer = WebsocketBufferPool.Rent(Encoding.UTF8.GetByteCount(notificationPingString));
        try
        {
            Encoding.UTF8.GetBytes(notificationPingString, 0, notificationPingString.Length, buffer, 0);

            await webSocket.SendAsync(new ReadOnlyMemory<byte>(buffer), WebSocketMessageType.Text, true,
                CancellationToken.None);
            
            TaskCompletionSource<object> socketFinishedTcs = new();
            // TODO: Handle receive of information and handle it in a background Task
            await socketFinishedTcs.Task;
        }
        finally
        {
            WebsocketBufferPool.Return(buffer);
        }
    }
}