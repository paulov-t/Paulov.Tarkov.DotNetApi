using ChatShared;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;
using Paulov.TarkovServices.Helpers;
using Paulov.TarkovServices.Providers.Interfaces;
using Paulov.TarkovServices.Services.Interfaces;

namespace Paulov.TarkovServices.Services
{
    public class AccountService : IAccountService
    {
        private ISaveProvider _saveProvider;

        public AccountService(ISaveProvider saveProvider)
        {
            _saveProvider = saveProvider ?? throw new ArgumentNullException(nameof(saveProvider));
        }

        public Account GetAccountByAID(string aid)
        {
            if (string.IsNullOrEmpty(aid))
            {
                throw new ArgumentException("Account ID cannot be null or empty.", nameof(aid));
            }
            var profiles = _saveProvider.GetProfiles();
            if (profiles == null || !profiles.Any())
            {
                return null;
            }
            return profiles.Values
                .Where(x => _saveProvider.GetAccountProfileMode(x).Characters.PMC.AccountId == aid)
                .FirstOrDefault();
        }

        public Dictionary<string, object> GetUpdatableChatMember(Account account, string gameMode = null)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account), "Account cannot be null.");
            }

            AccountProfileMode accountProfileMode;

            if (gameMode == null)
            {
                accountProfileMode = _saveProvider.GetAccountProfileMode(account);
            }
            else
            {
                switch (gameMode.ToUpperInvariant())
                {
                    case "PVE":
                        accountProfileMode = account.Modes.PVE;
                        break;
                    case "PVP":
                    case "REGULAR":
                        accountProfileMode = account.Modes.Regular;
                        break;
                    case "ARENA":
                        accountProfileMode = account.Modes.Arena;
                        break;
                    default:
                        throw new ArgumentException($"Unknown game mode: {gameMode}", nameof(gameMode));
                }
            }

            var pmc = accountProfileMode.Characters.PMC;
            if (pmc == null)
                return null;

            var info = new UpdatableChatMember.UpdatableChatMemberInfo();
            info.Nickname = pmc.Info.Nickname;
            info.Side = pmc.Info.Side == EFT.EPlayerSide.Usec ? EFT.EChatMemberSide.Usec : EFT.EChatMemberSide.Bear;
            info.Banned = false;
            info.Ignored = false;
            info.Level = pmc.Info.Level;
            info.PrestigeLevel = pmc.Info.PrestigeLevel;
            info.MemberCategory = pmc.Info.MemberCategory;
            info.SelectedMemberCategory = pmc.Info.SelectedMemberCategory;

            var member = new Dictionary<string, object>();
            member.Add("AccountId", pmc.AccountId);
            member.Add("_id", pmc.Id);
            member.Add("aid", pmc.AccountId);
            member.Add("Info", info);
            return member;
        }

        public MatchingGroupMember GetMatchingGroupMember(Account account, bool isLeader, bool isReady, string gameMode = null)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account), "Account cannot be null.");
            }
            var updatableChatMember = GetUpdatableChatMember(account, gameMode);
            if (updatableChatMember == null)
            {
                return null;
            }

            var accountMode = _saveProvider.GetAccountProfileMode(account);
            var info = accountMode.Characters.PMC.Info;


            return new MatchingGroupMember(
                account.AccountId,
                accountMode.Characters.PMC.AccountId,
                info,
                isLeader,
                isReady
            );
        }

        public JObject GetMatchingGroupMemberWithHealthAndPVR(Account account, bool isLeader, bool isReady, string gameMode)
        {
            var matchingGroupMember = GetMatchingGroupMember(account, isLeader, isReady, gameMode);

            var obj = JObject.FromObject(matchingGroupMember, DatabaseHelpers.CachedSerializer);
            var pmcProfile = _saveProvider.GetPmcProfile(account);
            if (pmcProfile == null)
            {
                throw new ArgumentNullException(nameof(pmcProfile), "PMC profile cannot be null.");
            }
            obj["Info"]["Health"] = JObject.FromObject(pmcProfile.Health, DatabaseHelpers.CachedSerializer);
            obj["PlayerVisualRepresentation"] = null;


            return obj;
        }

    }
}
