using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public class Study
    {
        public string StudyUID { get; set; }
        public string AccessionNumber { get; set; }
        public string StudyDescription { get; set; }
        public DateTime StudyDateTime { get; set; }
        public string Modalities { get; set; }
        public IDictionary<string, Series> Series { get; set; }

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
