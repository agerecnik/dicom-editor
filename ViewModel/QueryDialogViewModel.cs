using DicomEditor.Commands;
using DicomEditor.Services;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FellowOakDicom.Network;
using DicomEditor.Interfaces;

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
        public ICommand CancelCommand { get; }

        private readonly IImportService _importService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public QueryDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        public QueryDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute()
        {
            StartQuery();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void StartQuery()
        {
            try
            {
                await _importService.QueryAsync(_cancellationTokenSource.Token);
                ExecutionFinished = true;
                Status = "Completed";
            }
            catch (Exception e) when (e is ConnectionClosedPrematurelyException
            or DicomAssociationAbortedException
            or DicomAssociationRejectedException
            or DicomAssociationRequestTimedOutException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or AggregateException)
            {
                Status = e.Message;
            }
        }
    }
}
