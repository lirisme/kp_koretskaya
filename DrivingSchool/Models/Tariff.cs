using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class Tariff
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BaseCost { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Category { get; set; }
        public int DurationMonths { get; set; }
        public bool IncludesTheory { get; set; } = true;
        public bool IncludesPractice { get; set; } = true;
        public int PracticeHours { get; set; }
    }

    [Serializable]
    [XmlRoot("Tariffs")]
    public class TariffCollection
    {
        [XmlElement("Tariff")]
        public List<Tariff> Tariffs { get; set; } = new List<Tariff>();
    }
}