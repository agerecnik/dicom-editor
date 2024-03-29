﻿using DicomEditor.Commands;
using DicomEditor.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using FellowOakDicom.Network;
using System.IO;
using DicomEditor.Interfaces;
using FellowOakDicom;
using System.Threading.Tasks;

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

        public object Payload => throw new NotImplementedException();

        public ICommand CancelCommand { get; }

        private readonly IImportService _importService;
        private readonly IList<Series> _seriesList;
        private readonly string _path;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public ImportDialogViewModel(IImportService importService, IList<Series> seriesList) : this(importService)
        {
            _seriesList = seriesList;
            _path = null;
        }

        public ImportDialogViewModel(IImportService importService, string path) : this(importService)
        {
            _path = path;
            _seriesList = null;
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

        private ImportDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
            Status = "Canceled";
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
            or KeyNotFoundException
            or TaskCanceledException)
            {
                Status = e.Message;
                ExecutionFinished = true;
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
                Status = "Completed";
                ExecutionFinished = true;
            }
            catch (Exception e) when (e is FileFormatException
            or FileNotFoundException
            or DirectoryNotFoundException
            or DicomDataException
            or ArgumentException
            or ArgumentNullException
            or ArgumentOutOfRangeException
            or UnauthorizedAccessException
            or PathTooLongException
            or IOException
            or KeyNotFoundException
            or TaskCanceledException)
            {
                Status = e.Message;
                ExecutionFinished = true;
            }
        }
    }
}
