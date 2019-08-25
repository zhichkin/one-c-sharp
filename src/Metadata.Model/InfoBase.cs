using System;
using System.Data.SqlClient;
using Zhichkin.ORM;
using Zhichkin.Metadata.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Zhichkin.Metadata.Model
{
    public sealed partial class InfoBase : EntityBase
    {
        private static readonly IDataMapper _mapper = MetadataPersistentContext.Current.GetDataMapper(typeof(InfoBase));
        private static readonly IMetadataService service = new MetadataService();
        
        public InfoBase() : base(_mapper) { }
        public InfoBase(Guid identity) : base(_mapper, identity) { }
        public InfoBase(Guid identity, PersistentState state) : base(_mapper, identity, state) { }

        private string server = string.Empty;
        private string database = string.Empty;
        private string username = string.Empty;
        private string password = string.Empty;

        public string Server { set { Set<string>(value, ref server); } get { return Get<string>(ref server); } }
        public string Database { set { Set<string>(value, ref database); } get { return Get<string>(ref database); } }
        public string UserName { set { Set<string>(value, ref username); } get { return Get<string>(ref username); } }
        public string Password { set { Set<string>(value, ref password); } get { return Get<string>(ref password); } }

        private List<Namespace> namespaces = new List<Namespace>();
        public IList<Namespace> Namespaces
        {
            get
            {
                if (this.state == PersistentState.New) return namespaces;
                if (namespaces.Count > 0) return namespaces;
                IList<Namespace> children = service.GetChildren<InfoBase, Namespace>(this, "owner");
                foreach (Namespace child in children)
                {
                    child.Owner = this;
                }
                return children;
            }
        }

        public string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder helper = new SqlConnectionStringBuilder()
                {
                    DataSource = this.Server,
                    InitialCatalog = this.Database,
                    IntegratedSecurity = string.IsNullOrWhiteSpace(this.UserName)
                };
                if (!helper.IntegratedSecurity)
                {
                    helper.UserID = this.UserName;
                    helper.Password = this.Password;
                    helper.PersistSecurityInfo = false;
                }
                return helper.ToString();
            }
        }
        public Entity GetEntity(int typeCode) { return DataMapper.GetEntity(this, typeCode); }

        private ObservableCollection<Namespace> _ObservableNamespaces;
        public ObservableCollection<Namespace> ObservableNamespaces
        {
            set { _ObservableNamespaces = value; }
            get
            {
                if (_ObservableNamespaces == null)
                {
                    _ObservableNamespaces = new ObservableCollection<Namespace>(this.Namespaces);
                }
                return _ObservableNamespaces;
            }
        }
    }
}
