using Microsoft.AspNetCore.Mvc;
using Dapper;
using Test1.Contracts;
using Test1.Models;

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
                INNER JOIN location l ON m.LocationUid = l.UID;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(sql);

            var rows = await dbContext.Session.QueryAsync<MemberDto>(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            return Ok(rows);
        }

        // POST: api/members
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] MemberDto model, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string accountLookupSql = "SELECT UID FROM account WHERE Guid = @AccountGuid;";

            var accountUid = await dbContext.Session.ExecuteScalarAsync<int?>(accountLookupSql, new { AccountGuid = model.AccountGuid }, dbContext.Transaction).ConfigureAwait(false);

            if (accountUid == null)
            {
                return BadRequest($"Account with GUID {model.AccountGuid} not found");
            }

            const string locationLookupSql = "SELECT UID FROM location WHERE Guid = @LocationGuid;";

            var locationUid = await dbContext.Session.ExecuteScalarAsync<int?>(locationLookupSql, new { LocationGuid = model.LocationGuid }, dbContext.Transaction).ConfigureAwait(false);

            if (locationUid == null)
            {
                return BadRequest($"Location with GUID {model.LocationGuid} not found");
            }

            if (model.Primary)
            {
                const string checkPrimarySql = "SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid AND \"Primary\" = 1;";

                var primaryCount = await dbContext.Session.ExecuteScalarAsync<int>(checkPrimarySql, new { AccountUid = accountUid.Value }, dbContext.Transaction).ConfigureAwait(false);

                if (primaryCount > 0)
                {
                    return BadRequest("Account already has a primary member");
                }
            }

            const string sql = @"
                INSERT INTO member (
                    Guid,
                    AccountUid,
                    LocationUid,
                    CreatedUtc,
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

            var template = builder.AddTemplate(sql, new
            {
                Guid = Guid.NewGuid(),
                AccountUid = accountUid.Value,
                LocationUid = locationUid.Value,
                CreatedUtc = DateTime.UtcNow,
                model.Primary,
                model.JoinedDateUtc,
                model.CancelDateUtc,
                model.FirstName,
                model.LastName,
                model.Address,
                model.City,
                model.Locale,
                model.PostalCode,
                model.Cancelled
            });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Unable to add member");
            }
        }

        // DELETE: api/members/{id}
        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult> DeleteById(Guid id, CancellationToken cancellationToken)
        {
            await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken).ConfigureAwait(false);

            const string getMemberSql = @"
                SELECT AccountUid, ""Primary""
                FROM member
                WHERE Guid = @Guid;";

            var member = await dbContext.Session.QueryFirstOrDefaultAsync<dynamic>(getMemberSql, new { Guid = id }, dbContext.Transaction).ConfigureAwait(false);

            if (member == null)
            {
                return NotFound($"Member with GUID {id} not found");
            }

            const string countMembersSql = "SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid;";

            var memberCount = await dbContext.Session.ExecuteScalarAsync<int>(countMembersSql, new { AccountUid = (int)member.AccountUid }, dbContext.Transaction).ConfigureAwait(false);

            if (memberCount <= 1)
            {
                return BadRequest("Cannot delete the last member on the account");
            }

            if (member.Primary == 1)
            {
                const string promoteNextSql = @"
                    UPDATE member
                    SET ""Primary"" = 1
                    WHERE UID = (
                        SELECT UID
                        FROM member
                        WHERE AccountUid = @AccountUid AND Guid != @Guid
                        LIMIT 1
                    );";

                await dbContext.Session.ExecuteAsync(promoteNextSql, new { AccountUid = (int)member.AccountUid, Guid = id }, dbContext.Transaction).ConfigureAwait(false);
            }

            const string deleteSql = "DELETE FROM member WHERE Guid = @Guid;";

            var builder = new SqlBuilder();

            var template = builder.AddTemplate(deleteSql, new { Guid = id });

            var count = await dbContext.Session.ExecuteAsync(template.RawSql, template.Parameters, dbContext.Transaction).ConfigureAwait(false);

            dbContext.Commit();

            if (count == 1)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Unable to delete member");
            }
        }
    }
}