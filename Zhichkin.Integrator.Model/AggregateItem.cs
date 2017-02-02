using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Model;
using System.Collections.Generic;

namespace Zhichkin.Integrator.Model
{
    public sealed partial class AggregateItem : ValueObject
    {
        private static readonly IDataMapper _mapper = IntegratorPersistentContext.Current.GetDataMapper(typeof(AggregateItem));

        public static IList<AggregateItem> Select(Entity aggregate) { return DataMapper.Select(aggregate); }
        public static AggregateItem SelectOrCreate(Entity aggregate, Entity component)
        {
            if (aggregate == null) throw new ArgumentNullException("aggregate");
            if (component == null) throw new ArgumentNullException("component");
            if (aggregate == component) throw new InvalidOperationException("Aggregate and it's component can not be the same entity.");
            AggregateItem item = DataMapper.Select(aggregate, component);
            if (item != null) return item;
            //TODO: table parts property "Ссылка" has binary type instead of "ДокументСсылка" because it is exported from 1C like that.
            //IList<Property> connectors = DataMapper.GetConnectors(aggregate, component);
            //if (connectors.Count == 0) throw new InvalidOperationException("Aggregate and component have no any possible connector.");

            item = new AggregateItem();
            item.aggregate = aggregate;
            item.component = component;
            item.connector = component.Properties[0];
            item.Save();

            return item;
        }
        public static IList<Property> GetConnectors(Entity aggregate, Entity component)
        {
            return DataMapper.GetConnectors(aggregate, component);
        }

        public AggregateItem() : base(_mapper) { }
        public AggregateItem(PersistentState state) : base(_mapper, state) { }

        private Entity aggregate_old = null;
        private Entity component_old = null;
        private Property connector_old = null;

        private Entity aggregate = null;
        private Entity component = null;
        private Property connector = null;

        protected override void UpdateKeyValues()
        {
            aggregate_old = aggregate;
            component_old = component;
            connector_old = connector;
        }

        public Entity Aggregate { set { Set<Entity>(value, ref aggregate); } get { return Get<Entity>(ref aggregate); } }
        public Entity Component { set { Set<Entity>(value, ref component); } get { return Get<Entity>(ref component); } }
        public Property Connector { set { Set<Property>(value, ref connector); } get { return Get<Property>(ref connector); } }

        public override string ToString()
        {
            return string.Format("{0} -> {1}({2})",
                this.Aggregate == null ? string.Empty : this.Aggregate.Name,
                this.Component == null ? string.Empty : this.Component.Name,
                this.Connector == null ? string.Empty : this.Connector.Name);
        }
        public int CompareTo(AggregateItem other)
        {
            if (other == null) return 1;
            return this.ToString().CompareTo(other.ToString());
        }
    }
}
