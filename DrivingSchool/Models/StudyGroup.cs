using System;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudyGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Завершена";

        [XmlIgnore]
        public string Duration
        {
            get
            {
                if (StartDate == default || EndDate == default)
                    return "0 мес.";

                var totalMonths = (EndDate.Year - StartDate.Year) * 12 + (EndDate.Month - StartDate.Month);

                if (EndDate.Day < StartDate.Day)
                {
                    totalMonths--;
                }

                var remainingDays = (EndDate - StartDate.AddMonths(totalMonths)).Days;

                if (remainingDays > 0)
                    return $"{totalMonths} мес. {remainingDays} дн.";
                else
                    return $"{totalMonths} мес.";
            }
        }

        [XmlIgnore]
        public int StudentCount { get; set; }
    }

    [Serializable]
    [XmlRoot("StudyGroups")]
    public class StudyGroupCollection
    {
        [XmlElement("StudyGroup")]
        public System.Collections.Generic.List<StudyGroup> Groups { get; set; } = new System.Collections.Generic.List<StudyGroup>();
    }
}