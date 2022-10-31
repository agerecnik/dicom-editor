using System.ComponentModel;

namespace DicomEditor.Interfaces
{
    public interface IDialogViewModel
    {
        public bool ExecutionFinished { get; set; }
        public string Status { get; set; }
        public void Execute();
    }
}
