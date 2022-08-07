using DicomEditor.Model.Interfaces;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static DicomEditor.Model.IDICOMServer;

namespace DicomEditor.Model.Services
{
    public class ImportService : IImportService
    {
        private ISettingsService _settingsService;
        private ICache _cache;

        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string AccessionNumber { get; set; }
        public string StudyID { get; set; }
        public string Modality { get; set; }
        public Dictionary<string, Patient> QueryResult { get; set; }

        public ImportService(ISettingsService settingsService, ICache cache)
        {
            _settingsService = settingsService;
            _settingsService.SettingsSavedEvent += new SettingsSavedHandler(HandleSettingsSaved);
            _cache = cache;
        }

        public async Task<Dictionary<string, Patient>> Query()
        {
            string serverHost = _settingsService.GetServer(ServerType.QueryRetrieveServer).Host;
            int serverPort = 0;
            if (int.TryParse(_settingsService.GetServer(ServerType.QueryRetrieveServer).Port, out int port))
            {
                serverPort = port;
            }
            string serverAET = _settingsService.GetServer(ServerType.QueryRetrieveServer).AET;
            string appAET = _settingsService.DicomEditorAET;

            Dictionary<string, Patient> queryResult = await DicomQueryRetrieveService.QueryAsync(serverHost, serverPort, serverAET, appAET, PatientID, PatientName, AccessionNumber, StudyID, Modality);
            QueryResult = queryResult;
            return queryResult;
        }

        //TODO progress bar
        public async void Retrieve(List<Series> seriesList, IProgress<int> progress)
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
            int totalCount = seriesList.Count;
            int tempCount = 0;

            foreach (Series series in seriesList)
            {
                List<DicomDataset> retrievedDataset = await DicomQueryRetrieveService.Retrieve(serverHost, serverPort, serverAET, appAET, series);
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
                tempCount++;
                if (progress != null)
                {
                    progress.Report(tempCount * 100 / totalCount);
                }
            }

            _cache.LoadedSeries = retrievedSeries;
            _cache.LoadedInstances = retrievedInstances;
        }

        private void HandleSettingsSaved()
        {
            QueryResult = new();
        }
    }
}
