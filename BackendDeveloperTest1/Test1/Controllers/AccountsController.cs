using Microsoft.AspNetCore.Mvc;
using Dapper;
using Test1.Contracts;
using Test1.Models;

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
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);
            const string sql = @"
                SELECT
                    a.Guid,
                    l.Guid AS LocationGuid,
                    a.Status,
                    a.AccountType,
                    a.PaymentAmount,
                    a.EndDateUtc,
                    a.PendCancel,
                    a.PendCancelDateUtc,
                    a.PeriodStartUtc,
                    a.PeriodEndUtc,
                    a.NextBillingUtc,
                    a.CreatedUtc,
                    a.UpdatedUtc
                FROM account a
                INNER JOIN location l ON a.LocationUid = l.UID
                /**where**/;";

            var builder = new SqlBuilder();
            var template = builder.AddTemplate(sql);

            var rows = await dbContext.Session.QueryAsync<AccountDto>(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows);
        }

        // GET: api/accounts/{id}
        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);
            const string sql = @"
                SELECT
                    a.Guid,
                    l.Guid AS LocationGuid,
                    a.Status,
                    a.AccountType,
                    a.PaymentAmount,
                    a.EndDateUtc,
                    a.PendCancel,
                    a.PendCancelDateUtc,
                    a.PeriodStartUtc,
                    a.PeriodEndUtc,
                    a.NextBillingUtc,
                    a.CreatedUtc,
                    a.UpdatedUtc
                FROM account a
                INNER JOIN location l ON a.LocationUid = l.UID
                /**where**/;";

            var builder = new SqlBuilder();
            var template = builder.AddTemplate(sql);

            builder.Where("a.Guid = @Guid", new { Guid = id });

            var rows = await dbContext.Session.QueryAsync<AccountDto>(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            var account = rows.FirstOrDefault();

            if (account == null)
            {
                return NotFound($"Account with GUID {id} not found");
            }

            return Ok(account);
        }

        // POST: api/accounts
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AccountDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string locationLookupSql = "SELECT UID FROM location WHERE Guid = @LocationGuid;";

            var locationUid = await dbContext.Session.ExecuteScalarAsync<int?>(locationLookupSql, new { LocationGuid = model.LocationGuid }, dbContext.Transaction).ConfigureAwait(false);

            if (locationUid == null)
            {
                return BadRequest($"Location with GUID {model.LocationGuid} not found");
            }

            const string sql = @"
                INSERT INTO account (
                    Guid,
                    LocationUid,
                    CreatedUtc,
                    Status,
                    AccountType,
                    PaymentAmount,
                    EndDateUtc,
                    PendCancel,
                    PendCancelDateUtc,
                    PeriodStartUtc,
                    PeriodEndUtc,
                    NextBillingUtc
                ) VALUES (
                    @Guid,
                    @LocationUid,
                    @CreatedUtc,
                    @Status,
                    @AccountType,
                    @PaymentAmount,
                    @EndDateUtc,
                    @PendCancel,
                    @PendCancelDateUtc,
                    @PeriodStartUtc,
                    @PeriodEndUtc,
                    @NextBillingUtc
                );";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                LocationUid = locationUid.Value,
                CreatedUtc = DateTime.UtcNow,
                model.Status,
                model.AccountType,
                model.PaymentAmount,
                model.EndDateUtc,
                model.PendCancel,
                model.PendCancelDateUtc,
                model.PeriodStartUtc,
                model.PeriodEndUtc,
                model.NextBillingUtc
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Unable to add account");
            }
        }

        // PUT: api/accounts/{id}
        [HttpPut("{id:Guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] AccountDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string locationLookupSql = "SELECT UID FROM location WHERE Guid = @LocationGuid;";

            var locationUid = await dbContext.Session.ExecuteScalarAsync<int?>(locationLookupSql, new { LocationGuid = model.LocationGuid }, dbContext.Transaction).ConfigureAwait(false);

            if (locationUid == null)
            {
                return BadRequest($"Location with GUID {model.LocationGuid} not found");
            }

            const string sql = @"
                UPDATE account
                SET
                    LocationUid = @LocationUid,
                    UpdatedUtc = @UpdatedUtc,
                    Status = @Status,
                    AccountType = @AccountType,
                    PaymentAmount = @PaymentAmount,
                    EndDateUtc = @EndDateUtc,
                    PendCancel = @PendCancel,
                    PendCancelDateUtc = @PendCancelDateUtc,
                    PeriodStartUtc = @PeriodStartUtc,
                    PeriodEndUtc = @PeriodEndUtc,
                    NextBillingUtc = @NextBillingUtc
                WHERE Guid = @Guid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                Guid = id,
                LocationUid = locationUid.Value,
                UpdatedUtc = DateTime.UtcNow,
                model.Status,
                model.AccountType,
                model.PaymentAmount,
                model.EndDateUtc,
                model.PendCancel,
                model.PendCancelDateUtc,
                model.PeriodStartUtc,
                model.PeriodEndUtc,
                model.NextBillingUtc
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Unable to update account");
            }
        }

        // DELETE: api/accounts/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult> DeleteById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string sql = "DELETE FROM account WHERE Guid = @Guid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new { Guid = id });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
            {
                return NoContent();
            }
            else if (count == 0)
            {
                return NotFound($"Account with GUID {id} not found");
            }
            else
            {
                return BadRequest("Unable to delete account");
            }
        }

        // GET: api/accounts/{id}/members
        [HttpGet("{id:Guid}/members")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string sql = @"
                SELECT
                    m.Guid,
                    a.Guid AS AccountGuid,
                    l.Guid AS LocationGuid,
                    m.""Primary"",
                    m.JoinedDateUtc,
                    m.CancelDateUtc,
                    m.FirstName,
                    m.LastName,
                    m.Address,
                    m.City,
                    m.Locale,
                    m.PostalCode,
                    m.Cancelled,
                    m.CreatedUtc,
                    m.UpdatedUtc
                FROM member m
                INNER JOIN account a ON m.AccountUid = a.UID
                INNER JOIN location l ON m.LocationUid = l.UID
                /**where**/;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            builder.Where("a.Guid = @AccountGuid", new
            {
                AccountGuid = id
            });

            var rows = await dbContext.Session.QueryAsync<MemberDto>(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows);
        }

        // DELETE: api/accounts/{id}/members
        [HttpDelete("{id:Guid}/members")]
        public async Task<ActionResult> DeleteNonPrimaryMembers(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string accountLookupSql = "SELECT UID FROM account WHERE Guid = @AccountGuid;";

            var accountUid = await dbContext.Session.ExecuteScalarAsync<int?>(accountLookupSql, new { AccountGuid = id }, dbContext.Transaction).ConfigureAwait(false);

            if (accountUid == null)
            {
                return NotFound($"Account with GUID {id} not found");
            }

            const string sql = @"
                DELETE FROM member
                WHERE AccountUid = @AccountUid AND ""Primary"" = 0;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql, new
            {
                AccountUid = accountUid.Value
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            return Ok(new { DeletedCount = count });
        }
    }
}