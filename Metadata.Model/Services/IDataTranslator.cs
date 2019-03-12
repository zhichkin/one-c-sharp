using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Zhichkin.Hermes.Infrastructure;

namespace Zhichkin.Metadata.Model
{
    public interface IDataTranslator
    {
        Entity Metadata { get; }
        void Translate(IDataReader source, IDictionary<string, object> target);
    }

    public interface IValueTranslator
    {
        int TargetTypeCode { get; }
        int[] Ordinals { get; } // index corresponds to FieldPurpose enum value
        object Translate(IDataReader source);
    }

    public sealed class ValueTranslator : IValueTranslator
    {
        private readonly InfoBase _metadata;
        private readonly int _code = 0; // when field purpose is just Value ... we need the type code of it
        private readonly int[] _ordinals = new int[9] { -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public ValueTranslator(InfoBase metadata) { _metadata = metadata; } // for complex types - type code is stored in the database
        public ValueTranslator(InfoBase metadata, int targetTypeCode) : this(metadata)
        {
            _code = targetTypeCode;
        }
        public int TargetTypeCode { get { return _code; } }
        public int[] Ordinals{ get { return _ordinals; } }
        public object Translate(IDataReader source)
        {
            if (_code == 0)
            {
                return TranslateComplexType(source);
            }
            else
            {
                return TranslateSingleType(source);
            }
        }
        private object TranslateSingleType(IDataReader source)
        {
            Entity type = Entity.Empty;
            if (_code < 0)
            {
                type = InfoBase.Metadata.GetEntity(_code);
            }
            else // > 0 ReferenceObject
            {
                type = _metadata.GetEntity(_code);
            }
            int index = _ordinals[(int)FieldPurpose.Value];
            if (index == -1) // Ссылка ???
            {
                index = _ordinals[(int)FieldPurpose.Object];
            }
            object value = source[index];
            return ConvertValue(value, type);
        }
        private object TranslateComplexType(IDataReader source)
        {
            byte[] buffer;
            int locator = _ordinals[(int)FieldPurpose.Locator];
            int typeCode = _ordinals[(int)FieldPurpose.TypeCode];
            if (typeCode != -1)
            {
                buffer = (byte[])source[typeCode];
                buffer = buffer.Reverse().ToArray();
                typeCode = (int)_formatter.Deserialize(new MemoryStream(buffer), Entity.Int32);
            }
            int value = 0;
            Entity type = null;
            if (locator == -1) // ReferenceObject
            {
                type = _metadata.GetEntity(typeCode);
                value = _ordinals[(int)FieldPurpose.Object];
            }
            else
            {
                buffer = (byte[])source[locator];
                locator = (byte)_formatter.Deserialize(new MemoryStream(buffer), Entity.Byte);
                if (locator == 1) // Неопределено
                {
                    return null;
                }
                else if (locator == 2) // Булево
                {
                    type = Entity.Boolean;
                    value = _ordinals[(int)FieldPurpose.Boolean];
                }
                else if (locator == 3) // Число
                {
                    type = Entity.Decimal;
                    value = _ordinals[(int)FieldPurpose.Number];
                }
                else if (locator == 4) // Дата
                {
                    type = Entity.DateTime;
                    value = _ordinals[(int)FieldPurpose.DateTime];
                }
                else if (locator == 5) // Строка
                {
                    type = Entity.String;
                    value = _ordinals[(int)FieldPurpose.String];
                }
                else if (locator == 8) // Ссылка
                {
                    type = _metadata.GetEntity(typeCode);
                    value = _ordinals[(int)FieldPurpose.Object];
                }
                else
                {
                    return null;
                }
            }
            return ConvertValue(source[value], type);
        }
        private object ConvertValue(object value, Entity type)
        {
            if (value == DBNull.Value) return null;
            if (value is byte[] && type != Entity.Binary)
            {
                return _formatter.Deserialize(new MemoryStream((byte[])value), type);
            }
            else
            {
                return value;
            }
        }
    }

    public sealed class DataTranslator : IDataTranslator
    {
        private readonly Entity _metadata;
        private readonly Dictionary<string, IValueTranslator> _translators;
        
        public DataTranslator(Entity metadata)
        {
            _metadata = metadata;
            _translators = new Dictionary<string, IValueTranslator>();
            InitializeTranslator();
        }
        public Entity Metadata { get { return _metadata; } }
        public void Translate(IDataReader source, IDictionary<string, object> target)
        {
            foreach (KeyValuePair<string, IValueTranslator> translator in _translators)
            {
                if (target.ContainsKey(translator.Key))
                {
                    target[translator.Key] = translator.Value.Translate(source);
                }
                else
                {
                    target.Add(translator.Key, translator.Value.Translate(source));
                }
            }
        }
        private void InitializeTranslator()
        {
            IList<Property> properties = _metadata.Properties;
            int ordinal = 0;
            for (int i = 0; i < properties.Count; i++)
            {
                IValueTranslator translator = BuildValueTranslator(properties[i], ref ordinal);
                if (translator == null) continue;
                _translators.Add(properties[i].Name, translator);
            }
        }
        private IValueTranslator BuildValueTranslator(Property property, ref int ordinal)
        {
            IList<Field> fields = property.Fields;
            IValueTranslator translator;
            if (fields.Count == 0) return null;
            if (fields.Count > 1)
            {
                // complex type = -1
                translator = new ValueTranslator(_metadata.InfoBase);
            }
            else
            {
                translator = new ValueTranslator(
                    _metadata.InfoBase,
                    property.Relations.FirstOrDefault().Entity.Code);
            }
            for (int i = 0; i < fields.Count; i++)
            {
                Field field = fields[i];
                translator.Ordinals[(int)field.Purpose] = ordinal;
                ordinal++;
            }
            return translator;
        }
    }
}
