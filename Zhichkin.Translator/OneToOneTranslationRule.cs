using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Translator
{
    public class OneToOneTranslationRule : ITranslationRule
    {
        public OneToOneTranslationRule() { }
        public string TargetName { set; get; }
        public bool IsSyncKey { set; get; }
        public virtual void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = TargetName,
                IsKey = IsSyncKey,
                Type = sourceField.Type // TODO: type conversion to target field type
            });
            targetValues.Add(sourceValue);
        }
        public virtual void Reset() { }
    }
}
