using System.Collections;

namespace DicomEditor.Interfaces
{
    public interface ITreeModel
    {
        IEnumerable GetChildren(object parent);
        bool HasChildren(object parent);
    }
}
