using EFT;
using Newtonsoft.Json;

namespace Paulov.Tarkov.WebServer.DOTNET.Models
{
    /// <summary>
    /// TODO: Using this would be the preferred way, rather than reinventing the wheel
    /// The inherited class uses model descriptors (just like we did below!)
    /// We need GClass2030 to be Remapped!
    /// </summary>
    public class AccountProfileCharacter : GClass2030
    {
        public AccountProfileCharacter() { }

        public EFT.Profile GetProfile()
        {
            return new EFT.Profile(this);
        }
    }


    public class AccountProfileCharacter2
    {
        public AccountProfileCharacter2()
        {

        }

        [JsonProperty("_id")]
        public MongoID Id;

        [JsonProperty("aid")]
        public string AccountId;

        [JsonProperty("savage")]
        public MongoID? PetId;

        [JsonProperty("karmaValue")]
        public float KarmaValue;

        [JsonProperty("Info")]
        public GClass2023 Info;

        [JsonProperty("Customization")]
        public Dictionary<EBodyModelPart, MongoID> Customization = new Dictionary<EBodyModelPart, MongoID>();

        [JsonProperty("Encyclopedia")]
        public Dictionary<MongoID, bool> Encyclopedia = new Dictionary<MongoID, bool>();

        [JsonProperty("Health")]
        public Profile.ProfileHealthClass Health = new Profile.ProfileHealthClass();

        [JsonProperty("Inventory")]
        public GClass1717 Inventory;

        [JsonProperty("InsuredItems")]
        public GClass1361[] InsuredItems = Array.Empty<GClass1361>();

        [JsonProperty("Skills")]
        public GClass2024 Skills = new GClass2024();

        [JsonProperty("Notes")]
        public GClass2946.GClass2947 Notes = new GClass2946.GClass2947();

        [JsonProperty("TaskConditionCounters")]
        public Dictionary<MongoID, GClass2027> TaskConditionCounters = new Dictionary<MongoID, GClass2027>();

        [JsonProperty("Quests")]
        public List<QuestDataClass> QuestsData = new List<QuestDataClass>();

        [JsonProperty("Achievements")]
        public Dictionary<MongoID, int> AchievementsData = new Dictionary<MongoID, int>();

        [JsonProperty("Prestige")]
        public Dictionary<MongoID, int> PrestigeData = new Dictionary<MongoID, int>();

        [JsonProperty("UnlockedInfo")]
        public Profile.GClass2008 UnlockedRecipeInfo = new Profile.GClass2008();

        [JsonProperty("moneyTransferLimitData")]
        public Profile.GClass2009 TransferLimitData = new Profile.GClass2009();

        [JsonProperty("Bonuses")]
        public GClass2028[] Bonuses = Array.Empty<GClass2028>();

        [JsonProperty("Hideout")]
        public GClass2002 Hideout = new GClass2002();

        [JsonProperty("RagfairInfo")]
        public GClass2155 RagfairInfo = new GClass2155();

        [JsonProperty("WishList")]
        public Dictionary<MongoID, byte> WishList = new Dictionary<MongoID, byte>();

        [JsonProperty("Stats")]
        public GClass2021 Stats = new GClass2021();

        [JsonProperty("CheckedMagazines")]
        public Dictionary<MongoID, int> CheckedMagazines = new Dictionary<MongoID, int>();

        [JsonProperty("CheckedChambers")]
        public List<MongoID> CheckedChambers = new List<MongoID>();

        [JsonProperty("TradersInfo")]
        public Dictionary<MongoID, GClass2029> TradersInfo = new Dictionary<MongoID, GClass2029>();
    }

}
