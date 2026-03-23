namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Abstracts file storage operations (S3/MinIO).
/// Used for uploading PDF service records, avatars, and utility documents.
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string fileKey,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string fileKey);
}
