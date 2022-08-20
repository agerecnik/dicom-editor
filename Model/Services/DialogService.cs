using DicomEditor.Model.Interfaces;
using DicomEditor.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DicomEditor.Model.Services
{
    public  class DialogService : IDialogService
    {
        private static Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

        public static void RegisterDialog<TView, TViewModel>()
        {
            _mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public void ShowDialog<TViewModel>(string title, Action<string> callback, params object[] vmParameters)
        {
            var type = _mappings[typeof(TViewModel)];
            ShowDialogInternal(title, type, callback, typeof(TViewModel), vmParameters);
        }

        private static void ShowDialogInternal(string title, Type type, Action<string> callback, Type vmType, params object[] vmParameters)
        {
            var dialog = new DialogWindow();
            dialog.Title = title;

            //EventHandler closeEventHandler = null;
            //closeEventHandler = (s, e) =>
            //{
            //    callback(dialog.DialogResult.ToString());
            //    dialog.Closed -= closeEventHandler;
            //};
            //dialog.Closed += closeEventHandler;

            var content = Activator.CreateInstance(type);

            if (vmType != null)
            {
                var vm = Activator.CreateInstance(vmType, vmParameters);
                (content as FrameworkElement).DataContext = vm;
            }

            dialog.Content = content;

            dialog.ShowDialog();
        }
    }
}
