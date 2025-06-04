using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SIT.Arena
{
    [Serializable]
    public class ArenaGameModeSettings
    {
        public string Id;

        public int CalibrationPoints;

        //public EArenaGameMode Name;
        public string Name { get; set; } = "";// = EArenaGameMode.Coop.ToString();

        //public EArenaGameMode ParentMode = EArenaGameMode.Undefined;
        public string ParentMode { get; set; } = "";// = EArenaGameMode.Coop.ToString();

        public readonly List<EArenaGameMode> SubModes = new List<EArenaGameMode>();

        public readonly List<EArenaMap> SupportedMaps = new()
        {
            EArenaMap.Arena_AirPit,
        };

        public EArenaGameModeType ModeType;

        public ResultCalculationParameters ResultCalculationParameters = new ResultCalculationParameters();

        public TeamData[] TeamsData = new TeamData[1]
        {
        new TeamData
        {
            Size = 1
        }
        };

        public Dictionary<string, Dictionary<string, int>> ModeSettings = new Dictionary<string, Dictionary<string, int>>();

        public bool IsSubMode;

        public bool Enabled = true;

        public bool IsRanked;

        public bool FirstBloodEnabled = true;

        public int AveragePlayTime = 15;

        public string ArpTitle = string.Empty;

        public bool SetAllItemsSearched;

        public int MinPlayersForCustomStart;

        private string String_0 => Name.ToString();

        public string ShortName => String_0 + "ShortName";

        public string Description => String_0 + "Description";

        public string DescriptionShort => String_0 + "DescriptionShort";

        public string AveragePlayTimeText => string.Format("{0} {1}", AveragePlayTime, "MinutesShort");

        public bool IsContainer => SubModes.Count > 0;

        public IEnumerable<float> GetPlayersInMatchForUI()
        {
            //if (!Name.IsTeamFight() && Name != EArenaGameMode.Coop)
            //{
            //    TeamData[] teamsData = TeamsData;
            //    foreach (TeamData teamData in teamsData)
            //    {
            //        yield return teamData.Size;
            //    }
            //}
            //else
            //{
                yield return TeamsData.Max((TeamData data) => data.Size);
            //}
        }

       
    }

    public class ResultCalculationParameters
    {
        public int PresetCostPercentModifier = 300;

        public int FragsPercentModifier = 300;

        public int AssistPercentModifier = 100;

        public int DamagePercentModifier = 200;

        public int MVPPercentModifier = 100;

        public int KillExp = 100;

        public int HeadShotExp = 150;

        public int DoubleKillExp = 200;

        public int OverKillExp = 300;

        public int ObjectivePointExp = 100;

        public int TeammateKillExp = -400;

        public int ViewerKillExp = -800;

        public float HealExpMultiplier = 0.5f;

        public float LootingExpMultiplier = 0.5f;

        public float CommissionDecreasePerCharismaLevel = 0.01f;

        public int LastPriceWinningPlace = 1;

        public List<object> RewardsByPlace = new List<object>();

        public Dictionary<string, object> ParametersByRankingMode = new Dictionary<string, object>();
    }

    public class TeamData
    {
        public int Size;
    }


}
