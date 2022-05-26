using System.Collections;

namespace DicomEditor.Model.EditorModel.Tree
{
    public interface ITreeModel
    {
        IEnumerable GetChildren(object parent);
        bool HasChildren(object parent);
    }
}
