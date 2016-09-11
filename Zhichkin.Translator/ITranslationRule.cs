using Zhichkin.ChangeTracking;
using System.Collections.Generic;

namespace Zhichkin.Translator
{
    public interface ITranslationRule
    {
        bool IsKey { set; get; }
        void Apply(ChangeTrackingField sourceField, IList<ChangeTrackingField> targetFields);
    }
}
