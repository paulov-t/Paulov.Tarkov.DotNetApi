using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.SaveProviders;
using Paulov.TarkovServices.Services;
using Paulov.TarkovServices.Services.Interfaces;
using SIT.BSGHelperLibrary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Paulov.TarkovServices
{
    public class TradingProvider
    {
        public static Dictionary<EMoney, string> MoneyToString = new() { { EMoney.ROUBLES, "5449016a4bdc2d6f028b456f" }, { EMoney.EUROS, "569668774bdc2da2298b4568" }, { EMoney.DOLLARS, "5696686a4bdc2da3298b456a" } };

        public static ConcurrentDictionary<string, double> StaticPrices = new();
        private IInventoryService _inventoryService;

        //public static string DatabaseAssetPath => DatabaseProvider.DatabaseAssetPath;
        //public static string TradersAssetPath => Path.Combine(DatabaseProvider.DatabaseAssetPath, "traders");

        public TradingProvider(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService), "InventoryService cannot be null.");

        }

        static TradingProvider()
        {
            TryLoadTraders(out _);
        }

        public static bool TryLoadTraders(
         out Dictionary<string, object> traderByTraderId)
        {

            traderByTraderId = new Dictionary<string, object>();
            var entries = DatabaseService.GetDatabaseProvider().Entries.Where(x => x.FullName.StartsWith("database/traders")).ToArray();
            foreach (var traderDirectory in entries)
            {
                if (traderDirectory.Name.Contains("ragfair"))
                    continue;

                var entryStream = traderDirectory.Open();
                var ms = new MemoryStream();
                entryStream.CopyTo(ms);
                string json = Encoding.UTF8.GetString(ms.ToArray());

                if (!traderDirectory.Name.Equals("base.json"))
                    continue;

                traderByTraderId.Add(traderDirectory.FullName, JObject.Parse(json));
            }
            return traderByTraderId.Count > 0;
        }

        public ConcurrentDictionary<string, double> GetStaticPrices()
        {
            if (StaticPrices.Count > 0)
                return StaticPrices;

            if (!DatabaseHelpers.TryLoadItemTemplates(out var templates))
                return StaticPrices;

            if (!DatabaseHelpers.TryLoadTemplateFile("handbook.json", out var handbookTemplates))
                return StaticPrices;

            var handbookTemplateItems = handbookTemplates["Items"] as JArray;

            Dictionary<string, JObject> templateDictionary = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(templates);
            foreach (var template in templateDictionary)
            {
                if (template.Value == null)
                    continue;

                if (!((JObject)template.Value).TryGetValue("_type", out var typeObj))
                    continue;

                if (typeObj.ToString() == "Item")
                {
                    if (!StaticPrices.ContainsKey(template.Key))
                    {
                        if (handbookTemplateItems.Any(x => x["Id"].ToString() == template.Key))
                        {
                            if (!StaticPrices.ContainsKey(template.Key))
                                StaticPrices.TryAdd(template.Key, int.Parse(handbookTemplateItems.Single(x => x["Id"].ToString() == template.Key)["Price"].ToString()));
                        }
                        else
                        {
                            StaticPrices.TryAdd(template.Key, 1);
                        }
                    }
                }
            }

            return StaticPrices;
        }

        //internal Trader GetTraderById(string traderId)
        //{
        //    var assortJsonPath = Path.Combine("traders", traderId, "assort.json");
        //    DatabaseHelpers.TryLoadDatabaseFile(assortJsonPath, out JsonDocument assort);
        //    DatabaseHelpers.TryLoadDatabaseFile(Path.Combine("traders", traderId, "base.json"), out JsonDocument b);
        //    DatabaseHelpers.TryLoadDatabaseFile(Path.Combine("traders", traderId, "dialogue.json"), out JsonDocument dialogue);
        //    DatabaseHelpers.TryLoadDatabaseFile(Path.Combine("traders", traderId, "questassort.json"), out JsonDocument questAssort);

        //    return null;
        //}

        public EFT.TraderAssortment GetTraderAssortmentById(string traderId, string profileId)
        {
            var baseJsonPath = Path.Combine("traders", traderId, "base.json");
            var assortJsonPath = Path.Combine("traders", traderId, "assort.json");
            DatabaseHelpers.TryLoadDatabaseFile(baseJsonPath, out JsonDocument baseDocument);
            DatabaseHelpers.TryLoadDatabaseFile(assortJsonPath, out JsonDocument assortDocument);
            if (assortDocument == null)
                return new TraderAssortment();

#if DEBUG
            var debugBaseJson = baseDocument.RootElement.GetRawText();
            var debugAssortJson = assortDocument.RootElement.GetRawText();
#endif

            var baseAssortmentItems = Array.Empty<TraderAssortItem>();
            var baseAssortmentLLItems = new Dictionary<string, int>();
            var baseAssortmentBarter = new Dictionary<string, object>();
            try
            {
                var jArrayItems = JArray.Parse(assortDocument.RootElement.GetProperty("items").GetRawText());
                foreach (var jItem in jArrayItems)
                {
                    if (jItem["location"] != null)
                        jItem["location"].Parent.Remove();

                    if (jItem["side"] != null)
                        jItem["side"].Parent.Remove();
                }
                baseAssortmentItems = jArrayItems.ToObject<TraderAssortItem[]>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Assortment data section \"items\" is broken. Manually fix it.");
                Debug.WriteLine(ex);
            }
            try
            {
                baseAssortmentLLItems = assortDocument.RootElement.GetProperty("loyal_level_items").Deserialize<Dictionary<string, int>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Assortment data section \"loyal_level_items\" is broken. Manually fix it.");
                Debug.WriteLine(ex);
            }

            try
            {
                var jObjectBarterScheme = JObject.Parse(assortDocument.RootElement.GetProperty("barter_scheme").GetRawText());
                //foreach (var jItem in jObjectBarterScheme)
                //{
                //    var sideV = jItem.Value.Values("side");
                //    if (sideV != null && sideV.Count() > 0)
                //        jItem.Value["side"].Parent.Remove();
                //}

                // I would like for this to work but it always errors out
                // assort.RootElement.GetProperty("barter_scheme").Deserialize<Dictionary<string, BarterScheme>>(options);

                baseAssortmentBarter = jObjectBarterScheme.ToObject<Dictionary<string, object>>();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Assortment data section \"barter_scheme\" is broken. Manually fix it.");
                Debug.WriteLine(ex);
            }

            var saveProvider = new JsonFileSaveProvider();
            var account = saveProvider.LoadProfile(profileId);
            if (account == null)
                return new TraderAssortment();

            var pmcProfile = saveProvider.GetAccountProfileMode(account).Characters.PMC;
            var pmcTradersInfo = pmcProfile.TradersInfo;

            var resultTraderAssort = new EFT.TraderAssortment();
            resultTraderAssort.BarterScheme = new();
            resultTraderAssort.Items = new List<FlatItem>().ToArray();
            resultTraderAssort.LoyaltyLevelItems = new();
            List<TraderAssortItem> list = new();
            baseAssortmentLLItems = baseAssortmentLLItems.Where(x => x.Value <= 1).ToDictionary(x => x.Key, x => x.Value);
            foreach (var lli in baseAssortmentLLItems)
            {
                var item = baseAssortmentItems.FirstOrDefault(x => x.Id == lli.Key);
                if (item == null)
                    continue;

                list.Add(item);

                foreach (TraderAssortItem childItem in GetChildItems(baseAssortmentItems, item.Id))// baseAssortmentItems.Where(x => x.ParentId != "hideout" && x.ParentId == item.Id))
                {
                    list.Add(childItem);
                }

            }
            resultTraderAssort.NextResupply = (int)Math.Floor(((DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000) + 60000);
            resultTraderAssort.ExchangeRate = 1;
            var barterSchemeJson = baseAssortmentBarter.SITToJson();
            resultTraderAssort.BarterScheme = barterSchemeJson.SITParseJson<Dictionary<string, BarterScheme>>();
            resultTraderAssort.Items = list.Select(x => x.ToFlatItem()).ToArray();
            resultTraderAssort.LoyaltyLevelItems = baseAssortmentLLItems;
            return resultTraderAssort;
        }

        private TraderAssortItem[] GetChildItems(TraderAssortItem[] items, string parentId)
        {
            List<TraderAssortItem> childItems = new();
            foreach (var item in items)
            {
                if (item.ParentId == parentId)
                {
                    childItems.Add(item);
                    childItems.AddRange(GetChildItems(items, item.Id));
                }
            }
            return childItems.ToArray();
        }

        public enum EMoney
        {
            ROUBLES,
            EUROS,
            DOLLARS
        }

        public class Trader
        {
            public Trader(in EFT.TraderAssortment assort, in JObject ba, in JObject dialogue, in JObject questAssort)
            {
                Assort = assort;
                Base = ba;
                Dialogue = dialogue;
                QuestAssort = questAssort;
            }

            public EFT.TraderAssortment Assort { get; set; }
            public JObject Base { get; set; }
            public JObject Dialogue { get; set; }
            public JObject QuestAssort { get; set; }
        }

        public class ProcessSellTradeRequestData
        {
            public string Action { get; set; } = "sell_to_trader";
            public string type { get; set; }
            public string tid { get; set; }
            public string price { get; set; }
            public TradeItem[] items { get; set; }

        }

        public class ProcessTo
        {
            public string id { get; set; }
            public string container { get; set; }
            public ProcessToLocation location { get; set; }
            public bool isSearched { get; set; }

        }

        public class ProcessToLocation
        {
            public int x { get; set; }
            public int y { get; set; }
            public string r { get; set; }
            public string rotation { get; set; }
            public bool isSearched { get; set; }

        }

        public class TradeItem
        {
            public string id { get; set; }
            public int count { get; set; }
            public string scheme_id { get; set; }
        }
    }
}
