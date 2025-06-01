namespace Paulov.Tarkov.WebServer.DOTNET.Providers
{
    public class NotifierProvider
    {
        public Dictionary<string, object> CreateNotifierPacket(string SessionId)
        {
            string protocol = "http://";
            string externalIP = "localhost";
            string port = "6969";
            string resolvedIpHttp = $"{protocol}{externalIP}:{port}";
            Dictionary<string, object> packet = new();
            packet.Add("server", resolvedIpHttp);
            packet.Add("channel_id", SessionId);
            packet.Add("url", $"{resolvedIpHttp}/notifierServer/get/{SessionId}");
            packet.Add("notifierServer", $"{resolvedIpHttp}/notifierServer/get/{SessionId}");
            packet.Add("ws", $"ws://{externalIP}:{port}/notifierServer/getwebsocket/{SessionId}");
            return packet;
        }
    }
}
