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
        public List<Series> GetLoadedSeries();
        public DatasetTree GetInstance(string instanceUID);
        public Task StoreAsync(List<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
