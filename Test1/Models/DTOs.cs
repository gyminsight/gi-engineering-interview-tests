#nullable enable
using System.ComponentModel.DataAnnotations;
using Test1.Models;

namespace Test1.Models.DTOs;

/// <summary>
/// Data transfer object for Account responses.
/// </summary>
public class AccountDto
{
    public Guid Guid { get; set; }
    public Guid LocationGuid { get; set; }
    public AccountStatusType Status { get; set; }
    public AccountType AccountType { get; set; }
    public double? PaymentAmount { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public DateTime PeriodStartUtc { get; set; }
    public DateTime PeriodEndUtc { get; set; }
    public DateTime NextBillingUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool PendCancel { get; set; }
    public DateTime? PendCancelDateUtc { get; set; }
}

/// <summary>
/// Data transfer object for creating new Accounts.
/// </summary>
public class CreateAccountDto
{
    /// <summary>
    /// The GUID of the location where this account will be created.
    /// </summary>
    [Required(ErrorMessage = "LocationGuid is required.")]
    public Guid LocationGuid { get; set; }

    /// <summary>
    /// Initial status of the account. Defaults to GREEN.
    /// </summary>
    [EnumDataType(typeof(AccountStatusType), ErrorMessage = "Invalid account status.")]
    public AccountStatusType Status { get; set; } = AccountStatusType.GREEN;

    /// <summary>
    /// Type of the account. Defaults to OPENEND.
    /// </summary>
    [EnumDataType(typeof(AccountType), ErrorMessage = "Invalid account type.")]
    public AccountType AccountType { get; set; } = AccountType.OPENEND;

    /// <summary>
    /// Optional payment amount for the account.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "PaymentAmount must be a positive value.")]
    public double? PaymentAmount { get; set; }

    public DateTime? EndDateUtc { get; set; }
    public DateTime? PeriodStartUtc { get; set; }
    public DateTime? PeriodEndUtc { get; set; }
    public DateTime? NextBillingUtc { get; set; }
}

/// <summary>
/// Data transfer object for updating existing Accounts.
/// </summary>
public class UpdateAccountDto
{
    [EnumDataType(typeof(AccountStatusType), ErrorMessage = "Invalid account status.")]
    public AccountStatusType Status { get; set; }

    [EnumDataType(typeof(AccountType), ErrorMessage = "Invalid account type.")]
    public AccountType AccountType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "PaymentAmount must be a positive value.")]
    public double? PaymentAmount { get; set; }

    public DateTime? EndDateUtc { get; set; }
    public bool PendCancel { get; set; }
    public DateTime? PendCancelDateUtc { get; set; }

    [Required(ErrorMessage = "PeriodStartUtc is required.")]
    public DateTime PeriodStartUtc { get; set; }

    [Required(ErrorMessage = "PeriodEndUtc is required.")]
    public DateTime PeriodEndUtc { get; set; }

    [Required(ErrorMessage = "NextBillingUtc is required.")]
    public DateTime NextBillingUtc { get; set; }
}

/// <summary>
/// Data transfer object for Member responses.
/// </summary>
public class MemberDto
{
    public Guid Guid { get; set; }
    public Guid AccountGuid { get; set; }
    public Guid LocationGuid { get; set; }
    public bool IsPrimary { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Locale { get; set; }
    public string? PostalCode { get; set; }
    public DateTime JoinedDateUtc { get; set; }
    public DateTime? CancelDateUtc { get; set; }
    public bool Cancelled { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedUtc { get; set; }
}

/// <summary>
/// Data transfer object for creating new Members.
/// </summary>
public class CreateMemberDto
{
    /// <summary>
    /// The GUID of the account this member will belong to.
    /// </summary>
    [Required(ErrorMessage = "AccountGuid is required.")]
    public Guid AccountGuid { get; set; }

    /// <summary>
    /// Whether this member should be the primary member.
    /// Note: First member on an account is always made primary regardless of this value.
    /// Only one primary member is allowed per account.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// First name of the member.
    /// </summary>
    [StringLength(45, ErrorMessage = "FirstName cannot exceed 45 characters.")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the member.
    /// </summary>
    [StringLength(45, ErrorMessage = "LastName cannot exceed 45 characters.")]
    public string? LastName { get; set; }

    /// <summary>
    /// Street address of the member.
    /// </summary>
    [StringLength(45, ErrorMessage = "Address cannot exceed 45 characters.")]
    public string? Address { get; set; }

    /// <summary>
    /// City of the member.
    /// </summary>
    [StringLength(45, ErrorMessage = "City cannot exceed 45 characters.")]
    public string? City { get; set; }

    /// <summary>
    /// State/Province/Locale of the member.
    /// </summary>
    [StringLength(16, ErrorMessage = "Locale cannot exceed 16 characters.")]
    public string? Locale { get; set; }

    /// <summary>
    /// Postal/Zip code of the member.
    /// </summary>
    [StringLength(16, ErrorMessage = "PostalCode cannot exceed 16 characters.")]
    public string? PostalCode { get; set; }
}

/// <summary>
/// Data transfer object for Location responses.
/// </summary>
public class LocationDto
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Locale { get; set; }
    public string? PostalCode { get; set; }

    /// <summary>
    /// Count of non-cancelled accounts at this location.
    /// Only populated in list queries.
    /// </summary>
    public int ActiveAccountCount { get; set; }
}

/// <summary>
/// Data transfer object for creating new Locations.
/// </summary>
public class CreateLocationDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(45, ErrorMessage = "Name cannot exceed 45 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(45, ErrorMessage = "Address cannot exceed 45 characters.")]
    public string? Address { get; set; }

    [StringLength(45, ErrorMessage = "City cannot exceed 45 characters.")]
    public string? City { get; set; }

    [StringLength(45, ErrorMessage = "Locale cannot exceed 45 characters.")]
    public string? Locale { get; set; }

    [StringLength(16, ErrorMessage = "PostalCode cannot exceed 16 characters.")]
    public string? PostalCode { get; set; }
}
