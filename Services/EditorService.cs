using DicomEditor.Interfaces;
using DicomEditor.Model;
using DicomEditor.Model.EditorModel.Tree;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.Services
{
    public class EditorService : IEditorService
    {
        private readonly ISettingsService _settingsService;
        private readonly ICache _cache;
        private readonly IDICOMService _DICOMService;

        public string LocalExportPath { get; set; }

        public EditorService(ISettingsService settingsService, ICache cache, IDICOMService DICOMService)
        {
            _settingsService = settingsService;
            _cache = cache;
            _DICOMService = DICOMService;
        }

        public IList<Series> GetLoadedSeries()
        {
            return _cache.LoadedSeries;
        }

        public ITreeModel GetInstance(string instanceUID)
        {
            _cache.LoadedInstances.TryGetValue(instanceUID, out DicomDataset dataset);
            return DatasetTree.CreateTree(dataset);
        }

        public void SetAttributeValue(string instanceUID, IDatasetModel attribute, string value)
        {
            if (_cache.LoadedInstances.TryGetValue(instanceUID, out DicomDataset instance))
            {
                Stack<IDatasetModel> attributes = new();
                while (attribute is not null)
                {
                    attributes.Push(attribute);
                    attribute = attribute.ParentDataset;
                }

                while(attributes.Count > 1)
                {
                    attribute = attributes.Pop();
                    DicomSequence sequence = instance.GetSequence(attribute.Tag);
                    attribute = attributes.Pop();
                    int itemIndex = int.Parse(attribute.Value);
                    instance = sequence.Items[itemIndex];
                }

                DicomTag lastTag = attributes.Pop().Tag;
                instance.AddOrUpdate<string>(lastTag, value);
            }
        }

        public async Task StoreAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken)
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

                await _DICOMService.StoreAsync(serverHost, serverPort, serverAET, appAET, instances, progress, cancellationToken);
            }
        }

        public async Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (File.Exists(path))
            {
                throw new DirectoryNotFoundException("Invalid directory path: " + path);
            }

            Directory.CreateDirectory(path);

            int progressCounter = 0;

            foreach (Series series in seriesList)
            {
                foreach (Instance instance in series.Instances)
                {
                    if(cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                    {
                        string filePath = path;
                        if (path[^1] is '\\')
                        {
                            string fileName = progressCounter + ".dcm";
                            filePath += fileName;
                        }
                        else
                        {
                            string fileName = @"\" + progressCounter + ".dcm";
                            filePath += fileName;
                        }

                        DicomFile file = new(ds);
                        await file.SaveAsync(filePath, FellowOakDicom.IO.Writer.DicomWriteOptions.Default);

                    }
                    // TODO: throw exception if instance does not exist?

                    if (progress != null)
                    {
                        progressCounter++;
                        progress.Report(progressCounter);
                    }
                }
            }
        }
    }
}
