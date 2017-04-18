using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Module
{
    public class EntityViewModel : BindableBase
    {
        private readonly Entity _model;
        
        public EntityViewModel(Entity model)
        {
            if (model == null) throw new ArgumentNullException("model");
            _model = model;
            InitializeViewModel();
        }
        public void InitializeViewModel()
        {
            
        }
        public string InfoBase
        {
            get { return _model.InfoBase.Name; }
        }
        public string Namespace
        {
            get { return _model.Namespace.Name; }
        }
        public string Owner
        {
            get { return _model.Owner?.FullName; }
        }
        public string Name
        {
            get { return _model.Name; }
        }
        public string Alias
        {
            get { return _model.Alias; }
        }
        public string Code
        {
            get { return _model.Code.ToString(); }
        }
        public string MainTable
        {
            get { return _model.MainTable.Name; }
        }
        public IList<Table> Tables
        {
            get { return _model.Tables; }
        }
    }
}
