using FMT.FileTools;
using Paulov.TarkovServices.Models;
using System.IO.Compression;

namespace Paulov.TarkovServices.Providers.ZipDatabaseProviders
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

        public Stream Open(string entryName)
        {
            return ZipArchive.Entries.First(x => x.FullName == entryName).Open();
        }
    }
}
