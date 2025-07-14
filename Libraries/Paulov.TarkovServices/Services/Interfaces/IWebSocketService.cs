using EFT.Communications;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels.NotificationModels;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IWebSocketService
    {
        public ConcurrentDictionary<string, WebSocket> WebSockets { get; set; }
        public void AddWebSocket(string sessionId, WebSocket webSocket);
        public WebSocket GetWebSocket(string sessionId);
        public void DeleteWebSocket(string sessionId);
        public Task SendNotificationToWebSocket(string sessionId, BaseNotificationModel notificationModel, JObject additionalParams);
        public Task SendNotificationToWebSocket(string sessionId, ENotificationType notificationType, JObject additionalParams);
    }
}
