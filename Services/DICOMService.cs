using DicomEditor.Interfaces;
using DicomEditor.Model;
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
        public async Task<Dictionary<string, Patient>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, string patientID, string patientName, string accessionNumber, string studyID, string modality, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();

            // Find a list of Studies

            var request = CreateStudyRequest(patientID, patientName, accessionNumber, studyID, modality);

            Dictionary<string, Patient> patients = new();
            var studyUIDs = new List<string>();
            request.OnResponseReceived += (req, response) =>
            {
                DebugStudyResponse(response);
                string studyUID = response.Dataset?.GetSingleValue<string>(DicomTag.StudyInstanceUID);
                if (studyUID is not null and not "")
                {
                    studyUIDs.Add(studyUID);

                    string patientIDResult = response.Dataset?.GetSingleValueOrDefault(DicomTag.PatientID, "");
                    string patientNameResult = response.Dataset?.GetSingleValueOrDefault(DicomTag.PatientName, "");
                    string dateOfBirth = response.Dataset?.GetSingleValueOrDefault(DicomTag.PatientBirthDate, "");
                    string sex = response.Dataset?.GetSingleValueOrDefault(DicomTag.PatientSex, "");

                    if (!patients.ContainsKey(patientIDResult))
                    {
                        Patient patient = new(patientIDResult, patientNameResult, dateOfBirth, sex);
                        patients.Add(patientIDResult, patient);
                    }

                    string accessionNumberStudy = response.Dataset?.GetSingleValueOrDefault(DicomTag.AccessionNumber, "");
                    string studyDescription = response.Dataset?.GetSingleValueOrDefault(DicomTag.StudyDescription, "");
                    DateTime studyDate = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, new DateTime());
                    DateTime studyTime = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyTime, new DateTime());
                    string modalitiesInStudy = response.Dataset.TryGetString(DicomTag.ModalitiesInStudy, out var dummy) ? dummy : string.Empty;
                    DateTime studyDateTime = studyDate;
                    studyDateTime.AddHours(studyTime.Hour);
                    studyDateTime.AddMinutes(studyTime.Minute);

                    if (!patients[patientIDResult].Studies.ContainsKey(studyUID))
                    {
                        Study study = new(studyUID, accessionNumberStudy, studyDescription, studyDateTime, modalitiesInStudy);
                        patients[patientIDResult].Studies.Add(studyUID, study);
                    }
                }

            };
            await client.AddRequestAsync(request);
            await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);

            // find all series from a study that was previously returned
            foreach (string studyUID in studyUIDs)
            {
                if (studyUID is not null and not "")
                {
                    request = CreateSeriesRequestByStudyUID(studyUID);
                    request.OnResponseReceived += (req, response) =>
                    {
                        string seriesInstanceUID = response.Dataset?.GetSingleValue<string>(DicomTag.SeriesInstanceUID);
                        if (seriesInstanceUID is not null and not "")
                        {
                            string patientIDResult = response.Dataset?.GetSingleValueOrDefault(DicomTag.PatientID, "");
                            string seriesDescription = response.Dataset?.GetSingleValueOrDefault(DicomTag.SeriesDescription, "");
                            string modality = response.Dataset?.GetSingleValueOrDefault(DicomTag.Modality, "");

                            DateTime seriesDate;
                            DateTime seriesTime;
                            int numberOfInstances;
                            if (response.Dataset is not null)
                            {
                                seriesDate = response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesDate, new DateTime());
                                seriesTime = response.Dataset.GetSingleValueOrDefault(DicomTag.SeriesTime, new DateTime());
                                numberOfInstances = response.Dataset.GetSingleValueOrDefault(DicomTag.NumberOfSeriesRelatedInstances, 0);
                            }
                            else
                            {
                                seriesDate = new();
                                seriesTime = new();
                                numberOfInstances = 0;
                            }

                            DateTime seriesDateTime = seriesDate;
                            seriesDateTime.AddHours(seriesTime.Hour);
                            seriesDateTime.AddMinutes(seriesTime.Minute);

                            Series series = new(seriesInstanceUID, seriesDescription, seriesDateTime, modality, numberOfInstances, studyUID, new List<Instance>());
                            patients[patientIDResult].Studies[studyUID].Series.Add(seriesInstanceUID, series);
                        }
                    };
                    await client.AddRequestAsync(request);
                    await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
                }
            }

            return patients;
        }

        public async Task<List<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, Series series, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            var cGetRequest = CreateCGetBySeriesUID(series.StudyUID, series.SeriesUID);
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

            HashSet<string> sopClassUIDs = await RetrieveSOPClassUIDsAsync(client, series.SeriesUID);
            foreach (string sopClassUID in sopClassUIDs)
            {
                var pc = DicomPresentationContext.GetScpRolePresentationContext(
                    DicomUID.Parse(sopClassUID),
                    DicomTransferSyntax.ExplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRLittleEndian,
                    DicomTransferSyntax.ImplicitVRBigEndian);
                client.AdditionalPresentationContexts.Clear();
                client.AdditionalPresentationContexts.Add(pc);
            }

            await client.AddRequestAsync(cGetRequest);
            await client.SendAsync(cancellationToken, DicomClientCancellationMode.ImmediatelyReleaseAssociation);
            return retrievedSeries;
        }

        public async Task StoreAsync(string serverHost, int serverPort, string serverAET, string appAET, List<DicomDataset> series, IProgress<int> progress, CancellationToken cancellationToken)
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

        private DicomCGetRequest CreateCGetBySeriesUID(string studyUID, string seriesUID)
        {
            var request = new DicomCGetRequest(studyUID, seriesUID);
            // no more dicomtags have to be set
            return request;
        }

        private DicomCFindRequest CreateStudyRequest(string patientID, string patientName, string accessionNumber, string studyID, string modality)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

            // always add the encoding
            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            // add the dicom tags with empty values that should be included in the result of the QR Server
            // add the dicom tags that contain the filter criterias
            request.Dataset.AddOrUpdate(DicomTag.PatientID, patientID);
            request.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);
            request.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, "");
            request.Dataset.AddOrUpdate(DicomTag.PatientSex, "");

            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.AccessionNumber, accessionNumber);
            request.Dataset.AddOrUpdate(DicomTag.StudyID, studyID);
            request.Dataset.AddOrUpdate(DicomTag.StudyDescription, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyTime, "");
            request.Dataset.AddOrUpdate(DicomTag.ModalitiesInStudy, modality);

            return request;
        }

        private DicomCFindRequest CreateSeriesRequestByStudyUID(string studyInstanceUID)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Series);

            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            // add the dicom tags with empty values that should be included in the result
            // add the dicom tags that contain the filter criterias
            request.Dataset.AddOrUpdate(DicomTag.PatientID, "");
            request.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyInstanceUID);
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesDescription, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesDate, "");
            request.Dataset.AddOrUpdate(DicomTag.SeriesTime, "");
            request.Dataset.AddOrUpdate(DicomTag.Modality, "");
            request.Dataset.AddOrUpdate(DicomTag.NumberOfSeriesRelatedInstances, "");

            return request;
        }

        private DicomCFindRequest CreateImageRequestBySeriesUID(string seriesInstanceUID)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);

            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            // add the dicom tags with empty values that should be included in the result
            request.Dataset.AddOrUpdate(DicomTag.SOPClassUID, "");

            // add the dicom tags that contain the filter criterias
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, seriesInstanceUID);

            return request;
        }

        private async Task<HashSet<string>> RetrieveSOPClassUIDsAsync(IDicomClient client, string seriesUID)
        {
            HashSet<string> sopClassUIDs = new();
            if (seriesUID is not null and not "")
            {
                var request = CreateImageRequestBySeriesUID(seriesUID);
                request.OnResponseReceived += (req, response) =>
                {
                    string sopClassUID = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.SOPClassUID, null);
                    if (sopClassUID is not null and not "")
                    {
                        sopClassUIDs.Add(sopClassUID);
                    }
                };
                await client.AddRequestAsync(request);
                await client.SendAsync();
            }
            return sopClassUIDs;
        }

        private void DebugStudyResponse(DicomCFindResponse response)
        {
            if (response.Status == DicomStatus.Pending)
            {
                // print the results
                Console.WriteLine($"Patient {response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)}, {(response.Dataset.TryGetString(DicomTag.ModalitiesInStudy, out var dummy) ? dummy : string.Empty)}-Study from {response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, new DateTime())} with UID {response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty)} ");
            }
            if (response.Status == DicomStatus.Success)
            {
                Console.WriteLine(response.Status.ToString());
            }
        }


        private void DebugSerieResponse(DicomCFindResponse response)
        {
            try
            {
                if (response.Status == DicomStatus.Pending)
                {
                    // print the results
                    Console.WriteLine($"Serie {response.Dataset.GetSingleValue<string>(DicomTag.SeriesDescription)}, {response.Dataset.GetSingleValue<string>(DicomTag.Modality)}, {response.Dataset.GetSingleValue<int>(DicomTag.NumberOfSeriesRelatedInstances)} instances");
                }
                if (response.Status == DicomStatus.Success)
                {
                    Console.WriteLine(response.Status.ToString());
                }
            }
            catch (Exception)
            {
                // ignore errors
            }
        }
    }
}
