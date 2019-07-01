using System;
using System.Data;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Services
{
    public sealed class PropertyReferenceTranslator
    {
        private readonly PropertyReference property;
        public PropertyReferenceTranslator(PropertyReference property)
        {
            this.property = property;
        }
        public string ToSQL()
        {
            return $"[{property.Table.Alias}].[{property.Name}]";
            //string valueType = property.Property.Relations.ToString();
        }
        //public object Translate(IDataReader source)
        //{
        //    if (_code == 0)
        //    {
        //        return TranslateComplexType(source);
        //    }
        //    else
        //    {
        //        return TranslateSingleType(source);
        //    }
        //}
        //private object TranslateSingleType(IDataReader source)
        //{
        //    Entity type = Entity.Empty;
        //    if (_code < 0)
        //    {
        //        type = InfoBase.Metadata.GetEntity(_code);
        //    }
        //    else // > 0 ReferenceObject
        //    {
        //        type = _metadata.GetEntity(_code);
        //    }
        //    int index = _ordinals[(int)FieldPurpose.Value];
        //    if (index == -1) // Ссылка ???
        //    {
        //        index = _ordinals[(int)FieldPurpose.Object];
        //    }
        //    object value = source[index];
        //    return ConvertValue(value, type);
        //}
        //private object TranslateComplexType(IDataReader source)
        //{
        //    byte[] buffer;
        //    int locator = _ordinals[(int)FieldPurpose.Locator];
        //    int typeCode = _ordinals[(int)FieldPurpose.TypeCode];
        //    if (typeCode != -1)
        //    {
        //        buffer = (byte[])source[typeCode];
        //        buffer = buffer.Reverse().ToArray();
        //        typeCode = (int)_formatter.Deserialize(new MemoryStream(buffer), Entity.Int32);
        //    }
        //    int value = 0;
        //    Entity type = null;
        //    if (locator == -1) // ReferenceObject
        //    {
        //        type = _metadata.GetEntity(typeCode);
        //        value = _ordinals[(int)FieldPurpose.Object];
        //    }
        //    else
        //    {
        //        buffer = (byte[])source[locator];
        //        locator = (byte)_formatter.Deserialize(new MemoryStream(buffer), Entity.Byte);
        //        if (locator == 1) // Неопределено
        //        {
        //            return null;
        //        }
        //        else if (locator == 2) // Булево
        //        {
        //            type = Entity.Boolean;
        //            value = _ordinals[(int)FieldPurpose.Boolean];
        //        }
        //        else if (locator == 3) // Число
        //        {
        //            type = Entity.Decimal;
        //            value = _ordinals[(int)FieldPurpose.Number];
        //        }
        //        else if (locator == 4) // Дата
        //        {
        //            type = Entity.DateTime;
        //            value = _ordinals[(int)FieldPurpose.DateTime];
        //        }
        //        else if (locator == 5) // Строка
        //        {
        //            type = Entity.String;
        //            value = _ordinals[(int)FieldPurpose.String];
        //        }
        //        else if (locator == 8) // Ссылка
        //        {
        //            type = _metadata.GetEntity(typeCode);
        //            value = _ordinals[(int)FieldPurpose.Object];
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    return ConvertValue(source[value], type);
        //}
        //private object ConvertValue(object value, Entity type)
        //{
        //    if (value == DBNull.Value) return null;
        //    if (value is byte[] && type != Entity.Binary)
        //    {
        //        return null; //_formatter.Deserialize(new MemoryStream((byte[])value), type);
        //    }
        //    else
        //    {
        //        return value;
        //    }
        //}
    }
}
