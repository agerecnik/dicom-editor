using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom;
using System.Collections.Generic;

namespace DicomEditor.Services
{
    public class Cache : ICache
    {
        private List<Series> _loadedSeries;
        public List<Series> LoadedSeries
        {
            get => _loadedSeries;
            set
            {
                _loadedSeries = value;
            }
        }

        private Dictionary<string, DicomDataset> _loadedInstances;
        public Dictionary<string, DicomDataset> LoadedInstances
        {
            get => _loadedInstances;
            set
            {
                _loadedInstances = value;
            }
        }

        public Cache()
        {
            LoadedSeries = new();
        }

    }
}
