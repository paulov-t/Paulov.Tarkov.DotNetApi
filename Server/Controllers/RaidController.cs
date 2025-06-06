using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class RaidController : Controller
    {
        private ISaveProvider _saveProvider;
        public RaidController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider;
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        /// <summary>
        /// Updates the raid configuration for the current session.
        /// </summary>
        /// <remarks>This method processes a decompressed request body containing raid configuration data,
        /// updates the account profile with the new configuration, and saves the updated profile. Does not expect a response.</remarks>
        /// <returns>An <see cref="IActionResult"/> indicating the success of the operation.</returns>
        [Route("client/raid/configuration")]
        [HttpPost]
        public async Task<IActionResult> RaidConfiguration()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            var accountProfile = _saveProvider.GetAccountProfileMode(account);
            accountProfile.RaidConfiguration = requestBody;

            _saveProvider.SaveProfile(SessionId, account);
            return new BSGSuccessBodyResult(new { });
        }
    }
}
