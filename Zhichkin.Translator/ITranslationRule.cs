using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Translator
{
    public interface ITranslationRule
    {
        bool IsSyncKey { set; get; }
        void Reset();
        void Apply(ChangeTrackingField sourceField, object sourceValue, IList<ChangeTrackingField> targetFields, IList<object> targetValues);
    }
}
