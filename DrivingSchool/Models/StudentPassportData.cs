using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentPassportData
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string DocumentType { get; set; } = "Паспорт РФ";
        public string Series { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string IssuedBy { get; set; } = string.Empty;
        public string DivisionCode { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }

        [XmlIgnore]
        public string StudentName { get; set; } = string.Empty;
    }


    [Serializable]
    [XmlRoot("StudentPassportData")]
    public class StudentPassportDataCollection
    {
        [XmlElement("Passport")]
        public List<StudentPassportData> Passports { get; set; } = new List<StudentPassportData>();
    }
}