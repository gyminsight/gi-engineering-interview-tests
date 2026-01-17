namespace Test1.DTOs
{
    public class AccountUpdateDto
    {
        public Guid Guid { get; set; }
        public Guid LocationGuid { get; set; }
        public uint Status { get; set; }
        public int AccountType { get; set; }
        public DateTime? PeriodStartUtc { get; set; }
        public DateTime? PeriodEndUtc { get; set; }
        public DateTime? NextBillingUtc { get; set; }
        public double? PaymentAmount { get; set; }
        public bool PendCancel { get; set; }
        public DateTime? PendCancelDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
    }
}
