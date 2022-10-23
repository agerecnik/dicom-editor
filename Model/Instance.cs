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
        //TODO: add instance number

        public Instance(string instanceUID, string seriesUID)
        {
            InstanceUID = instanceUID;
            SeriesUID = seriesUID;
        }
    }
}
