using AutoHelper.Application.Common;
using AutoHelper.Domain.Chats;
using MediatR;

namespace AutoHelper.Application.Features.Chats.CreateChat;

/// <param name="Mode">Chat mode selecting the AI assistant behaviour.</param>
/// <param name="Title">Short user-provided title for the session.</param>
/// <param name="VehicleId">Optional vehicle to use as context for the conversation.</param>
/// <param name="DiagnosticsInput">
/// Required when Mode = FaultHelp. Contains symptoms and context for the diagnostic session.
/// </param>
/// <param name="WorkClarificationInput">
/// Required when Mode = WorkClarification. Describes works performed, costs, and guarantees.
/// </param>
/// <param name="PartnerAdviceInput">
/// Required when Mode = PartnerAdvice. Contains the user's request text and geolocation for proximity search.
/// </param>
/// <param name="Locale">UI locale used to instruct the LLM to reply in the correct language.</param>
public sealed record CreateChatCommand(
    ChatMode Mode,
    string Title,
    Guid? VehicleId = null,
    DiagnosticsInput? DiagnosticsInput = null,
    WorkClarificationInput? WorkClarificationInput = null,
    PartnerAdviceInput? PartnerAdviceInput = null,
    string Locale = "ru") : IRequest<Result<CreateChatResponse>>;
