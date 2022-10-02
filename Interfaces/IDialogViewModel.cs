namespace DicomEditor.Interfaces
{
    public interface IDialogViewModel
    {
        public bool ExecutionFinished { get; set; }
        public void Execute();
    }
}
