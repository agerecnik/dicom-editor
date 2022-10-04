using DicomEditor.Model;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Interfaces
{
    public interface IDICOMService
    {
        public Task<IDictionary<string, Patient>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, string patientID, string patientName, string accessionNumber, string studyID, string modality, CancellationToken cancellationToken);
        public Task<IList<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, Series series, IProgress<int> progress, CancellationToken cancellationToken);
        public Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, IList<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken);
        public Task<bool> VerifyAsync(string serverHost, int serverPort, string serverAET, string appAET);
    }
}
