using System;

namespace Test1.Models
{
    public class AccountDto
    {
        public Guid Guid { get; set; }

        public Guid LocationGuid { get; set; }

        public AccountStatusType Status {  get; set; }

        public AccountType AccountType { get; set; }

        public double? PaymentAmount { get; set; }

        public DateTime? EndDateUtc { get; set; }

        public bool PendCancel { get; set; }

        public DateTime? PendCancelDateUtc { get; set; }

        public DateTime PeriodStartUtc { get; set; }

        public DateTime PeriodEndUtc { get; set; }

        public DateTime NextBillingUtc { get; set; }

        public DateTime? CreatedUtc { get; set; }

        public DateTime? UpdateUtc { get; set; }
    }
}