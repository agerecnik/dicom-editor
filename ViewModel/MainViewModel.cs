using DicomEditor.Commands;
using DicomEditor.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ImportViewCommand { get; }
        public ICommand EditorViewCommand { get; }
        public ICommand SettingsViewCommand { get; }

        public MainViewModel(IImportService importService, IEditorService editorService, ISettingsService settingsService, ICache cache)
        {
            CurrentView = new ImportViewModel(importService);

            ImportViewCommand = new RelayCommand(o =>
            {
                CurrentView = new ImportViewModel(importService);
            });

            EditorViewCommand = new RelayCommand(o =>
            {
                CurrentView = new EditorViewModel(editorService);
                EditorViewModel evm = (EditorViewModel)CurrentView;
                evm.UpdateLoadedSeriesList();
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = new SettingsViewModel(settingsService, importService);
            });
        }
    }
}
