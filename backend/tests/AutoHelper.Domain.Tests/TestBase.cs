using AutoFixture;

namespace AutoHelper.Domain.Tests;

/// <summary>
/// Base class for Domain layer tests.
/// Provides a pre-configured AutoFixture instance.
/// No mocks needed here — domain logic is pure.
/// </summary>
public abstract class TestBase
{
    protected readonly Fixture AutoFixture = new();
}
