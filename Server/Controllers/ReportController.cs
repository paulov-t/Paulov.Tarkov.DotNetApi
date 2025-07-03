using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class ReportController : Controller
    {
        private ISaveProvider _saveProvider;
        private IInventoryService _inventoryService;
        private IGlobalsService _globalsService;
        private IAccountService _accountService;
        private IWebSocketService _webSocketService;

        public ReportController(ISaveProvider saveProvider, IInventoryService inventoryService, IGlobalsService globalsService, IAccountService accountService, IWebSocketService webSocketService)
        {
            _saveProvider = saveProvider;
            _inventoryService = inventoryService;
            _globalsService = globalsService;
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        /// <summary>
        /// Handles the submission of a client report by processing the request body and validating the user's session.
        /// </summary>
        /// <remarks>This endpoint expects a compressed request body containing key-value pairs, which
        /// will be decompressed and processed. If the user's session is invalid or the account cannot be found, an
        /// error response is returned.</remarks>
        /// <returns>An <see cref="IActionResult"/> representing the result of the operation.  Returns a success response with an
        /// empty JSON object if the account is valid. Returns an error response with status code 500 if the account is
        /// not found.</returns>
        [Route("client/report/send")]
        [HttpPost]
        public async Task<IActionResult> ReportSend()
        {
            var requestBody = await HttpBodyConverters.DecompressRequestBodyToDictionary(Request);

            var account = _saveProvider.LoadProfile(SessionId);
            if (account == null)
            {
                return new BSGErrorBodyResult(500, "Account not found");
            }

            return new BSGSuccessBodyResult(new JObject());
        }
    }
}
