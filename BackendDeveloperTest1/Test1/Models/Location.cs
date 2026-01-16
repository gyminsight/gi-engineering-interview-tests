using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System;

namespace Test1.Models
{
    public class Location
    {
        public uint UID { get; set; }
        public string Guid { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? UpdatedUtc { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public bool EnableBilling { get; set; }
        public int AccountStatus { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Locale { get; set; }
        public string? PostalCode { get; set; }
    }
}
