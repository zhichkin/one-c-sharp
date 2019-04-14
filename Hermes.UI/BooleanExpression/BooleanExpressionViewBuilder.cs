using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Zhichkin.Hermes.Model;

namespace Zhichkin.Hermes.UI
{
    public class BooleanExpressionViewBuilder
    {
        private UserControl _View;
        public BooleanExpressionViewBuilder() { }
        public UserControl Build(HermesViewModel parent, BooleanFunction model)
        {
            if (model == null) throw new ArgumentNullException();

            if (model is ComparisonOperator)
            {
                ComparisonOperatorViewModel viewModel = new ComparisonOperatorViewModel(parent, (ComparisonOperator)model);
                _View = new ComparisonOperatorView(viewModel);
            }
            else if (model is BooleanOperator)
            {
                BooleanOperator bo = (BooleanOperator)model;
                BooleanOperatorViewModel viewModel = new BooleanOperatorViewModel(parent, bo);
                List<BooleanFunctionViewModel> ovms = new List<BooleanFunctionViewModel>();
                foreach (BooleanFunction f in bo.Operands)
                {
                    ComparisonOperatorViewModel covm = new ComparisonOperatorViewModel(viewModel, (ComparisonOperator)f);
                    ovms.Add(covm);
                }
                viewModel.Operands = new ObservableCollection<BooleanFunctionViewModel>(ovms);
                _View = new BooleanOperatorView(viewModel);
            }

            return _View;
        }
    }
}
