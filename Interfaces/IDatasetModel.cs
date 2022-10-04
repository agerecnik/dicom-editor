using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Interfaces
{
    public interface IDatasetModel
    {
        public string Tag { get; set; }
        public string ValueRepresentation { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public ICollection<IDatasetModel> NestedDatasets { get; set; }
    }
}
