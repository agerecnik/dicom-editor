using System.ComponentModel;
using System.Windows.Input;

namespace DicomEditor.Interfaces
{
    public interface IDialogViewModel
    {
        public bool ExecutionFinished { get; set; }
        public string Status { get; set; }
        public object Payload { get; }
        public ICommand CancelCommand { get; }
        public void Execute();
    }
}
