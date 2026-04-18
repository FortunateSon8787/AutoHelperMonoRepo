using AutoHelper.Domain.Admins;
using Shouldly;

namespace AutoHelper.Domain.Tests.Admins;

public class AdminRefreshTokenTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        const string token = "some-random-token";
        const int expiryDays = 7;

        // Act
        var rt = AdminRefreshToken.Create(adminUserId, token, expiryDays);

        // Assert
        rt.AdminUserId.ShouldBe(adminUserId);
        rt.Token.ShouldBe(token);
        rt.IsRevoked.ShouldBeFalse();
        rt.IsExpired.ShouldBeFalse();
        rt.IsActive.ShouldBeTrue();
        rt.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow.AddDays(expiryDays - 1));
        rt.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Revoke_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var rt = AdminRefreshToken.Create(Guid.NewGuid(), "token", 7);

        // Act
        rt.Revoke();

        // Assert
        rt.IsRevoked.ShouldBeTrue();
        rt.IsActive.ShouldBeFalse();
    }


}
