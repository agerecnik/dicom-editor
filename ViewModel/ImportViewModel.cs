using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class ImportViewModel : ViewModelBase
    {
        private IImportService _importService;

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

        private Dictionary<string, Patient> _queryResult;
        public Dictionary<string, Patient> QueryResult
        {
            get => _queryResult;
            set
            {
                SetProperty(ref _queryResult, value);
            }
        }

        private List<Series> _selectedSeriesList;
        public List<Series> SelectedSeriesList
        {
            get => _selectedSeriesList;
            set => SetProperty(ref _selectedSeriesList, value);
        }

        private int _retrievalProgress;
        public int RetrievalProgress
        {
            get => _retrievalProgress;
            set => SetProperty(ref _retrievalProgress, value);
        }

        public ICommand QueryCommand { get; }

        public ICommand RetrieveCommand { get; }

        public ImportViewModel(IImportService importService)
        {
            _importService = importService;

            QueryCommand = new RelayCommand(async o =>
            {
               QueryResult = await _importService.Query();
            });

            RetrieveCommand = new RelayCommand(o =>
            {
                if (SelectedSeriesList is not null && SelectedSeriesList.Count > 0)
                {
                    var progress = new Progress<int>(percent =>
                    {
                        RetrievalProgress = percent;

                    });
                    _importService.Retrieve(SelectedSeriesList, progress);
                }
            });

            PatientID = _importService.PatientID;
            PatientName = _importService.PatientName;
            AccessionNumber = _importService.AccessionNumber;
            StudyID = _importService.StudyID;
            Modality = _importService.Modality;
            QueryResult = _importService.QueryResult;
            SelectedSeriesList = new();
        }

        public ImportViewModel() : this(new ImportService(new SettingsService(), new Cache()))
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
    }
}
