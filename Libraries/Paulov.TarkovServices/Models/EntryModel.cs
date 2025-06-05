using Paulov.TarkovServices.Providers;

namespace Paulov.TarkovServices.Models
{
    public class EntryModel
    {
        public string Name { get; private set; }

        public string FullName { get; private set; }

        public IDatabaseProvider Provider { get; private set; }

        public EntryModel(string name, string fullname, IDatabaseProvider provider)
        {
            Name = name;
            FullName = fullname;
            Provider = provider;
        }

        public Stream Open()
        {
            //var reader = SharpCompressZipDatabaseProvider.DatabaseAssetZipReader;
            //while (reader.MoveToNextEntry())
            //{
            //    if (!reader.Entry.IsDirectory)
            //    {
            //        var fullname = reader.Entry?.Key != null ? reader.Entry.Key : "/";
            //        if (fullname == FullName)
            //        {
            //            return reader.OpenEntryStream();
            //        }
            //    }
            //}
            return Provider.Open(FullName);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
