using DicomEditor.Commands;
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
        private readonly Series _series;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public StoreDialogViewModel(IEditorService editorService, Series series)
        {
            _editorService = editorService;
            _series = series;
            CancelStoreCommand = new RelayCommand(o => CancelStore());
        }

        public StoreDialogViewModel() : this(new EditorService(new SettingsService(), new Cache()), new Series("seriesUID", "description", new DateTime(), "modality", 0, "studyUID", new List<Instance>()))
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
            int totalCount = _series.Instances.Count;

            Progress<int> progress = null;
            if (totalCount > 0)
            {
                progress = new Progress<int>(progressCount =>
                {
                    StoreProgress = progressCount * 100 / totalCount;
                });
            }

            try
            {
                await _editorService.StoreAsync(_series, progress, _cancellationTokenSource.Token);
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
