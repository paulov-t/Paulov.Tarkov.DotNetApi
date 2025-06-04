using EFT;
using Newtonsoft.Json;
using Paulov.TarkovModels;

namespace Paulov.TarkovServices
{
    /// <summary>
    /// Provides functionality for managing user profiles, including creation, retrieval, and persistence.
    /// </summary>
    /// <remarks>The <see cref="SaveProvider"/> class is responsible for handling user account profiles,
    /// including operations such as creating new accounts, saving profiles to disk, loading profiles from disk, and
    /// retrieving specific profile details. Profiles are stored in memory and serialized to JSON files located in the
    /// application's user profile directory.  This class supports operations for both PMC (Player Main Character) and
    /// Scav profiles, as well as profile modes and inventory management.</remarks>
    public class SaveProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public static Random Randomizer { get; } = new Random();

        /// <summary>
        /// 
        /// </summary>
        public SaveProvider()
        {
            var jsonSettings = new JsonSerializerSettings() { Converters = DatabaseProvider.CachedSerializer.Converters };

            var userProfileDirectory = Path.Combine(AppContext.BaseDirectory, "user", "profiles");
            var profileFiles = Directory.GetFiles(userProfileDirectory);
            foreach (var profileFilePath in profileFiles)
            {
                var fileInfo = new FileInfo(profileFilePath);
                if (fileInfo == null)
                    continue;

                var fileText = File.ReadAllText(profileFilePath);
                if (fileText == null)
                    continue;

                var model = JsonConvert.DeserializeObject<Account>(fileText, jsonSettings);
                Profiles.Add(fileInfo.Name.Replace(".json", ""), model);
            }
        }

        private Dictionary<string, Account> Profiles { get; } = new Dictionary<string, Account>();

        public Dictionary<string, Account> GetProfiles()
        {
            return Profiles;
        }

        public string CreateAccount(Dictionary<string, object> parameters)
        {
            if (parameters == null)
                return null;

            var sessionId = new MongoID(true).ToString();
            var newProfileDetails = new Account()
            {
                AccountId = sessionId,
                Username = parameters["username"].ToString(),
                Password = parameters["password"].ToString(), // Needs to be Hashed!
                Edition = parameters["edition"].ToString()
            };

            CreateProfile(newProfileDetails);
            LoadProfile(sessionId);
            SaveProfile(sessionId);

            return sessionId;
        }

        public void SaveProfile(string sessionId, Account profileModel = null)
        {
            if (profileModel != null)
                Profiles[sessionId] = profileModel;

            var userProfileDirectory = Path.Combine(AppContext.BaseDirectory, "user", "profiles");
            Directory.CreateDirectory(userProfileDirectory);
            var filePath = Path.Combine(userProfileDirectory, $"{sessionId}.json");

            var jsonSettings = new JsonSerializerSettings() { Converters = DatabaseProvider.CachedSerializer.Converters };

            var serializedProfile = JsonConvert.SerializeObject(Profiles[sessionId], Formatting.Indented, jsonSettings);

            File.WriteAllText(filePath, serializedProfile);
        }

        public Account LoadProfile(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            var prof = Profiles[sessionId] as Account;
            //CleanIdsOfInventory(prof);

            return prof;
        }

        public AccountProfileMode GetAccountProfileMode(string sessionId)
        {
            var prof = Profiles[sessionId] as Account;
            if (prof == null)
                return null;

            if (prof.Modes == null)
                prof.Modes = new AccountProfileModes();

            switch (prof.CurrentMode)
            {
                case "regular":
                    return prof.Modes.Regular;
                case "pve":
                    return prof.Modes.PVE;
                case "arena":
                    return prof.Modes.Arena;
            }

            return null;
        }

        public AccountProfileCharacter GetPmcProfile(string sessionId)
        {
            var prof = Profiles[sessionId] as Account;
            if (prof == null)
                return null;

            var characters = GetAccountProfileMode(sessionId)?.Characters;
            if (characters == null)
                return null;

            var pmcObject = characters.PMC;
            if (pmcObject == null)
                return null;

            // Add Arena
            //if (!pmcObject.ContainsKey("Presets"))
            //    pmcObject.Add("Presets", new JObject());

            //if (!pmcObject.ContainsKey("RankInfo"))
            //    pmcObject.Add("RankInfo", new JObject());

            //if (pmcObject.ContainsKey("Info"))
            //{

            //    var info = JObject.Parse(JsonConvert.SerializeObject(pmcObject["Info"]));
            //    if (!info.ContainsKey("RankInfo"))
            //    {
            //        //var rankInfo = new JObject();
            //        ////new RankInfo() { id = "None", points = new Dictionary<string, int>() };
            //        //rankInfo.Add(new { id = "None", points = new JObject() });
            //        //info.Add("RankInfo", rankInfo);
            //    }
            //    //if (pmcObject["Info"]. == null)
            //    //    pmcObject["Info"].RankInfo = new JObject();
            //}

            return pmcObject;
        }

        public Dictionary<EFT.MongoID, EFT.Profile.TraderInfo> GetPmcProfileTradersInfo(string sessionId)
        {
            var pmcProfile = GetPmcProfile(sessionId);
            if (pmcProfile == null) return null;

            var objTradersInfo = pmcProfile.GetProfile().TradersInfo;// ["TradersInfo"].ToObject<Dictionary<string, EFT.Profile.TraderInfo>>();

            return objTradersInfo;
        }

        public AccountProfileCharacter GetScavProfile(string sessionId)
        {
            var characters = GetAccountProfileMode(sessionId)?.Characters;
            if (characters == null)
                return null;

            return characters.Scav;
        }

        private void CreateProfile(Account newProfileDetails)
        {
            Profiles.Add(newProfileDetails.AccountId, newProfileDetails);
        }

        public bool ProfileExists(string username, out string sessionId)
        {
            sessionId = null;
            foreach (var profile in Profiles.Values)
            {
                if (profile.Username == username)
                {
                    sessionId = profile.AccountId;
                    return true;
                }
            }

            return false;

        }

        public bool NameExists(string username)
        {
            foreach (var profile in Profiles.Values)
            {
                if (profile.Username == username)
                    return true;
            }

            return false;
        }

        public void CleanIdsOfInventory(Account profile)
        {
            if (profile == null)
                return;

            //var inventory = GetAccountProfileMode(profile.AccountId)?.Characters?.PMC?.Inventory;
            //CleanIdsOfItems(inventory);

        }

        public void CleanIdsOfItems(EFT.InventoryLogic.Inventory inventory)
        {
            //var equipmentId = inventory["equipment"].ToString();
            //var fastPanelId = inventory["fastPanel"].ToString();
            //var hideoutAreaStashesId = inventory["hideoutAreaStashes"].ToString();
            //var questRaidItemsId = inventory["questRaidItems"].ToString();
            //var questStashItemsId = inventory["questStashItems"].ToString();
            //var sortingTableId = inventory["sortingTable"].ToString();
            //var stashId = inventory["stash"].ToString();

            //Dictionary<string, string> remappedIds = new();
            //Dictionary<string, string> allRemappedIds = new();

            //remappedIds.Clear();
            //Dictionary<string, int> IdCounts = new();


            //foreach (var item in inventory["items"])
            //{
            //    if (item["_id"].ToString() == equipmentId)
            //        continue;

            //    if (item["_id"].ToString() == fastPanelId)
            //        continue;

            //    if (item["_id"].ToString() == hideoutAreaStashesId)
            //        continue;

            //    if (item["_id"].ToString() == questRaidItemsId)
            //        continue;

            //    if (item["_id"].ToString() == questStashItemsId)
            //        continue;

            //    if (item["_id"].ToString() == sortingTableId)
            //        continue;

            //    if (item["_id"].ToString() == stashId)
            //        continue;

            //    var oldId = item["_id"].ToString();
            //    var newId = MongoID.Generate();
            //    if (!remappedIds.ContainsKey(oldId))
            //    {
            //        remappedIds.Add(oldId, newId);
            //        item["_id"] = newId;
            //    }
            //}

            //foreach (var item in inventory["items"])
            //{
            //    var jO = item as JObject;
            //    if (jO.ContainsKey("parentId"))
            //    {
            //        if (remappedIds.ContainsKey(jO["parentId"].ToString()))
            //            jO["parentId"] = remappedIds[jO["parentId"].ToString()];

            //    }
            //}


        }

        //public List<FlatItem> GetProfileInventoryItems(string sessionId)
        //{
        //    var pmcProfile = GetPmcProfile(sessionId);
        //    var inv = (JToken)pmcProfile["Inventory"];
        //    List<FlatItem> items = new();
        //    var invItems = (JArray)inv["items"];
        //    foreach (var item in invItems)
        //    {
        //        try
        //        {
        //            items.Add(item.ToObject<FlatItem>());
        //        }
        //        catch { }
        //    }

        //    return items;
        //}

        //public void ProcessProfileChanges(string sessionId, Changes changes)
        //{
        //    if (!Profiles.ContainsKey(sessionId))
        //        return;

        //    var invItems = GetProfileInventoryItems(sessionId);

        //    if (changes.Stash != null)
        //    {
        //        if (changes.Stash.del != null)
        //        {

        //            foreach (var d in changes.Stash.del)
        //            {
        //                if (d == null)
        //                    continue;

        //                var itemToRemoveWithChildren = FindAndReturnChildrenByItems(invItems, d._id);
        //                foreach (var child in itemToRemoveWithChildren)
        //                {
        //                    invItems.Remove(child);
        //                }
        //            }
        //        }
        //    }

        //    var prof = Profiles[sessionId] as ProfileModel;
        //    var pmcProfile = GetPmcProfile(sessionId);
        //    var inv = (JToken)pmcProfile["Inventory"];
        //    inv["items"] = JArray.FromObject(invItems);
        //    pmcProfile["Inventory"]["items"] = inv["items"];
        //    prof.Characters["pmc"] = pmcProfile;

        //    SaveProfile(sessionId, prof);

        //}

        //public List<FlatItem> FindAndReturnChildrenByItems(List<FlatItem> items, string itemId)
        //{
        //    List<FlatItem> result = new();
        //    foreach (var item in items)
        //    {
        //        if (item.parentId == itemId)
        //        {
        //            result.AddRange(FindAndReturnChildrenByItems(items, itemId));
        //        }
        //    }

        //    result.Add(items.Find(x => x._id == itemId));
        //    return result;
        //}

    }
}
