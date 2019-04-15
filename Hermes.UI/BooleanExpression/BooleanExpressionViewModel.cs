﻿using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Zhichkin.Hermes.Model;
using Zhichkin.Shell;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewModel : HermesViewModel
    {
        private UserControl _View;
        private BooleanFunction _Model;
        private BooleanExpressionViewBuilder _ViewBuilder = new BooleanExpressionViewBuilder();
        private bool _IsCommandPanelVisible = true;

        public BooleanExpressionViewModel(HermesViewModel parent, string clause) : base(parent)
        {
            if (parent == null) throw new ArgumentNullException();
            this.Clause = clause;
            GetModelFromParent();
            this.AddNewConditionCommand = new DelegateCommand(this.AddNewCondition);
        }
        private void GetModelFromParent()
        {
            if (this.Parent is SelectStatementViewModel)
            {
                if (this.Clause == "WHERE")
                {
                    SelectStatement model = ((SelectStatementViewModel)this.Parent).Model as SelectStatement;
                    if (model != null)
                    {
                        _Model = model.WHERE;
                    }
                }
                //else if (clause == "HAVING")
            }
        }
        private void SetModelToParent()
        {
            if (this.Parent is SelectStatementViewModel)
            {
                if (this.Clause == "WHERE")
                {
                    SelectStatement model = ((SelectStatementViewModel)this.Parent).Model as SelectStatement;
                    if (model != null)
                    {
                        model.WHERE = _Model;
                    }
                }
                //else if (clause == "HAVING")
            }
        }
        public string Clause { get; private set; }
        public UserControl View
        {
            get { return _View; }
            set
            {
                _View = value;
                this.OnPropertyChanged("View");
            }
        }

        public bool IsCommandPanelVisible
        {
            get { return _IsCommandPanelVisible; }
            set
            {
                _IsCommandPanelVisible = value;
                this.OnPropertyChanged("IsCommandPanelVisible");
            }
        }

        public ICommand AddNewConditionCommand { get; private set; }
        private void AddNewCondition()
        {
            TableExpression table = ((SelectStatementViewModel)this.Parent).Model as TableExpression;
            if (table == null)
            {
                Z.Notify(new Notification { Title = "Hermes", Content = "Предложение FROM не содержит ни одной таблицы!" });
                return;
            }

            if (_Model == null)
            {
                _Model = new ComparisonOperator(((SelectStatementViewModel)this.Parent).Model);
                SetModelToParent();
                this.View = _ViewBuilder.Build(this, _Model);
            }
            else if (_Model is ComparisonOperator)
            {
                BooleanOperator bo = new BooleanOperator(((SelectStatementViewModel)this.Parent).Model);
                bo.AddChild(_Model);
                bo.AddChild(new ComparisonOperator(bo));
                _Model = bo;
                SetModelToParent();
                this.IsCommandPanelVisible = false;
                this.View = _ViewBuilder.Build(this, _Model);
            }
        }
        public void ClearBooleanExpression()
        {
            _Model = null;
            SetModelToParent();
            this.View = null;
            this.IsCommandPanelVisible = true;
        }
    }
}