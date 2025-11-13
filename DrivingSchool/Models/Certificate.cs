using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentCertificate
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string CertificateSeries { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public int VehicleCategoryId { get; set; }

        [XmlIgnore]
        public string FullNumber => $"{CertificateSeries} {CertificateNumber}";

        [XmlIgnore]
        public string CategoryCode { get; set; } = string.Empty;
    }

    [Serializable]
    [XmlRoot("StudentCertificates")]
    public class StudentCertificateCollection
    {
        [XmlElement("Certificate")]
        public List<StudentCertificate> Certificates { get; set; } = new List<StudentCertificate>();
    }
}