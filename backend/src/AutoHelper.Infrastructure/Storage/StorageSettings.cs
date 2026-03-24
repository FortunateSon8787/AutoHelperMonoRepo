namespace AutoHelper.Infrastructure.Storage;

public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    public string ServiceUrl { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;
}
