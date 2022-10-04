using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public class Study
    {
        public string StudyUID { get; }
        public string AccessionNumber { get; }
        public string StudyDescription { get; }
        public DateTime StudyDateTime { get; }
        public string Modalities { get; }
        public IDictionary<string, Series> Series { get; }

        public Study(string studyUID, string accessionNumber, string studyDescription, DateTime studyDateTime, string modalities)
        {
            StudyUID = studyUID;
            AccessionNumber = accessionNumber;
            StudyDescription = studyDescription;
            StudyDateTime = studyDateTime;
            Modalities = modalities;
            Series = new Dictionary<string, Series>();
        }
    }
}
