using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Reviews.CreateReview;

public sealed record CreateReviewCommand(
    Guid PartnerId,
    int Rating,
    string Comment,
    string Basis,
    Guid InteractionReferenceId) : IRequest<Result<Guid>>;
