using System;
using System.IO;
using System.Text;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public interface IBinaryFormatter
    {
        void Serialize(Stream stream, object value);
        object Deserialize(Stream stream, Entity type);
    }

    public sealed class BinaryFormatter : IBinaryFormatter
    {
        public void Serialize(Stream stream, object value)
        {
            byte[] buffer = new byte[16];
            if (value == null)
            {
                stream.Write(buffer, 0, buffer.Length);
                return;
            }

            Type t = value.GetType();
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                if (t == typeof(bool)) { writer.Write((bool)value); }
                else if (t == typeof(char)) { writer.Write((char)value); }
                else if (t == typeof(sbyte)) { writer.Write((sbyte)value); }
                else if (t == typeof(byte)) { writer.Write((byte)value); }
                else if (t == typeof(short)) { writer.Write((short)value); }
                else if (t == typeof(ushort)) { writer.Write((ushort)value); }
                else if (t == typeof(int)) { writer.Write((int)value); }
                else if (t == typeof(uint)) { writer.Write((uint)value); }
                else if (t == typeof(long)) { writer.Write((long)value); }
                else if (t == typeof(ulong)) { writer.Write((ulong)value); }
                else if (t == typeof(float)) { writer.Write((float)value); }
                else if (t == typeof(double)) { writer.Write((double)value); }
                else if (t == typeof(decimal)) { writer.Write((decimal)value); }
                else if (t == typeof(DateTime)) { Serialize((DateTime)value, writer); }
                else if (t == typeof(Guid)) { Serialize((Guid)value, writer); }
                else if (t == typeof(string)) { writer.Write((string)value); }
                else if (value is ReferenceObject) { Serialize((ReferenceObject)value, writer); }
            }
        }
        private void Serialize(DateTime source, BinaryWriter target)
        {
            target.Write(source.ToBinary());
        }
        private void Serialize(Guid source, BinaryWriter target)
        {
            target.Write(source.ToByteArray());
        }
        private void Serialize(ReferenceObject source, BinaryWriter target)
        {
            target.Write(source.Identity.ToByteArray());
        }

        public object Deserialize(Stream stream, Entity type)
        {
            if (type == Entity.Empty) return null;

            object value = null;
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (type == Entity.Boolean) { value = reader.ReadBoolean(); }
                else if (type == Entity.Char) { value = reader.ReadChar(); }
                else if (type == Entity.SByte) { value = reader.ReadSByte(); }
                else if (type == Entity.Byte) { value = reader.ReadByte(); }
                else if (type == Entity.Int16) { value = reader.ReadInt16(); }
                else if (type == Entity.UInt16) { value = reader.ReadUInt16(); }
                else if (type == Entity.Int32) { value = reader.ReadInt32(); }
                else if (type == Entity.UInt32) { value = reader.ReadUInt32(); }
                else if (type == Entity.Int64) { value = reader.ReadInt64(); }
                else if (type == Entity.UInt64) { value = reader.ReadUInt64(); }
                else if (type == Entity.Single) { value = reader.ReadSingle(); }
                else if (type == Entity.Double) { value = reader.ReadDouble(); }
                else if (type == Entity.Decimal) { value = reader.ReadDecimal(); }
                else if (type == Entity.DateTime) { value = ReadDateTime(reader); }
                else if (type == Entity.GUID) { value = ReadGuid(reader); }
                else if (type == Entity.String) { value = reader.ReadString(); }
                else { value = ReadReferenceObject(reader, type); }
            }
            return value;
        }
        private DateTime ReadDateTime(BinaryReader source)
        {
            return DateTime.FromBinary(source.ReadInt64());
        }
        private Guid ReadGuid(BinaryReader source)
        {
            return new Guid(source.ReadBytes(16));
        }
        private ReferenceObject ReadReferenceObject(BinaryReader source, Entity type)
        {
            Guid identity = new Guid(source.ReadBytes(16));
            if (identity == Guid.Empty) return null;
            return new ReferenceProxy(type, identity);
        }
    }
}
