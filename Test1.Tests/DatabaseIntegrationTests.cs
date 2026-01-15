using Xunit;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Dapper;
using Test1.Models;

namespace Test1.Tests
{
    /// <summary>
    /// Integration tests using an in-memory SQLite database.
    /// These test the actual SQL queries used in the controllers.
    /// 
    /// Uses xUnit IClassFixture for shared database setup.
    /// </summary>
    public class DatabaseIntegrationTests : IClassFixture<SqliteFixture>
    {
        private readonly SqliteFixture _fixture;

        public DatabaseIntegrationTests(SqliteFixture fixture)
        {
            _fixture = fixture;
            _fixture.ResetDatabase(); // Clean slate for each test
        }

        #region Location Query Tests

        [Fact]
        public void LocationsQuery_ShouldReturnActiveAccountCount()
        {
            // Arrange - Create location with mixed status accounts
            var locationGuid = _fixture.CreateLocation("Test Gym");
            _fixture.CreateAccount(locationGuid, AccountStatusType.GREEN);
            _fixture.CreateAccount(locationGuid, AccountStatusType.YELLOW);
            _fixture.CreateAccount(locationGuid, AccountStatusType.CANCELLED);
            _fixture.CreateAccount(locationGuid, AccountStatusType.COLLECTIONS);

            // Act - Run the actual query from LocationsController
            const string sql = @"
SELECT
    l.Guid,
    l.Name,
    (SELECT COUNT(*) FROM account a WHERE a.LocationUid = l.UID AND a.Status < @CancelledStatus) AS ActiveAccountCount
FROM location l
WHERE l.Guid = @Guid;";

            var result = _fixture.Connection.QueryFirst<LocationResult>(sql, new
            {
                Guid = locationGuid,
                CancelledStatus = (int)AccountStatusType.CANCELLED
            });

            // Assert
            result.ActiveAccountCount.Should().Be(2, "only GREEN and YELLOW are active");
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0)]  // No accounts
        [InlineData(3, 0, 0, 0, 3)]  // 3 GREEN
        [InlineData(1, 1, 1, 0, 3)]  // 1 each active status
        [InlineData(0, 0, 0, 5, 0)]  // 5 CANCELLED
        [InlineData(2, 1, 0, 3, 3)]  // Mixed
        public void LocationsQuery_ActiveCount_VariousScenarios(
            int greenCount, int yellowCount, int redCount, int cancelledCount,
            int expectedActive)
        {
            // Arrange
            var locationGuid = _fixture.CreateLocation("Test");
            
            for (int i = 0; i < greenCount; i++)
                _fixture.CreateAccount(locationGuid, AccountStatusType.GREEN);
            for (int i = 0; i < yellowCount; i++)
                _fixture.CreateAccount(locationGuid, AccountStatusType.YELLOW);
            for (int i = 0; i < redCount; i++)
                _fixture.CreateAccount(locationGuid, AccountStatusType.RED);
            for (int i = 0; i < cancelledCount; i++)
                _fixture.CreateAccount(locationGuid, AccountStatusType.CANCELLED);

            // Act
            var count = _fixture.Connection.QueryFirst<int>(@"
                SELECT COUNT(*) FROM account a 
                INNER JOIN location l ON a.LocationUid = l.UID 
                WHERE l.Guid = @Guid AND a.Status < @CancelledStatus",
                new { Guid = locationGuid, CancelledStatus = (int)AccountStatusType.CANCELLED });

            // Assert
            count.Should().Be(expectedActive);
        }

        private record LocationResult(Guid Guid, string Name, int ActiveAccountCount);

        #endregion

        #region Member Query Tests

        [Fact]
        public void MemberQuery_ShouldOrderPrimaryFirst()
        {
            // Arrange
            var locationGuid = _fixture.CreateLocation("Test");
            var accountGuid = _fixture.CreateAccount(locationGuid, AccountStatusType.GREEN);
            
            // Create members in specific order (non-primary first, then primary)
            _fixture.CreateMember(accountGuid, false, "Secondary", "One");
            _fixture.CreateMember(accountGuid, true, "Primary", "Member");
            _fixture.CreateMember(accountGuid, false, "Secondary", "Two");

            // Act - Query with same ORDER BY as controller
            var members = _fixture.Connection.Query<MemberResult>(@"
                SELECT m.FirstName, m.""Primary"" AS IsPrimary
                FROM member m
                INNER JOIN account a ON m.AccountUid = a.UID
                WHERE a.Guid = @AccountGuid
                ORDER BY m.""Primary"" DESC, m.CreatedUtc ASC",
                new { AccountGuid = accountGuid }).ToList();

            // Assert
            members.Should().HaveCount(3);
            members[0].FirstName.Should().Be("Primary", "primary member should be first");
            members[0].IsPrimary.Should().BeTrue();
        }

        [Fact]
        public void DeleteNonPrimaryQuery_ShouldOnlyDeleteSecondaryMembers()
        {
            // Arrange
            var locationGuid = _fixture.CreateLocation("Test");
            var accountGuid = _fixture.CreateAccount(locationGuid, AccountStatusType.GREEN);
            var accountUid = _fixture.GetAccountUid(accountGuid);
            
            _fixture.CreateMember(accountGuid, true, "Primary", "Member");
            _fixture.CreateMember(accountGuid, false, "Secondary", "One");
            _fixture.CreateMember(accountGuid, false, "Secondary", "Two");

            // Act - Run the actual delete query from controller
            var deleted = _fixture.Connection.Execute(
                @"DELETE FROM member WHERE AccountUid = @AccountUid AND ""Primary"" = 0",
                new { AccountUid = accountUid });

            // Assert
            deleted.Should().Be(2, "should delete 2 non-primary members");
            
            var remaining = _fixture.Connection.QueryFirst<int>(
                "SELECT COUNT(*) FROM member WHERE AccountUid = @AccountUid",
                new { AccountUid = accountUid });
            remaining.Should().Be(1, "only primary member should remain");
        }

        private record MemberResult(string FirstName, bool IsPrimary);

        #endregion

        #region Member Promotion Tests

        [Fact]
        public void PromotionQuery_ShouldSelectOldestMember()
        {
            // Arrange
            var locationGuid = _fixture.CreateLocation("Test");
            var accountGuid = _fixture.CreateAccount(locationGuid, AccountStatusType.GREEN);
            var accountUid = _fixture.GetAccountUid(accountGuid);
            
            // Create members with deliberate time gaps
            var primaryGuid = _fixture.CreateMemberWithDate(accountGuid, true, "Primary", DateTime.UtcNow.AddDays(-30));
            Thread.Sleep(10); // Ensure different timestamps
            var oldestSecondary = _fixture.CreateMemberWithDate(accountGuid, false, "OldSecondary", DateTime.UtcNow.AddDays(-20));
            Thread.Sleep(10);
            var newestSecondary = _fixture.CreateMemberWithDate(accountGuid, false, "NewSecondary", DateTime.UtcNow.AddDays(-10));

            var primaryUid = _fixture.GetMemberUid(primaryGuid);

            // Act - Run the promotion selection query from controller
            var nextPrimaryUid = _fixture.Connection.QueryFirst<int>(@"
                SELECT UID FROM member 
                WHERE AccountUid = @AccountUid AND UID != @CurrentMemberUid
                ORDER BY CreatedUtc ASC
                LIMIT 1",
                new { AccountUid = accountUid, CurrentMemberUid = primaryUid });

            var nextPrimaryName = _fixture.Connection.QueryFirst<string>(
                "SELECT FirstName FROM member WHERE UID = @UID",
                new { UID = nextPrimaryUid });

            // Assert
            nextPrimaryName.Should().Be("OldSecondary", "oldest secondary should be promoted");
        }

        #endregion
    }

    /// <summary>
    /// Test fixture that provides a shared SQLite in-memory database.
    /// Implements IDisposable for cleanup.
    /// </summary>
    public class SqliteFixture : IDisposable
    {
        public SqliteConnection Connection { get; }

        public SqliteFixture()
        {
            // Use a shared in-memory database
            Connection = new SqliteConnection("Data Source=:memory:");
            Connection.Open();
            CreateSchema();
        }

        public void ResetDatabase()
        {
            Connection.Execute("DELETE FROM member");
            Connection.Execute("DELETE FROM account");
            Connection.Execute("DELETE FROM location");
        }

        private void CreateSchema()
        {
            Connection.Execute(@"
                CREATE TABLE location (
                    UID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Guid TEXT NOT NULL,
                    CreatedUtc TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Disabled INTEGER NOT NULL DEFAULT 0,
                    EnableBilling INTEGER NOT NULL DEFAULT 0,
                    AccountStatus INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE account (
                    UID INTEGER PRIMARY KEY AUTOINCREMENT,
                    LocationUid INTEGER NOT NULL,
                    Guid TEXT NOT NULL,
                    CreatedUtc TEXT NOT NULL,
                    Status INTEGER NOT NULL,
                    AccountType INTEGER NOT NULL DEFAULT 2,
                    PendCancel INTEGER NOT NULL DEFAULT 0,
                    PeriodStartUtc TEXT NOT NULL,
                    PeriodEndUtc TEXT NOT NULL,
                    NextBillingUtc TEXT NOT NULL,
                    FOREIGN KEY(LocationUid) REFERENCES location(UID)
                );

                CREATE TABLE member (
                    UID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Guid TEXT NOT NULL,
                    AccountUid INTEGER NOT NULL,
                    LocationUid INTEGER NOT NULL,
                    CreatedUtc TEXT NOT NULL,
                    ""Primary"" INTEGER NOT NULL,
                    JoinedDateUtc TEXT NOT NULL,
                    FirstName TEXT,
                    LastName TEXT,
                    Cancelled INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY(AccountUid) REFERENCES account(UID),
                    FOREIGN KEY(LocationUid) REFERENCES location(UID)
                );");
        }

        public Guid CreateLocation(string name)
        {
            var guid = Guid.NewGuid();
            Connection.Execute(@"
                INSERT INTO location (Guid, CreatedUtc, Name) 
                VALUES (@Guid, @CreatedUtc, @Name)",
                new { Guid = guid.ToString(), CreatedUtc = DateTime.UtcNow.ToString("O"), Name = name });
            return guid;
        }

        public Guid CreateAccount(Guid locationGuid, AccountStatusType status)
        {
            var locationUid = Connection.QueryFirst<int>(
                "SELECT UID FROM location WHERE Guid = @Guid",
                new { Guid = locationGuid.ToString() });

            var guid = Guid.NewGuid();
            var now = DateTime.UtcNow;
            Connection.Execute(@"
                INSERT INTO account (LocationUid, Guid, CreatedUtc, Status, PeriodStartUtc, PeriodEndUtc, NextBillingUtc)
                VALUES (@LocationUid, @Guid, @CreatedUtc, @Status, @PeriodStart, @PeriodEnd, @NextBilling)",
                new
                {
                    LocationUid = locationUid,
                    Guid = guid.ToString(),
                    CreatedUtc = now.ToString("O"),
                    Status = (int)status,
                    PeriodStart = now.ToString("O"),
                    PeriodEnd = now.AddMonths(1).ToString("O"),
                    NextBilling = now.AddMonths(1).ToString("O")
                });
            return guid;
        }

        public Guid CreateMember(Guid accountGuid, bool isPrimary, string firstName, string lastName = "Test")
        {
            return CreateMemberWithDate(accountGuid, isPrimary, firstName, DateTime.UtcNow, lastName);
        }

        public Guid CreateMemberWithDate(Guid accountGuid, bool isPrimary, string firstName, DateTime createdUtc, string lastName = "Test")
        {
            var accountInfo = Connection.QueryFirst<(int UID, int LocationUid)>(@"
                SELECT UID, LocationUid FROM account WHERE Guid = @Guid",
                new { Guid = accountGuid.ToString() });

            var guid = Guid.NewGuid();
            Connection.Execute(@"
                INSERT INTO member (Guid, AccountUid, LocationUid, CreatedUtc, ""Primary"", JoinedDateUtc, FirstName, LastName)
                VALUES (@Guid, @AccountUid, @LocationUid, @CreatedUtc, @Primary, @JoinedDateUtc, @FirstName, @LastName)",
                new
                {
                    Guid = guid.ToString(),
                    AccountUid = accountInfo.UID,
                    LocationUid = accountInfo.LocationUid,
                    CreatedUtc = createdUtc.ToString("O"),
                    Primary = isPrimary ? 1 : 0,
                    JoinedDateUtc = createdUtc.ToString("O"),
                    FirstName = firstName,
                    LastName = lastName
                });
            return guid;
        }

        public int GetAccountUid(Guid accountGuid) =>
            Connection.QueryFirst<int>("SELECT UID FROM account WHERE Guid = @Guid",
                new { Guid = accountGuid.ToString() });

        public int GetMemberUid(Guid memberGuid) =>
            Connection.QueryFirst<int>("SELECT UID FROM member WHERE Guid = @Guid",
                new { Guid = memberGuid.ToString() });

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
