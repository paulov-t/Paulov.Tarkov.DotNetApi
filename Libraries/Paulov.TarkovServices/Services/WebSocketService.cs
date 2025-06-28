using Paulov.TarkovServices.Services.Interfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Paulov.TarkovServices.Services
{
    public class WebSocketService : IWebSocketService
    {
        public ConcurrentDictionary<string, WebSocket> WebSockets { get; set; } = new ConcurrentDictionary<string, WebSocket>();

        public void AddWebSocket(string sessionId, WebSocket webSocket)
        {
            WebSockets.TryAdd(sessionId, webSocket);
        }

        public WebSocket CreateWebSocket(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
            }
            var webSocket = new ClientWebSocket();
            if (WebSockets.TryAdd(sessionId, webSocket))
            {
                return webSocket;
            }
            else
            {
                throw new InvalidOperationException($"WebSocket for session ID {sessionId} already exists.");
            }
        }

        public void DeleteWebSocket(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
            }
            WebSockets.TryRemove(sessionId, out _);
        }

        public WebSocket GetWebSocket(string sessionId)
        {
            return WebSockets.TryGetValue(sessionId, out var webSocket) ? webSocket : null;
        }
    }
}
