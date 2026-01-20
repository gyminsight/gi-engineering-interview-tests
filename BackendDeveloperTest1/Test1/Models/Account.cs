
namespace Test1.Models
{
    public class Account
    {
        // "Uid" integer NOT NULL PRIMARY KEY AUTOINCREMENT
        public int Uid { get; set; }
        
        public int LocationUid { get; set; }

        public Guid Guid { get; set; }

        public DateTime CreatedUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public int Status { get; set;}

        // "EndDateUtc" datetime DEFAULT NULL - Nullable
        public DateTime? EndDateUtc { get; set; }

        public int AccountType { get; set; }

        // "PaymentAmount" double DEFAULT NULL - Nullable
        public double? PaymentAmount { get; set; }

        public bool PendCancel { get; set; }

        // "PendCancelDateUtc" datetime DEFAULT NULL - Nullable
        public DateTime? PendCancelDateUtc { get; set; }

        public DateTime PeriodStartUtc { get; set; }

        public DateTime PeriodEndUtc { get; set; }

        public DateTime NextBillingUtc { get; set; }

    }
}