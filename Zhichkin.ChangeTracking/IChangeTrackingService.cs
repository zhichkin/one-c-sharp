using System.Data.SqlClient;
using Zhichkin.Metadata.Model;

namespace Zhichkin.ChangeTracking
{
    public interface IChangeTrackingService
    {
        ClusteredIndexInfo GetClusteredIndexInfo(Table table);
        ClusteredIndexInfo GetClusteredIndexInfo(Table table, SqlCommand command);
        SnapshotIsolationState GetSnapshotIsolationState(InfoBase infoBase);
        void SwitchSnapshotIsolationState(InfoBase infoBase, bool on);
        ChangeTrackingDatabaseInfo GetChangeTrackingDatabaseInfo(InfoBase infoBase);
        ChangeTrackingTableInfo GetChangeTrackingTableInfo(Table table);
        void EnableDatabaseChangeTracking(InfoBase infoBase, ChangeTrackingDatabaseInfo info);
        void DisableDatabaseChangeTracking(InfoBase infoBase);
        void SwitchTableChangeTracking(Table table, bool on);
        void SwitchTableColumnsTracking(Table table, bool on);
    }
}
