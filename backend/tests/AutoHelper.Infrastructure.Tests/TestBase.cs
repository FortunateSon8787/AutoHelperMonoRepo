using AutoFixture;
using AutoFixture.AutoMoq;

namespace AutoHelper.Infrastructure.Tests;

/// <summary>
/// Base class for Infrastructure layer tests.
/// Integration tests use Testcontainers to spin up a real PostgreSQL instance.
/// </summary>
public abstract class TestBase
{
    protected readonly IFixture AutoFixture;

    protected TestBase()
    {
        AutoFixture = new Fixture().Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true
        });
    }
}
