using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using DicomEditor.Model;
using System.Windows.Media;

namespace DicomEditor.Interfaces
{
    public interface IEditorService
    {
        event EventHandler<SeriesListUpdatedEventArgs> SeriesListUpdatedEvent;
        event EventHandler AttributesUpdatedEvent;

        public string LocalExportPath { get; set; }

        public ICollection<Series> GetLoadedSeries();
        public Task<ITreeModel> GetInstance(string instanceUID, bool validate, string searchTerm, CancellationToken cancellationToken);
        public void SetAttributeValue(IList<Instance> instances, IDatasetModel attribute, string value);
        public void AddAttribute(IList<Instance> instances, IDatasetModel attribute, ushort group, ushort element, string value);
        public void AddSequenceItem(IList<Instance> instances, IDatasetModel attribute);
        public void DeleteAttribute(IList<Instance> instances, IDatasetModel attribute);
        public void DeleteSequenceItem(IList<Instance> instances, IDatasetModel attribute);
        public void DeleteInstance(string instanceUID);
        public void GenerateAndSetStudyUID(IList<Instance> instances);
        public void GenerateAndSetSeriesUID(IList<Instance> instances);
        public void GenerateAndSetInstanceUID(IList<Instance> instances);
        public Task StoreAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
        public Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken);
        public IList<ImageSource> GetImages(IList<Instance> instances);
    }

    public class SeriesListUpdatedEventArgs : EventArgs
    {
        public string SeriesUID { get; set; }

        public SeriesListUpdatedEventArgs(string seriesUID)
        {
            SeriesUID = seriesUID;
        }
    }
}
