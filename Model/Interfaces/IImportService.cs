using FellowOakDicom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Model.Interfaces
{
    public interface IImportService
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string AccessionNumber { get; set; }
        public string StudyID { get; set; }
        public string Modality { get; set; }
        public Dictionary<string, Patient> QueryResult { get; set; }

        public Task<Dictionary<string, Patient>> Query();
        public void Retrieve(List<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
