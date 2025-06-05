using FMT.FileTools;
using SharpCompress.Readers;

namespace Paulov.TarkovServices.Providers
{
    public class ZipDatabaseProvider
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

        public ZipDatabaseProvider()
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
                        entries.Add(new EntryModel(name, fullname));
                    }
                }
                return entries;
            }
        }
    }

    public class EntryModel
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public EntryModel(string name, string fullname)
        {
            Name = name;
            FullName = fullname;
        }

        public Stream Open()
        {
            var reader = ZipDatabaseProvider.DatabaseAssetZipReader;
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    var fullname = reader.Entry?.Key != null ? reader.Entry.Key : "/";
                    if (fullname == FullName)
                    {
                        return reader.OpenEntryStream();
                    }
                }
            }
            return null;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
