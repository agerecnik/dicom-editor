using DicomEditor.Commands;
using DicomEditor.Interfaces;
using DicomEditor.Model;
using FellowOakDicom.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class ExportDialogViewModel : ViewModelBase, IDialogViewModel
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
        private readonly string _path;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public ExportDialogViewModel(IEditorService editorService, IList<Series> seriesList)
        {
            _editorService = editorService;
            _seriesList = seriesList;
            _path = null;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        public ExportDialogViewModel(IEditorService editorService, IList<Series> seriesList, string path) : this(editorService, seriesList)
        {
            _path = path;
        }

        public ExportDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute()
        {
            if (_path is null)
            {
                Store();
            }
            else
            {
                LocalExport();
            }
        }

        public void OnClosing(object sender, CancelEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private async void Store()
        {
            try
            {
                await _editorService.StoreAsync(_seriesList, CreateProgress(), _cancellationTokenSource.Token);
                ExecutionFinished = true;
                Status = "Completed";
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
                Status = e.Message;
            }
        }

        private async void LocalExport()
        {
            try
            {
                await _editorService.LocalExportAsync(_seriesList, _path, CreateProgress(), _cancellationTokenSource.Token);
                ExecutionFinished = true;
                Status = "Completed";
            }
            catch (Exception e) when (e is UnauthorizedAccessException
            or DirectoryNotFoundException
            or IOException
            or ArgumentException
            or ArgumentNullException
            or PathTooLongException
            or NotSupportedException)
            {
                Status = e.Message;
            }
        }

        private IProgress<int> CreateProgress()
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

            return progress;
        }
    }
}
