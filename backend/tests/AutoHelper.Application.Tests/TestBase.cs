using AutoFixture;
using AutoFixture.AutoMoq;

namespace AutoHelper.Application.Tests;

/// <summary>
/// Base class for Application layer tests.
/// AutoFixture is configured with AutoMoq so that
/// interface dependencies are auto-mocked unless explicitly set up.
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
