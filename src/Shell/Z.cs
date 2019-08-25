using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Shell
{
    public static class Z
    {
        private static readonly ShellViewModel viewModel = new ShellViewModel();
        internal static ShellViewModel ViewModel { get { return viewModel; } }
        public static void Notify(INotification context)
        {
            viewModel.NotificationRequest.Raise(context);
        }
        public static void Confirm(IConfirmation context, Action<IConfirmation> callback)
        {
            viewModel.ConfirmationRequest.Raise(context, callback);
        }
        public static void ClearRightRegion(IRegionManager regionManager)
        {
            IRegion rightRegion = regionManager.Regions[RegionNames.RightRegion];
            if (rightRegion == null) return;
            foreach (object view in rightRegion.Views)
            {
                rightRegion.Remove(view);
            }
        }
        public static TParent GetParent<TParent>(this DependencyObject child) where TParent : DependencyObject
        {
            if (child == null) return null;
            if (child.GetType() == typeof(TParent)) return (TParent)child;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent.GetType() != typeof(TParent))
            {
                parent = GetParent<TParent>(parent);
            }
            return parent == null ? null : (TParent)parent;
        }
        public static TChild GetChild<TChild>(this DependencyObject parent) where TChild : DependencyObject
        {
            if (parent == null) return null;
            TChild result = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child.GetType() == typeof(TChild)) return (TChild)child;
                result = GetChild<TChild>(child);
                if (result != null) return result;
            }
            return result;
        }
        public static string GetErrorText(Exception ex)
        {
            string errorText = string.Empty;
            Exception error = ex;
            while (error != null)
            {
                errorText += (errorText == string.Empty) ? error.Message : Environment.NewLine + error.Message;
                error = error.InnerException;
            }
            return errorText;
        }
    }
}
