using DicomEditor.Model.Interfaces;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DicomEditor.Model.IDICOMServer;

namespace DicomEditor.Model.Services
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
        public Dictionary<string, Patient> QueryResult { get; set; }
        public string LocalImportPath { get; set; }

        public ImportService(ISettingsService settingsService, ICache cache, IDICOMService DICOMService)
        {
            _settingsService = settingsService;
            _settingsService.SettingsSavedEvent += new SettingsSavedHandler(HandleSettingsSaved);
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

        public async Task RetrieveAsync(List<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string serverHost = _settingsService.GetServer(ServerType.QueryRetrieveServer).Host;
            int serverPort = 0;
            if (int.TryParse(_settingsService.GetServer(ServerType.QueryRetrieveServer).Port, out int port))
            {
                serverPort = port;
            }
            string serverAET = _settingsService.GetServer(ServerType.QueryRetrieveServer).AET;
            string appAET = _settingsService.DicomEditorAET;

            List<Series> retrievedSeries = new();
            Dictionary<string, DicomDataset> retrievedInstances = new();

            foreach (Series series in seriesList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                List<DicomDataset> retrievedDataset = await _DICOMService.RetrieveAsync(serverHost, serverPort, serverAET, appAET, series, progress, cancellationToken);
                List<Instance> instances = new();
                foreach (DicomDataset dataset in retrievedDataset)
                {
                    string instanceUID = dataset?.GetSingleValueOrDefault<string>(DicomTag.SOPInstanceUID, "");
                    Instance instance = new(instanceUID, series.SeriesUID);
                    instances.Add(instance);
                    retrievedInstances.Add(instanceUID, dataset);
                }
                instances = instances.OrderBy(x => x.InstanceUID.Length).ThenBy(x => x.InstanceUID).ToList();
                foreach (Instance instance in instances)
                {
                    series.Instances.Add(instance);
                }
                retrievedSeries.Add(series);
            }

            _cache.LoadedSeries = retrievedSeries;
            _cache.LoadedInstances = retrievedInstances;
        }

        public async Task LocalImportAsync(string path, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string[] filePaths;
            if (File.Exists(path))
            {
                if(Path.GetExtension(path) != ".dcm")
                {
                    throw new FileFormatException("File format must be .dcm: " + path);
                }
                filePaths = new string[] {path};
            }
            else if(Directory.Exists(path))
            {
                filePaths = Directory.GetFiles(path, "*.dcm", SearchOption.TopDirectoryOnly);
                if(filePaths.Length == 0)
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

            foreach(string filePath in filePaths)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var file = await DicomFile.OpenAsync(filePath);
                DicomDataset dataset = file.Dataset;

                string instanceUID = dataset.GetSingleValueOrDefault<string>(DicomTag.SOPInstanceUID, "");
                // TODO: tryAdd?
                importedInstances.Add(instanceUID, dataset);

                if (progress != null)
                {
                    progressCounter++;
                    progress.Report(progressCounter);
                }
            }

            foreach(DicomDataset dataset in importedInstances.Values)
            {
                string instanceUID = dataset.GetSingleValueOrDefault<string>(DicomTag.SOPInstanceUID, "");
                string seriesUID = dataset.GetSingleValueOrDefault<string>(DicomTag.SeriesInstanceUID, "");

                Instance instance = new(instanceUID, seriesUID);
                Series series;
                if (!importedSeries.TryGetValue(seriesUID, out series))
                {
                    string seriesDescription = dataset.GetSingleValueOrDefault<string>(DicomTag.SeriesDescription, "");
                    DateTime seriesDate = dataset.GetSingleValueOrDefault<DateTime>(DicomTag.SeriesDate, new DateTime());
                    DateTime seriesTime = dataset.GetSingleValueOrDefault<DateTime>(DicomTag.SeriesTime, new DateTime());
                    DateTime seriesDateTime = seriesDate;
                    seriesDateTime.AddHours(seriesTime.Hour);
                    seriesDateTime.AddMinutes(seriesTime.Minute);
                    string modality = dataset.GetSingleValueOrDefault<string>(DicomTag.Modality, "");
                    string studyUID = dataset.GetSingleValueOrDefault<string>(DicomTag.StudyInstanceUID, "");

                    int numberOfInstances = 0;
                    foreach (DicomDataset ds in importedInstances.Values)
                    {
                        if (ds.GetSingleValueOrDefault<string>(DicomTag.SeriesInstanceUID, "") == seriesUID)
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

            _cache.LoadedSeries = importedSeries.Values.ToList();
            _cache.LoadedInstances = importedInstances;
        }

        private void HandleSettingsSaved()
        {
            QueryResult = new();
        }
    }
}
