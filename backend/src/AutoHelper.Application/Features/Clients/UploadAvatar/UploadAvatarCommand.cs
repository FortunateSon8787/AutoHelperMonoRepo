using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Clients.UploadAvatar;

/// <summary>
/// Uploads a new avatar for the currently authenticated customer.
/// Returns the public URL of the uploaded avatar.
/// </summary>
public sealed record UploadAvatarCommand(
    Stream Content,
    string FileName,
    string ContentType) : IRequest<Result<string>>;
