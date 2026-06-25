using System.Text;

namespace Paulov.Tarkov.WebServer.DOTNET.Middleware
{
    public class RobotHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public RobotHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("/robots") && context.Request.Path.Value.EndsWith(".txt"))
            {
                context.Response.StatusCode = 200;
                context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("OK"));
                return;
            }
            
            // TODO: This is a hack to stop it erroring for lacking a favicon.ico
            if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("/favicon.ico"))
            {
                context.Response.StatusCode = 200;
                context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes("OK"));
                return;
            }


            await _next(context);
        }
    }
}
