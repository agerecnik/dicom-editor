using DicomEditor.Commands;
using DicomEditor.Interfaces;
using System.Windows.Input;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.ViewModel
{
    public class DICOMServer : ViewModelBase, IDICOMServer
    {
        private readonly ISettingsService _settingsService;

        public ServerType Type { get; }

        private string _AET;
        public string AET
        {
            get => _AET;
            set
            {
                SetProperty(ref _AET, value);
                _settingsService.SetSetting(Type + nameof(AET), AET);
            }
        }

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                SetProperty(ref _host, value);
                _settingsService.SetSetting(Type + nameof(Host), Host);
            }
        }

        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                SetProperty(ref _port, value);
                _settingsService.SetSetting(Type + nameof(Port), Port);
            }
        }

        private VerificationStatus _status;
        public VerificationStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
            }
        }

        public ICommand VerifyCommand { get; }

        public DICOMServer(ServerType type, string aet, string host, string port, VerificationStatus status, ISettingsService settingsService)
        {
            _settingsService = settingsService;

            Type = type;
            AET = aet;
            Host = host;
            Port = port;
            Status = status;

            VerifyCommand = new RelayCommand(Verify, CanUseVerifyCommand);
        }

        private async void Verify(object o)
        {
            IDICOMServer server = (IDICOMServer)o;
            await _settingsService.VerifyAsync(server.Type);
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanUseVerifyCommand(object o)
        {
            if (AET is null || AET is ""
                || Host is null || Host is ""
                || Port is null || Port is ""
                || Status is VerificationStatus.InProgress)
            {
                return false;
            }
            return true;
        }
    }
}
