using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model.Interfaces
{
    public interface IDialogService
    {
        public void ShowDialog<TViewModel>(string title, Action<string> callback, params object[] parameters);
    }
}
