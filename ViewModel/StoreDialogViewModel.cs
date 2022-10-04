using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using DicomEditor.Services;
using FellowOakDicom.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class StoreDialogViewModel : ViewModelBase, IDialogViewModel
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

        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public ICommand CancelCommand { get; }

        private readonly IEditorService _editorService;
        private readonly IList<Series> _seriesList;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public StoreDialogViewModel(IEditorService editorService, IList<Series> seriesList)
        {
            _editorService = editorService;
            _seriesList = seriesList;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        public StoreDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute()
        {
            Store();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void Store()
        {
            int totalCount = 0;
            int tempCount = 0;

            foreach (Series series in _seriesList)
            {
                totalCount += series.NumberOfInstances;
            }

            Progress<int> progress = null;
            if (totalCount > 0)
            {
                progress = new Progress<int>(progressCount =>
                {
                    tempCount++;
                    Progress = tempCount * 100 / totalCount;

                });
            }

            try
            {
                await _editorService.StoreAsync(_seriesList, progress, _cancellationTokenSource.Token);
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
