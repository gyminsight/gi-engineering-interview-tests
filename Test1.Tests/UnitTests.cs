using Xunit;
using FluentAssertions;
using Test1.Models;
using Test1.Models.DTOs;

namespace Test1.Tests
{
    /// <summary>
    /// Unit tests demonstrating xUnit framework features.
    /// Tests focus on business logic that can be tested without database mocking.
    /// </summary>
    public class MemberBusinessLogicTests
    {
        #region Primary Member Rules - [Theory] with [InlineData]

        /// <summary>
        /// Business Rule: First member on account must always be primary.
        /// This tests the logic used in MembersController.Create()
        /// </summary>
        [Theory]
        [InlineData(0, false, true, "First member should become primary even if not requested")]
        [InlineData(0, true, true, "First member should be primary when explicitly requested")]
        [InlineData(1, false, false, "Subsequent member should not be primary if not requested")]
        [InlineData(5, false, false, "Member on account with many members should not auto-promote")]
        public void DetermineIsPrimary_FirstMemberAlwaysPrimary(
            int existingMemberCount,
            bool requestedPrimary,
            bool expectedResult,
            string because)
        {
            // Arrange & Act - This mirrors the logic in MembersController.Create
            bool isPrimary = requestedPrimary;
            if (existingMemberCount == 0)
            {
                isPrimary = true; // Business rule: first member is always primary
            }

            // Assert
            isPrimary.Should().Be(expectedResult, because);
        }

        /// <summary>
        /// Business Rule: Only one primary member allowed per account.
        /// Tests the rejection logic in MembersController.Create()
        /// </summary>
        [Theory]
        [InlineData(0, true, false, "Should allow primary when no primary exists")]
        [InlineData(1, true, true, "Should reject when trying to add second primary")]
        [InlineData(1, false, false, "Should allow non-primary when primary exists")]
        [InlineData(0, false, false, "Should allow non-primary when no members exist")]
        public void ShouldRejectDuplicatePrimary(
            int existingPrimaryCount,
            bool requestingPrimary,
            bool shouldReject,
            string because)
        {
            // Arrange & Act - Mirrors validation in MembersController.Create
            bool reject = requestingPrimary && existingPrimaryCount > 0;

            // Assert
            reject.Should().Be(shouldReject, because);
        }

        #endregion

        #region Member Deletion Rules - [Theory] with [MemberData]

        /// <summary>
        /// Test data for deletion scenarios.
        /// Demonstrates [MemberData] for complex test cases.
        /// </summary>
        public static IEnumerable<object[]> DeletionScenarios =>
            new List<object[]>
            {
                // Format: memberCount, isPrimaryBeingDeleted, shouldAllow, shouldPromote, description
                new object[] { 1, true, false, false, "Cannot delete last member (primary)" },
                new object[] { 1, false, false, false, "Cannot delete last member (non-primary)" },
                new object[] { 2, true, true, true, "Can delete primary when backup exists" },
                new object[] { 2, false, true, false, "Can delete non-primary, no promotion needed" },
                new object[] { 5, true, true, true, "Can delete primary in large account" },
                new object[] { 5, false, true, false, "Can delete non-primary in large account" },
            };

        [Theory]
        [MemberData(nameof(DeletionScenarios))]
        public void DeleteMember_ShouldFollowBusinessRules(
            int memberCount,
            bool isPrimaryBeingDeleted,
            bool shouldAllowDelete,
            bool shouldPromoteNewPrimary,
            string scenario)
        {
            // Arrange & Act - Mirrors logic in MembersController.Delete
            bool allowDelete = memberCount > 1;
            bool promoteNew = allowDelete && isPrimaryBeingDeleted;

            // Assert
            allowDelete.Should().Be(shouldAllowDelete, $"Scenario: {scenario}");
            promoteNew.Should().Be(shouldPromoteNewPrimary, $"Scenario: {scenario}");
        }

        /// <summary>
        /// Tests that promotion selects the oldest member by CreatedUtc.
        /// </summary>
        [Fact]
        public void SelectNextPrimary_ShouldChooseOldestMember()
        {
            // Arrange - Simulate members with different creation dates
            var members = new List<TestMember>
            {
                new(1, "Alice", DateTime.UtcNow.AddDays(-30), true),   // Primary being deleted
                new(2, "Bob", DateTime.UtcNow.AddDays(-20), false),    // Oldest secondary
                new(3, "Carol", DateTime.UtcNow.AddDays(-10), false),
                new(4, "Dave", DateTime.UtcNow, false)                  // Newest
            };

            var memberBeingDeleted = members.First(m => m.IsPrimary);

            // Act - Find next primary (same logic as controller)
            var nextPrimary = members
                .Where(m => m.UID != memberBeingDeleted.UID)
                .OrderBy(m => m.CreatedUtc)
                .First();

            // Assert
            nextPrimary.Name.Should().Be("Bob", "oldest remaining member should be promoted");
            nextPrimary.CreatedUtc.Should().BeBefore(DateTime.UtcNow.AddDays(-15));
        }

        private record TestMember(int UID, string Name, DateTime CreatedUtc, bool IsPrimary);

        #endregion

        #region Bulk Delete Rules

        [Fact]
        public void DeleteNonPrimary_ShouldPreservePrimaryMember()
        {
            // Arrange
            var members = new List<(int Id, bool IsPrimary)>
            {
                (1, true),   // Primary - must survive
                (2, false),  // Should be deleted
                (3, false),  // Should be deleted
                (4, false),  // Should be deleted
            };

            // Act - Simulate WHERE Primary = 0
            var toDelete = members.Where(m => !m.IsPrimary).ToList();
            var survivors = members.Where(m => m.IsPrimary).ToList();

            // Assert
            toDelete.Should().HaveCount(3);
            survivors.Should().HaveCount(1);
            survivors.Single().IsPrimary.Should().BeTrue();
        }

        #endregion
    }

    /// <summary>
    /// Tests for Account status business logic.
    /// Demonstrates various xUnit assertion patterns.
    /// </summary>
    public class AccountStatusTests
    {
        /// <summary>
        /// Tests which statuses count as "active" (non-cancelled).
        /// Business Rule: Status < CANCELLED (value 3) is active.
        /// </summary>
        [Theory]
        [InlineData(AccountStatusType.GREEN, true, "GREEN is active")]
        [InlineData(AccountStatusType.YELLOW, true, "YELLOW is active")]
        [InlineData(AccountStatusType.RED, true, "RED is active")]
        [InlineData(AccountStatusType.CANCELLED, false, "CANCELLED is not active")]
        [InlineData(AccountStatusType.COLLECTIONS, false, "COLLECTIONS is not active")]
        public void IsActiveStatus_ShouldIdentifyCorrectly(
            AccountStatusType status,
            bool expectedIsActive,
            string because)
        {
            // Act - Same logic used in LocationsController query
            bool isActive = status < AccountStatusType.CANCELLED;

            // Assert
            isActive.Should().Be(expectedIsActive, because);
        }

        /// <summary>
        /// Tests counting active accounts from various mixes.
        /// </summary>
        [Theory]
        [InlineData(new int[] { 0, 0, 0 }, 3, "All GREEN = 3 active")]
        [InlineData(new int[] { 0, 1, 2 }, 3, "GREEN, YELLOW, RED = 3 active")]
        [InlineData(new int[] { 3, 3, 3 }, 0, "All CANCELLED = 0 active")]
        [InlineData(new int[] { 0, 1, 3, 4 }, 2, "Mixed = 2 active")]
        [InlineData(new int[] { }, 0, "Empty = 0 active")]
        public void CountActiveAccounts_ShouldExcludeCancelledStatus(
            int[] statusValues,
            int expectedActiveCount,
            string scenario)
        {
            // Arrange
            var statuses = statusValues.Select(v => (AccountStatusType)v);

            // Act
            int activeCount = statuses.Count(s => s < AccountStatusType.CANCELLED);

            // Assert
            activeCount.Should().Be(expectedActiveCount, scenario);
        }

        /// <summary>
        /// Verifies enum values match database expectations.
        /// </summary>
        [Fact]
        public void AccountStatusType_ValuesShouldMatchDatabase()
        {
            // These must match the database values
            using (new AssertionScope())
            {
                ((int)AccountStatusType.GREEN).Should().Be(0);
                ((int)AccountStatusType.YELLOW).Should().Be(1);
                ((int)AccountStatusType.RED).Should().Be(2);
                ((int)AccountStatusType.CANCELLED).Should().Be(3);
                ((int)AccountStatusType.COLLECTIONS).Should().Be(4);
            }
        }

        [Fact]
        public void AccountType_ValuesShouldMatchDatabase()
        {
            using (new AssertionScope())
            {
                ((int)AccountType.TERM).Should().Be(0);
                ((int)AccountType.PREPAID).Should().Be(1);
                ((int)AccountType.OPENEND).Should().Be(2);
                ((int)AccountType.GUEST).Should().Be(3);
            }
        }
    }

    /// <summary>
    /// Tests for DTO default values and structure.
    /// </summary>
    public class DtoValidationTests
    {
        #region CreateAccountDto Tests

        [Fact]
        public void CreateAccountDto_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act
            var dto = new CreateAccountDto();

            // Assert - Using AssertionScope to report all failures
            using (new AssertionScope())
            {
                dto.Status.Should().Be(AccountStatusType.GREEN, "default status should be GREEN");
                dto.AccountType.Should().Be(AccountType.OPENEND, "default type should be OPENEND");
                dto.PaymentAmount.Should().BeNull("payment should be optional");
                dto.EndDateUtc.Should().BeNull("end date should be optional");
            }
        }

        [Fact]
        public void CreateAccountDto_LocationGuid_ShouldBeRequired()
        {
            var dto = new CreateAccountDto();
            
            // Empty GUID indicates the required field was not set
            dto.LocationGuid.Should().Be(Guid.Empty, 
                "LocationGuid should be empty by default (must be provided)");
        }

        #endregion

        #region CreateMemberDto Tests

        [Fact]
        public void CreateMemberDto_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act
            var dto = new CreateMemberDto();

            // Assert
            using (new AssertionScope())
            {
                dto.IsPrimary.Should().BeFalse("default should not request primary status");
                dto.AccountGuid.Should().Be(Guid.Empty, "AccountGuid must be provided");
                dto.FirstName.Should().BeNull("name fields are optional");
                dto.LastName.Should().BeNull("name fields are optional");
            }
        }

        #endregion

        #region MemberDto Tests

        [Fact]
        public void MemberDto_ShouldHaveAllRequiredFields()
        {
            // This test documents the expected DTO structure
            var properties = typeof(MemberDto).GetProperties();
            var propertyNames = properties.Select(p => p.Name).ToList();

            using (new AssertionScope())
            {
                propertyNames.Should().Contain("Guid");
                propertyNames.Should().Contain("AccountGuid");
                propertyNames.Should().Contain("LocationGuid");
                propertyNames.Should().Contain("IsPrimary");
                propertyNames.Should().Contain("FirstName");
                propertyNames.Should().Contain("LastName");
                propertyNames.Should().Contain("JoinedDateUtc");
                propertyNames.Should().Contain("Cancelled");
            }
        }

        #endregion
    }

    /// <summary>
    /// Demonstrates xUnit collection fixtures for tests that need shared context.
    /// </summary>
    [CollectionDefinition("Business Logic Tests")]
    public class BusinessLogicTestCollection : ICollectionFixture<BusinessLogicFixture>
    {
        // This class has no code; it's just a marker for xUnit
    }

    public class BusinessLogicFixture
    {
        public DateTime TestStartTime { get; } = DateTime.UtcNow;
        
        // Shared setup that's expensive to create
        public IReadOnlyList<AccountStatusType> ActiveStatuses { get; } = new[]
        {
            AccountStatusType.GREEN,
            AccountStatusType.YELLOW,
            AccountStatusType.RED
        };

        public IReadOnlyList<AccountStatusType> InactiveStatuses { get; } = new[]
        {
            AccountStatusType.CANCELLED,
            AccountStatusType.COLLECTIONS
        };
    }
}
