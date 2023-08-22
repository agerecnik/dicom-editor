using DicomEditor.Interfaces;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Services
{
    public class DICOMService : IDICOMService
    {
        public async Task<IList<DicomDataset>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, IList<Tuple<DicomTag, string>> attributes, DicomQueryRetrieveLevel level, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();
            var request = CreateCFindRequest(attributes, level);
            List<DicomDataset> datasets = new();
            request.OnResponseReceived += (req, response) =>
            {
                if (response.HasDataset)
                {
                    datasets.Add(response.Dataset);
                }
            };
            await client.AddRequestAsync(request);
            await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
            return datasets;
        }

        public async Task<IList<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, string studyInstanceUID, string seriesInstanceUID, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            var cGetRequest = new DicomCGetRequest(studyInstanceUID, seriesInstanceUID);
            List<DicomDataset> retrievedSeries = new();

            int progressCounter = 0;
            client.OnCStoreRequest += (req) =>
            {
                retrievedSeries.Add(req.Dataset);
                if (progress != null)
                {
                    progressCounter++;
                    progress.Report(progressCounter);
                }
                return Task.FromResult(new DicomCStoreResponse(req, DicomStatus.Success));
            };

            ISet<string> sopClassUIDs = await RetrieveSOPClassUIDsAsync(client, studyInstanceUID, seriesInstanceUID);
            client.AdditionalPresentationContexts.Clear();
            foreach (string sopClassUID in sopClassUIDs)
            {
                var pc = DicomPresentationContext.GetScpRolePresentationContext(
                    DicomUID.Parse(sopClassUID),
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRBigEndian);
                client.AdditionalPresentationContexts.Add(pc);
            }

            await client.AddRequestAsync(cGetRequest);
            await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
            return retrievedSeries;
        }

        public async Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, IList<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();
            int progressCounter = 0;
            foreach (DicomDataset instance in series)
            {
                if (cancellationToken.IsCancellationRequested)
                {
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
                };
                await client.AddRequestAsync(request);
                await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
            }
        }

        public async Task<bool> VerifyAsync(string serverHost, int serverPort, string serverAET, string appAET)
        {
            bool successful = false;
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();
            var request = new DicomCEchoRequest();
            request.OnResponseReceived += (req, res) =>
            {
                if (res.Status.State == DicomState.Success)
                {
                    successful = true;
                }
            };
            await client.AddRequestAsync(request);
            await client.SendAsync();
            return successful;
        }

        private DicomCFindRequest CreateCFindRequest(IEnumerable<Tuple<DicomTag, string>> attributes, DicomQueryRetrieveLevel level)
        {
            var request = new DicomCFindRequest(level);
            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");
            foreach (var attribute in attributes)
            {
                request.Dataset.AddOrUpdate(attribute.Item1, attribute.Item2);
            }
            return request;
        }

        private async Task<ISet<string>> RetrieveSOPClassUIDsAsync(IDicomClient client, string studyInstanceUID, string seriesInstanceUID)
        {
            HashSet<string> sopClassUIDs = new();
            if (!string.IsNullOrEmpty(seriesInstanceUID))
            {
                var attributes = new List<Tuple<DicomTag, string>>
                {
                    Tuple.Create(DicomTag.StudyInstanceUID, studyInstanceUID),
                    Tuple.Create(DicomTag.SeriesInstanceUID, seriesInstanceUID),
                    Tuple.Create(DicomTag.SOPClassUID, string.Empty),
                };
                var request = CreateCFindRequest(attributes, DicomQueryRetrieveLevel.Image);
                request.OnResponseReceived += (req, response) =>
                {
                    string sopClassUID = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.SOPClassUID, null);
                    if (!string.IsNullOrEmpty(sopClassUID))
                    {
                        sopClassUIDs.Add(sopClassUID);
                    }
                };
                await client.AddRequestAsync(request);
                await client.SendAsync();
            }
            return sopClassUIDs;
        }
    }
}
