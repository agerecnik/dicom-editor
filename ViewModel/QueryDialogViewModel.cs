using DicomEditor.Commands;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FellowOakDicom.Network;
using DicomEditor.Interfaces;
using FellowOakDicom;
using System.Collections.Generic;

namespace DicomEditor.ViewModel
{
    public class QueryDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private bool _executionFinished;
        public bool ExecutionFinished
        {
            get => _executionFinished;
            set => SetProperty(ref _executionFinished, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public object Payload => throw new NotImplementedException();

        public ICommand CancelCommand { get; }

        private readonly IImportService _importService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public QueryDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        public void Execute()
        {
            StartQuery();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
            Status = "Canceled";
        }

        private async void StartQuery()
        {
            try
            {
                await _importService.QueryAsync(_cancellationTokenSource.Token);
                Status = "Completed";
                ExecutionFinished = true; 
            }
            catch (Exception e) when (e is ConnectionClosedPrematurelyException
            or DicomAssociationAbortedException
            or DicomAssociationRejectedException
            or DicomAssociationRequestTimedOutException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or DicomDataException
            or AggregateException
            or ArgumentException
            or ArgumentNullException
            or KeyNotFoundException)
            {
                Status = e.Message;
                ExecutionFinished = true;
            }
        }
    }
}
