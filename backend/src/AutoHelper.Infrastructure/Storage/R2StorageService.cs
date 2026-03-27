using Amazon.S3;
using Amazon.S3.Model;
using AutoHelper.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AutoHelper.Infrastructure.Storage;

/// <summary>
/// IStorageService implementation backed by Cloudflare R2 (S3-compatible API).
/// R2 endpoint format: https://{accountId}.r2.cloudflarestorage.com
/// </summary>
public sealed class R2StorageService(IAmazonS3 s3, IOptions<StorageSettings> options) : IStorageService
{
    private readonly StorageSettings _settings = options.Value;

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileName,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await s3.PutObjectAsync(request, cancellationToken);

        return GetPublicUrl(fileName);
    }

    public async Task<Stream> DownloadAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey
        };

        var response = await s3.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileKey
        };

        await s3.DeleteObjectAsync(request, cancellationToken);
    }

    public string GetPublicUrl(string fileKey)
    {
        // Use explicit public base URL if configured (e.g. R2 custom domain or r2.dev bucket URL)
        if (!string.IsNullOrWhiteSpace(_settings.PublicBaseUrl))
            return $"{_settings.PublicBaseUrl.TrimEnd('/')}/{fileKey}";

        // Fallback to S3-path-style URL via the R2 endpoint
        return $"{_settings.ServiceUrl.TrimEnd('/')}/{_settings.BucketName}/{fileKey}";
    }
}
