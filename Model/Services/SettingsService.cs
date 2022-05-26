using System.Configuration;
using DicomEditor.Model.Interfaces;

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

        public SettingsService()
        {
            _queryRetrieveServerAET = GetSetting("QueryRetrieveServerAET");
            _queryRetrieveServerHost = GetSetting("QueryRetrieveServerHost");
            _queryRetrieveServerPort = GetSetting("QueryRetrieveServerPort");

            _storeServerAET = GetSetting("StoreServerAET");
            _storeServerHost = GetSetting("StoreServerHost");
            _storeServerPort = GetSetting("StoreServerPort");

            _dicomEditorAET = GetSetting("DicomEditorAET");
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
