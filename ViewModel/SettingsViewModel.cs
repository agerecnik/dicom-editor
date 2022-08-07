using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static DicomEditor.Model.IDICOMServer;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private ISettingsService _settingsService;

        public DICOMServerViewModel QueryRetrieveServer { get; set; }

        public DICOMServerViewModel StoreServer { get; set; }

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
            QueryRetrieveServer = new(qrServer.Type, qrServer.AET, qrServer.Host, qrServer.Port, qrServer.Status);

            IDICOMServer stServer = _settingsService.GetServer(ServerType.StoreServer);
            StoreServer = new(stServer.Type, stServer.AET, stServer.Host, stServer.Port, stServer.Status);

            DicomEditorAET = _settingsService.DicomEditorAET;

            SaveSettingsCommand = new RelayCommand(o =>
            {
                SaveSettings();
            });

            VerifyCommand = new RelayCommand(Verify, CanUseVerifyButton);
            
        }

        public SettingsViewModel() : this(new SettingsService())
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        private void SaveSettings()
        {
            _settingsService.SetServer(QueryRetrieveServer);
            _settingsService.SetServer(StoreServer);
            _settingsService.DicomEditorAET = _dicomEditorAET;
        }

        private async void Verify(object o)
        {
            SaveSettings();
            IDICOMServer server = (IDICOMServer)o;
            await _settingsService.VerifyAsync(server.Type);
        }

        private bool CanUseVerifyButton(object o)
        {
            IDICOMServer server = (IDICOMServer)o;
            if(server == null || server.Status == VerificationStatus.InProgress)
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
