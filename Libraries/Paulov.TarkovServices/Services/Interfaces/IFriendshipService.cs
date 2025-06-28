using EFT;
using Newtonsoft.Json.Linq;
using Paulov.TarkovModels;

namespace Paulov.TarkovServices.Services.Interfaces
{
    public interface IFriendshipService
    {
        public MongoID? SendFriendRequest(string fromId, string toId);

        public JObject CreateUpdatableChatMemberJObject(Account account, string gameMode = null);


    }
}
