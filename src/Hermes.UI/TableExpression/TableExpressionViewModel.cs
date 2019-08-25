using System.Collections.Generic;
using Zhichkin.Hermes.Model;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Hermes.UI
{
    public class TableExpressionViewModel : HermesViewModel
    {
        private List<PropertyReferenceViewModel> _Properties;
        public TableExpressionViewModel(HermesViewModel parent, TableExpression model) : base(parent, model)
        {

        }
        public string Name
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((TableExpression)this.Model).Name;
            }
        }
        public string FullName
        {
            get
            {
                if (this.Model == null) { return string.Empty; }
                TableExpression model = (TableExpression)this.Model;
                if (model.Entity == null) { return string.Empty; }
                return $"{model.Entity.InfoBase.Database}.{model.Entity.FullName}";
            }
        }
        public string Alias
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((TableExpression)this.Model).Alias;
            }
            set
            {
                if (this.Model == null) return; // TODO: ?
                ((TableExpression)this.Model).Alias = value;
                this.OnPropertyChanged("Alias");
            }
        }
        public List<PropertyReferenceViewModel> Properties
        {
            get
            {
                if (this.Model == null) return null;
                TableExpression table = (TableExpression)this.Model;
                if (_Properties == null)
                {
                    _Properties = new List<PropertyReferenceViewModel>();
                    if (table.Entity != null)
                    {
                        foreach (Property property in table.Entity.Properties)
                        {
                            _Properties.Add(new PropertyReferenceViewModel(this, this, new PropertyReference(table, table, property)));
                        }
                    }
                }
                return _Properties;
            }
        }

        public string SelectedHintItem
        {
            get
            {
                if (this.Model == null) return string.Empty;
                return ((TableExpression)this.Model).Hint;
            }
            set
            {
                if (this.Model == null) return;
                ((TableExpression)this.Model).Hint = value;
                this.OnPropertyChanged("SelectedHintItem");
            }
        }
        public List<string> HintTypeItemsSource { get { return HintTypes.HintTypesList; } }
    }
}
