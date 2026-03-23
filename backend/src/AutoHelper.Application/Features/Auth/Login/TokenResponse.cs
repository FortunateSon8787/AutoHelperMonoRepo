namespace AutoHelper.Application.Features.Auth.Login;

/// <summary>
/// Contains the access and refresh tokens returned after successful authentication.
/// </summary>
public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
