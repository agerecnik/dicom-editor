using DicomEditor.Model;
using FellowOakDicom;
using System.Collections.Generic;

namespace DicomEditor.Interfaces
{
    public interface ICache
    {
        public IDictionary<string, Series> LoadedSeries { get; set; }
        public IDictionary<string, DicomDataset> LoadedInstances { get; set; }
    }
}
