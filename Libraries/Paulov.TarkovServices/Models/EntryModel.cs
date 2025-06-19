using Paulov.TarkovServices.Providers.Interfaces;

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
            return Provider.GetEntryStream(FullName);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
