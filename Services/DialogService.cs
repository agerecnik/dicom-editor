﻿using DicomEditor.Interfaces;
using DicomEditor.View;
using System;
using System.Collections.Generic;
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

        public void ShowDialog<TViewModel>(string title, params object[] vmParameters)
        {
            var type = _mappings[typeof(TViewModel)];
            ShowDialogInternal(title, type, typeof(TViewModel), vmParameters);
        }

        private static void ShowDialogInternal(string title, Type type, Type vmType, params object[] vmParameters)
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
            dialog.Closing += viewModel.OnClosing;

            if (!viewModel.ExecutionFinished)
            {
                dialog.ShowDialog();
            }
        }
    }
}