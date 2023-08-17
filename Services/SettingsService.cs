using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using DicomEditor.Interfaces;
using DicomEditor.ViewModel;
using FellowOakDicom.Network;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.Services
{
    public class SettingsService : ISettingsService
    {
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

        private string _dicomRoot;
        public string DicomRoot
        {
            get => _dicomRoot;
            set
            {
                _dicomRoot = value;
                SetSetting("DicomRoot", value);
            }
        }

        public SettingsService(IDICOMService DICOMService)
        {
            _servers = new Dictionary<ServerType, IDICOMServer>
            {
                { ServerType.QueryRetrieveServer, new DICOMServer(
                    ServerType.QueryRetrieveServer,
                    GetSetting(string.Join("", ServerType.QueryRetrieveServer, "AET")),
                    GetSetting(string.Join("", ServerType.QueryRetrieveServer, "Host")),
                    GetSetting(string.Join("", ServerType.QueryRetrieveServer, "Port")),
                    VerificationStatus.NA,
                    this)
                },
                { ServerType.StoreServer, new DICOMServer(
                    ServerType.StoreServer,
                    GetSetting(string.Join("", ServerType.StoreServer, "AET")),
                    GetSetting(string.Join("", ServerType.StoreServer, "Host")),
                    GetSetting(string.Join("", ServerType.StoreServer, "Port")),
                    VerificationStatus.NA,
                    this)
                }
            };
            _dicomEditorAET = GetSetting("DicomEditorAET");
            _dicomRoot = GetSetting("DicomRoot");
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
            server.Status = VerificationStatus.InProgress;
            try
            {
                bool successful = await _DICOMService.VerifyAsync(server.Host, int.Parse(server.Port), server.AET, DicomEditorAET);
                if (successful)
                {
                    server.Status = VerificationStatus.Successful;
                }
                else
                {
                    server.Status = VerificationStatus.Failed;
                }
            }
            catch (Exception e) when (e is AggregateException
            or DicomAssociationRequestTimedOutException
            or DicomAssociationRejectedException
            or DicomAssociationAbortedException
            or ConnectionClosedPrematurelyException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or ArgumentException)
            {
                server.Status = VerificationStatus.Failed;
            }
        }

        public void SetSetting(string key, string value)
        {
            Configuration configuration =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (configuration.AppSettings.Settings[key].Value != value)
            {
                configuration.AppSettings.Settings[key].Value = value;
                configuration.Save(ConfigurationSaveMode.Minimal, true);
                ConfigurationManager.RefreshSection("appSettings");
                SettingsSavedEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
