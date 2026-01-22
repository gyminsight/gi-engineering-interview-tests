using Microsoft.AspNetCore.Mvc;
using Dapper;
using Test1.Contracts;
using Test1.Models;
using Test1.Models.DTOs;
using Test1.Middleware;

namespace Test1.Controllers;

/// <summary>
/// Controller for managing gym members.
/// Handles member creation with primary member rules and deletion with promotion logic.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<MembersController> _logger;

    public MembersController(ISessionFactory sessionFactory, ILogger<MembersController> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all members across all accounts.
    /// Members are grouped by account with primary members first.
    /// </summary>
    /// <returns>List of all members.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MemberDto>>> List(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving all members");

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    m.Guid,
    m.""Primary"" AS IsPrimary,
    m.FirstName,
    m.LastName,
    m.Address,
    m.City,
    m.Locale,
    m.PostalCode,
    m.JoinedDateUtc,
    m.CancelDateUtc,
    m.Cancelled,
    m.CreatedUtc,
    m.UpdatedUtc,
    a.Guid AS AccountGuid,
    l.Guid AS LocationGuid
FROM member m
INNER JOIN account a ON m.AccountUid = a.UID
INNER JOIN location l ON m.LocationUid = l.UID
ORDER BY a.Guid, m.""Primary"" DESC, m.CreatedUtc ASC;";

        var members = await dbContext.Session.QueryAsync<MemberDto>(sql, null, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Retrieved {Count} members", members.Count());
        return Ok(members);
    }

    /// <summary>
    /// Get a specific member by GUID.
    /// </summary>
    /// <param name="id">The member's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested member.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving member {MemberId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    m.Guid,
    m.""Primary"" AS IsPrimary,
    m.FirstName,
    m.LastName,
    m.Address,
    m.City,
    m.Locale,
    m.PostalCode,
    m.JoinedDateUtc,
    m.CancelDateUtc,
    m.Cancelled,
    m.CreatedUtc,
    m.UpdatedUtc,
    a.Guid AS AccountGuid,
    l.Guid AS LocationGuid
FROM member m
INNER JOIN account a ON m.AccountUid = a.UID
INNER JOIN location l ON m.LocationUid = l.UID
WHERE m.Guid = @Guid;";

        var member = await dbContext.Session.QueryFirstOrDefaultAsync<MemberDto>(sql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (member == null)
        {
            _logger.LogWarning("Member {MemberId} not found", id);
            throw new NotFoundException("Member", id);
        }

        return Ok(member);
    }

    /// <summary>
    /// Create a new member.
    /// 
    /// Business Rules:
    /// - First member on an account is automatically made primary.
    /// - Only one primary member is allowed per account.
    /// - Attempting to create a second primary member will fail.
    /// </summary>
    /// <param name="model">The member data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created member's identifier and primary status.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MemberCreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberCreateResponse>> Create([FromBody] CreateMemberDto model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating member for account {AccountGuid}, IsPrimary: {IsPrimary}",
            model.AccountGuid, model.IsPrimary);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Get account info
        const string accountSql = "SELECT UID, LocationUid FROM account WHERE Guid = @AccountGuid;";
        var accountInfo = await dbContext.Session.QueryFirstOrDefaultAsync<AccountInfo>(accountSql, new { model.AccountGuid }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountInfo == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Account {AccountGuid} not found when creating member", model.AccountGuid);
            throw new NotFoundException("Account", model.AccountGuid);
        }

        // Check existing primary member count
        const string primaryCheckSql = @"SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid AND ""Primary"" = 1;";
        var primaryCount = await dbContext.Session.QueryFirstOrDefaultAsync<int>(primaryCheckSql, new { AccountUid = accountInfo.UID }, dbContext.Transaction)
            .ConfigureAwait(false);

        // Business Rule: Reject if trying to create a second primary member
        if (model.IsPrimary && primaryCount > 0)
        {
            dbContext.Rollback();
            _logger.LogWarning("Rejected attempt to create second primary member on account {AccountGuid}", model.AccountGuid);
            throw new BusinessRuleException("This account already has a primary member. Only one primary member is allowed per account.");
        }

        // Check total member count for auto-promotion
        const string memberCountSql = "SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid;";
        var memberCount = await dbContext.Session.QueryFirstOrDefaultAsync<int>(memberCountSql, new { AccountUid = accountInfo.UID }, dbContext.Transaction)
            .ConfigureAwait(false);

        // Business Rule: First member is always primary
        bool isPrimary = model.IsPrimary || memberCount == 0;

        var newGuid = Guid.NewGuid();
        var now = DateTime.UtcNow;

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

        var parameters = new
        {
            Guid = newGuid,
            AccountUid = accountInfo.UID,
            LocationUid = accountInfo.LocationUid,
            CreatedUtc = now,
            UpdatedUtc = (DateTime?)null,
            Primary = isPrimary ? 1 : 0,
            JoinedDateUtc = now,
            CancelDateUtc = (DateTime?)null,
            model.FirstName,
            model.LastName,
            model.Address,
            model.City,
            model.Locale,
            model.PostalCode,
            Cancelled = 0
        };

        var count = await dbContext.Session.ExecuteAsync(sql, parameters, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (count != 1)
        {
            _logger.LogError("Failed to create member - insert returned {Count}", count);
            throw new InvalidOperationException("Unable to create member");
        }

        _logger.LogInformation("Created member {MemberId} (Primary: {IsPrimary}) for account {AccountGuid}",
            newGuid, isPrimary, model.AccountGuid);

        return CreatedAtAction(nameof(GetById), new { id = newGuid },
            new MemberCreateResponse { Guid = newGuid, IsPrimary = isPrimary });
    }

    /// <summary>
    /// Delete a member.
    /// 
    /// Business Rules:
    /// - Cannot delete the last member on an account.
    /// - When deleting the primary member, the oldest remaining member is promoted to primary.
    /// </summary>
    /// <param name="id">The member's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion result including promotion info.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(MemberDeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberDeleteResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting member {MemberId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Get member info
        const string memberSql = @"SELECT UID, AccountUid, ""Primary"" AS IsPrimary FROM member WHERE Guid = @Guid;";
        var memberInfo = await dbContext.Session.QueryFirstOrDefaultAsync<MemberInfo>(memberSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (memberInfo == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Member {MemberId} not found for deletion", id);
            throw new NotFoundException("Member", id);
        }

        // Count members on account
        const string countSql = "SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid;";
        var memberCount = await dbContext.Session.QueryFirstOrDefaultAsync<int>(countSql, new { memberInfo.AccountUid }, dbContext.Transaction)
            .ConfigureAwait(false);

        // Business Rule: Cannot delete the last member
        if (memberCount <= 1)
        {
            dbContext.Rollback();
            _logger.LogWarning("Rejected attempt to delete last member {MemberId} on account", id);
            throw new BusinessRuleException("Cannot delete the last member on an account. An account must have at least one member.");
        }

        bool promotedNewPrimary = false;

        // Business Rule: Promote another member if deleting the primary
        if (memberInfo.IsPrimary)
        {
            const string nextMemberSql = @"
SELECT UID FROM member 
WHERE AccountUid = @AccountUid AND UID != @CurrentMemberUid
ORDER BY CreatedUtc ASC
LIMIT 1;";

            var nextMemberUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(nextMemberSql,
                new { memberInfo.AccountUid, CurrentMemberUid = memberInfo.UID }, dbContext.Transaction)
                .ConfigureAwait(false);

            if (nextMemberUid != null)
            {
                const string promoteSql = @"UPDATE member SET ""Primary"" = 1, UpdatedUtc = @UpdatedUtc WHERE UID = @UID;";
                await dbContext.Session.ExecuteAsync(promoteSql,
                    new { UID = nextMemberUid.Value, UpdatedUtc = DateTime.UtcNow }, dbContext.Transaction)
                    .ConfigureAwait(false);

                promotedNewPrimary = true;
                _logger.LogInformation("Promoted member UID {NewPrimaryUid} to primary", nextMemberUid);
            }
        }

        // Delete the member
        const string deleteSql = "DELETE FROM member WHERE Guid = @Guid;";
        var deleteCount = await dbContext.Session.ExecuteAsync(deleteSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Deleted member {MemberId}, NewPrimaryPromoted: {Promoted}", id, promotedNewPrimary);

        return Ok(new MemberDeleteResponse
        {
            Success = true,
            Message = "Member deleted successfully",
            DeletedCount = deleteCount,
            NewPrimaryPromoted = promotedNewPrimary
        });
    }

    #region Internal Helper Classes

    private sealed class AccountInfo
    {
        public int UID { get; init; }
        public int LocationUid { get; init; }
    }

    private sealed class MemberInfo
    {
        public int UID { get; init; }
        public int AccountUid { get; init; }
        public bool IsPrimary { get; init; }
    }

    #endregion
}
