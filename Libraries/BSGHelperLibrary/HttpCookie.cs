using Microsoft.AspNetCore.Http;

namespace Paulov.Tarkov.WebServer.DOTNET.Middleware
{
    public class HttpCookie
    {
        public string GetSessionId(HttpRequest request)
        {
            return request.Cookies["PHPSESSID"].ToString();
        }
    }
}
