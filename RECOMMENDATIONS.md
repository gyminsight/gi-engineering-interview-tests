# Technical Recommendations

## Task 6: Database Index Recommendations

### Current Schema Analysis

The existing schema uses SQLite with foreign key relationships but lacks explicit indexes for query optimization. Based on the actual queries used in this application, here are prioritized recommendations:

### High Priority Indexes

These indexes directly support the queries used in the API endpoints:

```sql
-- 1. Account GUID Lookup (used in every account endpoint)
-- Query: WHERE a.Guid = @Guid
-- Impact: O(log n) vs O(n) for all account lookups
CREATE UNIQUE INDEX idx_account_guid ON account(Guid);

-- 2. Member GUID Lookup (used in every member endpoint)
-- Query: WHERE m.Guid = @Guid
CREATE UNIQUE INDEX idx_member_guid ON member(Guid);

-- 3. Members by Account (used for account member lists, deletion)
-- Query: WHERE AccountUid = @AccountUid
-- Also supports: DELETE FROM member WHERE AccountUid = @AccountUid
CREATE INDEX idx_member_accountuid ON member(AccountUid);

-- 4. Location GUID Lookup
-- Query: WHERE l.Guid = @Guid
CREATE UNIQUE INDEX idx_location_guid ON location(Guid);
```

### Medium Priority Indexes

These support specific business queries:

```sql
-- 5. Active Account Count per Location (Task 2 query)
-- Query: WHERE a.LocationUid = l.UID AND a.Status < @CancelledStatus
-- Composite index for the Location list query
CREATE INDEX idx_account_location_status ON account(LocationUid, Status);

-- 6. Primary Member Check and Promotion
-- Query: WHERE AccountUid = @AccountUid AND "Primary" = 1
-- Query: ORDER BY CreatedUtc ASC (for promotion)
CREATE INDEX idx_member_account_primary ON member(AccountUid, "Primary");

-- 7. Member Promotion by Age
-- Query: ORDER BY CreatedUtc ASC
CREATE INDEX idx_member_created ON member(AccountUid, CreatedUtc);
```

### Index Implementation Script

```sql
-- Run this script to add all recommended indexes
-- Safe to run multiple times (IF NOT EXISTS)

CREATE UNIQUE INDEX IF NOT EXISTS idx_account_guid ON account(Guid);
CREATE UNIQUE INDEX IF NOT EXISTS idx_member_guid ON member(Guid);
CREATE UNIQUE INDEX IF NOT EXISTS idx_location_guid ON location(Guid);
CREATE INDEX IF NOT EXISTS idx_member_accountuid ON member(AccountUid);
CREATE INDEX IF NOT EXISTS idx_account_location_status ON account(LocationUid, Status);
CREATE INDEX IF NOT EXISTS idx_member_account_primary ON member(AccountUid, "Primary");
CREATE INDEX IF NOT EXISTS idx_member_created ON member(AccountUid, CreatedUtc);
```

### Performance Impact Estimates

| Index | Affected Queries | Expected Improvement |
|-------|-----------------|---------------------|
| `idx_account_guid` | All account lookups | 90%+ for large datasets |
| `idx_member_guid` | All member lookups | 90%+ for large datasets |
| `idx_member_accountuid` | Account member lists, cascading deletes | 80%+ |
| `idx_account_location_status` | Location list with counts | 70%+ |

### SQLite-Specific Notes

- SQLite automatically indexes `PRIMARY KEY` columns (UID)
- Foreign keys do NOT get automatic indexes in SQLite
- Use `EXPLAIN QUERY PLAN` to verify index usage:
  ```sql
  EXPLAIN QUERY PLAN SELECT * FROM account WHERE Guid = 'xxx';
  ```
- For production migration to SQL Server/PostgreSQL, also consider:
  - Clustered vs non-clustered index strategy
  - Include columns for covering indexes
  - Fill factor for write-heavy tables

---

## Task 7: Code Structure and Maintainability Recommendations

### ✅ Already Implemented

The following improvements have been implemented in this codebase:

#### 1. Global Exception Handling
```csharp
// Middleware/ExceptionHandlingMiddleware.cs
app.UseGlobalExceptionHandling(); // In Program.cs
```
- Consistent RFC 7807 ProblemDetails responses
- Custom exceptions: `NotFoundException`, `BusinessRuleException`, `ValidationException`
- Environment-aware error details (verbose in development, safe in production)

#### 2. Input Validation with DataAnnotations
```csharp
public class CreateMemberDto
{
    [Required(ErrorMessage = "AccountGuid is required.")]
    public Guid AccountGuid { get; set; }
    
    [StringLength(100, ErrorMessage = "FirstName cannot exceed 100 characters.")]
    public string? FirstName { get; set; }
}
```

#### 3. Structured Logging
```csharp
_logger.LogInformation("Created member {MemberId} for account {AccountGuid}", newGuid, model.AccountGuid);
```
- ILogger<T> injection in all controllers
- Serilog with enriched context
- Request tracing with TraceId

#### 4. Standardized API Responses
```csharp
// Models/ApiResponses.cs
public class DeleteResponse { ... }
public class CreateResponse { ... }
public class MemberDeleteResponse : DeleteResponse { ... }
```

#### 5. Health Check Endpoint
```csharp
app.MapHealthChecks("/health");
```

#### 6. DTOs Consolidated
- All DTOs in `Models/DTOs.cs`
- LocationDto moved out of controller
- Proper namespacing

---

### 📋 Future Improvements (When Needed)

These are recommendations for when the codebase grows:

#### 1. Repository Pattern (When: >10 entities or complex queries)

**Current State**: SQL in controllers is acceptable for 3 entities with simple queries.

**When to Refactor**:
- Adding >3 more entities
- Complex business logic mixing multiple entities
- Need for query reuse across services
- Team prefers unit tests over integration tests

```csharp
// Example structure when needed:
public interface IAccountRepository
{
    Task<AccountDto?> GetByGuidAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<AccountDto>> GetAllAsync(CancellationToken ct);
    Task<Guid> CreateAsync(CreateAccountDto model, CancellationToken ct);
}
```

#### 2. Service Layer (When: Business logic exceeds CRUD)

**Current State**: Business logic is in controllers, which is fine for validation-level rules.

**When to Refactor**:
- Multi-entity transactions
- Complex workflows (e.g., account upgrade, bulk imports)
- Business rules needing unit test isolation

```csharp
// Example:
public class MemberService : IMemberService
{
    public async Task<MemberCreateResult> CreateMemberAsync(CreateMemberDto dto)
    {
        // Validate account exists
        // Check primary member rules
        // Create member
        // Send notifications (future)
        // Update analytics (future)
    }
}
```

#### 3. API Versioning (When: Breaking changes needed)

```csharp
// Add when API needs backward-incompatible changes:
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AccountsController : ControllerBase
```

#### 4. Caching (When: Read-heavy with stable data)

```csharp
// For location lists (rarely change):
builder.Services.AddMemoryCache();

// In controller:
if (!_cache.TryGetValue("locations", out var locations))
{
    locations = await GetLocationsFromDb();
    _cache.Set("locations", locations, TimeSpan.FromMinutes(5));
}
```

#### 5. Rate Limiting (When: Public API)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

#### 6. Background Jobs (When: Long-running operations)

For operations like:
- Bulk member imports
- Report generation
- Account status recalculation

Consider: Hangfire, Azure Functions, or hosted services.

---

### Testing Recommendations

#### Current Coverage
| Type | Files | Quality |
|------|-------|---------|
| Unit Tests | `UnitTests.cs` | Good - business logic |
| Integration Tests | `DatabaseIntegrationTests.cs` | Good - SQL verification |

#### Recommended Additions

1. **Controller Integration Tests** (using `WebApplicationFactory`):
```csharp
public class AccountsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", validDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

2. **API Contract Tests**:
- Verify response shapes match DTOs
- Test all error response formats

3. **Load Tests** (pre-production):
- k6 or NBomber for endpoint performance
- Verify index effectiveness

---

### Configuration Recommendations

#### Environment-Specific Settings

```json
// appsettings.Production.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    }
  }
}
```

#### Connection String Management

For production, use:
- Azure Key Vault
- AWS Secrets Manager
- Environment variables

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not configured");
```

---

### Documentation Recommendations

1. **OpenAPI/Swagger Enhancement**:
   - Add `[SwaggerOperation]` descriptions
   - Include request/response examples
   - Document error codes

2. **README Updates**:
   - API endpoint quick reference
   - Local development setup
   - Test execution instructions

3. **Architecture Decision Records (ADRs)**:
   - Document why Dapper over EF
   - Document primary member promotion logic
   - Document error handling strategy

---

## Summary

| Priority | Recommendation | Status | When to Implement |
|----------|---------------|--------|-------------------|
| High | Database indexes | 📋 Ready to run | Before production |
| High | Global exception handling | ✅ Done | - |
| High | Input validation | ✅ Done | - |
| High | Structured logging | ✅ Done | - |
| Medium | Repository pattern | 📋 Future | >10 entities |
| Medium | Service layer | 📋 Future | Complex workflows |
| Medium | API versioning | 📋 Future | Breaking changes |
| Low | Caching | 📋 Future | Performance needs |
| Low | Rate limiting | 📋 Future | Public API |

---

*These recommendations balance immediate needs with future scalability, following YAGNI (You Ain't Gonna Need It) principles while documenting the path forward.*
