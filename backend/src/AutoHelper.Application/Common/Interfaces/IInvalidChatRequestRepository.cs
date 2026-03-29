using AutoHelper.Domain.Chats;

namespace AutoHelper.Application.Common.Interfaces;

/// <summary>
/// Write-only repository for the InvalidChatRequest audit log.
/// Records are append-only; there is no retrieval requirement in the application layer.
/// </summary>
public interface IInvalidChatRequestRepository
{
    void Add(InvalidChatRequest request);
}
