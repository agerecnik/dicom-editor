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
using System.IO;

namespace DicomEditor.ViewModel
{
    public class RetrievalDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private bool _executionFinished;
        public  bool ExecutionFinished
        {
            get => _executionFinished;
            set => SetProperty(ref _executionFinished, value);
        }

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
        private readonly List<Series> _seriesList;
        private readonly string _path;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public RetrievalDialogViewModel(IImportService importService, List<Series> seriesList) : this(importService)
        {
            _seriesList = seriesList;
            _path = null;
        }

        public RetrievalDialogViewModel(IImportService importService, string path) : this(importService)
        {
            _path = path;
            _seriesList = null;
        }

        public RetrievalDialogViewModel() : this(new ImportService(new SettingsService(new DICOMService()), new Cache(), new DICOMService()), new List<Series>())
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
                Import();
            }
        }

        private void CancelRetrieval()
        {
            _cancellationTokenSource.Cancel();
        }

        private RetrievalDialogViewModel(IImportService importService)
        {
            _importService = importService;
            CancelRetrievalCommand = new RelayCommand(o => CancelRetrieval());
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
                    RetrievalProgress = tempCount * 100 / totalCount;

                });
            }

            try
            {
                await _importService.RetrieveAsync(_seriesList, progress, _cancellationTokenSource.Token);
                ExecutionFinished = true;
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

        private async void Import()
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
                    RetrievalProgress = tempCount * 100 / totalCount;

                });
            }

            try
            {
                await _importService.LocalImportAsync(_path, progress, _cancellationTokenSource.Token);
                ExecutionFinished = true;
                RetrievalStatus = "Completed";
            }
            catch (Exception e) when (e is FileFormatException
            or FileNotFoundException
            or DirectoryNotFoundException)
            {
                RetrievalStatus = e.Message;
            }
        }
    }
}
