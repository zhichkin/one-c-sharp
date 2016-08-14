using System;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Services
{
    public sealed class MetadataService : IMetadataService
    {
        public InfoBase GetMetadata(string connectionString)
        {
            InfoBase infoBase = new InfoBase();

            //IMetadataAdapter adapter = new COMMetadataAdapter();
            IMetadataAdapter adapter = new XMLMetadataAdapter();
            adapter.Load(connectionString, infoBase);

            return infoBase;
        }

        public List<InfoBase> GetInfoBases()
        {
            throw new NotImplementedException();
        }
    }
}