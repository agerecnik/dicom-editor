using DicomEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DicomEditor.Model.IDICOMServer;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.ViewModel
{
    public class DICOMServerViewModel : ViewModelBase, IDICOMServer
    {
        public DICOMServerViewModel(ServerType type, string aet, string host, string port, VerificationStatus status)
        {
            Type = type;
            AET = aet;
            Host = host;
            Port = port;
            Status = status;
        }

        public ServerType Type { get; }

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

        private VerificationStatus _status;
        public VerificationStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
            }
        }
    }
}
