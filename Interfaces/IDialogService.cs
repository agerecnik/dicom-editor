using System;

namespace DicomEditor.Interfaces
{
    public interface IDialogService
    {
        public TViewModel ShowDialog<TViewModel>(string title, params object[] parameters);
    }
}
