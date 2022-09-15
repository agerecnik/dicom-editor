using System;
using System.Collections.Generic;
using FellowOakDicom;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DicomEditor.Model.EditorModel.Tree;
using System.Threading;

namespace DicomEditor.Model.Interfaces
{
    public interface IEditorService
    {
        public List<Series> GetLoadedSeries();
        public DatasetTree GetInstance(string instanceUID);
        public Task StoreAsync(Series series, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
