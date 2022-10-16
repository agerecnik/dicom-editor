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
        public IDictionary<string, Series> LoadedSeries { get; set; }
        public IDictionary<string, DicomDataset> LoadedInstances { get; set; }
    }
}
