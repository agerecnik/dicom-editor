using FellowOakDicom;
using System.Collections.Generic;

namespace DicomEditor.Interfaces
{
    public interface IDatasetModel
    {
        public DicomTag Tag { get; set; }
        public string ValueRepresentation { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ICollection<IDatasetModel> NestedDatasets { get; set; }
        public IDatasetModel ParentDataset { get; set; }
    }
}
