namespace Test1.Models
{
    public class Member
    {
        public int Uid { get; set; }
        public Guid Guid { get; set; } 
        public Guid AccountGuid { get; set; }
        public Guid LocationGuid { get; set; }
        public uint AccountUid { get; set; }
        public uint LocationUid { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
        public bool Primary { get; set; }
        public DateTime? JoinedDateUtc { get; set; }
        public DateTime? CancelDateUtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Locale { get; set; }
        public string PostalCode { get; set; }
        public bool Cancelled { get; set; }
    }
}
