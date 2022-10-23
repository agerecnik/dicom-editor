using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using DicomEditor.Model;

namespace DicomEditor.Interfaces
{
    public interface IEditorService
    {
        public string LocalExportPath { get; set; }

        public IDictionary<string, Series> GetLoadedSeries();
        public ITreeModel GetInstance(string instanceUID);
        public void SetAttributeValue(IList<Instance> instances, IDatasetModel attribute, string value);
        public void GenerateAndSetStudyUID(IList<Instance> instances);
        public void GenerateAndSetSeriesUID(IList<Instance> instances);
        public void GenerateAndSetInstanceUID(IList<Instance> instances);
        public Task StoreAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
        public Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
