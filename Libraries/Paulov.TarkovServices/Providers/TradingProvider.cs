using EFT;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Paulov.TarkovServices
{
    public class TradingProvider
    {
        public static Dictionary<EMoney, string> MoneyToString = new() { { EMoney.ROUBLES, "5449016a4bdc2d6f028b456f" }, { EMoney.EUROS, "569668774bdc2da2298b4568" }, { EMoney.DOLLARS, "5696686a4bdc2da3298b456a" } };

        public static Dictionary<string, int> StaticPrices = new();

        //public static string DatabaseAssetPath => DatabaseProvider.DatabaseAssetPath;
        //public static string TradersAssetPath => Path.Combine(DatabaseProvider.DatabaseAssetPath, "traders");

        static TradingProvider()
        {
            TryLoadTraders(out _);
        }

        public static bool TryLoadTraders(
         out Dictionary<string, object> traderByTraderId)
        {

            traderByTraderId = new Dictionary<string, object>();
            foreach (var traderDirectory in DatabaseProvider.DatabaseAssetZipArchive.Entries.Where(x => x.FullName.StartsWith("traders")))
            {
                if (traderDirectory.Name.Contains("ragfair"))
                    continue;

                traderByTraderId.Add(traderDirectory.Name, JObject.Parse(File.ReadAllText(Path.Combine(traderDirectory.FullName, "base.json"))));
            }
            return traderByTraderId.Count > 0;
        }

        public Dictionary<string, int> GetStaticPrices()
        {
            if (StaticPrices.Count > 0)
                return StaticPrices;

            if (!DatabaseProvider.TryLoadItemTemplates(out var templates))
                return StaticPrices;

            if (!DatabaseProvider.TryLoadTemplateFile("handbook.json", out var handbookTemplates))
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
                                StaticPrices.Add(template.Key, int.Parse(handbookTemplateItems.Single(x => x["Id"].ToString() == template.Key)["Price"].ToString()));
                        }
                        else
                        {
                            StaticPrices.Add(template.Key, 1);
                        }
                    }
                }
            }

            return StaticPrices;
        }

        internal Trader GetTraderById(string traderId)
        {
            var assortJsonPath = Path.Combine("traders", traderId, "assort.json");
            DatabaseProvider.TryLoadDatabaseFile(assortJsonPath, out JsonDocument assort);
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("traders", traderId, "base.json"), out JsonDocument b);
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("traders", traderId, "dialogue.json"), out JsonDocument dialogue);
            DatabaseProvider.TryLoadDatabaseFile(Path.Combine("traders", traderId, "questassort.json"), out JsonDocument questAssort);

            //var traderAssortment = assort.ToObject<EFT.TraderAssortment>();
            //var trader = new Trader();

            return null;
        }

        public EFT.TraderAssortment GetTraderAssortmentById(string traderId, string profileId)
        {
            var assortJsonPath = Path.Combine("traders", traderId, "assort.json");
            DatabaseProvider.TryLoadDatabaseFile(assortJsonPath, out JsonDocument assort);
            if (assort == null)
                return new TraderAssortment();
            var options = new JsonSerializerOptions()
            {
                MaxDepth = 10,
                AllowTrailingCommas = true
                ,
                IgnoreReadOnlyFields = false,
                IgnoreReadOnlyProperties = false,
                IncludeFields = true,
                WriteIndented = false
                ,
                UnknownTypeHandling = System.Text.Json.Serialization.JsonUnknownTypeHandling.JsonElement
            ,
                UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip
            };
            //var baseAssortmentItems = Array.Empty<FlatItem>();
            //var baseAssortmentLLItems = new Dictionary<string, int>();
            //var baseAssortmentBarter = new Dictionary<string, object>();
            //try
            //{
            //    var jArrayItems = JArray.Parse(assort.RootElement.GetProperty("items").GetRawText());
            //    foreach (var jItem in jArrayItems)
            //    {
            //        if (jItem["location"] != null)
            //            jItem["location"].Remove();

            //        if (jItem["side"] != null)
            //            jItem["side"].Remove();
            //    }
            //    baseAssortmentItems = jArrayItems.ToObject<FlatItem[]>();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("There is something fucked in the Assortment data section \"items\". Manually fix it.");
            //    Debug.WriteLine(ex);
            //}
            //try
            //{
            //    baseAssortmentLLItems = assort.RootElement.GetProperty("loyal_level_items").Deserialize<Dictionary<string, int>>(options);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("There is something fucked in the Assortment data section \"loyal_level_items\". Manually fix it.");
            //    Debug.WriteLine(ex);
            //}

            //try
            //{
            //    var jObjectBarterScheme = JObject.Parse(assort.RootElement.GetProperty("barter_scheme").GetRawText());
            //    //foreach (var jItem in jObjectBarterScheme)
            //    //{
            //    //    var sideV = jItem.Value.Values("side");
            //    //    if (sideV.Count() > 0)
            //    //        jItem.Value["side"].Remove();
            //    //}

            //    // I would like for this to work but it always errors out
            //    // assort.RootElement.GetProperty("barter_scheme").Deserialize<Dictionary<string, BarterScheme>>(options);

            //    baseAssortmentBarter = jObjectBarterScheme.ToObject<Dictionary<string, object>>();

            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("There is something fucked in the Assortment data section \"barter_scheme\". Manually fix it.");
            //    Debug.WriteLine(ex);
            //}

            var saveProvider = new SaveProvider();
            var profile = saveProvider.LoadProfile(profileId);
            //var pmcProfile = saveProvider.GetPmcProfile(profileId);

            var pmcProfile = saveProvider.GetAccountProfileMode(profileId).Characters.PMC;
            var pmcTradersInfo = pmcProfile.TradersInfo;
            var tradersInfo = saveProvider.GetPmcProfileTradersInfo(profileId);
            var myTraderLevel = tradersInfo.ContainsKey(traderId) ? tradersInfo[traderId].LoyaltyLevel : 1;

            var resultTraderAssort = new EFT.TraderAssortment();
            //List<FlatItem> list = new();
            //foreach (var lli in baseAssortmentLLItems.Where(x => x.Value <= myTraderLevel))
            //{
            //    var item = baseAssortmentItems.FirstOrDefault(x => x._id == lli.Key);
            //    if (item == null)
            //        continue;


            //    list.Add(item);
            //}
            //resultTraderAssort.NextResupply = 1631489718;
            //resultTraderAssort.ExchangeRate = 1;
            //resultTraderAssort.BarterScheme = baseAssortmentBarter.SITToJson().SITParseJson<Dictionary<string, BarterScheme>>();
            //resultTraderAssort.Items = list.ToArray();
            //resultTraderAssort.LoyaltyLevelItems = baseAssortmentLLItems;
            return resultTraderAssort;
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
