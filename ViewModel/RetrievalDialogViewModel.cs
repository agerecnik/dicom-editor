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

namespace DicomEditor.ViewModel
{
    public class RetrievalDialogViewModel : ViewModelBase
    {
        private int _retrievalProgress;
        public int RetrievalProgress
        {
            get => _retrievalProgress;
            set => SetProperty(ref _retrievalProgress, value);
        }

        public ICommand CancelRetrievalCommand { get; }

        private CancellationTokenSource cancellationTokenSource = new();

        public RetrievalDialogViewModel(IImportService importService, List<Series> selectedSeriesList) {
            CancelRetrievalCommand = new RelayCommand(o => CancelRetrieval());
            Retrieve(importService, selectedSeriesList);
        }

        public RetrievalDialogViewModel() : this(new ImportService(new SettingsService(), new Cache()), new List<Series>())
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                throw new Exception("Use only for design mode");
            }
        }

        private void CancelRetrieval()
        {
            cancellationTokenSource.Cancel();
        }

        private void Retrieve(IImportService importService, List<Series> selectedSeriesList)
        {
            int totalCount = 0;
            int tempCount = 0;

            foreach (Series series in selectedSeriesList)
            {
                totalCount += series.NumberOfInstances;
            }

            Progress<int> progress = null;
            if (totalCount > 0)
            {
                progress = new Progress<int>(progressCount =>
                {
                    tempCount += progressCount;
                    RetrievalProgress = tempCount * 100 / totalCount;

                });
            }

            importService.Retrieve(selectedSeriesList, progress, cancellationTokenSource.Token);
        }
    }
}
