using System;
using System.Data.SqlClient;
using Zhichkin.ORM;
using Zhichkin.Metadata.Services;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class CustomSetting : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(CustomSetting));
        public static IDictionary<string, CustomSetting> Select(EntityBase owner) { return DataMapper.Select(owner); }
        public static CustomSetting Create(EntityBase owner, string name, string value)
        {
            if (owner == null) throw new ArgumentNullException("owner");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            CustomSetting setting = (CustomSetting)MetadataPersistentContext.Current.Factory.New(typeof(CustomSetting));
            setting.Owner = owner;
            setting.Name = name;
            setting.Value = value;
            setting.Save();
            return setting;
        }

        public CustomSetting() : base(_mapper) { }
        public CustomSetting(Guid identity) : base(_mapper, identity) { }
        public CustomSetting(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private EntityBase owner = null;
        public EntityBase Owner { set { Set<EntityBase>(value, ref owner); } get { return Get<EntityBase>(ref owner); } }

        private string _value = string.Empty;
        public string Value { set { Set<string>(value, ref _value); } get { return Get<string>(ref _value); } }
    }
}
