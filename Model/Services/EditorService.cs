using DicomEditor.Model.EditorModel.Tree;
using DicomEditor.Model.Interfaces;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DicomEditor.Model.IDICOMServer;

namespace DicomEditor.Model.Services
{
    public class EditorService : IEditorService
    {
        private ISettingsService _settingsService;
        private ICache _cache;

        public EditorService(ISettingsService settingsService, ICache cache)
        {
            _settingsService = settingsService;
            _cache = cache;
        }

        public List<Series> GetLoadedSeries()
        {
            return _cache.LoadedSeries;
        }

        public DatasetTree GetInstance(string instanceUID)
        {
            DicomDataset dataset = new();
            _cache.LoadedInstances.TryGetValue(instanceUID, out dataset);
            return DatasetTree.CreateTree(dataset);
        }

        public async Task StoreAsync(List<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken)
        {
            string serverHost = _settingsService.GetServer(ServerType.StoreServer).Host;
            int serverPort = 0;
            if (int.TryParse(_settingsService.GetServer(ServerType.StoreServer).Port, out int port))
            {
                serverPort = port;
            }
            string serverAET = _settingsService.GetServer(ServerType.StoreServer).AET;
            string appAET = _settingsService.DicomEditorAET;

            List<DicomDataset> instances = new();
            foreach (Series series in seriesList)
            {
                foreach (Instance instance in series.Instances)
                {
                    if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                    {
                        instances.Add(ds);
                    }
                    // TODO: throw exception if instance does not exist?
                }

                await DicomStoreService.StoreAsync(serverHost, serverPort, serverAET, appAET, instances, progress, cancellationToken);
            }
        }
    }
}
