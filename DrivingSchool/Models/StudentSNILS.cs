using System;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentSNILS
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime? IssueDate { get; set; }
        public string IssuedBy { get; set; } = string.Empty;

        [XmlIgnore]
        public string StudentName { get; set; } = string.Empty;
    }

    [Serializable]
    [XmlRoot("StudentSNILS")]
    public class StudentSNILSCollection
    {
        [XmlElement("SNILS")]
        public System.Collections.Generic.List<StudentSNILS> SNILSList { get; set; } = new System.Collections.Generic.List<StudentSNILS>();
    }
}