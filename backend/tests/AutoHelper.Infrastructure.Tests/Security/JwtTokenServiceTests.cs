using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoHelper.Domain.Admins;
using AutoHelper.Domain.Customers;
using AutoHelper.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

namespace AutoHelper.Infrastructure.Tests.Security;

public class JwtTokenServiceTests
{
    private readonly JwtSettings _settings = new()
    {
        Secret = "super-secret-key-that-is-long-enough-for-hmac256",
        AdminSecret = "admin-super-secret-key-long-enough-for-hmac256",
        Issuer = "AutoHelper",
        Audience = "AutoHelper.Client",
        AccessTokenExpiryMinutes = 15,
        RefreshTokenExpiryDays = 30,
        AdminAccessTokenExpiryMinutes = 15,
        AdminRefreshTokenExpiryDays = 7
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

    [Fact]
    public void AdminRefreshTokenExpiryDays_ShouldReturnValueFromSettings()
    {
        var sut = CreateSut();
        sut.AdminRefreshTokenExpiryDays.ShouldBe(_settings.AdminRefreshTokenExpiryDays);
    }

    // ─── GenerateAdminAccessToken ─────────────────────────────────────────────

    [Fact]
    public void GenerateAdminAccessToken_ShouldContainRoleClaim()
    {
        // Arrange
        var adminUser = AdminUser.Create("admin@test.com", "hash", AdminRole.Admin);
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var rawToken = sut.GenerateAdminAccessToken(adminUser);
        var parsed = handler.ReadJwtToken(rawToken);

        // Assert — must contain lowercase role claim
        var roleClaim = parsed.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.ShouldNotBeNull();
        roleClaim.Value.ShouldBe("admin");
    }

    [Fact]
    public void GenerateAdminAccessToken_SuperAdmin_ShouldContainSuperAdminRoleClaim()
    {
        var superAdmin = AdminUser.Create("superadmin@test.com", "hash", AdminRole.SuperAdmin);
        var sut = CreateSut();
        var handler = new JwtSecurityTokenHandler();

        var rawToken = sut.GenerateAdminAccessToken(superAdmin);
        var parsed = handler.ReadJwtToken(rawToken);

        var roleClaim = parsed.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        roleClaim.ShouldNotBeNull();
        roleClaim.Value.ShouldBe("superadmin");
    }

    [Fact]
    public void GenerateAdminAccessToken_ShouldHaveSeparateSecretFromClientToken()
    {
        // Admin tokens signed with AdminSecret must NOT validate with client Secret and vice versa
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");
        var adminUser = AdminUser.Create("admin@test.com", "hash", AdminRole.Admin);
        var sut = CreateSut();

        var clientToken = sut.GenerateAccessToken(customer);
        var adminToken = sut.GenerateAdminAccessToken(adminUser);

        // Tokens must be different strings (different signing keys → different signatures)
        clientToken.ShouldNotBe(adminToken);

        // Verify that tokens are structurally valid JWTs
        clientToken.Split('.').Length.ShouldBe(3);
        adminToken.Split('.').Length.ShouldBe(3);

        // Admin token must NOT validate against the client secret — different signing keys
        var handler2 = new JwtSecurityTokenHandler();
        var clientValidationParams = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
        Should.Throw<Exception>(() =>
            handler2.ValidateToken(adminToken, clientValidationParams, out _));
    }
}
