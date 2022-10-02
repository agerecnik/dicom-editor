using DicomEditor.Commands;
using DicomEditor.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using FellowOakDicom.Network;
using System.IO;
using DicomEditor.Interfaces;

namespace DicomEditor.ViewModel
{
    public class ImportDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private bool _executionFinished;
        public  bool ExecutionFinished
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

        private readonly IImportService _importService;
        private readonly List<Series> _seriesList;
        private readonly string _path;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public ImportDialogViewModel(IImportService importService, List<Series> seriesList) : this(importService)
        {
            _seriesList = seriesList;
            _path = null;
        }

        public ImportDialogViewModel(IImportService importService, string path) : this(importService)
        {
            _path = path;
            _seriesList = null;
        }

        public ImportDialogViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        public void Execute()
        {
            if(_seriesList is not null)
            {
                Retrieve();
            }
            else
            {
                LocalImport();
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        private ImportDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        private async void Retrieve()
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
                await _importService.RetrieveAsync(_seriesList, progress, _cancellationTokenSource.Token);
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

        private async void LocalImport()
        {
            int totalCount = 0;
            int tempCount = 0;
            Progress<int> progress = null;

            if (File.Exists(_path))
            {
                totalCount = 1;
            }
            else if(Directory.Exists(_path))
            {
                totalCount = Directory.GetFiles(_path, "*.dcm", SearchOption.TopDirectoryOnly).Length;
            }

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
                await _importService.LocalImportAsync(_path, progress, _cancellationTokenSource.Token);
                ExecutionFinished = true;
                Status = "Completed";
            }
            catch (Exception e) when (e is FileFormatException
            or FileNotFoundException
            or DirectoryNotFoundException)
            {
                Status = e.Message;
            }
        }
    }
}
