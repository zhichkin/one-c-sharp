using System;
using System.Collections.Generic;
using Zhichkin.ChangeTracking;
using Zhichkin.Hermes.Infrastructure;
using Zhichkin.Metadata.Model;
using M = Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Translator
{
    public class ManyToManyTranslationRule : OneToOneTranslationRule
    {
        public Dictionary<FieldPurpose, string> TargetFields = new Dictionary<FieldPurpose, string>();
        private Dictionary<FieldPurpose, ChangeTrackingField> SourceFields = new Dictionary<FieldPurpose, ChangeTrackingField>();
        private Dictionary<FieldPurpose, object> SourceValues = new Dictionary<FieldPurpose, object>();
        public Dictionary<int, int> TypeCodesLookup = new Dictionary<int, int>();
        public override void Reset()
        {
            SourceFields.Clear();
            SourceValues.Clear();
        }
        public override void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            FieldPurpose purpose = M.Utilities.ParseFieldPurpose(sourceField.Name);
            string name = string.Empty;
            if (!TargetFields.TryGetValue(purpose, out name)) return;

            SourceFields[purpose] = sourceField;
            SourceValues[purpose] = sourceValue;
            
            if (purpose == FieldPurpose.Locator) { SetLocatorValue(targetFields, targetValues); }
            else if (purpose == FieldPurpose.Boolean) { SetBooleanValue(targetFields, targetValues); return; }
            else if (purpose == FieldPurpose.Number) { SetNumberValue(targetFields, targetValues); return; }
            else if (purpose == FieldPurpose.DateTime) { SetDateTimeValue(targetFields, targetValues); return; }
            else if (purpose == FieldPurpose.String) { SetStringValue(targetFields, targetValues); return; }

            // если есть поле-локатор, тогда ждём его заполнения
            if (TargetFields.ContainsKey(FieldPurpose.Locator) && !SourceValues.ContainsKey(FieldPurpose.Locator)) // _TYPE
            {
                return;
            }

            // до тех пор пока не соберём все нужные значения, не устанавливаем их,
            // так как для ссылочных типов нужно проверить соответствие кодов типов
            if (!SourceValues.ContainsKey(FieldPurpose.TypeCode) || !SourceValues.ContainsKey(FieldPurpose.Object))
            {
                return;
            }
            int typeCode = Utilities.GetInt32((byte[])SourceValues[FieldPurpose.TypeCode]);
            //Type type = SourceValues[FieldPurpose.TypeCode].GetType();
            //if (type.IsArray && type.GetElementType() == typeof(byte))
            //{
            //    typeCode = Utilities.GetInt32((byte[])SourceValues[FieldPurpose.TypeCode]);
            //}
            bool typeCodeFound = false;
            Utilities.GetInt32((byte[])sourceValue);
            typeCodeFound = TypeCodesLookup.TryGetValue(typeCode, out typeCode);

            if (TargetFields.ContainsKey(FieldPurpose.Locator)) // _TYPE
            {
                if (typeCodeFound)
                {
                    SetCompoundValue(typeCode, targetFields, targetValues);
                }
                else // Составной тип - устанавливаем "объект не выбран"
                {
                    SetEmptyCompoundValue(targetFields, targetValues);
                }
            }
            else
            {
                if (typeCodeFound)
                {
                    SetReferenceValue(typeCode, targetFields, targetValues);
                }
                else // Регистратор - устанавливаем пустую ссылку
                {
                    SetEmptyReference(typeCode, targetFields, targetValues);
                }
            }
        }
        private void SetLocatorValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            if (TargetFields.ContainsKey(FieldPurpose.Locator)) // _TYPE
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = TargetFields[FieldPurpose.Locator],
                    Type = SourceFields[FieldPurpose.Locator].Type,
                    IsKey = this.IsSyncKey
                });
                targetValues.Add(SourceValues[FieldPurpose.Locator]);
            }
        }
        private void SetBooleanValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.Boolean],
                Type = SourceFields[FieldPurpose.Boolean].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(SourceValues[FieldPurpose.Boolean]);
        }
        private void SetStringValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.String],
                Type = SourceFields[FieldPurpose.String].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(SourceValues[FieldPurpose.String]);
        }
        private void SetNumberValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.Number],
                Type = SourceFields[FieldPurpose.Number].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(SourceValues[FieldPurpose.Number]);
        }
        private void SetDateTimeValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.DateTime],
                Type = SourceFields[FieldPurpose.DateTime].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(SourceValues[FieldPurpose.DateTime]);
        }
        private void SetCompoundValue(int typeCode, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            SetReferenceValue(typeCode, targetFields, targetValues);
        }
        private void SetEmptyCompoundValue(IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.TypeCode],
                Type = SourceFields[FieldPurpose.TypeCode].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(new byte[4]); // _TRef

            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.Object],
                Type = SourceFields[FieldPurpose.Object].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(new byte[16]); // _RRef
        }
        private void SetReferenceValue(int typeCode, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.TypeCode],
                Type = SourceFields[FieldPurpose.TypeCode].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(Utilities.GetByteArray(typeCode));

            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.Object],
                Type = SourceFields[FieldPurpose.Object].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(SourceValues[FieldPurpose.Object]);
        }
        private void SetEmptyReference(int typeCode, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.TypeCode],
                Type = SourceFields[FieldPurpose.TypeCode].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(Utilities.GetByteArray(typeCode)); // TODO !!!
            // 1С падает с критической ошибкой: "Ссылочная константа содержит недопустимый ссылочный номер таблицы"
            // Открыть список записей набора регистра и нажать кнопку "Обновить" командной панели формы списка

            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetFields[FieldPurpose.Object],
                Type = SourceFields[FieldPurpose.Object].Type,
                IsKey = this.IsSyncKey
            });
            targetValues.Add(new byte[16]); // чтобы хоть как-то найти косячную запись
        }
    }
}
