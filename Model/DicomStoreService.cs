using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public static class DicomStoreService
    {
        public static async Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, List<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();

            int progressCounter = 0;

            foreach (DicomDataset instance in series) {
                if (cancellationToken.IsCancellationRequested) {
                    break;
                }
                DicomFile file = new DicomFile(instance);
                var request = new DicomCStoreRequest(file, DicomPriority.Medium);
                request.OnResponseReceived += (req, response) =>
                {
                    if (progress != null && response.Status == DicomStatus.Success)
                    {
                        progressCounter++;
                        progress.Report(progressCounter);
                    }
                    Trace.WriteLine("C-Store Response Received, Status: " + response.Status);
                };
                await client.AddRequestAsync(request);
                await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
            }
        }
    }
}
