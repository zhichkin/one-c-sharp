using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Translator
{
    public class OneToManyTranslationRule : OneToOneTranslationRule
    {
        public string LocatorField = string.Empty;
        public string TypeCodeField = string.Empty;
        public int TypeCode = 0;
        public override void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            // код типа не определился (нет сопоставления)
            // записываем пустое значение составного типа
            if (TypeCodeField != string.Empty && TypeCode == 0)
            {
                SetEmptyCompoundValue(sourceField, sourceValue, targetFields, targetValues);
                return;
            }
            base.Apply(sourceField, sourceValue, targetFields, targetValues);
            if (LocatorField != string.Empty)
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = LocatorField, // _TYPE
                    Type = "binary", // binary(1)
                    IsKey = IsSyncKey
                });
                targetValues.Add(new byte[1] { 0x08 }); // reference type
            }
            if (TypeCodeField != string.Empty)
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = TypeCodeField,
                    Type = "binary", // binary(4)
                    IsKey = IsSyncKey
                });
                targetValues.Add(Utilities.GetByteArray(TypeCode));
            }
        }
        private void SetEmptyCompoundValue(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            base.Apply(sourceField, new byte[16], targetFields, targetValues); // _RRef

            targetFields.Add(new ChangeTrackingField()
            {
                Name = TypeCodeField, // _TRef
                Type = "binary", // binary(4)
                IsKey = IsSyncKey
            });
            targetValues.Add(new byte[4]);

            if (LocatorField != string.Empty)
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = LocatorField, // _TYPE
                    Type = "binary", // binary(1)
                    IsKey = IsSyncKey
                });
                targetValues.Add(new byte[1] { 0x01 }); // type is not choosed
            }
        }
        public override void Reset() { }
    }
}
