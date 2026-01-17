using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Test1.Contracts;
using Test1.Core;
using Test1.Models;
using static Test1.Controllers.LocationsController;
using static Test1.Controllers.MembersController;


namespace Test1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ISessionFactory _sessionFactory;

        public AccountsController(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        // GET: api/accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDto>>> List(CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
SELECT
    UID,
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
FROM account;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            var rows = await dbContext.Session.QueryAsync<AccountDto>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows); // Returns an HTTP 200 OK status with the data
        }

        // GET: api/accounts/{Guid}
        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
SELECT
    UID,
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
FROM account
/**where**/;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("Guid = @Guid", new
            {
                Guid = id
            });

            var rows = await dbContext.Session.QueryAsync<AccountDto>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows.FirstOrDefault()); // Returns an HTTP 200 OK status with the data
        }

        // POST: api/accounts
        [HttpPost]
        public async Task<ActionResult<string>> Create([FromBody] AccountDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
INSERT INTO account (
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
) VALUES (
    @LocationUid,
    @Guid,
    @CreatedUtc,
    @UpdatedUtc,
    @Status,
    @EndDateUtc,
    @AccountType,
    @PaymentAmount,
    @PendCancel,
    @PendCancelDateUtc,
    @PeriodStartUtc,
    @PeriodEndUtc,
    @NextBillingUtc
);";
            
            var builder = new SqlBuilder();

            // Pre-preparing values because adding null directly into the template's arguments is disallowed
            DateTime? UpdatedDateUTC = null;
            DateTime? EndDateUTC = null;
            DateTime? PaymentAmount = null;
            DateTime? PendCancelDateUTC = null;

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                model.LocationUid,
                CreatedDateUTC = DateTime.UtcNow,
                UpdatedDateUTC,
                model.Status,
                EndDateUTC,
                model.AccountType,
                PaymentAmount,
                model.PendCancel,
                PendCancelDateUTC,
                model.PeriodStartUtc,
                model.PeriodEndUtc,
                model.NextBillingUtc
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
                return Ok();
            else
                return BadRequest("Unable to add account");
        }

        // DELETE: api/accounts/{Guid}
        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult<AccountDto>> DeleteById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = "DELETE FROM account WHERE Guid = @Guid;";

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
                return BadRequest("Unable to delete account");
        }

        // UPDATE: api/accounts/{Guid}
        [HttpPost("{id:Guid}")]
        public async Task<ActionResult<AccountDto>> UpdateByID(Guid id, [FromBody] AccountDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            // Looking up current values for specified account
            const string lookupSql = @"
SELECT
    UID,
    LocationUid,
    Guid,
    CreatedUtc,
    UpdatedUtc,
    Status,
    EndDateUtc,
    AccountType,
    PaymentAmount,
    PendCancel,
    PendCancelDateUtc,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc
FROM account
/**where**/;";

            var lookupBuilder = new SqlBuilder();

            var lookupTemplate = lookupBuilder.AddTemplate(lookupSql);

            lookupBuilder.Where("Guid = @Guid", new
            {
                Guid = id
            });

            var lookupRows = await dbContext.Session.QueryAsync<AccountDto>(lookupTemplate.RawSql, lookupTemplate.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            AccountDto account = lookupRows.FirstOrDefault();

            if(account == null)
            {
                BadRequest("Unable to update account. Account not found.");
            }

            const string sql = @"
UPDATE account
SET LocationUid = @LocationUid,
    UpdatedUtc = @UpdatedUtc,
    Status = @Status,
    EndDateUtc = @EndDateUtc,
    AccountType = @AccountType,
    PaymentAmount = @PaymentAmount,
    PendCancel = @PendCancel,
    PendCancelDateUtc = @PendCancelDateUtc,
    PeriodStartUtc = @PeriodStartUtc,
    PeriodEndUtc = @PeriodEndUtc,
    NextBillingUtc = @NextBillingUtc
WHERE
    Guid = @Guid
;";

            var builder = new SqlBuilder();

            // Set values of the fields for the chosen account to match those specified in the request,
            //   but retain current values for fields not specified in the request (i.e., unspecified or null).
            var template = builder.AddTemplate(sql, new
            {
                account.Guid,
                LocationUid = model.LocationUid ?? account.LocationUid,
                UpdatedUtc = DateTime.Now,
                Status = model.Status ?? account.Status,
                EndDateUtc = model.EndDateUtc ?? account.EndDateUtc,
                AccountType = model.AccountType ?? account.AccountType,
                PaymentAmount = model.PaymentAmount ?? account.PaymentAmount,
                PendCancel = model.PendCancel ?? account.PendCancel,
                PendCancelDateUtc = model.PendCancelDateUtc ?? account.PendCancelDateUtc,
                PeriodStartUtc = model.PeriodStartUtc ?? account.PeriodStartUtc,
                PeriodEndUtc = model.PeriodEndUtc ?? account.PeriodEndUtc,
                NextBillingUtc = model.NextBillingUtc ?? account.NextBillingUtc
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
                return Ok();
            else
                return BadRequest("Unable to update account");
        }

        // GET: api/accounts/{Guid}/members
        [HttpGet("{id:Guid}/members")]
        public async Task<ActionResult<AccountDto>> ListMembers(Guid id, CancellationToken cancellationToken)
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
FROM member
    INNER JOIN account ON account.UID = member.AccountUid
/**where**/;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("account.Guid = @Guid", new
            {
                Guid = id
            });

            var rows = await dbContext.Session.QueryAsync<MemberDto>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows); // Returns an HTTP 200 OK status with the data
        }

        // DELETE: api/accounts/{Guid}/members
        [HttpDelete("{id:Guid}/members")]
        public async Task<ActionResult<AccountDto>> DeleteMembers(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
DELETE
FROM member
WHERE
    member.""Primary"" = 0
    AND member.AccountUid IN (
        SELECT
            account.UID
        FROM account
        WHERE
            account.Guid = @Guid
);";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = id
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            if (count > 0) // There should be 1 or more deletions
                return Ok();
            else
                return BadRequest("Unable to delete member(s)");
        }

        public class AccountDto
        {
            public int? UID { get; }
            public int? LocationUid { get; set; }
            public Guid? Guid { get; set; }
            public DateTime? CreatedUtc { get; set; }
            public DateTime? UpdatedUtc { get; set; }
            public AccountStatusType? Status { get; set; } // Assuming this is supposed to be an AccountStatusType. Unclear since the type also seems to apply to locations somehow.
            public DateTime? EndDateUtc { get; set; }
            public AccountType? AccountType { get; set; }
            public double? PaymentAmount { get; set; }
            public int? PendCancel { get; set; } // Unclear what this means
            public DateTime? PendCancelDateUtc { get; set; }
            public DateTime? PeriodStartUtc { get; set; }
            public DateTime? PeriodEndUtc { get; set; }
            public DateTime? NextBillingUtc { get; set; }
        }
    }
}
