using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Translator
{
    public class SimpleTranslationRule : ITranslationRule
    {
        public SimpleTranslationRule() { }
        public string Name = string.Empty;
        private bool _IsKey = false;
        public bool IsKey { set { _IsKey = value; } get { return _IsKey; } }
        public bool UseDefaultValue;
        public object DefaultValue;
        public virtual void Apply(ChangeTrackingField field, IList<ChangeTrackingField> target)
        {
            if (UseDefaultValue)
            {
                target.Add(new ChangeTrackingField()
                {
                    Name = Name,
                    Value = DefaultValue,
                    IsKey = field.IsKey
                });
            }
            else
            {
                target.Add(new ChangeTrackingField()
                {
                    Name = Name,
                    Value = field.Value,
                    IsKey = field.IsKey
                });
            }
        }
    }
}
