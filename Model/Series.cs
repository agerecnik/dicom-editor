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
        public string SeriesUID { get; }
        public string SeriesDescription { get; }
        public DateTime SeriesDateTime { get; }
        public string Modality { get; }
        public int NumberOfInstances { get; }
        public string StudyUID { get; }
        public ObservableCollection<Instance> Instances { get; }

        public Series(string seriesUID, string seriesDescription, DateTime seriesDateTime, string modality, int numberOfInstances, string studyUID, List<Instance> instances)
        {
            SeriesUID = seriesUID;
            SeriesDescription = seriesDescription;
            SeriesDateTime = seriesDateTime;
            Modality = modality;
            NumberOfInstances = numberOfInstances;
            StudyUID = studyUID;
            Instances = new ObservableCollection<Instance>(instances);
        }
    }
}
