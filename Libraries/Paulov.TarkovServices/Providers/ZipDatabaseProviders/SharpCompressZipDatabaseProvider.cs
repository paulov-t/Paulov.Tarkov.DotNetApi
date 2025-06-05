using FMT.FileTools;
using Paulov.TarkovServices.Models;
using SharpCompress.Readers;

namespace Paulov.TarkovServices.Providers.ZipDatabaseProviders
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

        public Stream Open(string entryName)
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
    }


}
