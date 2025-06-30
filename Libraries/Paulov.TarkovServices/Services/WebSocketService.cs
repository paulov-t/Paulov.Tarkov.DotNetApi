using EFT.Communications;
using Newtonsoft.Json.Linq;
using Paulov.TarkovServices.Services.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
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
            if (string.IsNullOrEmpty(sessionId))
            {
                Debug.WriteLine("Session ID is null or empty. Cannot retrieve WebSocket.");
                Console.WriteLine("Session ID is null or empty. Cannot retrieve WebSocket.");
                return null;
            }

            return WebSockets.TryGetValue(sessionId, out var webSocket) ? webSocket : null;
        }

        public void SendNotificationToWebSocket(string sessionId, ENotificationType notificationType, JObject additionalParams)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                Debug.WriteLine("Session ID is null or empty. Cannot send notification.");
                Console.WriteLine("Session ID is null or empty. Cannot send notification.");
                return;
            }

            // Get the BSGNamedAttribute for the ENotificationType enum value notificationType
            var attribute = notificationType.GetType()
                .GetField(notificationType.ToString())
                ?.GetCustomAttributes(typeof(BSGNamedAttribute), false)
                .FirstOrDefault() as BSGNamedAttribute;

            var type = attribute.Name;
            JObject jobj = new JObject();
            jobj.Add("type", type);
            jobj.Add("eventId", type);
            foreach (var param in additionalParams)
            {
                jobj.Add(param.Key, param.Value);
            }

            if (WebSockets.TryGetValue(sessionId, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var stringJson = jobj.ToJson();
                var buffer = System.Text.Encoding.UTF8.GetBytes(stringJson);
                var segment = new ArraySegment<byte>(buffer);
                webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
            }
            else
            {
                Debug.WriteLine($"WebSocket for session ID {sessionId} is not open or does not exist.");
                Console.WriteLine($"WebSocket for session ID {sessionId} is not open or does not exist.");
            }
        }
    }
}
