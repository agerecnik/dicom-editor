using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom;
using System.Collections.Generic;

namespace DicomEditor.Services
{
    public class Cache : ICache
    {
        private IList<Series> _loadedSeries;
        public IList<Series> LoadedSeries
        {
            get => _loadedSeries;
            set
            {
                _loadedSeries = value;
            }
        }

        private IDictionary<string, DicomDataset> _loadedInstances;
        public IDictionary<string, DicomDataset> LoadedInstances
        {
            get => _loadedInstances;
            set
            {
                _loadedInstances = value;
            }
        }

        public Cache()
        {
            LoadedSeries = new List<Series>();
        }

    }
}
