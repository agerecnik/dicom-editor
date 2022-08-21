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
    public static class DicomQueryRetrieveService
    {
        public static async Task<Dictionary<string, Patient>> QueryAsync(string serverHost, int serverPort, string serverAET, string appAET, string patietID, string patientName, string accessionNumber, string studyID, string modality)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();

            // Find a list of Studies

            var request = CreateStudyRequest(patietID, patientName, accessionNumber, studyID, modality);

            Dictionary<string, Patient> patients = new();
            var studyUIDs = new List<string>();
            request.OnResponseReceived += (req, response) =>
            {
                DebugStudyResponse(response);
                string studyUID = response.Dataset?.GetSingleValue<string>(DicomTag.StudyInstanceUID);
                if (studyUID is not null and not "")
                {
                    studyUIDs.Add(studyUID);

                    string patientIDResult = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.PatientID, "");
                    string patientNameResult = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.PatientName, "");
                    string dateOfBirth = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.PatientBirthDate, "");
                    string sex = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.PatientSex, "");

                    if (!patients.ContainsKey(patientIDResult))
                    {
                        Patient patient = new(patientIDResult, patientNameResult, dateOfBirth, sex);
                        patients.Add(patientIDResult, patient);
                    }

                    string accessionNumberStudy = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.AccessionNumber, "");
                    string studyDescription = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.StudyDescription, "");
                    DateTime studyDate = response.Dataset.GetSingleValueOrDefault<DateTime>(DicomTag.StudyDate, new DateTime());
                    DateTime studyTime = response.Dataset.GetSingleValueOrDefault<DateTime>(DicomTag.StudyTime, new DateTime());
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
            await client.SendAsync();

            // find all series from a study that previously was returned
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
                            string patientIDResult = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.PatientID, "");
                            string seriesDescription = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.SeriesDescription, "");
                            string modality = response.Dataset?.GetSingleValueOrDefault<string>(DicomTag.Modality, "");

                            DateTime seriesDate;
                            DateTime seriesTime;
                            int numberOfInstances;
                            if (response.Dataset is not null)
                            {
                                seriesDate = response.Dataset.GetSingleValueOrDefault<DateTime>(DicomTag.SeriesDate, new DateTime());
                                seriesTime = response.Dataset.GetSingleValueOrDefault<DateTime>(DicomTag.SeriesTime, new DateTime());
                                numberOfInstances = response.Dataset.GetSingleValueOrDefault<int>(DicomTag.NumberOfSeriesRelatedInstances, 0);
                            } else
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
                    await client.SendAsync();
                }
            }

            return patients;
        }

        public static async Task<List<DicomDataset>> RetrieveAsync(string serverHost, int serverPort, string serverAET, string appAET, Series series, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            var cGetRequest = CreateCGetBySeriesUID(series.StudyUID, series.SeriesUID);
            List<DicomDataset> retrievedSeries = new();

            int progressCounter = 0;
            client.OnCStoreRequest += (DicomCStoreRequest req) =>
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

        public static DicomCGetRequest CreateCGetBySeriesUID(string studyUID, string seriesUID)
        {
            var request = new DicomCGetRequest(studyUID, seriesUID);
            // no more dicomtags have to be set
            return request;
        }

        public static DicomCFindRequest CreateStudyRequest(string patientID, string patientName, string accessionNumber, string studyID, string modality)
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

        public static DicomCFindRequest CreateSeriesRequestByStudyUID(string studyInstanceUID)
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

        public static DicomCFindRequest CreateImageRequestBySeriesUID(string seriesInstanceUID)
        {
            var request = new DicomCFindRequest(DicomQueryRetrieveLevel.Image);

            request.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

            // add the dicom tags with empty values that should be included in the result
            request.Dataset.AddOrUpdate(DicomTag.SOPClassUID, "");

            // add the dicom tags that contain the filter criterias
            request.Dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, seriesInstanceUID);

            return request;
        }

        private static async Task<HashSet<string>> RetrieveSOPClassUIDsAsync(IDicomClient client, string seriesUID)
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

        public static void DebugStudyResponse(DicomCFindResponse response)
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


        public static void DebugSerieResponse(DicomCFindResponse response)
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
