using Zhichkin.Hermes.Model;
using System.Windows.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewBuilder
    {
        private UserControl _View;
        public BooleanExpressionViewBuilder() { }
        public UserControl Build(BindableBase parent, BooleanFunction model)
        {
            if (model == null) throw new ArgumentNullException();

            if (model is ComparisonOperator)
            {
                ComparisonOperatorViewModel viewModel = new ComparisonOperatorViewModel((ComparisonOperator)model);
                viewModel.Parent = parent;
                _View = new ComparisonOperatorView(viewModel);
            }
            else if (model is BooleanOperator)
            {
                BooleanOperator bo = (BooleanOperator)model;
                BooleanOperatorViewModel viewModel = new BooleanOperatorViewModel(bo);
                viewModel.Parent = parent;
                List<BooleanFunctionViewModel> ovms = new List<BooleanFunctionViewModel>();
                foreach (BooleanFunction f in bo.Operands)
                {
                    ComparisonOperatorViewModel covm = new ComparisonOperatorViewModel((ComparisonOperator)f);
                    covm.Parent = viewModel;
                    ovms.Add(covm);
                }
                viewModel.Operands = new ObservableCollection<BooleanFunctionViewModel>(ovms);
                _View = new BooleanOperatorView(viewModel);
            }

            return _View;
        }
    }
}
