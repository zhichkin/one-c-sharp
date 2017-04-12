using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Documents;
using Zhichkin.Metadata.Model;
using Zhichkin.ORM;
using Zhichkin.Shell;
using System.Reflection;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Metadata.UI
{
    public partial class ChameleonBox : UserControl
    {
        public static readonly DependencyProperty InfoBaseProperty;
        public static readonly DependencyProperty ChameleonTypeProperty;
        public static readonly DependencyProperty ChameleonValueProperty;
        private static void OnInfoBaseChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            ChameleonBox box = target as ChameleonBox;
            if (box == null) return;
            box.InfoBase = (args.NewValue == null ? null : (InfoBase)args.NewValue);
        }
        private static void OnChameleonTypeChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue) return;
            ChameleonBox box = target as ChameleonBox;
            if (box == null) return;
            box.ChameleonType = (args.NewValue == null ? null : (Entity)args.NewValue);
            box.UserControl_ValueTypeChanged(box, args);
        }
        private static void OnChameleonValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue) return;
            ChameleonBox box = target as ChameleonBox;
            if (box == null) return;
            box.ChameleonValue = (args.NewValue == null ? null : args.NewValue);
        }
        static ChameleonBox()
        {
            InfoBaseProperty = DependencyProperty.Register("InfoBase", typeof(InfoBase), typeof(ChameleonBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnInfoBaseChanged)));
            ChameleonTypeProperty = DependencyProperty.Register("ChameleonType", typeof(Entity), typeof(ChameleonBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnChameleonTypeChanged)));
            ChameleonValueProperty = DependencyProperty.Register("ChameleonValue", typeof(object), typeof(ChameleonBox),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    new PropertyChangedCallback(OnChameleonValueChanged)));
        }

        public ChameleonBox()
        {   
            InitializeComponent();
            this.SelectDataTypeDialog = new InteractionRequest<Confirmation>();
            this.SelectReferenceObjectDialog = new InteractionRequest<Confirmation>();
        }
        public InfoBase InfoBase
        {
            set { SetValue(InfoBaseProperty, value); }
            get { return (InfoBase)GetValue(InfoBaseProperty); }
        }
        public Entity ChameleonType
        {
            set { SetValue(ChameleonTypeProperty, value); }
            get { return (Entity)GetValue(ChameleonTypeProperty); }
        }
        public object ChameleonValue
        {
            set { SetValue(ChameleonValueProperty, value); }
            get { return GetValue(ChameleonValueProperty); }
        }
        private void UserControl_ValueTypeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChameleonBox box = sender as ChameleonBox;
            if (box == null) return;
            if (box.ContentTemplateSelector == null) return;
            DataTemplate template = box.ContentTemplateSelector.SelectTemplate(box, (DependencyObject)sender);
            box.ContentTemplate = template;
        }
        private void UpdateSourceObjectValue(ChameleonBox box)
        {
            BindingExpression binding = this.GetBindingExpression(ChameleonBox.ChameleonValueProperty);
            if (binding == null) return;
            binding.UpdateSource();
            object item = binding.DataItem;
            if (item != null && item is Persistent)
            {
                ((Persistent)item).Save();
            }
        }
        private void ClearChameleonValue_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BindingExpression binding = this.GetBindingExpression(ChameleonBox.ChameleonValueProperty);
            if (binding == null) return;
            object item = binding.DataItem;
            if (item == null) return;
            string propertyName = binding.ResolvedSourcePropertyName;
            if (string.IsNullOrEmpty(propertyName)) return;

            PropertyInfo property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return;
            property.SetValue(item, null);

            if (item != null && item is Persistent)
            {
                ((Persistent)item).Save();
            }
        }
        // Empty
        public InteractionRequest<Confirmation> SelectDataTypeDialog { get; private set; }
        public InteractionRequest<Confirmation> SelectReferenceObjectDialog { get; private set; }
        private void SelectChameleonTypeHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            FrameworkElement element = ((Hyperlink)sender).Parent as FrameworkElement;
            ChameleonBox box = element.GetParent<ChameleonBox>();
            if (box == null) return;

            Confirmation confirmation = new Confirmation()
            {
                Title = "Выберите тип значения",
                Content = this.InfoBase
            };
            this.SelectDataTypeDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed) this.OnDataTypeSelected(box, response.Content as Entity);
            });
        }
        private void OnDataTypeSelected(ChameleonBox target, Entity type)
        {
            if (type == null || type == Entity.Empty) return;
            if (type.Code > 0)
            {
                OnSelectReferenceObject(target, type);
                return;
            }
            target.ChameleonValue = Entity.GetDefaultValue(type);
            UpdateSourceObjectValue(target);
        }
        // DateTime
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker picker = sender as DatePicker;
            TextBox box = picker.GetChild<TextBox>();
            DatePicker_ProcessValue(box, false);
        }
        private void DatePicker_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DatePicker_ProcessValue((TextBox)sender, true);
        }
        private void DatePicker_ProcessValue(TextBox sender, bool useTextBox)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (sender == null) return;

            DatePicker target = element.GetParent<DatePicker>();
            if (target == null) return;

            DateTime value; bool valueChanged = false;
            if (useTextBox && DateTime.TryParse(sender.Text, out value))
            {
                target.SelectedDate = value;
            }
            if (this.ChameleonValue != null
                && this.ChameleonValue.GetType() == typeof(DateTime)
                && (DateTime)this.ChameleonValue !=
                (target.SelectedDate.HasValue ? target.SelectedDate.Value : DateTime.MinValue))
            {
                this.ChameleonValue = (target.SelectedDate ?? null);
                UpdateSourceObjectValue(this);
                valueChanged = true;
            }

            if (valueChanged)
            {
                DataGridCell cell = element.GetParent<DataGridCell>();
                if (cell == null) return;
                cell.IsEditing = false;
            }
        }
        // Decimal
        private void Decimal_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            TextBox box = sender as TextBox;
            if (box == null) return;

            decimal value; bool valueChanged = false;
            if (!decimal.TryParse(box.Text, out value))
            {
                value = 0M;
            }
            if (this.ChameleonValue != null
                && this.ChameleonValue.GetType() == typeof(decimal)
                && (decimal)this.ChameleonValue != value)
            {
                this.ChameleonValue = value;
                UpdateSourceObjectValue(this);
                valueChanged = true;
            }

            if (valueChanged)
            {
                DataGridCell cell = box.GetParent<DataGridCell>();
                if (cell == null) return;
                cell.IsEditing = false;
            }
        }
        // Int32
        private void Int32_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            TextBox box = sender as TextBox;
            if (box == null) return;

            int value; bool valueChanged = false;
            if (!int.TryParse(box.Text, out value))
            {
                value = 0;
            }
            if (this.ChameleonValue != null
                && this.ChameleonValue.GetType() == typeof(int)
                && (int)this.ChameleonValue != value)
            {
                this.ChameleonValue = value;
                UpdateSourceObjectValue(this);
                valueChanged = true;
            }

            if (valueChanged)
            {
                DataGridCell cell = box.GetParent<DataGridCell>();
                if (cell == null) return;
                cell.IsEditing = false;
            }
        }
        // String
        private void String_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            TextBox box = sender as TextBox;
            if (box == null) return;

            string value = box.Text; bool valueChanged = false;
            if (this.ChameleonValue != null
                && this.ChameleonValue.GetType() == typeof(string)
                && (string)this.ChameleonValue != value)
            {
                this.ChameleonValue = value;
                UpdateSourceObjectValue(this);
                valueChanged = true;
            }

            if (valueChanged)
            {
                DataGridCell cell = box.GetParent<DataGridCell>();
                if (cell == null) return;
                cell.IsEditing = false;
            }
        }
        // Boolean
        private void Boolean_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox box = sender as CheckBox;
            if (box == null) return;

            bool value = box.IsChecked.Value;
            bool valueChanged = false;
            if (this.ChameleonValue != null
                && this.ChameleonValue.GetType() == typeof(bool)
                && (bool)this.ChameleonValue != value)
            {
                this.ChameleonValue = value;
                UpdateSourceObjectValue(this);
                valueChanged = true;
            }

            if (valueChanged)
            {
                DataGridCell cell = box.GetParent<DataGridCell>();
                if (cell == null) return;
                cell.IsEditing = false;
            }
        }
        // ReferenceObject
        private void SelectChameleonValueHyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            FrameworkElement element = ((Hyperlink)sender).Parent as FrameworkElement;
            ChameleonBox box = element.GetParent<ChameleonBox>();
            if (box == null) return;
            OnSelectReferenceObject(box, box.ChameleonType);
        }
        private void OnSelectReferenceObject(ChameleonBox box, Entity type)
        {
            Confirmation confirmation = new Confirmation() { Title = string.Empty, Content = type };
            this.SelectReferenceObjectDialog.Raise(confirmation, response =>
            {
                if (response.Confirmed) this.OnReferenceObjectSelected(box, response.Content as ReferenceProxy);
            });
        }
        private void OnReferenceObjectSelected(ChameleonBox target, ReferenceProxy entity)
        {
            target.ChameleonValue = entity;
            target.ChameleonType = entity.Type;
            UpdateSourceObjectValue(target);
        }
    }
}
