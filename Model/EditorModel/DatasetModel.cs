using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DicomEditor.Model.EditorModel
{
    public class DatasetModel
    {
        private readonly ObservableCollection<DatasetModel> _nestedDatasets = new ObservableCollection<DatasetModel>();
        public ObservableCollection<DatasetModel> NestedDatasets
        {
            get { return _nestedDatasets; }
        }

        public string Tag { get; set; }
        public string ValueRepresentation { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public DatasetModel(string tag, string vr, string name, string value)
        {
            Tag = tag;
            ValueRepresentation = vr;
            Name = name;
            Value = value;
        }
    }
}
