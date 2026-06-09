using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using BSGHelperLibrary.ResponseModels;
using EFT.InventoryLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Paulov.Tarkov.AppInsights;
using Paulov.Tarkov.WebServer.DOTNET.Middleware;
using Paulov.TarkovServices.Services;
using Sirenix.Serialization;

namespace Paulov.Tarkov.WebServer.DOTNET.Controllers.api.v1
{
    public class ItemSearchController : Controller
    {
        IAppInsightsService AppInsightsService;
        public ItemSearchController(IAppInsightsService appInsightsService) 
        {
            this.AppInsightsService = appInsightsService;
        }
        

        [Route("/api/v1/itemSearch/getItemEnglishNameAndTpl/")]
        [HttpPost]
        public async Task<IActionResult> Items()
        {
            using FormReader formReader = new FormReader(Request.Body);
            Dictionary<string, StringValues> decodedForm = await formReader.ReadFormAsync();

            int start = int.Parse(decodedForm["start"].First());
            int length = int.MaxValue;
            if (decodedForm.TryGetValue("length", out  StringValues lengthValues))
            {
                length = int.Parse(lengthValues.First());
            }
            
            //Localization pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableLocalizations =
                DatabaseService.LoadDatabaseFileAsEnumerable("locales/global/en.json");
            Dictionary<string, string> localizationDictionary =
                enumerableLocalizations.ToDictionary(localization =>
                    localization.Key, localization => localization.Value.ToString());

            //Price pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerablePrices =
                DatabaseService.LoadDatabaseFileAsEnumerable("templates/prices.json");
            Dictionary<string, long> pricesDictionary =
                enumerablePrices.ToDictionary(price =>
                    price.Key, price => long.Parse(price.Value.ToString()));
            
            //Item pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableItems =
                DatabaseService.LoadDatabaseFileAsEnumerable("templates/items.json");
            List<MinimalTemplateItem> items = enumerableItems.Select(x =>
                new MinimalTemplateItem(x.Value, pricesDictionary.GetValueOrDefault(x.Key))).ToList();
            
            //Highest price
            long highestPrice = items.Max(x => x.Price);

            JArray rootResponseObject = [];
            int index = 0;
            foreach (MinimalTemplateItem item in items)
            {
                if (index++ < start) continue; //NOTE: I think needing this is a bug on the web side?
                //Localized name
                string localizedItemName = item.ItemID;
                if (!localizationDictionary.TryGetValue($"{item.ItemID} Name", out localizedItemName))
                {
                    _ = localizationDictionary.TryGetValue($"{item.ItemID} ShortName", out localizedItemName);
                }

                //Localized parent name
                string localizedParentItemName = "N/A";
                if (!string.IsNullOrEmpty(item.ParentID))
                {
                    _ = localizationDictionary.TryGetValue($"{item.ParentID} Name", out localizedParentItemName);
                }

                double priceRatio = 0;
                if (item.Price > 0)
                {
                    if (string.IsNullOrEmpty(item.PvERarity)) continue;
                    double initialRatio = Math.Round((double)item.Price / highestPrice * 100);
                    priceRatio = initialRatio * 3;
                    const double basePvERarityMultiplier = 2.5;

                    Enum.TryParse(item.PvERarity, true, out PvERarity rarity);
                    
                    priceRatio *= basePvERarityMultiplier + ((int)rarity * 0.25);
                    priceRatio = Math.Max(Math.Min(priceRatio, 100), 1);
                    priceRatio = Math.Ceiling(priceRatio);
                }
                int rating = 0;
                if (item.Props.Damage > 0)
                {
                    var armorDamage = Math.Max(1, item.Props.ArmorDamage > 0 ? item.Props.ArmorDamage * 2 : 1);
                    var penRating = Math.Max(1, item.Props.PenetrationPower > 0 ? item.Props.PenetrationPower * 2 : 1);
                    var damageRating = Math.Max(1, item.Props.Damage > 0 ? item.Props.Damage * 0.01 : 1);

                    rating = (int)Math.Round(armorDamage * penRating * damageRating);

                    if (rating == 0)
                        rating = 1;
                }

                if (rootResponseObject.Count > length) break;
                
                rootResponseObject.Add(new JObject
                {
                    ["itemId"] = item.ItemID,
                    ["langItem"] = localizedItemName,
                    ["rating"] = rating,
                    ["parentId"] = item.ParentID,
                    ["parentIdLang"] = localizedParentItemName,
                    ["price"] = item.Price,
                    ["priceRatio"] = priceRatio
                });
            }
            
            return new BSGSuccessBodyResult(rootResponseObject);
        }

        [Route("/api/v1/itemSearch/getAmmoCalibers/")]
        [HttpGet]
        public async Task<IActionResult> AmmoCalibers()
        {
            var data = new Dictionary<string, string>()
            {
                {"query", "{ ammo { item { id } caliber armorDamage damage penetrationPower ammoType }}"}
            };

            HashSet<string> hash = new HashSet<string>();
            JArray rootResponseObject = [];

            try
            {

                using (var httpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 5) })
                {
                    //Http response message
                    var httpResponse = await httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);

                    //Response content
                    var responseContent = JArray.Parse(JObject.Parse(await httpResponse.Content.ReadAsStringAsync())["data"]["ammo"].ToString());
                    foreach (var item in responseContent)
                    {
                        var tdaammo = new TarkovDevApiAmmo(item);
                        _ = tdaammo;
                        hash.Add(tdaammo.Caliber);
                    }

                    foreach (var item in hash.OrderBy(x => x))
                        rootResponseObject.Add(item);

                    return new BSGSuccessBodyResult(rootResponseObject);
                }
            }
            catch (Exception ex)
            {
                this.AppInsightsService?.TrackException(ex);
            }

            ////Item pre-loading
            //IEnumerable<MinimalTemplateItem> templatesItemsMinimalEnumerable =
            //    DatabaseService.LoadDatabaseFileAsEnumerable("templates/items.json")
            //        .Select(x => new MinimalTemplateItem(x.Value, 0));

            //const string ammoParentId = "5485a8684bdc2da71d8b4567";
            //string[] ammoIdsToIgnore = ["5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1"];

            //await Parallel.ForEachAsync(templatesItemsMinimalEnumerable, (item, _) =>
            //{
            //    if (!string.Equals(item.ParentID, ammoParentId)) return ValueTask.CompletedTask;
            //    if (ammoIdsToIgnore.Contains(item.ItemID)) return ValueTask.CompletedTask;
            //    if (!string.Equals(item.Props.AmmoType, "bullet")) return ValueTask.CompletedTask;

            //    if (!hash.Contains(item.Props.Caliber))
            //        hash.Add(item.Props.Caliber);

            //    return ValueTask.CompletedTask;
            //});

            //foreach (var item in hash.OrderBy(x => x))
            //    rootResponseObject.Add(item);

            //return new BSGSuccessBodyResult(rootResponseObject);

            return new BSGErrorBodyResult(400, "Unable to find calibers");
        }


        [Route("/api/v1/itemSearch/getAmmo/{caliber}")]
        [HttpPost]
        public async Task<IActionResult> Ammo(string caliber)
        {
            var data = new Dictionary<string, string>()
            {
                {"query", "{ ammo { item { id } caliber armorDamage damage penetrationPower ammoType }}"}
            };

            HashSet<TarkovDevApiAmmo> hash = new HashSet<TarkovDevApiAmmo>();

            try
            {
                using (var httpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 5) })
                {
                    //Http response message
                    var httpResponse = await httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);

                    //Response content
                    var responseContent = JArray.Parse(JObject.Parse(await httpResponse.Content.ReadAsStringAsync())["data"]["ammo"].ToString());
                    foreach (var item in responseContent)
                    {
                        var tdaammo = new TarkovDevApiAmmo(item);
                        _ = tdaammo;
                        hash.Add(tdaammo);
                    }
                }
            }
            catch (Exception ex)
            {
                this.AppInsightsService?.TrackException(ex);
                return new BSGErrorBodyResult(400, "Unable to find ammo from Tarkov Dev");
            }

            ////Localization pre-loading
            IEnumerable<KeyValuePair<string, JToken>> enumerableLocalizations =
                DatabaseService.LoadDatabaseFileAsEnumerable("locales/global/en.json");
            ConcurrentDictionary<string, string> localizationDictionary =
                new(enumerableLocalizations.Select(localization =>
                    new KeyValuePair<string, string>(localization.Key, localization.Value.ToString())));


            ////Item pre-loading
            //IEnumerable<MinimalTemplateItem> templatesItemsMinimalEnumerable =
            //    DatabaseService.LoadDatabaseFileAsEnumerable("templates/items.json")
            //        .Select(x => new MinimalTemplateItem(x.Value, 0));

            //const string ammoParentId = "5485a8684bdc2da71d8b4567";
            string[] ammoIdsToIgnore = ["5996f6d686f77467977ba6cc", "5d2f2ab648f03550091993ca", "5cde8864d7f00c0010373be1"];

            decimal ammoBestRating = -1;
            List<JObject> unorderedItems = new List<JObject>();
            await Parallel.ForEachAsync(hash, (item, _) =>
            {
                //if (!string.Equals(item.ParentID, ammoParentId)) return ValueTask.CompletedTask;
                if (ammoIdsToIgnore.Contains(item.Id)) return ValueTask.CompletedTask;
                if (!string.Equals(item.AmmoType, "bullet")) return ValueTask.CompletedTask;
                if (!string.Equals(item.Caliber, caliber)) return ValueTask.CompletedTask;

                //Localized name
                string localizedItemName = item.Id;
                if (!localizationDictionary.TryGetValue($"{item.Id} Name", out localizedItemName))
                {
                    localizationDictionary.TryGetValue($"{item.Id} ShortName", out localizedItemName);
                }

                decimal roughLargeNumberRating = 0;
                if (item.Damage > 0)
                {
                    var armorDamage = Math.Max(1, item.ArmorDamage > 0 ? item.ArmorDamage * 2.23 : 1);
                    var penRating = Math.Max(1, item.PenetrationPower > 0 ? item.PenetrationPower * 2.09 : 1);
                    var damageRating = Math.Max(1, item.Damage > 0 ? item.Damage * 0.015 : 1);

                    roughLargeNumberRating = (decimal)(armorDamage * penRating * damageRating);
                    if (roughLargeNumberRating > ammoBestRating)
                        ammoBestRating = roughLargeNumberRating;
                }

                unorderedItems.Add(new JObject
                {
                    ["itemId"] = item.Id,
                    ["langItem"] = localizedItemName,
                    ["caliber"] = item.Caliber,
                    ["armorDamage"] = item.ArmorDamage,
                    ["penetration"] = item.PenetrationPower,
                    ["damage"] = item.Damage,
                    ["rating"] = 0,
                    ["ratingNumber"] = roughLargeNumberRating,
                    ["ratingWord"] = "",
                    ["ratingTier"] = ""
                });
                return ValueTask.CompletedTask;
            });

            foreach (var item in unorderedItems)
            {
                var roughLargeNumberRating = (decimal)(item["ratingNumber"] ?? 0);

                int rating = 0;
                var ratioRating = roughLargeNumberRating / (decimal)ammoBestRating;
                rating = Math.Min(100, (int)Math.Round(ratioRating * 100, 3));

                if (rating == 0)
                    rating = 1;

                string ratingWord = "";
                if (rating > 90)
                    ratingWord = "Best in Caliber";
                else if (rating > 75)
                    ratingWord = "Very Good";
                else if (rating > 60)
                    ratingWord = "Good";
                else if (rating > 45)
                    ratingWord = "OK";
                else if (rating > 15)
                    ratingWord = "Bad";
                else
                    ratingWord = "Sh*t / Aim for Legs";

                string ratingTier = "F";
                if (rating > 90)
                    ratingTier = "S";
                else if (rating > 75)
                    ratingTier = "A";
                else if (rating > 60)
                    ratingTier = "B";
                else if (rating > 45)
                    ratingTier = "C";
                else if (rating > 30)
                    ratingTier = "D";
                else if (rating > 15)
                    ratingTier = "E";

                item["rating"] = rating;
                item["ratingWord"] = ratingWord;
                item["ratingTier"] = ratingTier;

                item.Remove("ratingNumber");
            }

            JArray rootResponseObject = JArray.FromObject(unorderedItems.OrderByDescending(x => x["rating"]));
            return new BSGSuccessBodyResult(rootResponseObject);
        }

        //TODO: Look into sharing references to shared strings such as caliber and parent ID
        private readonly struct MinimalTemplateItem(JToken templateItem, long price)
        {
            public readonly string ItemID = templateItem["_id"]?.ToString() ?? string.Empty;
            public readonly string ParentID = templateItem["_parent"]?.ToString() ?? string.Empty;
            public readonly string PvERarity = templateItem.SelectToken("_props.RarityPvE")?.ToString() ?? string.Empty;
            public readonly MinimalTemplateItemProps Props = new(templateItem["_props"]);
            public readonly long Price = price;
        }

        private readonly struct MinimalTemplateItemProps(JToken templateItemProps)
        {
            public readonly string Caliber = templateItemProps["Caliber"]?.ToString() ?? string.Empty;
            public readonly string AmmoType = templateItemProps["ammoType"]?.ToString() ?? string.Empty;
            public readonly int ArmorDamage = (int)(templateItemProps["ArmorDamage"] ?? 0);
            public readonly int PenetrationPower = (int)(templateItemProps["PenetrationPower"] ?? 0);
            public readonly int Damage = (int)(templateItemProps["Damage"] ?? 0);
        }

        public readonly struct TarkovDevApiAmmo(JToken ammoItem)
        {
            public readonly string Id = ammoItem["item"]?["id"]?.ToString() ?? string.Empty;
            public readonly string Caliber = ammoItem["caliber"]?.ToString() ?? string.Empty;
            public readonly string AmmoType = ammoItem["ammoType"]?.ToString() ?? string.Empty;
            public readonly int ArmorDamage = (int)(ammoItem["armorDamage"] ?? 0);
            public readonly int PenetrationPower = (int)(ammoItem["penetrationPower"] ?? 0);
            public readonly int Damage = (int)(ammoItem["damage"] ?? 0);
        }
    }

    enum PvERarity
    {
        Unknown = 0,
        Common = 0,
        Rare = 1,
        Superrare = 2,
        Not_exist = 2,
    }
}
