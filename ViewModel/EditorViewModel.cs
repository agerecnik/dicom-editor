using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class EditorViewModel : ViewModelBase
    {
        private IEditorService _editorService;
        private IDialogService _dialogService;

        private ICollection<Series> _loadedSeriesList;
        public ICollection<Series> LoadedSeriesList
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

        private ITreeModel _selectedInstanceAttributes;
        public ITreeModel SelectedInstanceAttributes
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

        private IDatasetModel _selectedAttribute;
        public IDatasetModel SelectedAttribute
        {
            get => _selectedAttribute;
            set
            {
                SetProperty(ref _selectedAttribute, value);
                if(value is not null)
                {
                    SelectedAttributeValue = value.Value;
                } else
                {
                    SelectedAttributeValue = "";
                }
            }
        }

        private string _selectedAttributeValue;
        public string SelectedAttributeValue
        {
            get => _selectedAttributeValue;
            set
            {
                SetProperty(ref _selectedAttributeValue, value);
            }
        }

        private string _group;
        public string Group
        {
            get => _group;
            set
            {
                if (ValidateTagValue(value))
                {
                    SetProperty(ref _group, value);
                }
                else
                {
                    SetProperty(ref _group, _group);
                }
            }
        }

        private string _element;
        public string Element
        {
            get => _element;
            set
            {
                if (ValidateTagValue(value))
                {
                    SetProperty(ref _element, value);
                }
                else
                {
                    SetProperty(ref _element, _element);
                }
            }
        }

        private string _addAttributeValue;
        public string AddAttributeValue
        {
            get => _addAttributeValue;
            set
            {
                SetProperty(ref _addAttributeValue, value);
            }
        }

        private bool _applyToAll;
        public bool ApplyToAll
        {
            get => _applyToAll;
            set
            {
                SetProperty(ref _applyToAll, value);
            }
        }

        private string _localExportPath;
        public string LocalExportPath
        {
            get => _localExportPath;
            set
            {
                SetProperty(ref _localExportPath, value);
                _editorService.LocalExportPath = value;
            }
        }

        public ICommand StoreCommand { get; }
        public ICommand LocalExportCommand { get; }
        public ICommand ModifyAttributeValueCommand { get; }
        public ICommand AddAttributeCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand GenerateStudyUIDCommand { get; }
        public ICommand GenerateSeriesUIDCommand { get; }
        public ICommand GenerateInstanceUIDCommand { get; }
        

        public EditorViewModel(IEditorService editorService, IDialogService dialogService)
        {
            _editorService = editorService;
            _dialogService = dialogService;

            StoreCommand = new RelayCommand(o => Store(), CanUseStoreCommand);
            LocalExportCommand = new RelayCommand(o => LocalExport(), CanUseLocalExportCommand);
            ModifyAttributeValueCommand = new RelayCommand(o => ModifyAttributeValue(), CanUseModifyAttributeValueCommand);
            AddAttributeCommand = new RelayCommand(o => AddAttribute(), CanUseAddAttributeCommand);
            AddItemCommand = new RelayCommand(o => AddItem(), CanUseAddItemCommand);
            DeleteCommand = new RelayCommand(o => Delete(), CanUseDeleteCommand);
            GenerateStudyUIDCommand = new RelayCommand(o => GenerateStudyUID(), CanUseGenerateUIDCommand);
            GenerateSeriesUIDCommand = new RelayCommand(o => GenerateSeriesUID(), CanUseGenerateUIDCommand);
            GenerateInstanceUIDCommand = new RelayCommand(o => GenerateInstanceUID(), CanUseGenerateUIDCommand);

            LocalExportPath = _editorService.LocalExportPath;
        }

        public EditorViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void UpdateLoadedSeriesList()
        {
            LoadedSeriesList = _editorService.GetLoadedSeries();
        }

        private void UpdateListOfAttributes()
        {
            SelectedInstanceAttributes = _editorService.GetInstance(SelectedInstance.InstanceUID);
            SelectedAttribute = null;
        }
        
        private void ModifyAttributeValue()
        {
            List<Instance> instances;
            if(ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            } else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                DicomTag tag = SelectedAttribute.Tag;
                _editorService.SetAttributeValue(instances, SelectedAttribute, SelectedAttributeValue);
                if(tag == DicomTag.SeriesInstanceUID)
                {
                    UpdateLoadedSeriesList();
                } else
                {
                    UpdateListOfAttributes();
                }
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
                UpdateListOfAttributes();
            }
        }

        private void AddAttribute()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                _editorService.AddAttribute(instances, SelectedAttribute, ushort.Parse(Group, System.Globalization.NumberStyles.HexNumber), ushort.Parse(Element, System.Globalization.NumberStyles.HexNumber), AddAttributeValue);
                UpdateListOfAttributes();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
                UpdateListOfAttributes();
            }
        }

        private void AddItem()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                _editorService.AddSequenceItem(instances, SelectedAttribute);
                UpdateListOfAttributes();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
                UpdateListOfAttributes();
            }
        }

        private void Delete()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                if (SelectedAttribute.Name is "Item")
                {
                    _editorService.DeleteSequenceItem(instances, SelectedAttribute);
                }
                else
                {
                    _editorService.DeleteAttribute(instances, SelectedAttribute);
                }
                UpdateListOfAttributes();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
            }
        }

        private void GenerateStudyUID()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                _editorService.GenerateAndSetStudyUID(instances);
                UpdateListOfAttributes();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
            }
        }

        private void GenerateSeriesUID()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                _editorService.GenerateAndSetSeriesUID(instances);
                UpdateListOfAttributes();
                UpdateLoadedSeriesList();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
            }
        }

        private void GenerateInstanceUID()
        {
            List<Instance> instances;
            if (ApplyToAll)
            {
                instances = new List<Instance>(SelectedSeries.Instances);
            }
            else
            {
                instances = new List<Instance>() { SelectedInstance };
            }

            try
            {
                _editorService.GenerateAndSetInstanceUID(instances);
                UpdateListOfAttributes();
                SelectedAttribute = null;
                SelectedAttributeValue = null;
            }
            catch (Exception e) when (e is DicomValidationException
            or ApplicationException
            or InvalidOperationException
            or DicomDataException
            or ArgumentNullException
            or FormatException
            or OverflowException
            or ArgumentException)
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", e.Message);
            }
        }

        private void Store()
        {
            if (SelectedSeries is not null)
            {
                var vm = _dialogService.ShowDialog<ExportDialogViewModel>("Store in progress", _editorService, new List<Series> { SelectedSeries });
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", vm.Status);
            }
        }

        private void LocalExport()
        {
            var vm = _dialogService.ShowDialog<ExportDialogViewModel>("Local export in progress", _editorService, new List<Series> { SelectedSeries }, LocalExportPath);
            _dialogService.ShowDialog<MessageDialogViewModel>("Notification", vm.Status);
        }

        private bool ValidateTagValue(string value)
        {
            if (value.Length > 4 || value.Length > 4)
            {
                return false;
            }
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanUseModifyAttributeValueCommand(object o)
        {
            if(SelectedAttribute is null || SelectedAttribute.Tag is null || SelectedAttribute.ValueRepresentation is "SQ")
            {
                return false;
            }
            return true;
        }

        private bool CanUseAddAttributeCommand(object o)
        {
            if (SelectedAttribute is null
                || Group is null || Group is ""
                || Element is null || Element is ""
                || SelectedAttribute.ValueRepresentation is "SQ")
            {
                return false;
            }
            return true;
        }

        private bool CanUseAddItemCommand(object o)
        {
            if (SelectedAttribute is null || SelectedAttribute.ValueRepresentation is not "SQ")
            {
                return false;
            }
            return true;
        }

        private bool CanUseDeleteCommand(object o)
        {
            if (SelectedAttribute is null)
            {
                return false;
            }
            return true;
        }

        private bool CanUseGenerateUIDCommand(object o)
        {
            if (SelectedSeries is null || SelectedInstance is null)
            {
                return false;
            }
            return true;
        }

        private bool CanUseStoreCommand(object o)
        {
            if (SelectedSeries is null)
            {
                return false;
            }
            return true;
        }

        private bool CanUseLocalExportCommand(object o)
        {
            if (SelectedSeries is null || LocalExportPath is null || LocalExportPath.Length <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
