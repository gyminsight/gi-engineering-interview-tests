using Microsoft.AspNetCore.Mvc;
using Dapper;
using Test1.Contracts;
using Test1.Models;
using Test1.Models.DTOs;
using Test1.Middleware;

namespace Test1.Controllers;

/// <summary>
/// Controller for managing gym locations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LocationsController : ControllerBase
{
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(ISessionFactory sessionFactory, ILogger<LocationsController> logger)
    {
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get all locations with active account counts.
    /// Active accounts are those with Status less than CANCELLED.
    /// </summary>
    /// <returns>List of locations with their active account counts.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LocationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationDto>>> List(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving all locations with active account counts");

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    l.Guid,
    l.Name,
    l.Address,
    l.City,
    l.Locale,
    l.PostalCode,
    (SELECT COUNT(*) FROM account a WHERE a.LocationUid = l.UID AND a.Status < @CancelledStatus) AS ActiveAccountCount
FROM location l;";

        var rows = await dbContext.Session.QueryAsync<LocationDto>(sql,
            new { CancelledStatus = (int)AccountStatusType.CANCELLED }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Retrieved {Count} locations", rows.Count());
        return Ok(rows);
    }

    /// <summary>
    /// Get a specific location by GUID.
    /// </summary>
    /// <param name="id">The location's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested location.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving location {LocationId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        const string sql = @"
SELECT
    l.Guid,
    l.Name,
    l.Address,
    l.City,
    l.Locale,
    l.PostalCode,
    (SELECT COUNT(*) FROM account a WHERE a.LocationUid = l.UID AND a.Status < @CancelledStatus) AS ActiveAccountCount
FROM location l
WHERE l.Guid = @Guid;";

        var location = await dbContext.Session.QueryFirstOrDefaultAsync<LocationDto>(sql,
            new { Guid = id, CancelledStatus = (int)AccountStatusType.CANCELLED }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (location == null)
        {
            _logger.LogWarning("Location {LocationId} not found", id);
            throw new NotFoundException("Location", id);
        }

        return Ok(location);
    }

    /// <summary>
    /// Create a new location.
    /// </summary>
    /// <param name="model">The location data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created location's identifier.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateResponse>> Create([FromBody] CreateLocationDto model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating location: {Name}", model.Name);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        var newGuid = Guid.NewGuid();

        const string sql = @"
INSERT INTO location (
    Guid,
    CreatedUtc,
    Disabled,
    EnableBilling,
    AccountStatus,
    Name,
    Address,
    City,
    Locale,
    PostalCode
) VALUES (
    @Guid,
    @CreatedUtc,
    @Disabled,
    @EnableBilling,
    @AccountStatus,
    @Name,
    @Address,
    @City,
    @Locale,
    @PostalCode
);";

        var parameters = new
        {
            Guid = newGuid,
            CreatedUtc = DateTime.UtcNow,
            Disabled = false,
            EnableBilling = false,
            AccountStatus = AccountStatusType.GREEN,
            model.Name,
            model.Address,
            model.City,
            model.Locale,
            model.PostalCode
        };

        var count = await dbContext.Session.ExecuteAsync(sql, parameters, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        if (count != 1)
        {
            _logger.LogError("Failed to create location - insert returned {Count}", count);
            throw new InvalidOperationException("Unable to create location");
        }

        _logger.LogInformation("Created location {LocationId}: {Name}", newGuid, model.Name);
        return CreatedAtAction(nameof(GetById), new { id = newGuid }, new CreateResponse { Guid = newGuid });
    }

    /// <summary>
    /// Delete a location.
    /// Note: Will fail if location has associated accounts due to foreign key constraints.
    /// </summary>
    /// <param name="id">The location's unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting location {LocationId}", id);

        await using var dbContext = await _sessionFactory.CreateContextAsync(cancellationToken)
            .ConfigureAwait(false);

        // Check if location exists
        const string checkSql = "SELECT UID FROM location WHERE Guid = @Guid;";
        var locationUid = await dbContext.Session.QueryFirstOrDefaultAsync<int?>(checkSql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (locationUid == null)
        {
            dbContext.Rollback();
            _logger.LogWarning("Location {LocationId} not found for deletion", id);
            throw new NotFoundException("Location", id);
        }

        // Check for associated accounts
        const string accountCheckSql = "SELECT COUNT(*) FROM account WHERE LocationUid = @LocationUid;";
        var accountCount = await dbContext.Session.QueryFirstOrDefaultAsync<int>(accountCheckSql, new { LocationUid = locationUid.Value }, dbContext.Transaction)
            .ConfigureAwait(false);

        if (accountCount > 0)
        {
            dbContext.Rollback();
            _logger.LogWarning("Cannot delete location {LocationId} - has {AccountCount} associated accounts", id, accountCount);
            throw new BusinessRuleException($"Cannot delete location with {accountCount} associated accounts. Delete accounts first.");
        }

        const string sql = "DELETE FROM location WHERE Guid = @Guid;";
        var count = await dbContext.Session.ExecuteAsync(sql, new { Guid = id }, dbContext.Transaction)
            .ConfigureAwait(false);

        dbContext.Commit();

        _logger.LogInformation("Deleted location {LocationId}", id);

        return Ok(new DeleteResponse
        {
            Success = true,
            Message = "Location deleted successfully",
            DeletedCount = count
        });
    }
}
