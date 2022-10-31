using DicomEditor.Commands;
using DicomEditor.Interfaces;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        public IDICOMServer QueryRetrieveServer { get; set; }
        public IDICOMServer StoreServer { get; set; }

        private string _dicomEditorAET;
        public string DicomEditorAET
        {
            get => _dicomEditorAET;
            set
            {
                SetProperty(ref _dicomEditorAET, value);
                _settingsService.DicomEditorAET = value;
            }
        }

        private string _dicomRoot;
        public string DicomRoot
        {
            get => _dicomRoot;
            set
            {
                SetProperty(ref _dicomRoot, value);
                _settingsService.DicomRoot = value;
            }
        }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            QueryRetrieveServer = _settingsService.GetServer(ServerType.QueryRetrieveServer);
            StoreServer = _settingsService.GetServer(ServerType.StoreServer);
            DicomEditorAET = _settingsService.DicomEditorAET;
            DicomRoot = _settingsService.DicomRoot;
        }

        public SettingsViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }
    }
}
