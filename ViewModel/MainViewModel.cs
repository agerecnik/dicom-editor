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
        public ImportViewModel ImportVM { get; set; }
        public EditorViewModel EditorVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ImportViewCommand { get; }
        public ICommand EditorViewCommand { get; }
        public ICommand SettingsViewCommand { get; }

        public MainViewModel(ImportViewModel importVM, EditorViewModel editorVM, SettingsViewModel settingsVM)
        {
            ImportVM = importVM;
            EditorVM = editorVM;
            SettingsVM = settingsVM;

            CurrentView = ImportVM;

            ImportViewCommand = new RelayCommand(o =>
            {
                CurrentView = ImportVM;
            });

            EditorViewCommand = new RelayCommand(o =>
            {
                CurrentView = EditorVM;
                EditorVM.UpdateLoadedSeriesList();
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVM;
            });
        }

        public MainViewModel() : this(new ImportViewModel(), new EditorViewModel(), new SettingsViewModel())
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }
    }
}
