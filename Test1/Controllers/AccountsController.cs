using Microsoft.AspNetCore.Mvc;
using Dapper;
using Test1.Contracts;
using Test1.Models;
using Test1.Models.DTOs;
using Test1.Middleware;

namespace Test1.Controllers;

/// <summary>
/// Controller for managing gym membership accounts.
/// Provides CRUD operations and member management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ISessionFactory sessionFactory, ILogger<AccountsController> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all accounts.
    /// </summary>
    /// <returns>List of all accounts with their associated location.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountDto>>> List(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving all accounts");

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    a.Guid,
    a.Status,
    a.AccountType,
    a.PaymentAmount,
    a.CreatedUtc,
    a.UpdatedUtc,
    a.PeriodStartUtc,
    a.PeriodEndUtc,
    a.NextBillingUtc,
    a.EndDateUtc,
    a.PendCancel,
    a.PendCancelDateUtc,
    l.Guid AS LocationGuid
FROM account a
INNER JOIN location l ON a.LocationUid = l.UID;";

        var rows = await dbContext.Session.QueryAsync<AccountDto>(sql, null, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Retrieved {Count} accounts", rows.Count());
        return Ok(rows);
    }

    /// <summary>
    /// Get a specific account by GUID.
    /// </summary>
    /// <param name="id">The account's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested account.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving account {AccountId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    a.Guid,
    a.Status,
    a.AccountType,
    a.PaymentAmount,
    a.CreatedUtc,
    a.UpdatedUtc,
    a.PeriodStartUtc,
    a.PeriodEndUtc,
    a.NextBillingUtc,
    a.EndDateUtc,
    a.PendCancel,
    a.PendCancelDateUtc,
    l.Guid AS LocationGuid
FROM account a
INNER JOIN location l ON a.LocationUid = l.UID
WHERE a.Guid = @Guid;";

        var account = await dbContext.Session.QueryFirstOrDefaultAsync<AccountDto>(sql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found", id);
            throw new NotFoundException("Account", id);
        }

        return Ok(account);
    }

    /// <summary>
    /// Create a new account.
    /// </summary>
    /// <param name="model">The account data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created account's identifier.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateResponse>> Create([FromBody] CreateAccountDto model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating account at location {LocationGuid}", model.LocationGuid);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Validate location exists
        const string locationSql = "SELECT UID FROM location WHERE Guid = @LocationGuid;";
        var locationUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(locationSql, new { model.LocationGuid }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (locationUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Location {LocationGuid} not found", model.LocationGuid);
            throw new NotFoundException("Location", model.LocationGuid);
        }

        var newGuid = Guid.NewGuid();
        var now = DateTime.UtcNow;

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

        var parameters = new
        {
            LocationUid = locationUid.Value,
            Guid = newGuid,
            CreatedUtc = now,
            UpdatedUtc = (DateTime?)null,
            Status = model.Status,
            EndDateUtc = model.EndDateUtc,
            AccountType = model.AccountType,
            PaymentAmount = model.PaymentAmount,
            PendCancel = false,
            PendCancelDateUtc = (DateTime?)null,
            PeriodStartUtc = model.PeriodStartUtc ?? now,
            PeriodEndUtc = model.PeriodEndUtc ?? now.AddMonths(1),
            NextBillingUtc = model.NextBillingUtc ?? now.AddMonths(1)
        };

        var count = await dbContext.Session.ExecuteAsync(sql, parameters, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (count != 1)
        {
            _logger.LogError("Failed to create account - insert returned {Count}", count);
            throw new InvalidOperationException("Unable to create account");
        }

        _logger.LogInformation("Created account {AccountId}", newGuid);
        return CreatedAtAction(nameof(GetById), new { id = newGuid }, new CreateResponse { Guid = newGuid });
    }

    /// <summary>
    /// Update an existing account.
    /// </summary>
    /// <param name="id">The account's unique identifier.</param>
    /// <param name="model">The updated account data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Update(Guid id, [FromBody] UpdateAccountDto model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating account {AccountId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Validate account exists
        const string checkSql = "SELECT UID FROM account WHERE Guid = @Guid;";
        var accountUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(checkSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Account {AccountId} not found for update", id);
            throw new NotFoundException("Account", id);
        }

        const string sql = @"
UPDATE account SET
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

        var parameters = new
        {
            Guid = id,
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
        };

        var count = await dbContext.Session.ExecuteAsync(sql, parameters, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Updated account {AccountId}", id);
        return Ok(ApiResponse.Ok("Account updated successfully"));
    }

    /// <summary>
    /// Delete an account and all its members.
    /// </summary>
    /// <param name="id">The account's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion result including member cascade count.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(AccountDeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDeleteResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting account {AccountId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Validate account exists
        const string checkSql = "SELECT UID FROM account WHERE Guid = @Guid;";
        var accountUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(checkSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Account {AccountId} not found for deletion", id);
            throw new NotFoundException("Account", id);
        }

        // Delete all members associated with this account (cascade)
        const string deleteMembersSql = "DELETE FROM member WHERE AccountUid = @AccountUid;";
        var membersDeleted = await dbContext.Session.ExecuteAsync(deleteMembersSql, new { AccountUid = accountUid.Value }, dbContext.Transaction)
            .ConfigureAwait(false);

        // Delete the account
        const string sql = "DELETE FROM account WHERE Guid = @Guid;";
        var count = await dbContext.Session.ExecuteAsync(sql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Deleted account {AccountId} with {MemberCount} members", id, membersDeleted);

        return Ok(new AccountDeleteResponse
        {
            Success = true,
            Message = "Account deleted successfully",
            DeletedCount = count,
            MembersDeleted = membersDeleted
        });
    }

    /// <summary>
    /// Get all members for a specific account.
    /// Members are ordered with primary member first, then by creation date.
    /// </summary>
    /// <param name="id">The account's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of members belonging to the account.</returns>
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IEnumerable<MemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving members for account {AccountId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Validate account exists
        const string checkSql = "SELECT UID FROM account WHERE Guid = @Guid;";
        var accountUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(checkSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Account {AccountId} not found when retrieving members", id);
            throw new NotFoundException("Account", id);
        }

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
WHERE a.Guid = @AccountGuid
ORDER BY m.""Primary"" DESC, m.CreatedUtc ASC;";

        var members = await dbContext.Session.QueryAsync<MemberDto>(sql, new { AccountGuid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Retrieved {Count} members for account {AccountId}", members.Count(), id);
        return Ok(members);
    }

    /// <summary>
    /// Delete all non-primary members from an account.
    /// The primary member is always preserved.
    /// </summary>
    /// <param name="id">The account's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of deleted members.</returns>
    [HttpDelete("{id:guid}/members/non-primary")]
    [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResponse>> DeleteNonPrimaryMembers(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting non-primary members from account {AccountId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Validate account exists
        const string checkSql = "SELECT UID FROM account WHERE Guid = @Guid;";
        var accountUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(checkSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Account {AccountId} not found for non-primary member deletion", id);
            throw new NotFoundException("Account", id);
        }

        const string sql = @"DELETE FROM member WHERE AccountUid = @AccountUid AND ""Primary"" = 0;";

        var count = await dbContext.Session.ExecuteAsync(sql, new { AccountUid = accountUid.Value }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Deleted {Count} non-primary members from account {AccountId}", count, id);

        return Ok(new DeleteResponse
        {
            Success = true,
            Message = "Non-primary members deleted successfully",
            DeletedCount = count
        });
    }
}
