using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Model
{
    public sealed partial class TranslationRule : ValueObject
    {
        private static readonly IDataMapper _mapper = IntegratorPersistentContext.Current.GetDataMapper(typeof(TranslationRule));

        public TranslationRule() : base(_mapper) { }
        public TranslationRule(PersistentState state) : base(_mapper, state) { }

        private Entity source_old = null;
        private Entity target_old = null;
        private Property source_property_old = null;

        private Entity source = null;
        private Entity target = null;
        private Property source_property = null;
        private Property target_property = null;
        private bool is_sync_key = false;

        protected override void UpdateKeyValues()
        {
            source_old = source;
            target_old = target;
            source_property_old = source_property;
        }

        public Entity Source { set { Set<Entity>(value, ref source); } get { return Get<Entity>(ref source); } }
        public Entity Target { set { Set<Entity>(value, ref target); } get { return Get<Entity>(ref target); } }
        public Property SourceProperty { set { Set<Property>(value, ref source_property); } get { return Get<Property>(ref source_property); } }
        public Property TargetProperty { set { Set<Property>(value, ref target_property); } get { return Get<Property>(ref target_property); } }
        public bool IsSyncKey { set { Set<bool>(value, ref is_sync_key); } get { return Get<bool>(ref is_sync_key); } }

        public override string ToString()
        {
            return string.Format("{0}({1}) -> {2}({3})",
                this.Source == null ? string.Empty : this.Source.Name,
                this.SourceProperty == null ? string.Empty : this.SourceProperty.Name,
                this.Target == null ? string.Empty : this.Target.Name,
                this.TargetProperty == null ? string.Empty : this.TargetProperty.Name);
        }
        public int CompareTo(TranslationRule other)
        {
            if (other == null) return 1;
            return this.ToString().CompareTo(other.ToString());
        }
    }
}
