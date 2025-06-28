using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IWebSocketService
    {
        public ConcurrentDictionary<string, WebSocket> WebSockets { get; set; }

        public WebSocket CreateWebSocket(string sessionId);
        public void AddWebSocket(string sessionId, WebSocket webSocket);
        public WebSocket GetWebSocket(string sessionId);
        public void DeleteWebSocket(string sessionId);

    }
}
