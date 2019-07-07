using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Hermes.Services;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class QueryExpressionViewModel : HermesViewModel
    {
        public QueryExpressionViewModel(HermesViewModel parent, QueryExpression model) : base(parent, model)
        {
            this.TypeSelectionDialog = new InteractionRequest<Confirmation>();
            this.ReferenceObjectSelectionDialog = new InteractionRequest<Confirmation>();

            this.QueryParameters = new ObservableCollection<ParameterExpressionViewModel>();
            this.QueryExpressions = new ObservableCollection<SelectStatementViewModel>();

            this.AddNewParameterCommand = new DelegateCommand(this.AddNewParameter);
            this.RemoveParameterCommand = new DelegateCommand<string>(this.RemoveParameter);

            this.ShowSQLCommand = new DelegateCommand(this.ShowSQL);
            this.ExecuteQueryCommand = new DelegateCommand(this.ExecuteQuery);
        }
        public InteractionRequest<Confirmation> TypeSelectionDialog { get; private set; }
        public InteractionRequest<Confirmation> ReferenceObjectSelectionDialog { get; private set; }
        
        public ObservableCollection<SelectStatementViewModel> QueryExpressions { get; private set; }
        public ObservableCollection<ParameterExpressionViewModel> QueryParameters { get; private set; }

        public ICommand AddNewParameterCommand { get; private set; }
        private void AddNewParameter()
        {
            QueryExpression model = this.Model as QueryExpression;
            if (model.Parameters == null) { model.Parameters = new List<ParameterExpression>(); }

            ParameterExpression parameter = new ParameterExpression(model);
            model.Parameters.Add(parameter);
            parameter.Name = "Parameter" + model.Parameters.Count.ToString();

            ParameterExpressionViewModel childVM = new ParameterExpressionViewModel(this, parameter);
            this.QueryParameters.Add(childVM);
        }
        public ICommand RemoveParameterCommand { get; private set; }
        private void RemoveParameter(string name)
        {
            QueryExpression model = this.Model as QueryExpression;
            if (model.Parameters == null) { model.Parameters = new List<ParameterExpression>(); }

            for (int i = 0; i < model.Parameters.Count; i++)
            {
                if (model.Parameters[i].Name == name)
                {
                    model.Parameters.RemoveAt(i);
                    break;
                }
            }

            for (int i = 0; i < this.QueryParameters.Count; i++)
            {
                if (this.QueryParameters[i].Name == name)
                {
                    this.QueryParameters.RemoveAt(i);
                    break;
                }
            }
        }

        public ICommand ShowSQLCommand { get; private set; }
        private void ShowSQL()
        {
            QueryExpression model = this.Model as QueryExpression;
            if (model == null) return;

            HermesService service = new HermesService();
            this.SQLText = service.ToSQL(model);
           this.IsSQLTabSelected = true;
        }

        public ICommand ExecuteQueryCommand { get; private set; }
        private void ExecuteQuery()
        {
            QueryExpression model = this.Model as QueryExpression;
            if (model == null) return;

            HermesService service = new HermesService();
            try
            {
                this.QueryResultTable = service.ExecuteQuery(model);
                this.IsQueryResultTabSelected = true;
            }
            catch (Exception ex)
            {
                Z.Notify(new Notification() { Title = "Hermes", Content = Z.GetErrorText(ex) });
            }
        }

        // Tab "Query Design"
        private bool _IsQueryTabSelected = true;
        public bool IsQueryTabSelected
        {
            get { return _IsQueryTabSelected; }
            set
            {
                _IsQueryTabSelected = value;
                this.OnPropertyChanged("IsQueryTabSelected");
            }
        }

        // Tab "SQL"
        private bool _IsSQLTabSelected = false;
        public bool IsSQLTabSelected
        {
            get { return _IsSQLTabSelected; }
            set
            {
                _IsSQLTabSelected = value;
                this.OnPropertyChanged("IsSQLTabSelected");
            }
        }
        private string _SQLText = string.Empty;
        public string SQLText
        {
            get { return _SQLText; }
            set
            {
                _SQLText = value;
                this.OnPropertyChanged("SQLText");
            }
        }

        // Tab "Query Result"
        private bool _IsQueryResultTabSelected = false;
        public bool IsQueryResultTabSelected
        {
            get { return _IsQueryResultTabSelected; }
            set
            {
                _IsQueryResultTabSelected = value;
                this.OnPropertyChanged("IsQueryResultTabSelected");
            }
        }
        private IEnumerable _QueryResultTable;
        public IEnumerable QueryResultTable
        {
            get { return _QueryResultTable; }
            set
            {
                _QueryResultTable = value;
                this.OnPropertyChanged("QueryResultTable");
            }
        }
    }
}
