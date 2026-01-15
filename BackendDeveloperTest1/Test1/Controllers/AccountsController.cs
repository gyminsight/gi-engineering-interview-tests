using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Test1.Contracts;
using Test1.Core;
using Test1.Models;
using static Test1.Controllers.MembersController;


namespace Test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    account.Guid,
    account.Status,
    account.PaymentAmount,
    account.PeriodStartUtc,
    account.PeriodEndUtc,
    account.NextBillingUtc
FROM
    account
LEFT JOIN
    location ON account.LocationUid = location.UID;
";
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
    account.Guid,
    account.Status,
    account.PaymentAmount,
    account.PeriodStartUtc,
    account.PeriodEndUtc,
    account.NextBillingUtc
FROM
    account
LEFT JOIN
    location ON account.LocationUid = location.UID;
";

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
    Guid,
    CreatedUtc,
    Status,
    AccountType,
    PaymentAmount,
    PendCancel,,
    PeriodStartUtc,
    PeriodEndUtc,
    NextBillingUtc,
) VALUES (
    @Guid,
    @LocationUid,
    @Status,
    @AccountType,
    @PaymentAmount,
    @PendCancel,
    @PeriodStartUtc,
    @PeriodEndUtc,
    @NextBillingUtc,
);";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                CreatedUtc = DateTime.UtcNow,
                Status = AccountStatusType.GREEN,
                PaymentAmount = 0.0,
                PendCancel = false,
                AccountType = AccountType.TERM,
                PeriodStartUtc = DateTime.UtcNow,
                PeriodEndUtc = DateTime.UtcNow.AddMonths(1),
                NextBillingUtc = DateTime.UtcNow.AddMonths(1)
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

            const string sql = @"
DELETE FROM account WHERE LocationUid = @Guid;
DELETE FROM location WHERE UID = @Guid;
";

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

        // GET /api/accounts/{id}/members
        // get all members associated with an account
        [HttpGet("{id:Guid}/members")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllMembers(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
                .ConfigureAwait(false);

            const string sql = @"
SELECT
    member.Guid,
    member.FirstName,
    member.LastName,
    member.Address,
    member.City,
    member.Locale,
    member.PostalCode,
    member.JoinedDateUtc,
    member.CancelDateUtc,
    member.Cancelled
FROM member
LEFT JOIN account ON member.AccountUid = account.UID
WHERE account.Guid = @AccountGuid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                AccountGuid = id
            });

            var rows = await dbContext.Session.QueryAsync<MemberDto>(template.RawSql, template.Parameters, dbContext.Transaction)
                .ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows);
        }

        public class AccountDto 
        {
            public Guid Guid {get;set;}
            public string Status {get;set;}
            public string PaymentAmount {get;set;}
            public string PeriodStartUtc {get;set;}
            public string PeriodEndUtc {get;set;}
            public string NextBillingUtc {get;set;}
        }

        
    }
}
