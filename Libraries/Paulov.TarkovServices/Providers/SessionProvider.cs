using Paulov.TarkovServices.Providers.Interfaces;

namespace Paulov.TarkovServices.Providers
{
    public class SessionProvider : ISessionProvider
    {
        public string SessionId { get; set; }
    }
}
