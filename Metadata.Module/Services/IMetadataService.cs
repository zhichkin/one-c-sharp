using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public interface IMetadataService
    {
        InfoBase GetMetadata(string connectionString);
        List<InfoBase> GetInfoBases();
    }

    public interface IMetadataAdapter
    {
        void Load(string connectionString, InfoBase infoBase);
    }
}
