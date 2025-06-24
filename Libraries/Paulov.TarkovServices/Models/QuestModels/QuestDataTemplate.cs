using EFT;
using EFT.Quests;
using Newtonsoft.Json;
using static RawQuestClass;

namespace Paulov.TarkovServices.Models.QuestModels
{
    /// <summary>
    /// This is based on RawQuestClass, however RawQuestClass has issues with serialization and deserialization
    /// </summary>
    public sealed class QuestDataTemplate
    {
        [JsonProperty("name")]
        private string Name { get; set; }

        [JsonProperty("successMessageText")]
        public string QuestSuccessMessageKey { get; set; }

        [JsonProperty("changeQuestMessageText")]
        private string ChangeQuestMessageText { get; set; }

        [JsonProperty("description")]
        private string Description { get; set; }

        [JsonProperty("acceptPlayerMessage")]
        public string AcceptPlayerMessageKey { get; set; }

        [JsonProperty("declinePlayerMessage")]
        public string DeclinePlayerMessageKey { get; set; }

        [JsonProperty("completePlayerMessage")]
        public string CompletePlayerMessageKey { get; set; }

        [JsonProperty("acceptanceAndFinishingSource")]
        public EProfileType acceptanceAndFinishingSource { get; set; }

        [JsonProperty("progressSource")]
        public EProfileType ProgressSource { get; set; }

        [JsonProperty("rankingModes")]
        public string[] RankingModes { get; set; }

        [JsonProperty("gameModes")]
        public string[] GameModes { get; set; }

        [JsonProperty("arenaLocations")]
        public string[] ArenaLocationIds { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("location")]
        public string LocationId { get; set; }

        [JsonProperty("min_level")]
        public int Level { get; set; }

        [JsonProperty("restartable")]
        public bool Restartable { get; set; }

        [JsonProperty("traderId")]
        public string TraderId { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("type")]
        public EQuestType QuestType { get; set; }

        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("status")]
        public EQuestStatus AppearStatus { get; set; }

        [JsonProperty("KeyQuest")]
        public bool KeyQuest { get; set; }

        //[JsonProperty("rewards")]
        //public Dictionary<EQuestStatus, IReadOnlyList<GClass3798>> Rewards { get; set; }

        [JsonProperty("conditions")]
        //public ConditionsDict Conditions { get; set; }
        public QuestConditions Conditions { get; set; }

        [JsonProperty("canShowNotificationsInGame")]
        public bool CanShowNotificationsInGame { get; set; }

        [JsonProperty("instantComplete")]
        public bool InstantComplete { get; set; }

        [JsonProperty("side")]
        public EPlayerGroup PlayerGroup { get; set; }

        [JsonProperty("secretQuest")]
        public bool ServerOnly { get; set; }
    }

    public class QuestConditions
    {
        [JsonProperty("Started")]
        public List<QuestCondition> Started { get; set; }

        [JsonProperty("AvailableForFinish")]
        public List<QuestCondition> AvailableForFinish { get; set; }

        [JsonProperty("AvailableForStart")]
        public List<QuestCondition> AvailableForStart { get; set; }

        [JsonProperty("Success")]
        public List<QuestCondition> Success { get; set; }

        [JsonProperty("Fail")]
        public List<QuestCondition> Fail { get; set; }
    }

    public class QuestCondition
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("index")]
        public int? Index { get; set; }

        [JsonProperty("compareMethod")]
        public string? CompareMethod { get; set; }

        [JsonProperty("dynamicLocale")]
        public bool? DynamicLocale { get; set; }

        //[JsonProperty("visibilityConditions")]
        //public List<VisibilityCondition>? VisibilityConditions { get; set; }

        [JsonProperty("globalQuestCounterId")]
        public string? GlobalQuestCounterId { get; set; }

        [JsonProperty("parentId")]
        public string? ParentId { get; set; }

        /// <summary>
        ///     Can be: string[] or string
        /// </summary>
        //[JsonProperty("target")]
        //[JsonConverter(typeof(ListOrTConverterFactory))]
        //public ListOrT<string>? Target { get; set; }

        [JsonProperty("value")]
        //[JsonConverter(typeof(StringToNumberFactoryConverter))]
        public double? Value { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        //[JsonProperty("status")]
        //public List<QuestStatusEnum>? Status { get; set; }

        [JsonProperty("availableAfter")]
        public int? AvailableAfter { get; set; }

        [JsonProperty("dispersion")]
        public double? Dispersion { get; set; }

        [JsonProperty("onlyFoundInRaid")]
        public bool? OnlyFoundInRaid { get; set; }

        [JsonProperty("oneSessionOnly")]
        public bool? OneSessionOnly { get; set; }

        [JsonProperty("isResetOnConditionFailed")]
        public bool? IsResetOnConditionFailed { get; set; }

        [JsonProperty("isNecessary")]
        public bool? IsNecessary { get; set; }

        [JsonProperty("doNotResetIfCounterCompleted")]
        public bool? DoNotResetIfCounterCompleted { get; set; }

        [JsonProperty("dogtagLevel")]
        //[JsonConverter(typeof(StringToNumberFactoryConverter))]
        public int? DogtagLevel { get; set; }

        [JsonProperty("traderId")]
        public string? TraderId { get; set; }

        [JsonProperty("maxDurability")]
        public double? MaxDurability { get; set; }

        [JsonProperty("minDurability")]
        public double? MinDurability { get; set; }

        //[JsonProperty("counter")]
        //public QuestConditionCounter? Counter { get; set; }

        [JsonProperty("plantTime")]
        public double? PlantTime { get; set; }

        [JsonProperty("zoneId")]
        public string? ZoneId { get; set; }

        [JsonProperty("countInRaid")]
        public bool? CountInRaid { get; set; }

        [JsonProperty("completeInSeconds")]
        public double? CompleteInSeconds { get; set; }

        [JsonProperty("isEncoded")]
        public bool? IsEncoded { get; set; }

        [JsonProperty("conditionType")]
        public string? ConditionType { get; set; }

        //[JsonProperty("areaType")]
        //public HideoutAreas? AreaType { get; set; }

        //[JsonProperty("baseAccuracy")]
        //public ValueCompare? BaseAccuracy { get; set; }

        //[JsonProperty("containsItems")]
        //public List<string>? ContainsItems { get; set; }

        //[JsonProperty("durability")]
        //public ValueCompare? Durability { get; set; }

        //[JsonProperty("effectiveDistance")]
        //public ValueCompare? EffectiveDistance { get; set; }

        //[JsonProperty("emptyTacticalSlot")]
        //public ValueCompare? EmptyTacticalSlot { get; set; }

        //[JsonProperty("ergonomics")]
        //public ValueCompare? Ergonomics { get; set; }

        //[JsonProperty("height")]
        //public ValueCompare? Height { get; set; }

        //[JsonProperty("hasItemFromCategory")]
        //public List<string>? HasItemFromCategory { get; set; }

        //[JsonProperty("magazineCapacity")]
        //public ValueCompare? MagazineCapacity { get; set; }

        //[JsonProperty("muzzleVelocity")]
        //public ValueCompare? MuzzleVelocity { get; set; }

        //[JsonProperty("recoil")]
        //public ValueCompare? Recoil { get; set; }

        //[JsonProperty("weight")]
        //public ValueCompare? Weight { get; set; }

        //[JsonProperty("width")]
        //public ValueCompare? Width { get; set; }
    }
}
