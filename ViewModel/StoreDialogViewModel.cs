﻿using DicomEditor.Commands;
using DicomEditor.Model;
using DicomEditor.Model.Interfaces;
using DicomEditor.Model.Services;
using FellowOakDicom.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private string _storeStatus;
        public string StoreStatus
        {
            get => _storeStatus;
            set => SetProperty(ref _storeStatus, value);
        }

        private int _storeProgress;
        public int StoreProgress
        {
            get => _storeProgress;
            set => SetProperty(ref _storeProgress, value);
        }

        public ICommand CancelStoreCommand { get; }

        private readonly IEditorService _editorService;
        private readonly List<Series> _seriesList;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public StoreDialogViewModel(IEditorService editorService, List<Series> seriesList)
        {
            _editorService = editorService;
            _seriesList = seriesList;
            CancelStoreCommand = new RelayCommand(o => CancelStore());
            ExecutionFinished = false;
        }

        public StoreDialogViewModel() : this(new EditorService(new SettingsService(), new Cache()), new List<Series>())
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

        private void CancelStore()
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
                    StoreProgress = tempCount * 100 / totalCount;

                });
            }

            try
            {
                await _editorService.StoreAsync(_seriesList, progress, _cancellationTokenSource.Token);
                ExecutionFinished = true;
                StoreStatus = "Completed";
            }
            catch (Exception e) when (e is ConnectionClosedPrematurelyException
            or DicomAssociationAbortedException
            or DicomAssociationRejectedException
            or DicomAssociationRequestTimedOutException
            or DicomNetworkException
            or DicomRequestTimedOutException
            or AggregateException)
            {
                StoreStatus = e.Message;
            }
        }
    }
}