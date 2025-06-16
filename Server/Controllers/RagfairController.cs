using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class RagfairController : ControllerBase
    {
        private JsonFileSaveProvider _saveProvider;
        public RagfairController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        [Route("client/ragfair/find")]
        [HttpPost]
        public async void Find(int? retry, bool? debug)
        {
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new { }, Request, Response);
        }
    }
}
