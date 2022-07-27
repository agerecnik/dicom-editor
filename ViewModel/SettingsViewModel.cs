using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private IImportService _importService;

        private ISettingsService _settingsService;

        public DICOMServer QueryRetrieveServer { get; set; }

        public DICOMServer StoreServer { get; set; }

        private string _dicomEditorAET;
        public string DicomEditorAET
        {
            get => _dicomEditorAET;
            set
            {
                SetProperty(ref _dicomEditorAET, value);
            }
        }

        public ICommand SaveSettingsCommand { get; }

        public ICommand VerifyCommand { get; }

        public SettingsViewModel(ISettingsService settingsService, IImportService importService)
        {
            _importService = importService;
            _settingsService = settingsService;
            QueryRetrieveServer = new(ServerType.QueryRetrieve);
            StoreServer = new(ServerType.Store);

            SaveSettingsCommand = new RelayCommand(o =>
            {
                SaveSettings();
            });

            VerifyCommand = new RelayCommand(async o =>
            {
                SaveSettings();
                DICOMServer server = (DICOMServer)o;
                await _settingsService.VerifyAsync(server.ServerType);
                if (server.ServerType is ServerType.QueryRetrieve)
                {
                    server.VerificationStatus = settingsService.QueryRetrieveServerVerificationStatus;
                } else if(server.ServerType is ServerType.Store)
                {
                    server.VerificationStatus = settingsService.StoreServerVerificationStatus;
                }

            });

            QueryRetrieveServer.AET = _settingsService.QueryRetrieveServerAET;
            QueryRetrieveServer.Host = _settingsService.QueryRetrieveServerHost;
            QueryRetrieveServer.Port = _settingsService.QueryRetrieveServerPort;
            QueryRetrieveServer.VerificationStatus = _settingsService.QueryRetrieveServerVerificationStatus;
            StoreServer.AET = _settingsService.StoreServerAET;
            StoreServer.Host = _settingsService.StoreServerHost;
            StoreServer.Port = _settingsService.StoreServerPort;
            StoreServer.VerificationStatus = _settingsService.StoreServerVerificationStatus;
            DicomEditorAET = _settingsService.DicomEditorAET;
        }

        public SettingsViewModel() : this(new SettingsService(), new ImportService(new SettingsService(), new Cache()))
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        private void SaveSettings()
        {
            _settingsService.QueryRetrieveServerAET = QueryRetrieveServer.AET;
            _settingsService.QueryRetrieveServerHost = QueryRetrieveServer.Host;
            _settingsService.QueryRetrieveServerPort = QueryRetrieveServer.Port;
            _settingsService.StoreServerAET = StoreServer.AET;
            _settingsService.StoreServerHost = StoreServer.Host;
            _settingsService.StoreServerPort = StoreServer.Port;
            _settingsService.DicomEditorAET = _dicomEditorAET;

            if (_importService.QueryResult is not null)
            {
                _importService.QueryResult = null;
            }
        }
    }

    public class DICOMServer : ViewModelBase
    {
        public DICOMServer(ServerType serverType)
        {
            ServerType = serverType;
        }

        public ServerType ServerType{ get; }

        private string _AET;
        public string AET
        {
            get => _AET;
            set
            {
                SetProperty(ref _AET, value);
            }
        }

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                SetProperty(ref _host, value);
            }
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                SetProperty(ref _port, value);
            }
        }

        private VerificationStatus _verificationStatus;
        public VerificationStatus VerificationStatus
        {
            get => _verificationStatus;
            set
            {
                SetProperty(ref _verificationStatus, value);
            }
        }
    }
}
