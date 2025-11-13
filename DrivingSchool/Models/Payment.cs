using System;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class Payment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = "Наличные";
    }

    [Serializable]
    [XmlRoot("Payments")]
    public class PaymentCollection
    {
        [XmlElement("Payment")]
        public System.Collections.Generic.List<Payment> Payments { get; set; } = new System.Collections.Generic.List<Payment>();
    }
}