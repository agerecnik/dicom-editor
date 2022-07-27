using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model.Interfaces
{
    public interface ISettingsService
    {
        enum ServerType
        {
            QueryRetrieve,
            Store
        }
        enum VerificationStatus
        {
            Successful,
            Failed,
            NA
        }

        public string QueryRetrieveServerHost { get; set; }
        public string QueryRetrieveServerPort { get; set; }
        public string QueryRetrieveServerAET { get; set; }
        public string StoreServerHost { get; set; }
        public string StoreServerPort { get; set; }
        public string StoreServerAET { get; set; }
        public string DicomEditorAET { get; set; }
        public VerificationStatus QueryRetrieveServerVerificationStatus { get; set; }
        public VerificationStatus StoreServerVerificationStatus { get; set; }

        public Task VerifyAsync(ServerType serverType);
    }
}
