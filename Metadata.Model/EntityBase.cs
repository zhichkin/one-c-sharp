using System;
using System.Transactions;
using Zhichkin.ORM;

namespace Zhichkin.Metadata.Model
{
    public abstract class EntityBase : ReferenceObject, IComparable<EntityBase>
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

        public int CompareTo(EntityBase other)
        {
            if (other == null) return 1;
            if (this.GetType() != other.GetType()) throw new InvalidOperationException();
            return this.Name.CompareTo(other.Name);
        }

        public override void Save()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                base.Save();
                scope.Complete();
            }
        }

        public override void Kill()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                base.Kill();
                scope.Complete();
            }
        }
    }
}