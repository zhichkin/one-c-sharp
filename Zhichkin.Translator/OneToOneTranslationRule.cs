using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Translator
{
    public class OneToOneTranslationRule : ITranslationRule
    {
        public OneToOneTranslationRule() { }
        public string Name = string.Empty;
        private bool _IsKey = false;
        public bool IsKey { set { _IsKey = value; } get { return _IsKey; } }
        public bool UseDefaultValue;
        public object DefaultValue;
        public virtual void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues)
        {
            targetFields.Add(new ChangeTrackingField()
            {
                Name = Name,
                Type = sourceField.Type,
                IsKey = sourceField.IsKey
            });
            if (UseDefaultValue)
            {
                targetValues.Add(DefaultValue);
            }
            else
            {
                targetValues.Add(sourceValue);
            }
        }
    }
}
