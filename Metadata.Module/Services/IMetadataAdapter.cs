using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public interface IMetadataAdapter
    {
        void Load(string connectionString, InfoBase infoBase);
    }
}
