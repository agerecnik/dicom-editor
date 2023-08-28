using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        private IImportService _importService;
        private IDialogService _dialogService;

        private string _patientID;
        public string PatientID
        {
            get => _patientID;
            set
            {
                SetProperty(ref _patientID, value);
                _importService.PatientID = value;
            }
        }

        private string _patientName;
        public string PatientName
        {
            get => _patientName;
            set
            {
                SetProperty(ref _patientName, value);
                _importService.PatientName = value;
            }
        }

        private string _accessionNumber;
        public string AccessionNumber
        {
            get => _accessionNumber;
            set
            {
                SetProperty(ref _accessionNumber, value);
                _importService.AccessionNumber = value;
            }
        }

        private string _studyID;
        public string StudyID
        {
            get => _studyID;
            set
            {
                SetProperty(ref _studyID, value);
                _importService.StudyID = value;
            }
        }

        private string _modality;
        public string Modality
        {
            get => _modality;
            set
            {
                SetProperty(ref _modality, value);
                _importService.Modality = value;
            }
        }

        private ICollection<Patient> _queryResult;
        public ICollection<Patient> QueryResult
        {
            get => _queryResult;
            set
            {
                SetProperty(ref _queryResult, value);
            }
        }

        private IList<Series> _selectedSeriesList;
        public IList<Series> SelectedSeriesList
        {
            get => _selectedSeriesList;
            set => SetProperty(ref _selectedSeriesList, value);
        }

        public string _localImportPath;
        public string LocalImportPath
        {
            get => _localImportPath;
            set
            {
                SetProperty(ref _localImportPath, value);
                _importService.LocalImportPath = value;
            }
        }

        public ICommand QueryCommand { get; }
        public ICommand RetrieveCommand { get; }
        public ICommand LocalImportCommand { get; }


        public ImportViewModel(IImportService importService, IDialogService dialogService)
        {
            _importService = importService;
            _dialogService = dialogService;

            QueryCommand = new RelayCommand(o => Query());
            RetrieveCommand = new RelayCommand(o => Retrieve(), CanUseRetrieveCommand);
            LocalImportCommand = new RelayCommand(o => LocalImport(), CanUseLocalImportCommand);

            PatientID = _importService.PatientID;
            PatientName = _importService.PatientName;
            AccessionNumber = _importService.AccessionNumber;
            StudyID = _importService.StudyID;
            Modality = _importService.Modality;
            QueryResult = _importService.QueryResult;
            LocalImportPath = _importService.LocalImportPath;
            SelectedSeriesList = new List<Series>();
        }

        public ImportViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void ParseSelectedEntry(object selectedEntry)
        {
            List<Series> seriesList = new();

            if(selectedEntry.GetType() == typeof(Patient))
            {
                Patient patient = (Patient)selectedEntry;
                foreach(Study study in patient.Studies.Values)
                {
                    foreach(Series series in study.Series.Values)
                    {
                        seriesList.Add(series);
                    }
                }
            }
            else if(selectedEntry.GetType() == typeof(Study))
            {
                Study study = (Study)selectedEntry;
                foreach(Series series in study.Series.Values)
                {
                    seriesList.Add(series);
                }
            }
            else if(selectedEntry.GetType() == typeof(Series))
            {
                seriesList.Add((Series)selectedEntry);
            }

            SelectedSeriesList = seriesList;
        }

        private void Query()
        {
            var vm = _dialogService.ShowDialog<QueryDialogViewModel>("Query in progress", _importService);
            if(vm.Status != "Completed")
            {
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", vm.Status);
            }
            QueryResult = _importService.QueryResult;
        }

        private void Retrieve()
        {
            if (SelectedSeriesList is not null && SelectedSeriesList.Count > 0)
            {
                var vm = _dialogService.ShowDialog<ImportDialogViewModel>("Retrieval in progress", _importService, SelectedSeriesList);
                _dialogService.ShowDialog<MessageDialogViewModel>("Notification", vm.Status);
            }
        }

        private void LocalImport()
        {
            var vm = _dialogService.ShowDialog<ImportDialogViewModel>("Import in progress", _importService, LocalImportPath);
            _dialogService.ShowDialog<MessageDialogViewModel>("Notification", vm.Status);
        }

        private bool CanUseRetrieveCommand(object o)
        {
            if (SelectedSeriesList is null || SelectedSeriesList.Count <= 0)
            {
                return false;
            }
            return true;
        }

        private bool CanUseLocalImportCommand(object o)
        {
            if (LocalImportPath is null || LocalImportPath.Length <= 0)
            {
                return false;
            }
            return true;
        }
    }
}
