using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Translator
{
    public class OneToManyTranslationRule : SimpleTranslationRule
    {
        public string LocatorField = string.Empty;
        public string TypeCodeField = string.Empty;
        public int TypeCode = 0;
        public override void Apply(ChangeTrackingField field, IList<ChangeTrackingField> target)
        {
            base.Apply(field, target);
            if (LocatorField != string.Empty)
            {
                target.Add(new ChangeTrackingField()
                {
                    Name = LocatorField,
                    Value = 0x08,
                    IsKey = field.IsKey
                });
            }
            if (TypeCodeField != string.Empty)
            {
                target.Add(new ChangeTrackingField()
                {
                    Name = TypeCodeField,
                    Value = TypeCode,
                    IsKey = field.IsKey
                });
            }
        }
    }
}
