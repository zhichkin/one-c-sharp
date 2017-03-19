using System;
using Zhichkin.ORM;
using Zhichkin.Metadata.Services;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public abstract class EntityBase : ReferenceObject, IComparable
    {
        private static int typeCode = 0;

        public EntityBase(IDataMapper mapper) : base(mapper) { }
        public EntityBase(IDataMapper mapper, Guid identity) : base(mapper, identity) { }
        public EntityBase(IDataMapper mapper, Guid identity, PersistentState state) : base(mapper, identity, state) { }

        protected string name = string.Empty;

        public string Name { set { Set<string>(value, ref name); } get { return Get<string>(ref name); } }

        public int TypeCode
        {
            get
            {
                if (typeCode > 0) return typeCode;
                typeCode = MetadataPersistentContext.Current.TypeCodes[this.GetType()];
                return typeCode;
            }
        }

        public override string ToString() { return this.Name; }

        public virtual int CompareTo(object other)
        {
            return this.CompareTo((EntityBase)other);
        }
        public virtual int CompareTo(EntityBase other)
        {
            if (other == null) return 1;
            if (this.GetType() != other.GetType()) throw new InvalidOperationException();
            return this.Name.CompareTo(other.Name);
        }

        public IDictionary<string, CustomSetting> CustomSettings
        {
            get
            {
                return CustomSetting.Select(this);
            }
        }
    }
}