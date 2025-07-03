using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// Retrieves an updatable chat member representation for the specified account and game mode.
        /// </summary>
        /// <param name="account">The account for which to retrieve the chat member information.</param>
        /// <param name="gameMode">The game mode to filter the account profile, or null for default.</param>
        /// <returns>A dictionary containing updatable chat member information, or null if no PMC is found.</returns>
        public Dictionary<string, object> GetUpdatableChatMember(Account account, string gameMode = null);

        /// <summary>
        /// Retrieves an account associated with the specified account identifier (AID).
        /// </summary>
        /// <remarks>Use this method to retrieve account details based on a unique identifier. Ensure that
        /// the provided AID is valid and not null or empty.</remarks>
        /// <param name="aid">The unique account identifier (AID) used to locate the account. Cannot be null or empty. NOTE: This is the AID that is assigned to the PMC character aid.</param>
        /// <returns>The <see cref="Account"/> object associated with the specified AID, or <see langword="null"/> if no account
        /// is found.</returns>
        public Account GetAccountByAID(string aid);

        public MatchingGroupMember GetMatchingGroupMember(Account account, bool isLeader, bool isReady, string gameMode);

    }
}
