using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public class Series
    {
        public string SeriesUID { get; set; }
        public string SeriesDescription { get; set; }
        public DateTime SeriesDateTime { get; set; }
        public string Modality { get; set; }
        public int NumberOfInstances { get; set; }
        public string StudyUID { get; set; }
        public string PatientID { get; set; }
        public ICollection<Instance> Instances { get; set; }

        public Series(string seriesUID, string seriesDescription, DateTime seriesDateTime, string modality, int numberOfInstances, string studyUID, string patientID, List<Instance> instances)
        {
            SeriesUID = seriesUID;
            SeriesDescription = seriesDescription;
            SeriesDateTime = seriesDateTime;
            Modality = modality;
            NumberOfInstances = numberOfInstances;
            StudyUID = studyUID;
            PatientID = patientID;
            Instances = new ObservableCollection<Instance>(instances);
        }

        public Series(Series s)
        {
            SeriesUID = s.SeriesUID;
            SeriesDescription = s.SeriesDescription;
            SeriesDateTime = s.SeriesDateTime;
            Modality = s.Modality;
            NumberOfInstances = s.NumberOfInstances;
            StudyUID = s.StudyUID;
            PatientID = s.PatientID;
            Instances = new ObservableCollection<Instance>(s.Instances);
        }
    }
}
