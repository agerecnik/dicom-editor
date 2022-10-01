using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Model.Interfaces
{
    public interface IDICOMService
    {
        public Task<Dictionary<string, Patient>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, string patientID, string patientName, string accessionNumber, string studyID, string modality, CancellationToken cancellationToken);
        public Task<List<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, Series series, IProgress<int> progress, CancellationToken cancellationToken);
        public Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, List<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken);
        public Task<bool> VerifyAsync(string serverHost, int serverPort, string serverAET, string appAET);
    }
}
