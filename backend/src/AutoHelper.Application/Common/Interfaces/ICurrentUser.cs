namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user.
/// Implemented in Infrastructure using IHttpContextAccessor.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
