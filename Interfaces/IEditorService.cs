using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using DicomEditor.Model;
using FellowOakDicom;

namespace DicomEditor.Interfaces
{
    public interface IEditorService
    {
        public string LocalExportPath { get; set; }

        public ICollection<Series> GetLoadedSeries();
        public ITreeModel GetInstance(string instanceUID, bool doValidation);
        public void SetAttributeValue(IList<Instance> instances, IDatasetModel attribute, string value);
        public void AddAttribute(IList<Instance> instances, IDatasetModel attribute, ushort group, ushort element, string value);
        public void AddSequenceItem(IList<Instance> instances, IDatasetModel attribute);
        public void DeleteAttribute(IList<Instance> instances, IDatasetModel attribute);
        public void DeleteSequenceItem(IList<Instance> instances, IDatasetModel attribute);
        public void GenerateAndSetStudyUID(IList<Instance> instances);
        public void GenerateAndSetSeriesUID(IList<Instance> instances);
        public void GenerateAndSetInstanceUID(IList<Instance> instances);
        public Task StoreAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
        public Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
