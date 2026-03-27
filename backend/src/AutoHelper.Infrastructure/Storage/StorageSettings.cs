namespace AutoHelper.Infrastructure.Storage;

public sealed class StorageSettings
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Storage provider to use. Supported values: "MinIO", "R2".
    /// Defaults to "MinIO" for backward compatibility.
    /// </summary>
    public string Provider { get; init; } = "MinIO";

    // ─── MinIO / generic S3 ───────────────────────────────────────────────────

    public string ServiceUrl { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;

    // ─── Cloudflare R2 specific ───────────────────────────────────────────────

    /// <summary>Cloudflare Account ID. Required when Provider = "R2".</summary>
    public string CloudflareAccountId { get; init; } = string.Empty;

    /// <summary>
    /// Public bucket URL for generating public file URLs (e.g. https://pub-xxx.r2.dev).
    /// If empty, falls back to the S3-path URL format.
    /// </summary>
    public string PublicBaseUrl { get; init; } = string.Empty;
}
