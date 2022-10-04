using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public class Patient
    {
        public string PatientID { get; }
        public string PatientName { get; }
        public string DateOfBirth { get; }
        public string Sex { get; }
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
