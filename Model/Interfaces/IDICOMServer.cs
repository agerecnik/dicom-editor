using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DicomEditor.Model.Interfaces.ISettingsService;

namespace DicomEditor.Model
{
    public interface IDICOMServer
    {
        enum ServerType
        {
            QueryRetrieveServer,
            StoreServer
        }
        enum VerificationStatus
        {
            Successful,
            Failed,
            InProgress,
            NA
        }

        public ServerType Type { get; }
        public string AET { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public VerificationStatus Status { get; set; }
    }
}
