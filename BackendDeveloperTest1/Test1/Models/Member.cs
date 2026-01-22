namespace Test1.Models
{
    public class Member
    {
        public int Uid { get; set; }

        public Guid Guid { get; set; }

        public int AccountUid { get; set; }

        public int LocationUid { get; set; }

        public DateTime CreatedUtc { get; set; }

        // "UpdatedUtc" datetime DEFAULT NULL - Nullable
        public DateTime? UpdatedUtc { get; set; }

        // Primary 'tinyint' (0 = false, 1 = true) - makes code easier to write
        public bool Primary { get; set; }

        public DateTime JoinedDateUtc { get; set; }

        // "CancelDateUtc" datetime DEFAULT NULL - Nullable
        public DateTime? CancelDateUtc { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Locale { get; set; }

        public string PostalCode { get; set; }

        //Cancelled 'tinyint' (0 = false, 1 = true) - makes code easier to write
        public bool Cancelled { get; set; }

    }
}