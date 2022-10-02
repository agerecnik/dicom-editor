using DicomEditor.Model;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Interfaces
{
    public interface ICache
    {
        public List<Series> LoadedSeries { get; set; }
        public Dictionary<string, DicomDataset> LoadedInstances { get; set; }
    }
}
