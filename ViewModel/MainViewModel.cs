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
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ImportViewCommand { get; }
        public ICommand EditorViewCommand { get; }
        public ICommand SettingsViewCommand { get; }

        public MainViewModel(IImportService importService, IEditorService editorService, ISettingsService settingsService, IDialogService dialogService)
        {
            CurrentView = new ImportViewModel(importService, dialogService);
            ImportViewCommand = new RelayCommand(o => SetViewModel(new ImportViewModel(importService, dialogService)));
            EditorViewCommand = new RelayCommand(o => SetViewModel(new EditorViewModel(editorService, dialogService)));
            SettingsViewCommand = new RelayCommand(o => SetViewModel(new SettingsViewModel(settingsService)));
        }

        public MainViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        private void SetViewModel(ViewModelBase vm)
        {
            var previousVm = CurrentView;
            CurrentView = vm;
            previousVm.Dispose();
        }
    }
}
