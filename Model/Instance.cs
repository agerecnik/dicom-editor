namespace DicomEditor.Model
{
    public class Instance
    {
        public string InstanceUID { get; set; }
        public string SeriesUID { get; set; }
        public string InstanceNumber { get; set; }
        public bool IsImage { get; set; }

        public Instance(string instanceUID, string seriesUID, string instanceNumber, bool isImage)
        {
            InstanceUID = instanceUID;
            SeriesUID = seriesUID;
            InstanceNumber = instanceNumber;
            IsImage = isImage;
        }
    }
}
