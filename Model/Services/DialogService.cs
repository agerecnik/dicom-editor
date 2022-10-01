using DicomEditor.Model.Interfaces;
using DicomEditor.View;
using DicomEditor.ViewModel;
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
        private static readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

        public static void RegisterDialog<TView, TViewModel>()
        {
            _mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public void ShowDialog<TViewModel>(string title, params object[] vmParameters)
        {
            var type = _mappings[typeof(TViewModel)];
            ShowDialogInternal(title, type, typeof(TViewModel), vmParameters);
        }

        private static void ShowDialogInternal(string title, Type type, Type vmType, params object[] vmParameters)
        {
            var dialog = new DialogWindow();
            dialog.Title = title;

            var content = Activator.CreateInstance(type);
            var vm = Activator.CreateInstance(vmType, vmParameters);
            (content as FrameworkElement).DataContext = vm;

            IDialogViewModel viewModel = (IDialogViewModel)vm;
            viewModel.Execute();

            dialog.Content = content;

            if (!viewModel.ExecutionFinished)
            {
                dialog.ShowDialog();
            }
        }
    }
}
