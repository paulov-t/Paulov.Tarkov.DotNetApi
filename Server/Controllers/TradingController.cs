using Microsoft.AspNetCore.Mvc;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.Tarkov.WebServer.DOTNET.Providers;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers
{
    public class TradingController : ControllerBase
    {
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
        public async void TraderSettings(int? retry, bool? debug)
        {
            //if (TradingProvider.TryLoadTraders(out var items))
            //{
            //    var listOfTraders = items.Values;
            //    await HttpBodyConverters.CompressIntoResponseBodyBSG(listOfTraders, Request, Response);
            //}
            await HttpBodyConverters.CompressIntoResponseBodyBSG(new object[0], Request, Response);

        }

        [Route("/client/trading/api/getTraderAssort/{traderId}")]
        [HttpPost]
        public async void GetTraderAssort(int? retry, bool? debug, string traderId)
        {
            var tradingProvider = new TradingProvider();

            EFT.TraderAssortment traderAssortment = new();
            //traderAssortment.Items = new List<FlatItem>().ToArray();
            traderAssortment.BarterScheme = new Dictionary<string, EFT.BarterScheme>();
            traderAssortment.LoyaltyLevelItems = new Dictionary<string, int>();

            if (traderId == "ragfair")
            {
                await HttpBodyConverters.CompressIntoResponseBodyBSG(traderAssortment, Request, Response);
                return;
            }

            var traderAssortmentForPlayer = tradingProvider.GetTraderAssortmentById(traderId, SessionId);
            await HttpBodyConverters.CompressIntoResponseBodyBSG(traderAssortmentForPlayer, Request, Response);
        }
    }
}
