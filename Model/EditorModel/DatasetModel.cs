using DicomEditor.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DicomEditor.Model.EditorModel
{
    public class DatasetModel : IDatasetModel
    {
        public string Tag { get; set; }
        public string ValueRepresentation { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ICollection<IDatasetModel> NestedDatasets { get; set; }

        public DatasetModel(string tag, string vr, string name, string value)
        {
            Tag = tag;
            ValueRepresentation = vr;
            Name = name;
            Value = value;
            NestedDatasets = new ObservableCollection<IDatasetModel>();
        }
    }
}
