using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentSystem.Shared
{
    public class TransactionDto
    {
        public string SourceUsername { get; set; }
        public string DestinationUsername { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
