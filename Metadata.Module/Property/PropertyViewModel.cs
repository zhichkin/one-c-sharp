using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.Module
{
    public class PropertyViewModel : BindableBase
    {
        private readonly Property _model;
        
        public PropertyViewModel(Property model)
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
            get { return _model.Entity.InfoBase.Name; }
        }
        public string Namespace
        {
            get { return _model.Entity.Namespace.Name; }
        }
        public string Owner
        {
            get { return _model.Entity.FullName; }
        }
        public string Ordinal
        {
            get { return _model.Ordinal.ToString(); }
        }
        public string Name
        {
            get { return _model.Name; }
        }
        public string Purpose
        {
            get { return _model.Purpose.ToString(); }
        }
        public IList<Relation> Relations
        {
            get { return _model.Relations; }
        }
    }
}
