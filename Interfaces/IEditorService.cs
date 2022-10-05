using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DicomEditor.Model.EditorModel.Tree;
using System.Threading;
using DicomEditor.Model;

namespace DicomEditor.Interfaces
{
    public interface IEditorService
    {
        public string LocalExportPath { get; set; }

        public IList<Series> GetLoadedSeries();
        public ITreeModel GetInstance(string instanceUID);
        public Task StoreAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
        public Task LocalExportAsync(IList<Series> seriesList, string path, IProgress<int> progress, CancellationToken cancellationToken);

    }
}
