using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentTuition
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public decimal FullAmount { get; set; }
        public decimal Discount { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [XmlIgnore]
        public decimal FinalAmount
        {
            get
            {
                decimal final = FullAmount - Discount;
                return final >= 0 ? final : 0;
            }
        }
    }

    [Serializable]
    [XmlRoot("StudentTuitions")]
    public class StudentTuitionCollection
    {
        [XmlElement("StudentTuition")]
        public List<StudentTuition> Tuitions { get; set; } = new List<StudentTuition>();
    }
}