using AutoHelper.Domain.Customers;
using AutoHelper.Domain.Customers.Events;
using Shouldly;

namespace AutoHelper.Domain.Tests.Customers;

public class CustomerTests
{
    // ─── CreateWithPassword ───────────────────────────────────────────────────

    [Fact]
    public void CreateWithPassword_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        const string name = "John Doe";
        const string email = "John.Doe@Example.COM";
        const string hash = "hashed_password";

        // Act
        var customer = Customer.CreateWithPassword(name, email, hash, contacts: "+1234567890");

        // Assert
        customer.Id.ShouldNotBe(Guid.Empty);
        customer.Name.ShouldBe(name);
        customer.Email.ShouldBe("john.doe@example.com"); // normalized
        customer.PasswordHash.ShouldBe(hash);
        customer.Contacts.ShouldBe("+1234567890");
        customer.SubscriptionStatus.ShouldBe(SubscriptionStatus.Free);
        customer.AuthProvider.ShouldBe(AuthProvider.Local);
        customer.GoogleId.ShouldBeNull();
        customer.RegistrationDate.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void CreateWithPassword_ShouldRaiseCustomerRegisteredEvent()
    {
        // Act
        var customer = Customer.CreateWithPassword("Alice", "alice@test.com", "hash");

        // Assert
        customer.DomainEvents.OfType<CustomerRegisteredEvent>()
            .ShouldContain(evt => evt.Email == "alice@test.com");
    }

    // ─── CreateWithGoogle ─────────────────────────────────────────────────────

    [Fact]
    public void CreateWithGoogle_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        const string googleId = "google-sub-12345";

        // Act
        var customer = Customer.CreateWithGoogle(
            name: "Bob Smith",
            email: "BOB@GMAIL.COM",
            googleId: googleId,
            googleEmail: "BOB@GMAIL.COM",
            googlePicture: "https://lh3.googleusercontent.com/photo.jpg",
            googleRefreshToken: "google-rt-token");

        // Assert
        customer.AuthProvider.ShouldBe(AuthProvider.Google);
        customer.Email.ShouldBe("bob@gmail.com");
        customer.GoogleEmail.ShouldBe("bob@gmail.com");
        customer.GoogleId.ShouldBe(googleId);
        customer.PasswordHash.ShouldBeNull();
        customer.SubscriptionStatus.ShouldBe(SubscriptionStatus.Free);
    }

    [Fact]
    public void CreateWithGoogle_ShouldRaiseCustomerRegisteredEvent()
    {
        // Act
        var customer = Customer.CreateWithGoogle("Bob", "bob@gmail.com", "gid", null, null, null);

        // Assert
        customer.DomainEvents.ShouldContain(e => e is CustomerRegisteredEvent);
    }

    // ─── UpdateGoogleInfo ─────────────────────────────────────────────────────

    [Fact]
    public void UpdateGoogleInfo_WithNewToken_ShouldUpdateBothFields()
    {
        // Arrange
        var customer = Customer.CreateWithGoogle("Carol", "carol@gmail.com", "gid", null, "old-pic", "old-rt");

        // Act
        customer.UpdateGoogleInfo("new-pic", "new-rt");

        // Assert
        customer.GooglePicture.ShouldBe("new-pic");
        customer.GoogleRefreshToken.ShouldBe("new-rt");
    }

    [Fact]
    public void UpdateGoogleInfo_WithNullToken_ShouldNotOverwriteExistingToken()
    {
        // Arrange
        var customer = Customer.CreateWithGoogle("Carol", "carol@gmail.com", "gid", null, "old-pic", "existing-rt");

        // Act
        customer.UpdateGoogleInfo("new-pic", googleRefreshToken: null);

        // Assert
        customer.GoogleRefreshToken.ShouldBe("existing-rt"); // preserved
        customer.GooglePicture.ShouldBe("new-pic");
    }

    // ─── RefreshToken ─────────────────────────────────────────────────────────

    [Fact]
    public void RefreshToken_Create_ShouldCreateActiveToken()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var token = RefreshToken.Create(customerId, "my-token-value", expiryDays: 30);

        // Assert
        token.Id.ShouldNotBe(Guid.Empty);
        token.Token.ShouldBe("my-token-value");
        token.CustomerId.ShouldBe(customerId);
        token.IsRevoked.ShouldBeFalse();
        token.IsActive.ShouldBeTrue();
        token.IsExpired.ShouldBeFalse();
        token.ExpiresAt.ShouldBeGreaterThan(DateTime.UtcNow);
    }

    [Fact]
    public void RefreshToken_Revoke_ShouldMakeTokenInactive()
    {
        // Arrange
        var token = RefreshToken.Create(Guid.NewGuid(), "some-token", expiryDays: 30);

        // Act
        token.Revoke();

        // Assert
        token.IsRevoked.ShouldBeTrue();
        token.IsActive.ShouldBeFalse();
    }
}
