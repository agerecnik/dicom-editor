using DicomEditor.Commands;
using DicomEditor.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DicomEditor.ViewModel
{
    public class GetInstanceTreeDialogViewModel : ViewModelBase, IDialogViewModel
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

        private object _payload;
        public object Payload
        {
            get => _payload;
        }

        public ICommand CancelCommand { get; }

        private readonly IEditorService _editorService;
        private readonly string _instanceUID;
        private readonly bool _validate;
        private readonly string _searchTerm;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public GetInstanceTreeDialogViewModel(IEditorService editorService, string instanceUID, bool validate, string searchTerm)
        {
            _editorService = editorService;
            _instanceUID = instanceUID;
            _validate = validate;
            _searchTerm = searchTerm;
            CancelCommand = new RelayCommand(o => Cancel());
            ExecutionFinished = false;
        }

        public void Execute()
        {
            GetInstance();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
            Status = "Canceled";
        }

        private async void GetInstance()
        {
            try
            {
                _payload = await _editorService.GetInstance(_instanceUID, _validate, _searchTerm, _cancellationTokenSource.Token);
                Status = "Completed";
                ExecutionFinished = true;
            }
            catch (Exception e) when (e is ArgumentException
            or TaskCanceledException)
            {
                Status = e.Message;
                ExecutionFinished = true;
            }
        }
    }
}
