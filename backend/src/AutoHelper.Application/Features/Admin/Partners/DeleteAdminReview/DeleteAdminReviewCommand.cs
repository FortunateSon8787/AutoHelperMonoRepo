using AutoHelper.Application.Common;
using MediatR;

namespace AutoHelper.Application.Features.Admin.Partners.DeleteAdminReview;

public sealed record DeleteAdminReviewCommand(Guid ReviewId) : IRequest<Result>;
