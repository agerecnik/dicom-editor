using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom;
using FellowOakDicom.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.Services
{
    public class ImportService : IImportService
    {
        private readonly ISettingsService _settingsService;
        private readonly ICache _cache;
        private readonly IDICOMService _DICOMService;

        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string AccessionNumber { get; set; }
        public string StudyID { get; set; }
        public string Modality { get; set; }
        public ICollection<Patient> QueryResult { get; set; }
        public string LocalImportPath { get; set; }

        public ImportService(ISettingsService settingsService, ICache cache, IDICOMService DICOMService)
        {
            _settingsService = settingsService;
            WeakEventManager<ISettingsService, EventArgs>.AddHandler(_settingsService, "SettingsSavedEvent", HandleSettingsSaved);
            _cache = cache;
            _DICOMService = DICOMService;
        }

        public async Task QueryAsync(CancellationToken cancellationToken)
        {
            string serverHost = _settingsService.GetServer(ServerType.QueryRetrieveServer).Host;
            int serverPort = 0;
            if (int.TryParse(_settingsService.GetServer(ServerType.QueryRetrieveServer).Port, out int port))
            {
                serverPort = port;
            }
            string serverAET = _settingsService.GetServer(ServerType.QueryRetrieveServer).AET;
            string appAET = _settingsService.DicomEditorAET;

            var attributes = new List<Tuple<DicomTag, string>>
            {
                Tuple.Create(DicomTag.PatientID, PatientID),
                Tuple.Create(DicomTag.PatientName, PatientName),
                Tuple.Create(DicomTag.AccessionNumber, AccessionNumber),
                Tuple.Create(DicomTag.StudyID, StudyID),
                Tuple.Create(DicomTag.Modality, Modality),
                Tuple.Create(DicomTag.PatientBirthDate, string.Empty),
                Tuple.Create(DicomTag.PatientSex, string.Empty),
                Tuple.Create(DicomTag.StudyInstanceUID, string.Empty),
                Tuple.Create(DicomTag.StudyDescription, string.Empty),
                Tuple.Create(DicomTag.StudyDate, string.Empty),
                Tuple.Create(DicomTag.StudyTime, string.Empty),
            };

            var studies = await _DICOMService.QueryAsync(serverHost, serverPort, serverAET, appAET, attributes, DicomQueryRetrieveLevel.Study, cancellationToken);

            Dictionary<string, Patient> patients = new();

            foreach(var dataset in studies)
            {
                string studyUID = dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID);
                if (!string.IsNullOrWhiteSpace(studyUID))
                {
                    string patientIDResult = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
                    string patientNameResult = dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
                    string dateOfBirth = dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
                    string sex = dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);

                    if (!patients.ContainsKey(patientIDResult))
                    {
                        Patient patient = new(patientIDResult, patientNameResult, dateOfBirth, sex);
                        patients.Add(patientIDResult, patient);
                    }

                    string accessionNumberStudy = dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty);
                    string studyDescription = dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, "No study description");
                    DateTime studyDate = dataset.GetSingleValueOrDefault(DicomTag.StudyDate, new DateTime());
                    DateTime studyTime = dataset.GetSingleValueOrDefault(DicomTag.StudyTime, new DateTime());
                    string modalitiesInStudy = dataset.TryGetString(DicomTag.ModalitiesInStudy, out var dummy) ? dummy : string.Empty;
                    DateTime studyDateTime = studyDate;
                    studyDateTime.AddHours(studyTime.Hour);
                    studyDateTime.AddMinutes(studyTime.Minute);

                    if (!patients[patientIDResult].Studies.ContainsKey(studyUID))
                    {
                        Study study = new(studyUID, accessionNumberStudy, studyDescription, studyDateTime, modalitiesInStudy);
                        patients[patientIDResult].Studies.Add(studyUID, study);
                    }
                }
            }

            foreach (Patient patient in patients.Values)
            {
                foreach (Study study in patient.Studies.Values)
                {
                    if (!string.IsNullOrWhiteSpace(study.StudyUID))
                    {
                        attributes = new List<Tuple<DicomTag, string>>
                        {
                            Tuple.Create(DicomTag.PatientID, patient.PatientID),
                            Tuple.Create(DicomTag.StudyInstanceUID, study.StudyUID),
                            Tuple.Create(DicomTag.SeriesInstanceUID, string.Empty),
                            Tuple.Create(DicomTag.SeriesInstanceUID, string.Empty),
                            Tuple.Create(DicomTag.SeriesDescription, string.Empty),
                            Tuple.Create(DicomTag.SeriesDate, string.Empty),
                            Tuple.Create(DicomTag.SeriesTime, string.Empty),
                            Tuple.Create(DicomTag.Modality, string.Empty),
                            Tuple.Create(DicomTag.NumberOfSeriesRelatedInstances, string.Empty)
                        };

                        var seriesList = await _DICOMService.QueryAsync(serverHost, serverPort, serverAET, appAET, attributes, DicomQueryRetrieveLevel.Series, cancellationToken);

                        foreach(var dataset in seriesList)
                        {
                            string patientIDResult = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
                            string seriesInstanceUID = dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID);
                            if (!string.IsNullOrWhiteSpace(seriesInstanceUID))
                            {
                                string seriesDescription = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, "No series description");
                                string modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);

                                DateTime seriesDate = dataset.GetSingleValueOrDefault(DicomTag.SeriesDate, new DateTime());
                                DateTime seriesTime = dataset.GetSingleValueOrDefault(DicomTag.SeriesTime, new DateTime());
                                int numberOfInstances = dataset.GetSingleValueOrDefault(DicomTag.NumberOfSeriesRelatedInstances, 0);
                                DateTime seriesDateTime = seriesDate;
                                seriesDateTime.AddHours(seriesTime.Hour);
                                seriesDateTime.AddMinutes(seriesTime.Minute);

                                Series series = new(seriesInstanceUID, seriesDescription, seriesDateTime, modality, numberOfInstances, study.StudyUID, patient.PatientID, new List<Instance>());
                                patients[patientIDResult].Studies[study.StudyUID].Series.Add(seriesInstanceUID, series);
                            }
                        };
                    }
                }
            }

            QueryResult = patients.Values;
        }

        public async Task RetrieveAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string serverHost = _settingsService.GetServer(ServerType.QueryRetrieveServer).Host;
            int serverPort = 0;
            if (int.TryParse(_settingsService.GetServer(ServerType.QueryRetrieveServer).Port, out int port))
            {
                serverPort = port;
            }
            string serverAET = _settingsService.GetServer(ServerType.QueryRetrieveServer).AET;
            string appAET = _settingsService.DicomEditorAET;

            Dictionary<string, Series> retrievedSeries = new();
            Dictionary<string, DicomDataset> retrievedInstances = new();

            foreach (Series series in seriesList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                IList<DicomDataset> retrievedDataset = await _DICOMService.RetrieveAsync(serverHost, serverPort, serverAET, appAET, series.StudyUID, series.SeriesUID, progress, cancellationToken);
                List<Instance> instances = new();
                foreach (DicomDataset dataset in retrievedDataset)
                {
                    string instanceUID = dataset?.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);
                    string instanceNumber = dataset?.GetSingleValueOrDefault(DicomTag.InstanceNumber, instanceUID);
                    Instance instance = new(instanceUID, series.SeriesUID, instanceNumber);
                    instances.Add(instance);
                    retrievedInstances.Add(instanceUID, dataset);
                }
                ObservableCollection<Instance> orderedInstances = new (instances.OrderBy(x => x.InstanceNumber.Length).ThenBy(x => x.InstanceNumber));
                series.Instances = orderedInstances;
                retrievedSeries.Add(series.SeriesUID, series);
            }

            _cache.LoadedSeries = retrievedSeries;
            _cache.LoadedInstances = retrievedInstances;
        }

        public async Task LocalImportAsync(string path, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string[] filePaths;
            if (File.Exists(path))
            {
                if (Path.GetExtension(path) != ".dcm")
                {
                    throw new FileFormatException(string.Join(" ", "File format must be .dcm:", path));
                }
                filePaths = new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                filePaths = Directory.GetFiles(path, "*.dcm", SearchOption.TopDirectoryOnly);
                if (filePaths.Length == 0)
                {
                    throw new FileNotFoundException(string.Join(" ", "There are no .dcm files in", path));
                }
            }
            else
            {
                throw new DirectoryNotFoundException(string.Join(" ", "Path does not exist:", path));
            }

            int progressCounter = 0;
            Dictionary<string, Series> importedSeries = new();
            Dictionary<string, DicomDataset> importedInstances = new();

            await Task.Run(async () =>
            {
                foreach (string filePath in filePaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var file = await DicomFile.OpenAsync(filePath);
                    DicomDataset dataset = file.Dataset;

                    string instanceUID = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);
                    importedInstances.Add(instanceUID, dataset);

                    if (progress != null)
                    {
                        progressCounter++;
                        progress.Report(progressCounter);
                    }
                }
            

                foreach (DicomDataset dataset in importedInstances.Values)
                {
                    string instanceUID = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty);
                    string seriesUID = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty);
                    string instanceNumber = dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, instanceUID);

                    Instance instance = new(instanceUID, seriesUID, instanceNumber);
                    if (!importedSeries.TryGetValue(seriesUID, out Series series))
                    {
                        string seriesDescription = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, "No series description");
                        DateTime seriesDate = dataset.GetSingleValueOrDefault(DicomTag.SeriesDate, new DateTime());
                        DateTime seriesTime = dataset.GetSingleValueOrDefault(DicomTag.SeriesTime, new DateTime());
                        DateTime seriesDateTime = seriesDate;
                        seriesDateTime.AddHours(seriesTime.Hour);
                        seriesDateTime.AddMinutes(seriesTime.Minute);
                        string modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, string.Empty);
                        string studyUID = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
                        string patientID = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);

                        int numberOfInstances = 0;
                        foreach (DicomDataset ds in importedInstances.Values)
                        {
                            if (ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty) == seriesUID)
                            {
                                numberOfInstances++;
                            }
                        }

                        series = new(seriesUID, seriesDescription, seriesDateTime, modality, numberOfInstances, studyUID, patientID, new List<Instance>());
                        series.Instances.Add(instance);
                        importedSeries.Add(seriesUID, series);
                    }
                    else
                    {
                        series.Instances.Add(instance);
                    }
                }

                foreach (Series s in importedSeries.Values)
                {
                    ObservableCollection<Instance> orderedInstances = new(s.Instances.OrderBy(x => x.InstanceNumber.Length).ThenBy(x => x.InstanceNumber));
                    s.Instances = orderedInstances;
                }
            }, cancellationToken);

            _cache.LoadedSeries = importedSeries;
            _cache.LoadedInstances = importedInstances;
        }

        private void HandleSettingsSaved(object source, EventArgs args)
        {
            QueryResult = new Collection<Patient>();
        }
    }
}
