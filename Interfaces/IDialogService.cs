namespace DicomEditor.Interfaces
{
    public interface IDialogService
    {
        public void ShowDialog<TViewModel>(string title, params object[] parameters);
    }
}
