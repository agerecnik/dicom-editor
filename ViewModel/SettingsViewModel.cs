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
            }
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand VerifyCommand { get; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _settingsService.UpdatedVerificationStatusEvent += new UpdatedVerificationStatusHandler(HandleUpdatedVerificationStatus);

            IDICOMServer qrServer = _settingsService.GetServer(ServerType.QueryRetrieveServer);
            QueryRetrieveServer = new DICOMServerViewModel(qrServer.Type, qrServer.AET, qrServer.Host, qrServer.Port, qrServer.Status);

            IDICOMServer stServer = _settingsService.GetServer(ServerType.StoreServer);
            StoreServer = new DICOMServerViewModel(stServer.Type, stServer.AET, stServer.Host, stServer.Port, stServer.Status);

            DicomEditorAET = _settingsService.DicomEditorAET;

            SaveSettingsCommand = new RelayCommand(SaveSettings, CanUseSaveSettingsOrVerifyCommand);
            VerifyCommand = new RelayCommand(Verify, CanUseSaveSettingsOrVerifyCommand);
        }

        public SettingsViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        private void SaveSettings(object o)
        {
            _settingsService.SetServer(QueryRetrieveServer);
            _settingsService.SetServer(StoreServer);
            _settingsService.DicomEditorAET = _dicomEditorAET;
        }

        private async void Verify(object o)
        {
            SaveSettings(o);
            IDICOMServer server = (IDICOMServer)o;
            await _settingsService.VerifyAsync(server.Type);
        }

        private bool CanUseSaveSettingsOrVerifyCommand(object o)
        {
            if (QueryRetrieveServer.AET is null || QueryRetrieveServer.AET is ""
                || QueryRetrieveServer.Host is null || QueryRetrieveServer.Host is ""
                || QueryRetrieveServer.Port is null || QueryRetrieveServer.Port is ""
                || StoreServer.AET is null || StoreServer.AET is ""
                || StoreServer.Host is null || StoreServer.Host is ""
                || StoreServer.Port is null || StoreServer.Port is ""
                || DicomEditorAET is null || DicomEditorAET is "")
            {
                return false;
            }
            return true;
        }

        private void HandleUpdatedVerificationStatus(ServerType type)
        {
            if(type == ServerType.QueryRetrieveServer)
            {
                QueryRetrieveServer.Status = _settingsService.GetServer(type).Status;
            } else if(type == ServerType.StoreServer)
            {
                StoreServer.Status = _settingsService.GetServer(type).Status;
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
