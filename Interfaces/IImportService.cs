using DicomEditor.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DicomEditor.Interfaces
{
    public interface IImportService
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string AccessionNumber { get; set; }
        public string StudyID { get; set; }
        public string Modality { get; set; }
        public IDictionary<string, Patient> QueryResult { get; set; }
        public string LocalImportPath { get; set; }

        public Task QueryAsync(CancellationToken cancellationToken);
        public Task RetrieveAsync(IList<Series> seriesList, IProgress<int> progress, CancellationToken cancellationToken);
        public Task LocalImportAsync(string path, IProgress<int> progress, CancellationToken cancellationToken);
    }
}
