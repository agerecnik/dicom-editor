using DicomEditor.Model.EditorModel.Tree;
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
    public class EditorService : IEditorService
    {
        private ISettingsService _settingsService;
        private ICache _cache;

        public EditorService(ISettingsService settingsService, ICache cache)
        {
            _settingsService = settingsService;
            _cache = cache;
        }

        public List<Series> GetLoadedSeries()
        {
            return _cache.LoadedSeries;
        }

        public DatasetTree GetInstance(string instanceUID)
        {
            DicomDataset dataset = new();
            _cache.LoadedInstances.TryGetValue(instanceUID, out dataset);
            return DatasetTree.CreateTree(dataset);
        }
    }
}
