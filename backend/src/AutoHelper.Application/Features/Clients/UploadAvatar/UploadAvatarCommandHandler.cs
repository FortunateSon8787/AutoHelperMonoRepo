using AutoHelper.Application.Common;
using AutoHelper.Application.Common.Interfaces;
using MediatR;

namespace AutoHelper.Application.Features.Clients.UploadAvatar;

public sealed class UploadAvatarCommandHandler(
    ICustomerRepository customers,
    IStorageService storage,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : IRequestHandler<UploadAvatarCommand, Result<string>>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken ct)
    {
        if (currentUser.Id is null)
            return Result<string>.Failure("User is not authenticated.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            return Result<string>.Failure("Avatar must be a JPEG, PNG, or WebP image.");

        if (request.Content.Length > MaxFileSizeBytes)
            return Result<string>.Failure("Avatar file size must not exceed 5 MB.");

        var customer = await customers.GetByIdAsync(currentUser.Id.Value, ct);
        if (customer is null)
            return Result<string>.Failure("Customer not found.");

        var extension = request.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => string.Empty
        };

        var fileKey = $"avatars/{customer.Id}{extension}";
        var avatarUrl = await storage.UploadAsync(request.Content, fileKey, request.ContentType, ct);

        customer.UpdateAvatar(avatarUrl);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success(avatarUrl);
    }
}
