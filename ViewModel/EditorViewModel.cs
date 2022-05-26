using DicomEditor.Model;
using DicomEditor.Model.EditorModel.Tree;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DicomEditor.ViewModel
{
    public class EditorViewModel : ViewModelBase
    {
        private IEditorService _editorService;

        private ObservableCollection<Series> _loadedSeriesList;
        public ObservableCollection<Series> LoadedSeriesList
        {
            get => _loadedSeriesList;
            set
            {
                SetProperty(ref _loadedSeriesList, value);
            }
        }

        private Series _selectedSeries;
        public Series SelectedSeries
        {
            get => _selectedSeries;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedSeries, value);
                }
            }
        }

        private Instance _selectedInstance;
        public Instance SelectedInstance
        {
            get => _selectedInstance;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedInstance, value);
                    UpdateListOfAttributes();
                }
            }
        }

        private DatasetTree _selectedInstanceAttributes;
        public DatasetTree SelectedInstanceAttributes
        {
            get => _selectedInstanceAttributes;
            set
            {
                if (value != null)
                {
                    SetProperty(ref _selectedInstanceAttributes, value);
                }
            }
        }

        public EditorViewModel(IEditorService editorService)
        {
            _editorService = editorService;
        }

        public EditorViewModel() : this(new EditorService(new SettingsService(), new Cache()))
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void UpdateLoadedSeriesList()
        {
            LoadedSeriesList = new ObservableCollection<Series>(_editorService.GetLoadedSeries());
        }

        private void UpdateListOfAttributes()
        {
            SelectedInstanceAttributes = _editorService.GetInstance(SelectedInstance.InstanceUID);
        }
    }
}
