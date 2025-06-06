using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class RagfairController : ControllerBase
    {
        private SaveProvider _saveProvider;
        public RagfairController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as SaveProvider;
        }

        [Route("client/ragfair/find")]
        [HttpPost]
        public async void Find(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }
    }
}
