using System.ComponentModel;

namespace DicomEditor.Interfaces
{
    public interface IDialogViewModel
    {
        public bool ExecutionFinished { get; set; }
        public void Execute();
        public void OnClosing(object sender, CancelEventArgs e);
    }
}
