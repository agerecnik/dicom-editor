using System;
using System.Collections.Generic;
using FellowOakDicom;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DicomEditor.Model.EditorModel.Tree;

namespace DicomEditor.Model.Interfaces
{
    public interface IEditorService
    {
        public List<Series> GetLoadedSeries();
        public DatasetTree GetInstance(string instanceUID);
    }
}
