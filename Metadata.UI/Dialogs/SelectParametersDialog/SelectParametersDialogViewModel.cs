using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Zhichkin.Metadata.Model;

namespace Zhichkin.Metadata.UI
{
    public class SelectParametersDialogViewModel : BindableBase, IInteractionRequestAware
    {
        private Entity _metadata; // input
        private Confirmation _notification;
        private List<FilterParameterViewModel> _parameters = new List<FilterParameterViewModel>(); // output
        
        public SelectParametersDialogViewModel()
        {
            this.SelectCommand = new DelegateCommand(this.Confirm);
            this.CancelCommand = new DelegateCommand(this.Cancel);
        }
        # region " Basic functions "
        public ICommand SelectCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public void Confirm()
        {
            if (_notification != null)
            {
                _notification.Confirmed = true;
                _notification.Content = GetReturnResult();
            }
            this.FinishInteraction();
        }
        public void Cancel()
        {
            if (_notification != null)
            {
                _notification.Confirmed = false;
                _notification.Content = null;
            }
            this.FinishInteraction();
        }
        public INotification Notification
        {
            get
            {
                return _notification;
            }
            set
            {
                _notification = value as Confirmation;
                if (_notification == null) return;
                _metadata = _notification.Content as Entity;
                InitializeViewModel();
            }
        }
        public Action FinishInteraction { get; set; }
        #endregion
        private void InitializeViewModel()
        {
            if (_parameters.Count > 0) return;
            //_parameters.Clear();
            foreach (Property property in _metadata.Properties)
            {
                if (property.Fields.Count == 0) continue;
                if (property.Name == "Ссылка" ||
                    property.Name == "ИмяПредопределенныхДанных") continue;

                Entity type = Entity.Empty;
                if (property.Relations.Count == 1)
                {
                    type = property.Relations[0].Entity;
                }
                if (!this.IsAllowedType(type)) continue;

                FilterParameterViewModel parameter = new FilterParameterViewModel()
                {
                    UseMe = false,
                    InfoBase = _metadata.InfoBase,
                    Name = property.Name,
                    Type = type,
                    FilterOperator = FilterOperator.Equal
                };
                if (property.Relations.Count == 1)
                {
                    parameter.Value = Entity.GetDefaultValue(type);
                }
                else
                {
                    parameter.Value = null;
                }
                _parameters.Add(parameter);
            }
            this.OnPropertyChanged("Parameters");
        }
        public List<FilterParameterViewModel> Parameters { get { return _parameters; } }
        // types supported by ChameleonBox user control
        private bool IsAllowedType(Entity type)
        {
            return type == Entity.Empty
                || type == Entity.DateTime
                || type == Entity.Boolean
                || type == Entity.Int32
                || type == Entity.Decimal
                || type == Entity.String
                || type.Code > 0; // ReferenceObject
        }
        private List<FilterParameter> GetReturnResult()
        {
            List<FilterParameter> result = new List<FilterParameter>();
            foreach (FilterParameterViewModel parameter in _parameters)
            {
                if (parameter.UseMe == false) continue;
                result.Add(new FilterParameter()
                    {
                        Name     = parameter.Name,
                        Operator = parameter.FilterOperator,
                        Value    = parameter.Value
                    });
            }
            return result;
        }
    }
}
