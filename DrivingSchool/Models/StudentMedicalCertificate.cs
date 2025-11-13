using System;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentMedicalCertificate
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Series { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string MedicalInstitution { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public DateTime? ValidUntil { get; set; }
    }

    [Serializable]
    [XmlRoot("StudentMedicalCertificates")]
    public class StudentMedicalCertificateCollection
    {
        [XmlElement("Certificate")]
        public System.Collections.Generic.List<StudentMedicalCertificate> Certificates { get; set; } = new System.Collections.Generic.List<StudentMedicalCertificate>();
    }
}