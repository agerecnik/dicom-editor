using DicomEditor.Commands;
using DicomEditor.Interfaces;
using System;
using System.ComponentModel;
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

        public MainViewModel(IImportService importService, IEditorService editorService, ISettingsService settingsService, ICache cache, IDialogService dialogService)
        {
            CurrentView = new ImportViewModel(importService, dialogService);

            ImportViewCommand = new RelayCommand(o =>
            {
                CurrentView = new ImportViewModel(importService, dialogService);
            });

            EditorViewCommand = new RelayCommand(o =>
            {
                CurrentView = new EditorViewModel(editorService, dialogService);
                EditorViewModel evm = (EditorViewModel)CurrentView;
                evm.UpdateLoadedSeriesList();
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = new SettingsViewModel(settingsService);
            });
        }

        public MainViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }
    }
}
