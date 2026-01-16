using Dapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Test1.Contracts;
using Test1.Core;
using Test1.Models;
using static Test1.Controllers.LocationsController;


namespace Test1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly ISessionFactory _sessionFactory;

        public MembersController(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public class MemberDto
        {
            public int UID { get; set; }
            public Guid Guid { get; set; }
            public uint AccountUid { get; set; }
            public uint LocationUid { get; set; }
            public DateTime CreatedUtc { get; set; }
            public DateTime? UpdatedUtc { get; set; }
            public short Primary {  get; set; }
            public DateTime JoinedDateUtc { get; set; }
            public DateTime? CancelDateUtc { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string Locale { get; set; }
            public string PostalCode { get; set; }
            public short Cancelled { get; set; }
        }
    }
}
