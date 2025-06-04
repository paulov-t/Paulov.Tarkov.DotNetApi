using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Arena
{
    [Serializable]
    public class ArenaCustomGameBaseInfo
    {
        public string OwnerProfileId;

        public string Id;

        public string Name;

        public string Region;

        public int MaxSlotsCount;

        public int MaxObserversCount;

        public int OccupiedSlotsCount;

        public int TeamsCount;

        public bool IsTournament;

        public bool HasPassword;

        public EArenaGameMode Mode;

        public EArenaMap Map;

        public int EntryAttemptsLeft;

        public int? EntryRetryAfter;

        public bool Equals(ArenaCustomGameBaseInfo other)
        {
            if (OwnerProfileId == other.OwnerProfileId && Id == other.Id && Name == other.Name && Region == other.Region && MaxSlotsCount == other.MaxSlotsCount && MaxObserversCount == other.MaxObserversCount && OccupiedSlotsCount == other.OccupiedSlotsCount && TeamsCount == other.TeamsCount && IsTournament == other.IsTournament && HasPassword == other.HasPassword && Mode == other.Mode && Map == other.Map && EntryAttemptsLeft == other.EntryAttemptsLeft)
            {
                return EntryRetryAfter == other.EntryRetryAfter;
            }
            return false;
        }
    }

}
