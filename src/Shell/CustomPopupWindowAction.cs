using System;
using System.Windows;
using Microsoft.Practices.Prism.Interactivity;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace Zhichkin.Shell
{
    public class CustomPopupWindowAction : PopupWindowAction
    {
        protected override void Invoke(object parameter)
        {
            var args = parameter as InteractionRequestedEventArgs;
            if (args == null) return;

            // If the WindowContent shouldn't be part of another visual tree.
            if (this.WindowContent != null && this.WindowContent.Parent != null)
            {
                return;
            }

            Window wrapperWindow = this.GetWindow(args.Context);
            wrapperWindow.ResizeMode = ResizeMode.CanResize;
            wrapperWindow.SizeToContent = SizeToContent.WidthAndHeight;

            // We invoke the callback when the interaction's window is closed.
            var callback = args.Callback;
            EventHandler handler = null;
            handler =
                (o, e) => {
                    wrapperWindow.Closed -= handler;
                    wrapperWindow.Content = null;
                    if (callback != null) callback();
                };
            wrapperWindow.Closed += handler;

            if (this.CenterOverAssociatedObject && this.AssociatedObject != null && App.Current.MainWindow != null)
            {
                // If we should center the popup over the parent window we subscribe to the SizeChanged event
                // so we can change its position after the dimensions are set.
                SizeChangedEventHandler sizeHandler = null;
                sizeHandler =
                    (o, e) => {
                        wrapperWindow.SizeChanged -= sizeHandler;

                        // вот это было дописано, чтобы все всплывающие окна
                        // центрировались относительно главного окна приложения
                        FrameworkElement view = App.Current.MainWindow;
                        Point position = view.PointToScreen(new Point(0, 0));

                        wrapperWindow.Top = position.Y + ((view.ActualHeight - wrapperWindow.ActualHeight) / 2);
                        wrapperWindow.Left = position.X + ((view.ActualWidth - wrapperWindow.ActualWidth) / 2);
                    };
                wrapperWindow.SizeChanged += sizeHandler;
            }
            if (this.IsModal)
            {
                wrapperWindow.ShowDialog();
            }
            else
            {
                wrapperWindow.Show();
            }
        }
    }
}