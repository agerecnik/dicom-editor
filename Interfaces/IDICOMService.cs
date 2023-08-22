using DicomEditor.Model;
using FellowOakDicom;
using FellowOakDicom.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Interfaces
{
    public interface IDICOMService
    {
        public Task<IList<DicomDataset>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, IList<Tuple<DicomTag, string>> attributes, DicomQueryRetrieveLevel level, CancellationToken cancellationToken);
        public Task<IList<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, string studyInstanceUID, string seriesInstanceUID, IProgress<int> progress, CancellationToken cancellationToken);
        public Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, IList<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken);
        public Task<bool> VerifyAsync(string serverHost, int serverPort, string serverAET, string appAET);
    }
}
