using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using DicomEditor.Model.Interfaces;
using static DicomEditor.Model.IDICOMServer;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.Model.Services
{
    public class SettingsService : ISettingsService
    {
        public event UpdatedVerificationStatusHandler UpdatedVerificationStatusEvent;
        public event SettingsSavedHandler SettingsSavedEvent;

        private readonly IDictionary<ServerType, IDICOMServer> _servers;
        private readonly IDICOMService _DICOMService;

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

        public SettingsService(IDICOMService DICOMService)
        {
            _servers = new Dictionary<ServerType, IDICOMServer>
            {
                { ServerType.QueryRetrieveServer, new DICOMServer(ServerType.QueryRetrieveServer, GetSetting(ServerType.QueryRetrieveServer + "AET"), GetSetting(ServerType.QueryRetrieveServer + "Host"), GetSetting(ServerType.QueryRetrieveServer + "Port")) },
                { ServerType.StoreServer, new DICOMServer(ServerType.StoreServer, GetSetting(ServerType.StoreServer + "AET"), GetSetting(ServerType.StoreServer + "Host"), GetSetting(ServerType.StoreServer + "Port")) }
            };
            _dicomEditorAET = GetSetting("DicomEditorAET");
            _DICOMService = DICOMService;
        }

        public void SetServer(IDICOMServer server)
        {
            _servers[server.Type] = server;
            SetSetting(server.Type + nameof(server.AET), server.AET);
            SetSetting(server.Type + nameof(server.Host), server.Host);
            SetSetting(server.Type + nameof(server.Port), server.Port);
        }

        public IDICOMServer GetServer(ServerType type)
        {
            return _servers[type];
        }

        public async Task VerifyAsync(ServerType type)
        {
            IDICOMServer server = _servers[type];
            UpdateVerificationStatus(server, VerificationStatus.InProgress);
            try
            {
                bool successful = await _DICOMService.VerifyAsync(server.Host, Int32.Parse(server.Port), server.AET, DicomEditorAET);
                if(successful)
                {
                    UpdateVerificationStatus(server, VerificationStatus.Successful);
                } else
                {
                    UpdateVerificationStatus(server, VerificationStatus.Failed);
                }
            } catch(AggregateException)
            {
                UpdateVerificationStatus(server, VerificationStatus.Failed);
            }
        }

        private void SetSetting(string key, string value)
        {
            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");
            SettingsSavedEvent();

        }

        private string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private void UpdateVerificationStatus(IDICOMServer server, VerificationStatus status)
        {
            server.Status = status;
            UpdatedVerificationStatusEvent(server.Type);
        }
    }

    public class DICOMServer : IDICOMServer
    {
        public DICOMServer(ServerType type, string aet, string host, string port)
        {
            Type = type;
            AET = aet;
            Host = host;
            Port = port;
            Status = VerificationStatus.NA;
        }
        public ServerType Type { get; }
        public string AET { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public VerificationStatus Status { get; set; }
    }
}
