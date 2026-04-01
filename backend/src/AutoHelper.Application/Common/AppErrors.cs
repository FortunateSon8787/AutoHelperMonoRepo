namespace AutoHelper.Application.Common;

/// <summary>
/// Central registry of all application-level errors.
/// Every possible failure returned via Result.Failure must be defined here.
/// When errors are added, removed, or changed — update .claude/ERRORS.md accordingly.
/// </summary>
public static class AppErrors
{
    // ─── Auth ─────────────────────────────────────────────────────────────────

    public static class Auth
    {
        public static readonly AppError NotAuthenticated =
            new("AUTH_001", "User is not authenticated.");

        public static readonly AppError InvalidCredentials =
            new("AUTH_002", "Invalid email or password.");

        public static readonly AppError RefreshTokenInvalid =
            new("AUTH_003", "Refresh token is invalid or has expired.");

        public static readonly AppError RefreshTokenNotFound =
            new("AUTH_004", "Refresh token not found.");

        public static readonly AppError EmailAlreadyExists =
            new("AUTH_005", "A customer with this email address already exists.");
    }

    // ─── Customer ─────────────────────────────────────────────────────────────

    public static class Customer
    {
        public static readonly AppError NotFound =
            new("CUSTOMER_001", "Customer not found.");

        public static readonly AppError PasswordChangeNotAvailableForOAuth =
            new("CUSTOMER_002", "Password change is not available for OAuth accounts.");

        public static readonly AppError IncorrectCurrentPassword =
            new("CUSTOMER_003", "Current password is incorrect.");

        public static readonly AppError AvatarInvalidContentType =
            new("CUSTOMER_004", "Avatar must be a JPEG, PNG, or WebP image.");

        public static readonly AppError AvatarFileTooLarge =
            new("CUSTOMER_005", "Avatar file size must not exceed 5 MB.");
    }

    // ─── Vehicle ──────────────────────────────────────────────────────────────

    public static class Vehicle
    {
        public static readonly AppError NotFound =
            new("VEHICLE_001", "Vehicle not found.");

        public static readonly AppError VinAlreadyExists =
            new("VEHICLE_002", "A vehicle with this VIN already exists.");

        public static readonly AppError OwnerNotFound =
            new("VEHICLE_005", "Owner not found.");

        public static readonly AppError PartnerNameRequiredForInRepair =
            new("VEHICLE_003", "Partner name is required when status is InRepair.");

        public static readonly AppError DocumentRequiredForRecycledOrDismantled =
            new("VEHICLE_004", "Document URL is required when status is Recycled or Dismantled.");
    }

    // ─── ServiceRecord ────────────────────────────────────────────────────────

    public static class ServiceRecord
    {
        public static readonly AppError NotFound =
            new("SERVICE_RECORD_001", "Service record not found.");

        public static readonly AppError AccessDenied =
            new("SERVICE_RECORD_002", "Access denied. You do not own this vehicle.");
    }

    // ─── Partner ──────────────────────────────────────────────────────────────

    public static class Partner
    {
        public static readonly AppError NotFound =
            new("PARTNER_001", "Partner not found.");

        public static readonly AppError ProfileNotFound =
            new("PARTNER_002", "Partner profile not found.");

        public static readonly AppError ProfileNotFoundForAccount =
            new("PARTNER_003", "Partner profile not found for this account.");

        public static readonly AppError AlreadyExistsForAccount =
            new("PARTNER_004", "A partner profile already exists for this account.");

        public static readonly AppError NotVerifiedOrInactive =
            new("PARTNER_008", "Only verified and active partners can perform this action.");

        public static readonly AppError InvalidType =
            new("PARTNER_005", "Invalid partner type.");

        public static readonly AppError InvalidWorkingOpenFrom =
            new("PARTNER_006", "Invalid WorkingOpenFrom format. Expected HH:mm.");

        public static readonly AppError InvalidWorkingOpenTo =
            new("PARTNER_007", "Invalid WorkingOpenTo format. Expected HH:mm.");
    }

    // ─── AdCampaign ───────────────────────────────────────────────────────────

    public static class AdCampaign
    {
        public static readonly AppError NotFound =
            new("AD_CAMPAIGN_001", "Ad campaign not found.");

        public static readonly AppError AccessDenied =
            new("AD_CAMPAIGN_002", "Access denied. This campaign belongs to a different partner.");

        public static readonly AppError InvalidAdType =
            new("AD_CAMPAIGN_003", "Invalid ad type.");

        public static readonly AppError InvalidTargetCategory =
            new("AD_CAMPAIGN_004", "Invalid target category.");
    }

    // ─── Review ───────────────────────────────────────────────────────────────

    public static class Review
    {
        public static readonly AppError PartnerNotFound =
            new("REVIEW_001", "Partner not found.");

        public static readonly AppError InvalidBasis =
            new("REVIEW_002", "Invalid review basis.");

        public static readonly AppError DuplicateReview =
            new("REVIEW_003", "A review for this interaction already exists.");
    }

    // ─── Chat ─────────────────────────────────────────────────────────────────

    public static class Chat
    {
        public static readonly AppError CustomerNotFound =
            new("CHAT_001", "Customer not found.");

        public static readonly AppError NotFound =
            new("CHAT_002", "Chat not found.");

        public static readonly AppError SubscriptionRequired =
            new("CHAT_003", "Active subscription is required to use this chat mode.");

        public static readonly AppError CreateSubscriptionRequired =
            new("CHAT_004", "AI chat requires an active subscription.");

        public static readonly AppError DiagnosticsInputRequired =
            new("CHAT_005", "DiagnosticsInput is required for FaultHelp mode.");

        public static readonly AppError WorkClarificationInputRequired =
            new("CHAT_006", "WorkClarificationInput is required for WorkClarification mode.");

        public static readonly AppError PartnerAdviceInputRequired =
            new("CHAT_007", "PartnerAdviceInput is required for PartnerAdvice mode.");

        public static readonly AppError ChatIsCompleted =
            new("CHAT_008", "This chat is completed and no longer accepts messages.");

        public static readonly AppError QuotaExceeded =
            new("CHAT_009", "AI request quota exceeded. Upgrade your plan or top up requests.");
    }

    // ─── Admin ────────────────────────────────────────────────────────────────

    public static class Admin
    {
        public static readonly AppError CustomerNotFound =
            new("ADMIN_001", "Customer not found.");

        public static readonly AppError CustomerAlreadyBlocked =
            new("ADMIN_002", "Customer account is already blocked.");

        public static readonly AppError CustomerNotBlocked =
            new("ADMIN_003", "Customer account is not blocked.");

        public static readonly AppError VehicleNotFound =
            new("ADMIN_004", "Vehicle not found.");

        public static readonly AppError PartnerNotFound =
            new("ADMIN_005", "Partner not found.");

        public static readonly AppError PartnerAlreadyVerified =
            new("ADMIN_006", "Partner is already verified.");

        public static readonly AppError PartnerAlreadyDeactivated =
            new("ADMIN_007", "Partner is already deactivated.");

        public static readonly AppError ReviewNotFound =
            new("ADMIN_008", "Review not found.");

        public static readonly AppError AdCampaignNotFound =
            new("ADMIN_009", "Ad campaign not found.");

        public static readonly AppError AdCampaignAlreadyActive =
            new("ADMIN_010", "Ad campaign is already active.");

        public static readonly AppError AdCampaignAlreadyInactive =
            new("ADMIN_011", "Ad campaign is already inactive.");
    }

    // ─── Subscription ─────────────────────────────────────────────────────────

    public static class Subscription
    {
        public static readonly AppError NotFound =
            new("SUBSCRIPTION_001", "Subscription not found.");

        public static readonly AppError AlreadyActive =
            new("SUBSCRIPTION_002", "Customer already has an active subscription.");

        public static readonly AppError InvalidPlan =
            new("SUBSCRIPTION_003", "Invalid subscription plan.");

        public static readonly AppError InsufficientRequestsForTopUp =
            new("SUBSCRIPTION_004", "Requests top-up is only available with an active subscription.");
    }
}
