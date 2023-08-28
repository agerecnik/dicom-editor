using DicomEditor.Interfaces;
using DicomEditor.Model;
using DicomEditor.Model.EditorModel.Tree;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using static DicomEditor.Interfaces.IDICOMServer;

namespace DicomEditor.Services
{
    public class EditorService : IEditorService
    {
        public event EventHandler<SeriesListUpdatedEventArgs> SeriesListUpdatedEvent;
        public event EventHandler AttributesUpdatedEvent;

        private readonly ISettingsService _settingsService;
        private readonly ICache _cache;
        private readonly IDICOMService _DICOMService;
        private readonly Random _random;

        public string LocalExportPath { get; set; }

        public EditorService(ISettingsService settingsService, ICache cache, IDICOMService DICOMService)
        {
            _settingsService = settingsService;
            _cache = cache;
            _DICOMService = DICOMService;
            _random = new Random();
        }

        public ICollection<Series> GetLoadedSeries()
        {
            ObservableCollection<Series> tempCollection = new();
            foreach (var series in _cache.LoadedSeries.Values.OrderBy(x => x.SeriesDescription))
            {
                tempCollection.Add(new Series(series));
            }
            return tempCollection;
        }

        public async Task<ITreeModel> GetInstance(string instanceUID, bool validate, string searchTerm, CancellationToken cancellationToken)
        {
            if (_cache.LoadedInstances.TryGetValue(instanceUID, out DicomDataset dataset))
            {
                return await DatasetTree.CreateTree(dataset, validate, searchTerm, cancellationToken);
            }
            else
            {
                throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instanceUID));
            }
        }

        public void SetAttributeValue(IList<Instance> instances, IDatasetModel attribute, string value)
        {
            if (attribute.Tag is null)
            {
                throw new ArgumentException("Invalid attribute tag");
            }
            if (attribute.ValueRepresentation is "SQ")
            {
                throw new InvalidOperationException("Cannot assign value to sequence");
            }

            value = TrimFromZero(value);

            IList<IDatasetModel> attributes = GetAttributePathFromParentToChild(attribute);

            DicomTag lastTag = attributes[^1].Tag;
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

                    if (lastTag == DicomTag.SOPInstanceUID)
                    {
                        UpdateInstanceUID(instance, value, ds);
                    }
                    else if (lastTag == DicomTag.SeriesInstanceUID)
                    {
                        UpdateSeriesUID(instance, value, ds);
                    }
                    else
                    {
                        ds.AddOrUpdate<string>(lastTag, value);
                        if (lastTag == DicomTag.SeriesDescription)
                        {
                            Series series = _cache.LoadedSeries[instance.SeriesUID];
                            if (series.Instances.ElementAt(0).InstanceUID == instance.InstanceUID)
                            {
                                series.SeriesDescription = value;
                                SeriesListUpdatedEvent?.Invoke(this, new(instance.SeriesUID));
                            }
                        }
                    }
                }
                else
                {
                    if (lastTag == DicomTag.SeriesInstanceUID)
                    {
                        SeriesListUpdatedEvent?.Invoke(this, new(value));
                    }
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            if (lastTag == DicomTag.SeriesInstanceUID)
            {
                SeriesListUpdatedEvent?.Invoke(this, new(value));
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void AddAttribute(IList<Instance> instances, IDatasetModel attribute, ushort group, ushort element, string value)
        {
            IList<IDatasetModel> attributes = GetAttributePathFromParentToChild(attribute);

            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    for (int i = 0; i <= attributes.Count - 2; i += 2)
                    {
                        DicomSequence sequence = ds.GetSequence(attributes[i].Tag);
                        int itemIndex = int.Parse(attributes[i + 1].Value);
                        ds = sequence.Items[itemIndex];
                    }

                    DicomTag newTag = new(group, element);
                    var vrs = newTag.DictionaryEntry.ValueRepresentations;
                    if (vrs.Length == 1 && vrs[0] == DicomVR.SQ)
                    {
                        DicomSequence seq = new(newTag);
                        ds.Add(seq);
                    }
                    else
                    {
                        ds.Add<string>(newTag, value);
                    }
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void AddSequenceItem(IList<Instance> instances, IDatasetModel attribute)
        {
            if (attribute.ValueRepresentation is not "SQ")
            {
                throw new InvalidOperationException("Item can only be added to a sequence");
            }

            IList<IDatasetModel> attributes = GetAttributePathFromParentToChild(attribute);

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

                    DicomDataset emptyDs = new();
                    ds.GetSequence(attributes[^1].Tag).Items.Add(emptyDs);
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteAttribute(IList<Instance> instances, IDatasetModel attribute)
        {
            if (attribute.Tag is null)
            {
                throw new ArgumentException("Invalid attribute tag");
            }

            IList<IDatasetModel> attributes = GetAttributePathFromParentToChild(attribute);

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

                    ds.Remove(lastTag);
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteSequenceItem(IList<Instance> instances, IDatasetModel attribute)
        {
            if (attribute.Name is not "Item")
            {
                throw new InvalidOperationException("Only item can be deleted");
            }

            IList<IDatasetModel> attributes = GetAttributePathFromParentToChild(attribute);

            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    DicomSequence sequence = null;
                    for (int i = 0; i <= attributes.Count - 2; i += 2)
                    {
                        sequence = ds.GetSequence(attributes[i].Tag);
                        int itemIndex = int.Parse(attributes[i + 1].Value);
                        ds = sequence.Items[itemIndex];
                    }

                    sequence?.Items.Remove(ds);
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void DeleteInstance(string instanceUID)
        {
            if (_cache.LoadedInstances.TryGetValue(instanceUID, out DicomDataset instance))
            {
                if (instance.TryGetValue<string>(DicomTag.SeriesInstanceUID, 0, out string seriesUID))
                {
                    if (!_cache.LoadedInstances.Remove(instanceUID))
                    {
                        throw new ArgumentException(string.Join(" ", "Couldn't remove the instance with the following UID:", instanceUID));
                    }

                    if (_cache.LoadedSeries.TryGetValue(seriesUID, out Series series))
                    {
                        series.Instances.Remove(series.Instances.Single(s => s.InstanceUID == instanceUID));
                        if (series.Instances.Count == 0)
                        {
                            _cache.LoadedSeries.Remove(seriesUID);
                        }
                        SeriesListUpdatedEvent?.Invoke(this, new(seriesUID));
                    }
                    else
                    {
                        throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist in any of the currently loaded series:", instanceUID));
                    }
                }
                else
                {
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not contain series UID:", instanceUID));
                }
            }
            else
            {
                throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instanceUID));
            }
        }

        public void GenerateAndSetStudyUID(IList<Instance> instances)
        {
            string generatedUID;
            if (!string.IsNullOrWhiteSpace(_settingsService.DicomRoot))
            {
                generatedUID = GenerateUIDFromRoot(1, 0);
            }
            else
            {
                generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
            }

            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    ds.AddOrUpdate<string>(DicomTag.StudyInstanceUID, generatedUID);
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void GenerateAndSetSeriesUID(IList<Instance> instances)
        {
            string generatedUID;
            if (!string.IsNullOrWhiteSpace(_settingsService.DicomRoot))
            {
                generatedUID = GenerateUIDFromRoot(2, 0);
            }
            else
            {
                generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
            }

            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    UpdateSeriesUID(instance, generatedUID, ds);
                }
                else
                {
                    SeriesListUpdatedEvent?.Invoke(this, new(generatedUID));
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            SeriesListUpdatedEvent?.Invoke(this, new(generatedUID));
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public void GenerateAndSetInstanceUID(IList<Instance> instances)
        {
            int counter = 0;
            foreach (Instance instance in instances)
            {
                if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                {
                    string generatedUID;
                    if (!string.IsNullOrWhiteSpace(_settingsService.DicomRoot))
                    {
                        generatedUID = GenerateUIDFromRoot(3, counter);
                    }
                    else
                    {
                        generatedUID = DicomUIDGenerator.GenerateDerivedFromUUID().UID;
                    }
                    UpdateInstanceUID(instance, generatedUID, ds);
                    counter++;
                }
                else
                {
                    AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                }
            }
            AttributesUpdatedEvent?.Invoke(this, EventArgs.Empty);
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
                        throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                    }
                }

                await _DICOMService.StoreAsync(serverHost, serverPort, serverAET, appAET, instances, progress, cancellationToken);
            }
        }

        public async Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (File.Exists(path))
            {
                throw new DirectoryNotFoundException(string.Join(" ", "Invalid directory path:", path));
            }

            Directory.CreateDirectory(path);

            int progressCounter = 0;

            foreach (Series series in seriesList)
            {
                foreach (Instance instance in series.Instances)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_cache.LoadedInstances.TryGetValue(instance.InstanceUID, out DicomDataset ds))
                    {
                        StringBuilder filePath = new(path);
                        if (path[^1] is not '\\')
                        {
                            filePath.Append('\\');
                        }
                        filePath.Append(progressCounter);
                        filePath.Append(".dcm");

                        DicomFile file = new(ds);
                        await file.SaveAsync(filePath.ToString(), FellowOakDicom.IO.Writer.DicomWriteOptions.Default);
                    }
                    else
                    {
                        throw new ArgumentException(string.Join(" ", "Instance with the following UID does not exist:", instance.InstanceUID));
                    }

                    if (progress != null)
                    {
                        progressCounter++;
                        progress.Report(progressCounter);
                    }
                }
            }
        }

        public Tuple<IList<ImageSource>, double[]> GetImages(IList<Instance> instances)
        {
            IList<ImageSource> images = new List<ImageSource>();
            double center = 0;
            double width = 0;
            double[] centerAndWidth = new double[2];
            foreach (Instance instance in instances)
            {
                var dicomImage = new DicomImage(_cache.LoadedInstances[instance.InstanceUID]);
                center = dicomImage.WindowCenter;
                width = dicomImage.WindowWidth;
                var frames = Enumerable.Range(0, dicomImage.NumberOfFrames).Select(frame => dicomImage.RenderImage(frame).As<ImageSource>());
                frames.Each(frame => images.Add(frame));
            }
            return Tuple.Create(images, new[] { center, width });
        }

        public IList<ImageSource> GetImages(IList<Instance> instances, double windowCenter, double windowWidth)
        {
            List<ImageSource> images = new();
            foreach (Instance instance in instances)
            {
                var dicomImage = new DicomImage(_cache.LoadedInstances[instance.InstanceUID]);
                dicomImage.WindowCenter = windowCenter;
                dicomImage.WindowWidth = windowWidth;
                var frames = Enumerable.Range(0, dicomImage.NumberOfFrames).Select(frame => dicomImage.RenderImage(frame).As<ImageSource>());
                images.AddRange(frames);
            }
            return images;
        }

        private IList<IDatasetModel> GetAttributePathFromParentToChild(IDatasetModel attribute)
        {
            List<IDatasetModel> attributes = new();
            while (attribute is not null)
            {
                attributes.Insert(0, attribute);
                attribute = attribute.ParentDataset;
            }
            return attributes;
        }

        private void UpdateSeriesUID(Instance instance, string newUID, DicomDataset ds)
        {
            ds.AddOrUpdate<string>(DicomTag.SeriesInstanceUID, newUID);
            if (_cache.LoadedSeries.TryGetValue(instance.SeriesUID, out Series series))
            {
                series.Instances.Remove(instance);
                if (!_cache.LoadedSeries.TryGetValue(newUID, out Series newSeries))
                {
                    newSeries = new(newUID, series.SeriesDescription, series.SeriesDateTime, series.Modality, series.NumberOfInstances, series.StudyUID, series.PatientID, new List<Instance>());
                    _cache.LoadedSeries[newUID] = newSeries;
                }
                newSeries.Instances.Add(instance);
            }
            if (series.Instances.Count == 0)
            {
                _cache.LoadedSeries.Remove(series.SeriesUID);
            }
            instance.SeriesUID = newUID;
        }

        private void UpdateInstanceUID(Instance instance, string newUID, DicomDataset ds)
        {
            ds.AddOrUpdate<string>(DicomTag.SOPInstanceUID, newUID);
            if (!_cache.LoadedInstances.TryAdd(newUID, ds))
            {
                ds.AddOrUpdate<string>(DicomTag.SOPInstanceUID, instance.InstanceUID);
                throw new ArgumentException(string.Join(" ", "Instance with the following UID already exists:", newUID));
            }
            _cache.LoadedInstances.Remove(instance.InstanceUID);
            instance.InstanceUID = newUID;
        }

        private string GenerateUIDFromRoot(int type, int counter)
        {
            DateTime current = DateTime.Now;
            StringBuilder uid = new(_settingsService.DicomRoot);
            if (_settingsService.DicomRoot[^1] is not '.')
            {
                uid.Append('.');
            }
            uid.Append(current.Year);
            uid.Append(current.Month);
            uid.Append(current.Day);
            uid.Append(current.Hour);
            uid.Append(current.Minute);
            uid.Append(current.Second);
            uid.Append(current.Millisecond);
            uid.Append('.');
            uid.Append(type);
            uid.Append('.');
            uid.Append(counter);
            uid.Append('.');
            uid.Append(_random.Next(100000));

            return uid.ToString();
        }

        private string TrimFromZero(string input)
        {
            int index = input.IndexOf('\0');
            if (index < 0)
                return input;

            return input.Substring(0, index);
        }
    }
}
