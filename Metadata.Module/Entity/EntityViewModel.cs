using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Zhichkin.Metadata.Model;
using Zhichkin.Shell;

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

            this.PropertyPopup = new InteractionRequest<Confirmation>();
            this.CreateNewPropertyCommand = new DelegateCommand(this.CreateNewProperty);
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
        public Table MainTable
        {
            get { return _model.MainTable; }
        }
        public IList<Property> Properties
        {
            get { return _model.Properties; }
        }
        public IList<Table> Tables
        {
            get { return _model.Tables; }
        }

        public ICommand CreateNewPropertyCommand { private set; get; }
        public InteractionRequest<Confirmation> PropertyPopup { private set; get; }
        private void CreateNewProperty()
        {
            Confirmation confirmation = new Confirmation()
            {
                Title = "Z-Metadata",
                Content = new Property()
                {
                    Entity = _model,
                    Purpose = PropertyPurpose.Property,
                    Ordinal = _model.Properties.Count,
                    Name = $"NewProperty{_model.Properties.Count}"
                }
            };
            this.PropertyPopup.Raise(confirmation, response =>
            {
                if (response.Confirmed)
                {
                    Property content = response.Content as Property;
                    if (content != null)
                    {
                        _model.Properties.Add(content);
                    }
                }
            });
        }
    }
}
