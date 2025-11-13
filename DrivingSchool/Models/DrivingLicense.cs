using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentDrivingLicense
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Series { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Categories { get; set; } = string.Empty;
        public string IssuedBy { get; set; } = string.Empty;
        public string DivisionCode { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }

        [XmlIgnore]
        public string FullNumber => $"{Series} {Number}";

        [XmlIgnore]
        public bool IsExpired => ExpiryDate < DateTime.Now;

        [XmlIgnore]
        public string Status => IsExpired ? "Просрочено" : "Действительно";
    }

    [Serializable]
    [XmlRoot("StudentDrivingLicenses")]
    public class StudentDrivingLicenseCollection
    {
        [XmlElement("DrivingLicense")]
        public List<StudentDrivingLicense> Licenses { get; set; } = new List<StudentDrivingLicense>();
    }
}