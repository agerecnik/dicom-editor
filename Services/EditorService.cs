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

        public IDictionary<string, Series> GetLoadedSeries()
        {
            return _cache.LoadedSeries;
        }

        public ITreeModel GetInstance(string instanceUID)
        {
            if(_cache.LoadedInstances.TryGetValue(instanceUID, out DicomDataset dataset))
            {
                return DatasetTree.CreateTree(dataset);
            }
            else
            {
                throw new ArgumentException("Instance with the following UID does not exist: " + instanceUID);
            }
        }

        public void SetAttributeValue(IList<Instance> instances, IDatasetModel attribute, string value)
        {
            if(attribute.Tag is null)
            {
                throw new ArgumentException("Invalid attribute tag");
            }
            if (attribute.ValueRepresentation is "SQ")
            {
                throw new InvalidOperationException("Cannot assign value to sequence");
            }

            List<IDatasetModel> attributes = new();
            while (attribute is not null)
            {
                attributes.Insert(0, attribute);
                attribute = attribute.ParentDataset;
            }

            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    for (int i = 0; i < attributes.Count - 2; i += 2)
                    {
                        DicomSequence sequence = ds.GetSequence(attributes[i].Tag);
                        int itemIndex = int.Parse(attributes[i + 1].Value);
                        ds = sequence.Items[itemIndex];
                    }

                    DicomTag lastTag = attributes[^1].Tag;
                    ds.AddOrUpdate<string>(lastTag, value);

                    if (lastTag == DicomTag.SOPInstanceUID)
                    {
                        if (!_cache.LoadedInstances.TryAdd(value, ds))
                        {
                            ds.AddOrUpdate<string>(lastTag, instance.InstanceUID);
                            throw new ArgumentException("Instance with the following UID already exists: " + instance.InstanceUID);
                        }
                        _cache.LoadedInstances.Remove(instance.InstanceUID);
                        instance.InstanceUID = value;
                    }
                    else if (lastTag == DicomTag.SeriesInstanceUID)
                    {
                        if (_cache.LoadedSeries.TryGetValue(instance.SeriesUID, out Series series))
                        {
                            series.Instances.Remove(instance);
                            if (!_cache.LoadedSeries.TryGetValue(value, out Series newSeries))
                            {
                                newSeries = new(value, series.SeriesDescription, series.SeriesDateTime, series.Modality, series.NumberOfInstances, series.StudyUID, new List<Instance>());
                                _cache.LoadedSeries[value] = newSeries;
                            }
                            newSeries.Instances.Add(instance);
                        }
                        if (series.Instances.Count == 0)
                        {
                            _cache.LoadedSeries.Remove(series.SeriesUID);
                        }
                        instance.SeriesUID = value;
                    }
                }
                else
                {
                    throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                }
            }
        }

        public void GenerateAndSetStudyUID(IList<Instance> instances)
        {
            // TODO: add option for root UID in settings service and add it to the automatically generated UID
            // TODO: add check for empty or null root UID
            string generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    ds.AddOrUpdate<string>(DicomTag.StudyInstanceUID, generatedUID);
                }
                else
                {
                    throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                }
            }
        }

        public void GenerateAndSetSeriesUID(IList<Instance> instances)
        {
            // TODO: add option for root UID in settings service and add it to the automatically generated UID
            // TODO: add check for empty or null root UID
            string generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    ds.AddOrUpdate<string>(DicomTag.SeriesInstanceUID, generatedUID);
                    if(_cache.LoadedSeries.TryGetValue(instance.SeriesUID, out Series series)) {
                        series.Instances.Remove(instance);
                        if(!_cache.LoadedSeries.TryGetValue(generatedUID, out Series newSeries))
                        {
                            newSeries = new(generatedUID, series.SeriesDescription, series.SeriesDateTime, series.Modality, series.NumberOfInstances, series.StudyUID, new List<Instance>());
                            _cache.LoadedSeries[generatedUID] = newSeries;
                        }
                        newSeries.Instances.Add(instance);
                    }
                    if(series.Instances.Count == 0)
                    {
                        _cache.LoadedSeries.Remove(series.SeriesUID);
                    }
                    instance.SeriesUID = generatedUID;
                }
                else
                {
                    throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                }
            }
        }

        public void GenerateAndSetInstanceUID(IList<Instance> instances)
        {
            foreach (Instance instance in instances)
            {
                // TODO: add option for root UID in settings service and add it to the automatically generated UID
                // TODO: add check for empty or null root UID
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    string generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
                    ds.AddOrUpdate<string>(DicomTag.SOPInstanceUID, generatedUID);
                    if (!_cache.LoadedInstances.TryAdd(generatedUID, ds))
                    {
                        ds.AddOrUpdate<string>(DicomTag.SOPInstanceUID, instance.InstanceUID);
                        throw new ArgumentException("Instance with the following UID already exists: " + instance.InstanceUID);
                    }
                    _cache.LoadedInstances.Remove(instance.InstanceUID);
                    instance.InstanceUID = generatedUID;
                }
                else
                {
                    throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                }
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
                    else
                    {
                        throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                    }
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
                    else
                    {
                        throw new ArgumentException("Instance with the following UID does not exist: " + instance.InstanceUID);
                    }

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
