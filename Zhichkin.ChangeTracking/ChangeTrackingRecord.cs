using System;

namespace Zhichkin.ChangeTracking
{
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
