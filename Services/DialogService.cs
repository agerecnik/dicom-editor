using DicomEditor.Interfaces;
using DicomEditor.View;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using static DicomEditor.Interfaces.IDialogViewModel;

namespace DicomEditor.Services
{
    public class DialogService : IDialogService
    {
        private static readonly Dictionary<Type, Type> _mappings = new();

        public static void RegisterDialog<TView, TViewModel>()
        {
            _mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public TViewModel ShowDialog<TViewModel>(string title, params object[] vmParameters)
        {
            var type = _mappings[typeof(TViewModel)];
            return (TViewModel)ShowDialogInternal(title, type, typeof(TViewModel), true, vmParameters);
        }

        public TViewModel Show<TViewModel>(string title, params object[] vmParameters)
        {
            var type = _mappings[typeof(TViewModel)];
            return (TViewModel)ShowDialogInternal(title, type, typeof(TViewModel), false, vmParameters);
        }

        private object ShowDialogInternal(string title, Type type, Type vmType, bool modal, params object[] vmParameters)
        {
            var dialog = new DialogWindow
            {
                Title = title
            };

            var content = Activator.CreateInstance(type);
            var vm = Activator.CreateInstance(vmType, vmParameters);
            (content as FrameworkElement).DataContext = vm;

            IDialogViewModel viewModel = (IDialogViewModel)vm;
            viewModel.Execute();

            dialog.Content = content;

            if (!viewModel.ExecutionFinished)
            {
                if (modal)
                {
                    dialog.ShowDialog();
                }
                else
                {
                    dialog.Show();
                }
            }
            return vm;
        }
    }
}
