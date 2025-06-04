using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Paulov.TarkovServices
{
    public class NotifierProvider
    {
        public JObject CreateNotifierPacket(HttpRequest request, HttpResponse response, string sessionId)
        {
            var host = request.Host.ToString();

            var wsUrl = $"wss://{host}/{sessionId}";
            // Note: This is a bit of a hack to deal with "localhost" not supporting Secure Web Sockets due to authentication issues
            //var unsupportedHostnames = new string[] {
            //        "localhost",
            //        "127.0.0.1"
            //};
            //if (unsupportedHostnames.IndexOf(host) != -1)
            //    wsUrl = $"ws://{host}/{sessionId}";

            JObject packet = new();
            packet.Add("server", host);
            packet.Add("channel_id", sessionId);
            packet.Add("ws", wsUrl);
            return packet;
        }
    }
}
