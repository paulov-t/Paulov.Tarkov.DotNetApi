using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovModels.ServerModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Launcher.Support
{
    public sealed class MatchmakerSupportController : ControllerBase
    {
        private ISaveProvider _saveProvider;
        private IMatchingService _matchingService;

        public MatchmakerSupportController(ISaveProvider saveProvider, IMatchingService matchingService)
        {
            _saveProvider = saveProvider;
            _matchingService = matchingService;
        }

        [Route("client/server/add")]
        [HttpPost]
        public async Task<IActionResult> ServerAdd()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);
            if (!requestBody.ContainsKey("externalIP") || !requestBody.ContainsKey("localIP") ||
                !requestBody.ContainsKey("port") || !requestBody.ContainsKey("isGroupLeader"))
            {
                return new BSGErrorBodyResult(400, "Missing required parameters.");
            }

            var externalServer = new ServerItemModel(
                requestBody["externalIP"].ToString(),
                int.Parse(requestBody["port"].ToString()),
                EServerItemStatus.Matchmaking,
                DateTime.Now,
                1,
                bool.Parse(requestBody["isGroupLeader"].ToString()));
            _matchingService.AddServer(externalServer);
            var localServer = new ServerItemModel(
               requestBody["localIP"].ToString(),
               int.Parse(requestBody["port"].ToString()),
               EServerItemStatus.Matchmaking,
               DateTime.Now,
               1,
               bool.Parse(requestBody["isGroupLeader"].ToString()));
            _matchingService.AddServer(localServer);


            return new BSGSuccessBodyResult(new { });
        }

        [Route("client/server/remove")]
        [HttpPost]
        public async Task<IActionResult> ServerRemove()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            return new BSGSuccessBodyResult(new { });
        }

    }
}