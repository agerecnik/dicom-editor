using System;
using System.Configuration;
using System.Threading.Tasks;
using DicomEditor.Model.Interfaces;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.Model.Services
{
    public class SettingsService : ISettingsService
    {
        private string _queryRetrieveServerAET;
        public string QueryRetrieveServerAET
        {
            get => _queryRetrieveServerAET;
            set
            {
                _queryRetrieveServerAET = value;
                SetSetting("QueryRetrieveServerAET", value);
            }
        }

        private string _queryRetrieveServerHost;
        public string QueryRetrieveServerHost
        {
            get => _queryRetrieveServerHost;
            set
            {
                _queryRetrieveServerHost = value;
                SetSetting("QueryRetrieveServerHost", value);
            }
        }

        private string _queryRetrieveServerPort;
        public string QueryRetrieveServerPort
        {
            get => _queryRetrieveServerPort;
            set
            {
                _queryRetrieveServerPort = value;
                SetSetting("QueryRetrieveServerPort", value);
            }
        }

        private string _storeServerAET;
        public string StoreServerAET
        {
            get => _storeServerAET;
            set
            {
                _storeServerAET = value;
                SetSetting("StoreServerAET", value);
            }
        }

        private string _storeServerHost;
        public string StoreServerHost
        {
            get => _storeServerHost;
            set
            {
                _storeServerHost = value;
                SetSetting("StoreServerHost", value);
            }
        }

        private string _storeServerPort;
        public string StoreServerPort
        {
            get => _storeServerPort;
            set
            {
                _storeServerPort = value;
                SetSetting("StoreServerPort", value);
            }
        }

        private string _dicomEditorAET;
        public string DicomEditorAET
        {
            get => _dicomEditorAET;
            set
            {
                _dicomEditorAET = value;
                SetSetting("DicomEditorAET", value);
            }
        }

        public VerificationStatus QueryRetrieveServerVerificationStatus { get; set; }
        public VerificationStatus StoreServerVerificationStatus { get; set; }

        public SettingsService()
        {
            _queryRetrieveServerAET = GetSetting("QueryRetrieveServerAET");
            _queryRetrieveServerHost = GetSetting("QueryRetrieveServerHost");
            _queryRetrieveServerPort = GetSetting("QueryRetrieveServerPort");

            _storeServerAET = GetSetting("StoreServerAET");
            _storeServerHost = GetSetting("StoreServerHost");
            _storeServerPort = GetSetting("StoreServerPort");

            _dicomEditorAET = GetSetting("DicomEditorAET");

            QueryRetrieveServerVerificationStatus = VerificationStatus.NA;
            StoreServerVerificationStatus = VerificationStatus.NA;

        }

        public async Task VerifyAsync(ServerType server)
        {
            if(server is ServerType.QueryRetrieve)
            {
                bool successful = await DicomVerificationService.VerifyAsync(QueryRetrieveServerHost, Int32.Parse(QueryRetrieveServerPort), QueryRetrieveServerAET, DicomEditorAET);
                QueryRetrieveServerVerificationStatus = (successful ? VerificationStatus.Successful : VerificationStatus.Failed);
            } else if (server is ServerType.Store)
            {
                bool successful = await DicomVerificationService.VerifyAsync(StoreServerHost, Int32.Parse(StoreServerPort), StoreServerAET, DicomEditorAET);
                StoreServerVerificationStatus = (successful ? VerificationStatus.Successful : VerificationStatus.Failed);
            }
        }

        private static void SetSetting(string key, string value)
        {
            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
