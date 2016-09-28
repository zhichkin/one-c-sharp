using System;

namespace Zhichkin.ChangeTracking
{
    [Serializable]
    public sealed class ChangeTrackingMessage
    {
        public string SYS_CHANGE_OPERATION = string.Empty; // I, U, D
        public ChangeTrackingField[] Fields = null;
        public ChangeTrackingRecord[] Records = null;
    }
    [Serializable]
    public sealed class ChangeTrackingRecord
    {
        public object[] Values = null; // Fields
    }
    [Serializable]
    public sealed class ChangeTrackingField
    {
        public bool IsKey = false; // U, D
        public string Name = string.Empty;
        public string Type = string.Empty;
    }
}
