using Paulov.TarkovServices.Models;

namespace Paulov.TarkovServices.Providers
{
    public interface IDatabaseProvider
    {
        public List<EntryModel> Entries { get; }
        public Stream Open(string entryName);
    }
}
