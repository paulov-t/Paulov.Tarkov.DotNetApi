using FMT.FileTools;
using Newtonsoft.Json;
using Paulov.TarkovServices.Models;
using Paulov.TarkovServices.Providers.Interfaces;
using SharpCompress.Readers;
using System.Data;
using System.Text;

namespace Paulov.TarkovServices.Providers.DatabaseProviders.ZipDatabaseProviders
{
    public sealed class SharpCompressZipDatabaseProvider : IDatabaseProvider
    {
        public static Stream DatabaseAssetStream { get { return EmbeddedResourceHelper.GetEmbeddedResourceByName("database.zip"); } }

        public static IReader DatabaseAssetZipReader
        {
            get
            {
                var reader = ReaderFactory.Open(DatabaseAssetStream);
                return reader;
            }
        }

        public SharpCompressZipDatabaseProvider()
        {

        }

        public List<EntryModel> Entries
        {
            get
            {
                List<EntryModel> entries = new();
                var reader = DatabaseAssetZipReader;
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        var fullname = reader.Entry?.Key != null ? reader.Entry.Key : "/";
                        var lastIndexOfSlash = fullname != null ? fullname.LastIndexOf('/') : 0;
                        var name = fullname != null ? fullname.Substring(lastIndexOfSlash + 1) : "";
                        entries.Add(new EntryModel(name, fullname, this));
                    }
                }
                return entries;
            }
        }

        public Stream GetEntryStream(string entryName)
        {
            var reader = DatabaseAssetZipReader;
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    var fullname = reader.Entry?.Key != null ? reader.Entry.Key : "/";
                    if (fullname == entryName)
                    {
                        return reader.OpenEntryStream();
                    }
                }
            }
            return null;
        }

        public void Connect(string connectionString)
        {
        }

        public void Disconnect()
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

        public void ExecuteCommand(string query)
        {
        }
    }


}
