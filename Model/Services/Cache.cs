using DicomEditor.Model.Interfaces;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model.Services
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
