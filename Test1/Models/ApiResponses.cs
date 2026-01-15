namespace Test1.Models;

/// <summary>
/// Standard response wrapper for successful API operations.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T? Data { get; init; }
    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

/// <summary>
/// Standard response for operations that don't return data.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

/// <summary>
/// Response for delete operations that may affect multiple records.
/// </summary>
public class DeleteResponse
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }
    public int DeletedCount { get; init; }
}

/// <summary>
/// Response for create operations that return the new resource identifier.
/// </summary>
public class CreateResponse
{
    public Guid Guid { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Response for member creation that includes primary status.
/// </summary>
public class MemberCreateResponse : CreateResponse
{
    public bool IsPrimary { get; init; }
}

/// <summary>
/// Response for member deletion that indicates promotion.
/// </summary>
public class MemberDeleteResponse : DeleteResponse
{
    public bool NewPrimaryPromoted { get; init; }
}

/// <summary>
/// Response for account deletion with member cascade info.
/// </summary>
public class AccountDeleteResponse : DeleteResponse
{
    public int MembersDeleted { get; init; }
}
