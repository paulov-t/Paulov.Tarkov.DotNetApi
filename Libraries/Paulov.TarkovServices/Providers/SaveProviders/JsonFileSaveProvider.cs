using EFT;
using Newtonsoft.Json;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services;

namespace Paulov.TarkovServices.Providers.SaveProviders
{
    /// <summary>
    /// Provides functionality for managing user profiles, including creation, retrieval, and persistence.
    /// </summary>
    /// <remarks>The <see cref="JsonFileSaveProvider"/> class is responsible for handling user account profiles,
    /// including operations such as creating new accounts, saving profiles to disk, loading profiles from disk, and
    /// retrieving specific profile details. Profiles are stored in memory and serialized to JSON files located in the
    /// application's user profile directory.  This class supports operations for both PMC (Player Main Character) and
    /// Scav profiles, as well as profile modes and inventory management.</remarks>
    public class JsonFileSaveProvider : ISaveProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public static Random Randomizer { get; } = new Random();

        /// <summary>
        /// 
        /// </summary>
        public JsonFileSaveProvider()
        {
            //var jsonSettings = new JsonSerializerSettings() { Converters = DatabaseProvider.CachedSerializer.Converters };

            //var userProfileDirectory = Path.Combine(AppContext.BaseDirectory, "user", "profiles");
            //var profileFiles = Directory.GetFiles(userProfileDirectory);

            //var profilesToDelete = new List<string>();
            //foreach (var profileFilePath in profileFiles)
            //{
            //    var fileInfo = new FileInfo(profileFilePath);
            //    if (fileInfo == null)
            //        continue;

            //    var fileText = File.ReadAllText(profileFilePath);
            //    if (fileText == null)
            //        continue;

            //    try
            //    {
            //        var model = fileText.ParseJsonTo<Account>();
            //        //var model = JsonConvert.DeserializeObject<Account>(fileText, jsonSettings);
            //        Profiles.Add(fileInfo.Name.Replace(".json", ""), model);
            //    }
            //    catch
            //    {
            //        profilesToDelete.Add(profileFilePath);
            //    }
            //}

            //foreach (var item in profilesToDelete)
            //{
            //    File.Delete(item);
            //}
        }

        //private Dictionary<string, Account> Profiles { get; } = new Dictionary<string, Account>();

        public Dictionary<string, Account> GetProfiles()
        {
            // AccountProfileCharacter requires Globals to be loaded, so we load them here.
            GlobalsService.Instance.LoadGlobalsIntoComfortSingleton();

            //return Profiles;
            var jsonSettings = new JsonSerializerSettings() { Converters = DatabaseService.CachedSerializer.Converters };

            var userProfileDirectory = Path.Combine(AppContext.BaseDirectory, "user", "profiles");
            Directory.CreateDirectory(userProfileDirectory);
            var profileFiles = Directory.GetFiles(userProfileDirectory);

            var profiles = new Dictionary<string, Account>();
            var profilesToDelete = new List<string>();
            foreach (var profileFilePath in profileFiles)
            {
                var fileInfo = new FileInfo(profileFilePath);
                if (fileInfo == null)
                    continue;

                var fileText = File.ReadAllText(profileFilePath);
                if (fileText == null)
                    continue;

                try
                {
                    var model = fileText.ParseJsonTo<Account>();
                    profiles.Add(fileInfo.Name.Replace(".json", ""), model);
                }
                catch
                {
                    profilesToDelete.Add(profileFilePath);
                }
            }

            foreach (var item in profilesToDelete)
            {
                File.Delete(item);
            }

            return profiles;
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
            var account = LoadProfile(sessionId);
            SaveProfile(sessionId, account);

            return sessionId;
        }

        public void SaveProfile(string sessionId, Account accountModel)
        {
            if (string.IsNullOrEmpty(sessionId))
                throw new ArgumentNullException(nameof(sessionId), "Session ID cannot be null or empty.");

            if (accountModel == null)
                throw new ArgumentNullException(nameof(accountModel), "Profile model cannot be null.");

            var userProfileDirectory = Path.Combine(AppContext.BaseDirectory, "user", "profiles");
            Directory.CreateDirectory(userProfileDirectory);
            var filePath = Path.Combine(userProfileDirectory, $"{sessionId}.json");

            var jsonSettings = new JsonSerializerSettings() { Converters = DatabaseService.CachedSerializer.Converters };

            var serializedProfile = JsonConvert.SerializeObject(accountModel, Formatting.Indented, jsonSettings);

            File.WriteAllText(filePath, serializedProfile);
        }

        public Account LoadProfile(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            if (!GetProfiles().ContainsKey(sessionId))
                return null;

            var prof = GetProfiles()[sessionId] as Account;
            //CleanIdsOfInventory(prof);

            return prof;
        }

        public AccountProfileMode GetAccountProfileMode(Account account)
        {
            switch (account.CurrentMode.ToLower())
            {
                case "regular":
                    return account.Modes.Regular;
                case "pve":
                    return account.Modes.PVE;
                case "arena":
                    return account.Modes.Arena;
            }

            return null;
        }

        public AccountProfileMode GetAccountProfileMode(string sessionId)
        {
            var account = GetProfiles()[sessionId] as Account;
            if (account == null)
                return null;

            if (account.Modes == null)
                account.Modes = new AccountProfileModes();

            return GetAccountProfileMode(account);
        }

        public AccountProfileCharacter GetPmcProfile(string sessionId)
        {
            var prof = GetProfiles()[sessionId];
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

        public Dictionary<MongoID, Profile.TraderInfo> GetPmcProfileTradersInfo(string sessionId)
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
            var profiles = GetProfiles();
            profiles.Add(newProfileDetails.AccountId, newProfileDetails);
            SaveProfile(newProfileDetails.AccountId, newProfileDetails);
        }

        public bool ProfileExists(string username, out string sessionId)
        {
            sessionId = null;
            foreach (var profile in GetProfiles().Values)
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
            foreach (var profile in GetProfiles().Values)
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
