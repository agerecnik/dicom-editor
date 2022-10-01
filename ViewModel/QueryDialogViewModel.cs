using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FellowOakDicom.Network;

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

        private string _queryStatus;
        public string QueryStatus
        {
            get => _queryStatus;
            set => SetProperty(ref _queryStatus, value);
        }
        public ICommand CancelQueryCommand { get; }

        private readonly IImportService _importService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public QueryDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelQueryCommand = new RelayCommand(o => CancelQuery());
            ExecutionFinished = false;
        }

        public QueryDialogViewModel() : this(new ImportService(new SettingsService(), new Cache()))
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

        private void CancelQuery()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void StartQuery()
        {
            try
            {
                await _importService.QueryAsync(_cancellationTokenSource.Token);
                ExecutionFinished = true;
                QueryStatus = "Completed";
            }
            catch (Exception e) when (e is ConnectionClosedPrematurelyException
            or DicomAssociationAbortedException
            or DicomAssociationRejectedException
            or DicomAssociationRequestTimedOutException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or AggregateException)
            {
                QueryStatus = e.Message;
            }
        }
    }
}
