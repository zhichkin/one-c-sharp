using System;
using System.Collections.Generic;

namespace Zhichkin.ChangeTracking
{
    public enum SnapshotIsolationState : byte { OFF = 0, ON = 1, ON_OFF = 2, OFF_ON = 3 }

    public sealed class ChangeTrackingDatabaseInfo
    {
        public ChangeTrackingDatabaseInfo() { }
        public bool IS_AUTO_CLEANUP_ON;
        public int RETENTION_PERIOD;
        public short RETENTION_PERIOD_UNITS; // 1 - minutes, 2 - hours, 3 - days
        public string RETENTION_PERIOD_UNITS_DESC; // Minutes, Hours, Days
        public long MAX_CLEANUP_VERSION;
    }

    public sealed class ChangeTrackingTableInfo
    {
        public ChangeTrackingTableInfo() { }
        public bool IS_TRACK_COLUMNS_UPDATED_ON;
        public long BEGIN_VERSION;
        public long MIN_VALID_VERSION;
        public long CLEANUP_VERSION;
    }

    public sealed class ClusteredIndexInfo
    {
        public ClusteredIndexInfo() { }
        public string NAME;
        public bool IS_UNIQUE;
        public bool IS_PRIMARY_KEY;
        public List<ClusteredIndexColumnInfo> COLUMNS = new List<ClusteredIndexColumnInfo>();
        public bool HasNullableColumns
        {
            get
            {
                bool result = false;
                foreach (ClusteredIndexColumnInfo item in COLUMNS)
                {
                    if (item.IS_NULLABLE)
                    {
                        return true;
                    }
                }
                return result;
            }
        }
        public ClusteredIndexColumnInfo GetColumnByName(string name)
        {
            ClusteredIndexColumnInfo info = null;
            for (int i = 0; i < COLUMNS.Count; i++)
            {
                if (COLUMNS[i].NAME == name) return COLUMNS[i];
            }
            return info;
        }
    }

    public sealed class ClusteredIndexColumnInfo
    {
        public ClusteredIndexColumnInfo() { }
        public byte KEY_ORDINAL;
        public string NAME;
        public bool IS_NULLABLE;
    }

    [Serializable]
    public sealed class ChangeTrackingRecord
    {
        public string SYS_CHANGE_OPERATION = string.Empty;
        public ChangeTrackingField[] Fields = null;
    }
    [Serializable]
    public sealed class ChangeTrackingField
    {
        public bool IsKey = false;
        public string Name = string.Empty;
        public object Value = null;
    }
}
