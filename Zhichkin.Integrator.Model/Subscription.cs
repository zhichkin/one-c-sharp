using System;
using Zhichkin.ORM;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Integrator.Model
{
    public sealed partial class Subscription : EntityBase
    {
        private static readonly IDataMapper _mapper = IntegratorPersistentContext.Current.GetDataMapper(typeof(Subscription));
        public static IList<Subscription> Select() { return DataMapper.Select(); }

        public Subscription() : base(_mapper) { }
        public Subscription(Guid identity) : base(_mapper, identity) { }
        public Subscription(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private Publisher publisher = null;
        private Entity subscriber = null;
        private List<TranslationRule> rules = new List<TranslationRule>();

        ///<summary>Publisher (data source)</summary>
        public Publisher Publisher { set { Set<Publisher>(value, ref publisher); } get { return Get<Publisher>(ref publisher); } }
        ///<summary>Subscriber (data target)</summary>
        public Entity Subscriber { set { Set<Entity>(value, ref subscriber); } get { return Get<Entity>(ref subscriber); } }
        ///<summary>Translation rules</summary>
        public IList<TranslationRule> TranslationRules
        {
            get
            {
                if (this.state == PersistentState.New) return rules;
                if (rules.Count > 0) return rules;
                return DataMapper.GetTranslationRules(this);
            }
        }
        public override string ToString()
        {
            return string.Format("{0} -> {1}",
                this.Publisher == null ? string.Empty : this.Publisher.Name,
                this.Subscriber == null ? string.Empty : this.Subscriber.Name);
        }
    }
}
