using System;
using System.Collections.Generic;

namespace MockCentennial.Models
{
    public class InvoiceCourse
    {
        public string courseCode { get; set; }
        public double fee { get; set; }
    }
    public class Invoice
    {
        public bool isFullTime { get; set; }
        public double registrationFee { get; set; }
        public List<InvoiceCourse> tuition { get; set; }
        public double total { get; set; }
    }

    public class InvoiceInfo
    {
        public int RegistrationId { get; set; }
        public DateTime? DatePaymentMade { get; set; }
        public DateTime? DateRefundIssued { get; set; }
        public Invoice Invoice { get; set; }
    }
}