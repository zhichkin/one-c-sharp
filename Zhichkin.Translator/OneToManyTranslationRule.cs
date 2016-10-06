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
            base.Apply(sourceField, sourceValue, targetFields, targetValues);
            if (LocatorField != string.Empty)
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = LocatorField, // _TYPE
                    Type = "binary", // binary(1)
                    IsKey = sourceField.IsKey
                });
                targetValues.Add(new byte[1] { 0x08 }); // reference type
            }
            if (TypeCodeField != string.Empty)
            {
                targetFields.Add(new ChangeTrackingField()
                {
                    Name = TypeCodeField,
                    Type = "binary", // binary(4)
                    IsKey = sourceField.IsKey
                });
                targetValues.Add(Utilities.GetByteArray(TypeCode));
            }
        }
    }
}
