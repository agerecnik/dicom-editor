using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FellowOakDicom.Network;


namespace DicomEditor.ViewModel
{
    public class RetrievalDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private string _retrievalStatus;
        public string RetrievalStatus
        {
            get => _retrievalStatus;
            set => SetProperty(ref _retrievalStatus, value);
        }

        private int _retrievalProgress;
        public int RetrievalProgress
        {
            get => _retrievalProgress;
            set => SetProperty(ref _retrievalProgress, value);
        }

        public ICommand CancelRetrievalCommand { get; }

        private readonly IImportService _importService;
        private readonly List<Series> _selectedSeriesList;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public RetrievalDialogViewModel(IImportService importService, List<Series> selectedSeriesList) {
            _importService = importService;
            _selectedSeriesList = selectedSeriesList;
            CancelRetrievalCommand = new RelayCommand(o => CancelRetrieval());
            
        }

        public RetrievalDialogViewModel() : this(new ImportService(new SettingsService(), new Cache()), new List<Series>())
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute()
        {
            Retrieve();
        }

        private void CancelRetrieval()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void Retrieve()
        {
            int totalCount = 0;
            int tempCount = 0;

            foreach (Series series in _selectedSeriesList)
            {
                totalCount += series.NumberOfInstances;
            }

            Progress<int> progress = null;
            if (totalCount > 0)
            {
                progress = new Progress<int>(progressCount =>
                {
                    tempCount++;
                    RetrievalProgress = tempCount * 100 / totalCount;

                });
            }

            try
            {
                await _importService.RetrieveAsync(_selectedSeriesList, progress, _cancellationTokenSource.Token);
                RetrievalStatus = "Completed";
            }
            catch (Exception e) when (e is ConnectionClosedPrematurelyException
            or DicomAssociationAbortedException
            or DicomAssociationRejectedException
            or DicomAssociationRequestTimedOutException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or AggregateException
            or ArgumentException)
            {
                RetrievalStatus = e.Message;
            }
        }
    }
}
