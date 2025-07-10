using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels.ServerModels;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Launcher.Support
{
    public sealed class MatchmakerSupportController : ControllerBase
    {
        private ISaveProvider _saveProvider;
        public MatchmakerSupportController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider;
        }

        [Route("client/server/add")]
        [HttpPost]
        public async Task<IActionResult> ServerAdd()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            new ServerItemModel("127.0.0.1", 17000, EServerItemStatus.Offline, DateTime.Now, 1);

            return new BSGSuccessBodyResult(new { });
        }

        [Route("client/server/remove")]
        [HttpPost]
        public async Task<IActionResult> ServerRemove()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            new ServerItemModel("127.0.0.1", 17000, EServerItemStatus.Offline, DateTime.Now, 1);

            return new BSGSuccessBodyResult(new { });
        }

    }
}