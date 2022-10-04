namespace DicomEditor.Interfaces
{
    public interface IDICOMServer
    {
        public enum ServerType
        {
            QueryRetrieveServer,
            StoreServer
        }
        public enum VerificationStatus
        {
            Successful,
            Failed,
            InProgress,
            NA
        }

        public ServerType Type { get; }
        public string AET { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public VerificationStatus Status { get; set; }
    }
}
