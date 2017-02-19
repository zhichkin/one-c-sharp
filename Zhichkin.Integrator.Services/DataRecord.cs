using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhichkin.Integrator.Services
{
    public interface IDataMessage
    {
        string Key { set; get; } // identity of the Subscription class
        IList<IDataEntity> Entities { get; }
    }
    public interface IDataEntity
    {
        //string Table { set; get; }
        //IList<string> Keys { get; }
        string Key { set; get; } // identity of the Entity class
        IList<IDataRecord> Records { get; }
    }
    public enum DataRecordType { Select, Insert, Update, Delete }

    public sealed class DataMessage : IDataMessage // IDataReader | IDataAdapter ???
    {
        private List<IDataEntity> _entities = new List<IDataEntity>();
        public DataMessage() { }
        public string Key { set; get; }
        public IList<IDataEntity> Entities { get { return _entities; } }
    }
    public sealed class DataEntity : IDataEntity
    {
        //private List<string> _keys = new List<string>();
        private List<IDataRecord> _records = new List<IDataRecord>();
        public DataEntity() { }
        //public string Table { set; get; }
        //public IList<string> Keys { get { return _keys; } }
        public string Key { set; get; }
        public IList<IDataRecord> Records { get { return _records; } }
    }
    public sealed class DataRecord : IDataRecord
    {
        private DataRecordType _recordType = DataRecordType.Select;
        private Dictionary<string, int> _names = new Dictionary<string, int>();
        private List<object> _values = new List<object>();

        public DataRecord() { }
        public DataRecord(DataRecordType type) { _recordType = type; }

        public DataRecordType RecordType { set { _recordType = value; } get { return _recordType; } }
        public void AddColumnValue(string name, object value)
        {
            _names.Add(name, _names.Count);
            _values.Add(value);
        }
        public void SetColumnValue(string name, object value)
        {
            _values[_names[name]] = value;
        }
        public void RemoveColumn(string name)
        {
            _values.RemoveAt(_names[name]);
            _names.Remove(name);
        }
        public void RemoveColumn(int i)
        {
            _names.Remove(GetName(i));
            _values.RemoveAt(i);
        }
        public object GetValue(int i) { return this[i]; }
        public object this[string name] { get { return _values[_names[name]]; } }
        public object this[int i] { get { return _values[i]; } }
        public int FieldCount { get { return _names.Count; } }
        public string GetName(int i)
        {
            foreach (KeyValuePair<string, int> item in _names)
            {
                if (item.Value == i) return item.Key;
            }
            throw new IndexOutOfRangeException();
        }
        public int GetOrdinal(string name)
        {
            return _names[name];
        }
        public bool IsDBNull(int i)
        {
            return _values[i] == DBNull.Value;
        }

        # region " Values getters "
        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }
        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }
        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }
        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }
        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }
        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }
        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }
        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }
        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }
        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }
        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }
        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }
        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }
        public string GetString(int i)
        {
            throw new NotImplementedException();
        }
        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
