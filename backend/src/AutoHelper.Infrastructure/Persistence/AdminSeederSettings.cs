namespace AutoHelper.Infrastructure.Persistence;

public sealed class AdminSeederSettings
{
    public const string SectionName = "AdminSeeder";

    public string SuperAdminEmail { get; init; } = string.Empty;
    public string SuperAdminPassword { get; init; } = string.Empty;
}
