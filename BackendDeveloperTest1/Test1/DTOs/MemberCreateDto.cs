namespace Test1.DTOs
{
    public class MemberCreateDto
    {
        
        public int UID { get; set; }
        public uint AccountUid { get; set; }
        public uint LocationUid { get; set; }
        public bool Primary { get; set; }
        public DateTime? JoinedDateUtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Locale { get; set; }
        public string PostalCode { get; set; }
        public bool Cancelled { get; set; }
        public DateTime? CancelDateUtc { get; set; }
    }
}
