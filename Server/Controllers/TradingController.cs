using BSGHelperLibrary.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Providers.SaveProviders;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class TradingController : ControllerBase
    {
        private readonly JsonFileSaveProvider _saveProvider;
        public TradingController(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider as JsonFileSaveProvider;
        }

        private string SessionId
        {
            get
            {
                return HttpSessionHelpers.GetSessionId(Request, HttpContext);
            }
        }

        private int AccountId
        {
            get
            {
                var aid = HttpContext.Session.GetInt32("AccountId");
                return aid.Value;
            }
        }

        [Route("client/trading/api/traderSettings")]
        [HttpPost]
        public async Task<IActionResult> TraderSettings(int? retry)
        {
            DatabaseProvider.TryLoadTraders(out JObject traders);

            JArray arrayResponse = new JArray();
            foreach (var key in traders)
            {
                arrayResponse.Add(key.Value);
            }

            return new BSGSuccessBodyResult(arrayResponse);
        }

        [Route("/client/trading/api/getTraderAssort/{traderId}")]
        [HttpPost]
        public async Task<IActionResult> GetTraderAssort(int? retry, bool? debug, string traderId)
        {
            var sessionId = SessionId;
#if DEBUG
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = _saveProvider.GetProfiles().Keys.First();
            }
#endif

            var tradingProvider = new TradingProvider();

            EFT.TraderAssortment traderAssortment = new();
            //traderAssortment.Items = new List<FlatItem>().ToArray();
            traderAssortment.BarterScheme = new Dictionary<string, EFT.BarterScheme>();
            traderAssortment.LoyaltyLevelItems = new Dictionary<string, int>();

            if (traderId == "ragfair")
            {
                return new BSGSuccessBodyResult(traderAssortment);
            }

            var traderAssortmentForPlayer = tradingProvider.GetTraderAssortmentById(traderId, sessionId);
            return new BSGSuccessBodyResult(traderAssortmentForPlayer);
        }

        [Route("/client/trading/api/getTraders")]
        [HttpPost]
        public async Task<IActionResult> GetTraders()
        {
            var sessionId = SessionId;

            var tradingProvider = new TradingProvider();
            TradingProvider.TryLoadTraders(out var traders);

            List<dynamic> result = new List<dynamic>();
            foreach (var i in traders)
            {
                result.Add(new
                {
                    traderId = ((dynamic)i.Value)._id,
                    name = ((dynamic)i.Value).nickname
                });
            }
            return new BSGSuccessBodyResult(result);

        }
    }
}
