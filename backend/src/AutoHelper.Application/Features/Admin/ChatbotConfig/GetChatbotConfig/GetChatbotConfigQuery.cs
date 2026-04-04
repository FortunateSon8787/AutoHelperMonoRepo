using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.ChatbotConfig.GetChatbotConfig;

public sealed record GetChatbotConfigQuery : IRequest<Result<ChatbotConfigResponse>>;
