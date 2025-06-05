using Paulov.TarkovServices.Models;
using System.Data;

namespace Paulov.TarkovServices.Providers.Interfaces
{
    public interface IDatabaseProvider
    {
        public void Connect(string connectionString);

        public void Disconnect();

        public List<EntryModel> Entries { get; }

        public Stream GetEntryStream(string entryName);

        DataTable GetData(string query);

        void ExecuteCommand(string query);

    }
}
