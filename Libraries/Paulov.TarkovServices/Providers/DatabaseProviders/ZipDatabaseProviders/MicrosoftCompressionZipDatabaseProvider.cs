using FMT.FileTools;
using Newtonsoft.Json;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using System.Data;
using System.IO.Compression;
using System.Text;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.ZipDatabaseProviders
{
    public sealed class MicrosoftCompressionZipDatabaseProvider : IDatabaseProvider
    {
        public static Stream DatabaseAssetStream { get { return EmbeddedResourceHelper.GetEmbeddedResourceByName("database.zip"); } }
        public static ZipArchive ZipArchive { get { return new ZipArchive(DatabaseAssetStream); } }
        public List<EntryModel> Entries
        {
            get
            {
                return ZipArchive.Entries.Select(x => new EntryModel(x.Name, x.FullName, this)).ToList();
            }
        }

        public void Connect(string connectionString)
        {
        }

        public void Disconnect()
        {
        }

        public void ExecuteCommand(string query)
        {
        }

        public DataTable GetData(string query)
        {
            var entryStream = GetEntryStream(query);
            var ms = new MemoryStream();
            entryStream.CopyTo(ms);

            var json = Encoding.UTF8.GetString(ms.ToArray());

            DataTable dataTable = new();
            if (string.IsNullOrWhiteSpace(json))
            {
                return dataTable;
            }
            dataTable = JsonConvert.DeserializeObject<DataTable>(json);
            return dataTable;
        }

        public Stream GetEntryStream(string entryName)
        {
            return ZipArchive.Entries.First(x => x.FullName == entryName).Open();
        }
    }
}
