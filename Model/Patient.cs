using System.Collections.Generic;

namespace DicomEditor.Model
{
    public class Patient
    {
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public string DateOfBirth { get; set; }
        public string Sex { get; set; }
        public IDictionary<string, Study> Studies { get; }

        public Patient(string patientID, string patientName, string dateOfBirth, string sex)
        {
            PatientID = patientID;
            PatientName = patientName;
            DateOfBirth = dateOfBirth;
            Sex = sex;
            Studies = new Dictionary<string, Study>();
        }
    }
}
