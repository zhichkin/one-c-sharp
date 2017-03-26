using Zhichkin.Metadata.Model;
using System.Data;
using System.Collections.Generic;

namespace Zhichkin.DXM.Model
{
    public interface IChangeTrackingService
    {
        void Enable(Entity entity);
        void Disable(Entity entity);
        bool IsEnabled(Entity entity);
        IDataReader GetChanges();

    }
}
