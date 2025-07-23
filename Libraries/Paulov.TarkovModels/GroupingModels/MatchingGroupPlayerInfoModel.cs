using EFT;
using Newtonsoft.Json;

namespace Paulov.TarkovModels.GroupingModels
{
    public class MatchingGroupPlayerInfoModel
    {
        [JsonProperty("Nickname")]
        public string Nickname;

        [JsonProperty("Side")]
        public EPlayerSide Side;

        [JsonProperty("Level")]
        public int Level;

        [JsonProperty("PrestigeLevel")]
        public int PrestigeLevel;

        [JsonProperty("MemberCategory")]
        public EMemberCategory MemberCategory;

        [JsonProperty("SelectedMemberCategory")]
        public EMemberCategory SelectedMemberCategory;

        [JsonProperty("SavageLockTime")]
        public double SavageLockTime;

        [JsonProperty("SavageNickname")]
        public string SavageNickname;

        [JsonProperty("GameVersion")]
        public string GameVersion;

        [JsonProperty("HasCoopExtension")]
        public bool HasCoopExtension;

        [JsonProperty("Health")]
        public Profile.ProfileHealthInfo Health;

        public MatchingGroupPlayerInfoModel(string nickname, EPlayerSide side, int level, int prestigeLevel, EMemberCategory memberCategory, EMemberCategory selectedMemberCategory, double savageLockTime, string savageNickname, string gameVersion, bool hasCoopExtension, Profile.ProfileHealthInfo health)
        {
            Nickname = nickname ?? throw new ArgumentNullException(nameof(nickname), "Nickname cannot be null.");
            Side = side;
            Level = level;
            PrestigeLevel = prestigeLevel;
            MemberCategory = memberCategory;
            SelectedMemberCategory = selectedMemberCategory;
            SavageLockTime = savageLockTime;
            SavageNickname = savageNickname ?? throw new ArgumentNullException(nameof(savageNickname), "SavageNickname cannot be null.");
            GameVersion = gameVersion ?? throw new ArgumentNullException(nameof(gameVersion), "GameVersion cannot be null.");
            HasCoopExtension = hasCoopExtension;
            Health = health ?? throw new ArgumentNullException(nameof(health), "Health cannot be null.");
        }

        public MatchingGroupPlayerInfoModel(AccountProfileCharacter character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character), "Character cannot be null.");
            }
            Nickname = character.Info.Nickname ?? throw new ArgumentNullException(nameof(character.Info.Nickname), "Nickname cannot be null.");
            Side = character.Info.Side;
            Level = character.Info.Level;
            PrestigeLevel = character.Info.PrestigeLevel;
            MemberCategory = character.Info.MemberCategory;
            SelectedMemberCategory = character.Info.SelectedMemberCategory;
            SavageLockTime = character.Info.SavageLockTime;
            SavageNickname = "Other Savage";
            GameVersion = character.Info.GameVersion ?? throw new ArgumentNullException(nameof(character.Info.GameVersion), "GameVersion cannot be null.");
            HasCoopExtension = character.Info.HasCoopExtension;
            Health = character.Health ?? throw new ArgumentNullException(nameof(character.Health), "Health cannot be null.");
        }
    }
}
