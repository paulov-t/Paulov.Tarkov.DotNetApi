using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;

namespace SIT.Arena
{
    public class ArenaController : Controller
    {
        public ArenaController()
        {

        }

        [Route("/client/leaderboard")]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ClientLeaderboard(int? retry, bool? debug)
        {
            return new BSGSuccessBodyResult("{}");
        }

        [Route("client/arena/server/list")]
        [HttpPost]
        public async void ArenaServerList(
           [FromQuery] int? retry
       , [FromQuery] bool? debug
          )
        {
            // -------------------------------
            // ServerItem[]

            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var result = Array.Empty<object>();

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        }

        [Route("client/arena/presets")]
        [HttpPost]
        public async void ArenaPresets(
          [FromQuery] int? retry
      , [FromQuery] bool? debug
         )
        {
            // -------------------------------
            // ArenaPresetsResponse

            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var result = new Dictionary<string, object>();
            result.Add("presets", Array.Empty<object>());
            result.Add("presetTypes", Array.Empty<object>());

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        }

        /// <summary>
        /// This is the Server List on the Custom Game Mode screen
        /// </summary>
        /// <param name="retry"></param>
        /// <param name="debug"></param>
        [Route("client/game/custom/list")]
        [HttpPost]
        public async void ArenaGameCustomList(
          [FromQuery] int? retry
      , [FromQuery] bool? debug
         )
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var result = Array.Empty<ArenaCustomGameBaseInfo>();

            await HttpBodyConverters.CompressIntoResponseBodyBSG(JsonConvert.SerializeObject(result), Request, Response);
        }
    }
}
