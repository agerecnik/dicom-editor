using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public class Instance
    {
        public string InstanceUID { get; set; }
        public string SeriesUID { get; set; }
        public string InstanceNumber { get; set; }

        public Instance(string instanceUID, string seriesUID, string instanceNumber)
        {
            InstanceUID = instanceUID;
            SeriesUID = seriesUID;
            InstanceNumber = instanceNumber;
        }
    }
}
