# AutoHelper Backend Error Codes

All application errors are defined in `backend/src/AutoHelper.Application/Common/AppErrors.cs`.
The `AppError` value object: `sealed record AppError(string Code, string Description)`.

**IMPORTANT:** When adding, removing, or changing errors in `AppErrors.cs` — update this file accordingly.

---

## Auth

| Code | Description |
|------|-------------|
| `AUTH_001` | User is not authenticated. |
| `AUTH_002` | Invalid email or password. |
| `AUTH_003` | Refresh token is invalid or has expired. |
| `AUTH_004` | Refresh token not found. |
| `AUTH_005` | A customer with this email address already exists. |

---

## Customer

| Code | Description |
|------|-------------|
| `CUSTOMER_001` | Customer not found. |
| `CUSTOMER_002` | Password change is not available for OAuth accounts. |
| `CUSTOMER_003` | Current password is incorrect. |
| `CUSTOMER_004` | Avatar must be a JPEG, PNG, or WebP image. |
| `CUSTOMER_005` | Avatar file size must not exceed 5 MB. |

---

## Vehicle

| Code | Description |
|------|-------------|
| `VEHICLE_001` | Vehicle not found. |
| `VEHICLE_002` | A vehicle with this VIN already exists. |
| `VEHICLE_003` | Partner name is required when status is InRepair. |
| `VEHICLE_004` | Document URL is required when status is Recycled or Dismantled. |
| `VEHICLE_005` | Owner not found. |

---

## ServiceRecord

| Code | Description |
|------|-------------|
| `SERVICE_RECORD_001` | Service record not found. |
| `SERVICE_RECORD_002` | Access denied. You do not own this vehicle. |

---

## Partner

| Code | Description |
|------|-------------|
| `PARTNER_001` | Partner not found. |
| `PARTNER_002` | Partner profile not found. |
| `PARTNER_003` | Partner profile not found for this account. |
| `PARTNER_004` | A partner profile already exists for this account. |
| `PARTNER_005` | Invalid partner type. |
| `PARTNER_006` | Invalid WorkingOpenFrom format. Expected HH:mm. |
| `PARTNER_007` | Invalid WorkingOpenTo format. Expected HH:mm. |
| `PARTNER_008` | Only verified and active partners can perform this action. |

---

## AdCampaign

| Code | Description |
|------|-------------|
| `AD_CAMPAIGN_001` | Ad campaign not found. |
| `AD_CAMPAIGN_002` | Access denied. This campaign belongs to a different partner. |
| `AD_CAMPAIGN_003` | Invalid ad type. |
| `AD_CAMPAIGN_004` | Invalid target category. |

---

## Review

| Code | Description |
|------|-------------|
| `REVIEW_001` | Partner not found. |
| `REVIEW_002` | Invalid review basis. |
| `REVIEW_003` | A review for this interaction already exists. |

---

## Chat

| Code | Description |
|------|-------------|
| `CHAT_001` | Customer not found. |
| `CHAT_002` | Chat not found. |
| `CHAT_003` | Active subscription is required to use this chat mode. |
| `CHAT_004` | AI chat requires an active subscription. |
| `CHAT_005` | DiagnosticsInput is required for FaultHelp mode. |
| `CHAT_006` | WorkClarificationInput is required for WorkClarification mode. |
| `CHAT_007` | PartnerAdviceInput is required for PartnerAdvice mode. |
| `CHAT_008` | This chat is completed and no longer accepts messages. |
| `CHAT_009` | AI request quota exceeded. Upgrade your plan or top up requests. |

---

## Subscription

| Code | Description |
|------|-------------|
| `SUBSCRIPTION_001` | Subscription not found. |
| `SUBSCRIPTION_002` | Customer already has an active subscription. |
| `SUBSCRIPTION_003` | Invalid subscription plan. |
| `SUBSCRIPTION_004` | Requests top-up is only available with an active subscription. |

---

## Error Format in HTTP Responses

All errors are returned as `ProblemDetails` with:
- `title` — the error `Code` (machine-readable, e.g. `VEHICLE_001`)
- `detail` — the human-readable `Description`

```json
{
  "title": "VEHICLE_001",
  "detail": "Vehicle not found.",
  "status": 404
}
```
