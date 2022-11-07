using System.Threading.Tasks;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.Interfaces
{
    public delegate void SettingsSavedHandler();

    public interface ISettingsService
    {
        event SettingsSavedHandler SettingsSavedEvent;

        public string DicomEditorAET { get; set; }
        public string DicomRoot { get; set; }

        public void SetServer(IDICOMServer server);
        public IDICOMServer GetServer(ServerType type);
        public Task VerifyAsync(ServerType serverType);
        public void SetSetting(string key, string value);
        public string GetSetting(string key);
    }
}
