using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class RagfairController : ControllerBase
    {
        [Route("client/ragfair/find")]
        [HttpPost]
        public async void Find(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }
    }
}
