namespace Test1.DTOs
{
    public class MemberCreateDto
    {
        public Guid AccountGuid { get; set; }
        public Guid LocationGuid { get; set; }
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
