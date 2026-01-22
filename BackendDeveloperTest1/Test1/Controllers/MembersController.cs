using Dapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using Test1.Contracts;
using Test1.Core;
using Test1.Models;
using static Test1.Controllers.AccountsController;
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

        // GET: api/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> List(CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
SELECT
  member.UID,
  member.Guid,
  member.AccountUid,
  member.LocationUid,
  member.CreatedUtc,
  member.UpdatedUtc,
  member.""Primary"",
  member.JoinedDateUtc,
  member.CancelDateUtc,
  member.FirstName,
  member.LastName,
  member.Address,
  member.City,
  member.Locale,
  member.PostalCode,
  member.Cancelled
FROM member;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            var rows = await dbContext.Session.QueryAsync<MemberDto>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows); // Returns an HTTP 200 OK status with the data
        }

        // POST: api/members
        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] MemberDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string primaryCheckSql = @"
SELECT
    member.Guid
FROM member
/**where**/;";

            var primaryCheckBuilder = new SqlBuilder();

            var primaryCheckTemplate = primaryCheckBuilder.AddTemplate(primaryCheckSql);

            primaryCheckBuilder.Where("member.AccountUid = @AccountUid AND member.\"Primary\" > 0", new
            {
                model.AccountUid
            });

            var primaryCheckRows = await dbContext.Session.QueryAsync<MemberDto>(primaryCheckTemplate.RawSql, primaryCheckTemplate.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            bool primaryExists = primaryCheckRows.Any();

            const string sql = @"
INSERT INTO member (
  Guid,
  AccountUid,
  LocationUid,
  CreatedUtc,
  UpdatedUtc,
  ""Primary"",
  JoinedDateUtc,
  CancelDateUtc,
  FirstName,
  LastName,
  Address,
  City,
  Locale,
  PostalCode,
  Cancelled
) VALUES (
  @Guid,
  @AccountUid,
  @LocationUid,
  @CreatedUtc,
  @UpdatedUtc,
  @Primary,
  @JoinedDateUtc,
  @CancelDateUtc,
  @FirstName,
  @LastName,
  @Address,
  @City,
  @Locale,
  @PostalCode,
  @Cancelled
);";

            var builder = new SqlBuilder();

            DateTime? UpdatedUtc = null;
            DateTime? CancelDateUtc = null;

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                model.AccountUid,
                model.LocationUid,
                CreatedUtc = DateTime.Now,
                UpdatedUtc,
                Primary = (primaryExists) ? 0: 1, // If there's already a primary member for the specified account, don't make this new member primary. If there isn't, make it primary.
                JoinedDateUtc = DateTime.Now,
                CancelDateUtc,
                model.FirstName,
                model.LastName,
                model.Address,
                model.City,
                model.Locale,
                model.PostalCode,
                Cancelled = 0
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
                return Ok();
            else
                return BadRequest("Unable to add member");
        }

        // DELETE: api/members/{Guid}
        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult<MemberDto>> DeleteById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string primaryCheckSql = @"
SELECT
    delmember.UID,
    delmember.Guid,
    delmember.AccountUid,
    delmember.LocationUid,
    delmember.""Primary"",
    COUNT(accountmember.AccountUid = delmember.AccountUid) AS AccountMembersCount
FROM member AS delmember
JOIN member AS accountmember
    ON accountmember.AccountUid = delmember.AccountUid
    AND delmember.Guid = @Guid;";

            var primaryCheckBuilder = new SqlBuilder();

            var primaryCheckTemplate = primaryCheckBuilder.AddTemplate(primaryCheckSql);

            primaryCheckBuilder.Where("Guid = @Guid", new
            {
                Guid = id
            });

            var primaryCheckRows = await dbContext.Session.QueryAsync<AccountMemberDto>(primaryCheckTemplate.RawSql, primaryCheckTemplate.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            AccountMemberDto delMember = primaryCheckRows.FirstOrDefault();

            if (delMember.Primary > 0) // if the specified member is the account's primary
            {
                if (delMember.AccountMembersCount < 2) // and if there isn't at least one other member on this account
                {
                    return BadRequest("Cannot delete last member of account");
                }
                else // but instead if there's at least one other member on this account
                {
                    // update the next member of this account to be primary
                    const string primaryUpdateSql = @"
UPDATE member
SET ""Primary"" = 1
WHERE UID = (
    SELECT
        UID
    FROM member
    WHERE
        AccountUid = @AccountUid
        AND ""Primary"" < 1
    ORDER BY
        CreatedUtc ASC,
        UID ASC
    LIMIT 1
)";

                    var primaryUpdateBuilder = new SqlBuilder();

                    // Set values of the fields for the chosen account to match those specified in the request,
                    //   but retain current values for fields not specified in the request (i.e., unspecified or null).
                    var primaryUpdateTemplate = primaryUpdateBuilder.AddTemplate(primaryUpdateSql, new
                    {
                        delMember.AccountUid
                    });

                    var primaryUpdateCount = await dbContext.Session.ExecuteAsync(primaryUpdateTemplate.RawSql, primaryUpdateTemplate.Parameters, dbContext.Transaction)
                        .ConfigureAwait(false);
                }
            }

            // If we've arrived at this point then either:
            //   a) The member chosen for deletion was not primary
            //   or b) The chosen member was primary, and we just set the successor to be primary when this one is gone
            // Either way, it's safe to delete the chosen member now.
            const string sql = "DELETE FROM member WHERE Guid = @Guid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = id
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
                return Ok();
            else
                return BadRequest("Unable to delete member");
        }

        public class AccountMemberDto
        {
            public int UID { get; set; }
            public Guid Guid { get; set; }
            public uint AccountUid { get; set; }
            public uint LocationUid { get; set; }
            public short Primary { get; set; }
            public int AccountMembersCount { get; set; }
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
