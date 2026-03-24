using System.IdentityModel.Tokens.Jwt;
using AutoHelper.Domain.Customers;
using AutoHelper.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Shouldly;

namespace AutoHelper.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly JwtSettings _settings = new()
    {
        Secret = "super-secret-key-that-is-long-enough-for-hmac256",
        Issuer = "AutoHelper",
        Audience = "AutoHelper.Client",
        AccessTokenExpiryMinutes = 15,
        RefreshTokenExpiryDays = 30
    };

    private JwtTokenService CreateSut() =>
        new(Options.Create(_settings));

    // ─── GenerateAccessToken ──────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtString()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var sut = CreateSut();

        // Act
        var token = sut.GenerateAccessToken(customer);

        // Assert — must be a non-empty three-part JWT (header.payload.signature)
        token.ShouldNotBeNullOrWhiteSpace();
        token.Split('.').Length.ShouldBe(3);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectCustomerClaims()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var rawToken = sut.GenerateAccessToken(customer);
        var parsed = handler.ReadJwtToken(rawToken);

        // Assert — standard identity claims
        parsed.Subject.ShouldBe(customer.Id.ToString());
        parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
            .ShouldBe(customer.Email);
        parsed.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value
            .ShouldBe(customer.Name);
        parsed.Claims.FirstOrDefault(c => c.Type == "subscription")?.Value
            .ShouldBe(customer.SubscriptionStatus.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUniqueJtiPerToken()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Bob", "bob@test.com", "hash");
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token1 = handler.ReadJwtToken(sut.GenerateAccessToken(customer));
        var token2 = handler.ReadJwtToken(sut.GenerateAccessToken(customer));

        // Assert — each token must have a unique jti
        var jti1 = token1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = token2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        jti1.ShouldNotBe(jti2);
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseConfiguredIssuerAndAudience()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Carol", "carol@test.com", "hash");
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var parsed = handler.ReadJwtToken(sut.GenerateAccessToken(customer));

        // Assert
        parsed.Issuer.ShouldBe(_settings.Issuer);
        parsed.Audiences.ShouldContain(_settings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldExpireAfterConfiguredMinutes()
    {
        // Arrange
        var customer = Customer.CreateWithPassword("Dave", "dave@test.com", "hash");
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();
        var before = DateTime.UtcNow;

        // Act
        var parsed = handler.ReadJwtToken(sut.GenerateAccessToken(customer));

        // Assert — expiry must be roughly AccessTokenExpiryMinutes from now
        var expectedExpiry = before.AddMinutes(_settings.AccessTokenExpiryMinutes);
        parsed.ValidTo.ShouldBeGreaterThanOrEqualTo(before.AddMinutes(_settings.AccessTokenExpiryMinutes - 1));
        parsed.ValidTo.ShouldBeLessThanOrEqualTo(expectedExpiry.AddSeconds(5)); // allow small clock drift
    }

    // ─── GenerateRefreshToken ─────────────────────────────────────────────────

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyBase64String()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var token = sut.GenerateRefreshToken();

        // Assert
        token.ShouldNotBeNullOrWhiteSpace();

        // 64 random bytes encoded as base64 → 88 characters (with padding)
        Convert.TryFromBase64String(token, new byte[128], out var bytesWritten).ShouldBeTrue();
        bytesWritten.ShouldBe(64);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokenEachCall()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var tokens = Enumerable.Range(0, 10).Select(_ => sut.GenerateRefreshToken()).ToList();

        // Assert — all tokens must be distinct (collision probability is negligible for 64-byte random values)
        tokens.Distinct().Count().ShouldBe(tokens.Count);
    }

    // ─── RefreshTokenExpiryDays ───────────────────────────────────────────────

    [Fact]
    public void RefreshTokenExpiryDays_ShouldReturnValueFromSettings()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        sut.RefreshTokenExpiryDays.ShouldBe(_settings.RefreshTokenExpiryDays);
    }
}
