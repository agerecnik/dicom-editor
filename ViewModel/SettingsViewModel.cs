using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace DicomEditor.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        public ISettingsService SettingsService { get; set; }

        private string _queryRetrieveServerAET;
        public string QueryRetrieveServerAET
        {
            get => _queryRetrieveServerAET;
            set
            {
                SetProperty(ref _queryRetrieveServerAET, value);
                SettingsService.QueryRetrieveServerAET = value;
            }
        }

        private string _queryRetrieveServerHost;
        public string QueryRetrieveServerHost
        {
            get => _queryRetrieveServerHost;
            set
            {
                SetProperty(ref _queryRetrieveServerHost, value);
                SettingsService.QueryRetrieveServerHost = value;
            }
        }

        private string _queryRetrieveServerPort;
        public string QueryRetrieveServerPort
        {
            get => _queryRetrieveServerPort;
            set
            {
                SetProperty(ref _queryRetrieveServerPort, value);
                SettingsService.QueryRetrieveServerPort = value;
            }
        }

        private string _storeServerAET;
        public string StoreServerAET
        {
            get => _storeServerAET;
            set
            {
                SetProperty(ref _storeServerAET, value);
                SettingsService.StoreServerAET = value;
            }
        }

        private string _storeServerHost;
        public string StoreServerHost
        {
            get => _storeServerHost;
            set
            {
                SetProperty(ref _storeServerHost, value);
                SettingsService.StoreServerHost = value;
            }
        }

        private string _storeServerPort;
        public string StoreServerPort
        {
            get => _storeServerPort;
            set
            {
                SetProperty(ref _storeServerPort, value);
                SettingsService.StoreServerPort = value;
            }
        }

        private string _dicomEditorAET;
        public string DicomEditorAET
        {
            get => _dicomEditorAET;
            set
            {
                SetProperty(ref _dicomEditorAET, value);
                SettingsService.DicomEditorAET = value;
            }
        }

        public SettingsViewModel(ISettingsService settingsService)
        {
            SettingsService = settingsService;

            QueryRetrieveServerAET = SettingsService.QueryRetrieveServerAET;
            QueryRetrieveServerHost = SettingsService.QueryRetrieveServerHost;
            QueryRetrieveServerPort = SettingsService.QueryRetrieveServerPort;
            StoreServerAET = SettingsService.StoreServerAET;
            StoreServerHost = SettingsService.StoreServerHost;
            StoreServerPort = SettingsService.StoreServerPort;
            DicomEditorAET = SettingsService.DicomEditorAET;
        }

        public SettingsViewModel() : this(new SettingsService())
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }
    }
}
