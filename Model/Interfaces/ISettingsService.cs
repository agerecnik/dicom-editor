using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DicomEditor.Model.IDICOMServer;

namespace DicomEditor.Model.Interfaces
{
    public delegate void UpdatedVerificationStatusHandler(ServerType type);
    public delegate void SettingsSavedHandler();

    public interface ISettingsService
    {
        event UpdatedVerificationStatusHandler UpdatedVerificationStatusEvent;
        event SettingsSavedHandler SettingsSavedEvent;

        public string DicomEditorAET { get; set; }

        public void SetServer(IDICOMServer server);
        public IDICOMServer GetServer(ServerType type);
        public Task VerifyAsync(ServerType serverType);
    }
}
