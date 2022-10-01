using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.ViewModel
{
    public interface IDialogViewModel
    {
        public bool ExecutionFinished { get; set; }
        public void Execute();
    }
}
