using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public IDictionary<string, Patient> QueryResult { get; set; }
        public string LocalImportPath { get; set; }

        public ImportService(ISettingsService settingsService, ICache cache, IDICOMService DICOMService)
        {
            _settingsService = settingsService;
            _settingsService.SettingsSavedEvent += HandleSettingsSaved;
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

            QueryResult = await _DICOMService.QueryAsync(serverHost, serverPort, serverAET, appAET, PatientID, PatientName, AccessionNumber, StudyID, Modality, cancellationToken);
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

                IList<DicomDataset> retrievedDataset = await _DICOMService.RetrieveAsync(serverHost, serverPort, serverAET, appAET, series, progress, cancellationToken);
                List<Instance> instances = new();
                foreach (DicomDataset dataset in retrievedDataset)
                {
                    string instanceUID = dataset?.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "");
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
                    throw new FileFormatException("File format must be .dcm: " + path);
                }
                filePaths = new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                filePaths = Directory.GetFiles(path, "*.dcm", SearchOption.TopDirectoryOnly);
                if (filePaths.Length == 0)
                {
                    throw new FileNotFoundException("There are no .dcm files in " + path);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Path does not exist: " + path);
            }

            int progressCounter = 0;
            Dictionary<string, Series> importedSeries = new();
            Dictionary<string, DicomDataset> importedInstances = new();

            foreach (string filePath in filePaths)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var file = await DicomFile.OpenAsync(filePath);
                DicomDataset dataset = file.Dataset;

                string instanceUID = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "");
                // TODO: tryAdd?
                importedInstances.Add(instanceUID, dataset);

                if (progress != null)
                {
                    progressCounter++;
                    progress.Report(progressCounter);
                }
            }

            foreach (DicomDataset dataset in importedInstances.Values)
            {
                string instanceUID = dataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, "");
                string seriesUID = dataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "");
                string instanceNumber = dataset.GetSingleValueOrDefault(DicomTag.InstanceNumber, instanceUID);

                Instance instance = new(instanceUID, seriesUID, instanceNumber);
                if (!importedSeries.TryGetValue(seriesUID, out Series series))
                {
                    string seriesDescription = dataset.GetSingleValueOrDefault(DicomTag.SeriesDescription, "");
                    DateTime seriesDate = dataset.GetSingleValueOrDefault(DicomTag.SeriesDate, new DateTime());
                    DateTime seriesTime = dataset.GetSingleValueOrDefault(DicomTag.SeriesTime, new DateTime());
                    DateTime seriesDateTime = seriesDate;
                    seriesDateTime.AddHours(seriesTime.Hour);
                    seriesDateTime.AddMinutes(seriesTime.Minute);
                    string modality = dataset.GetSingleValueOrDefault(DicomTag.Modality, "");
                    string studyUID = dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, "");

                    int numberOfInstances = 0;
                    foreach (DicomDataset ds in importedInstances.Values)
                    {
                        if (ds.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, "") == seriesUID)
                        {
                            numberOfInstances++;
                        }
                    }

                    series = new(seriesUID, seriesDescription, seriesDateTime, modality, numberOfInstances, studyUID, new List<Instance>());
                    series.Instances.Add(instance);
                    importedSeries.Add(seriesUID, series);
                }
                else
                {
                    series.Instances.Add(instance);
                }
            }

            _cache.LoadedSeries = importedSeries;
            _cache.LoadedInstances = importedInstances;
        }

        private void HandleSettingsSaved()
        {
            QueryResult = new Dictionary<string, Patient>();
        }
    }
}
