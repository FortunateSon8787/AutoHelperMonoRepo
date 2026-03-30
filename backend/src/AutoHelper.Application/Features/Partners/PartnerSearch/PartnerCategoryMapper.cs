using AutoHelper.Domain.Partners;

namespace AutoHelper.Application.Features.Partners.PartnerSearch;

/// <summary>
/// Maps service category strings (used by the LLM) to PartnerType enum values
/// and to Google Places API included types.
/// </summary>
internal static class PartnerCategoryMapper
{
    // Maps LLM category key → (PartnerType, Google Places includedType)
    private static readonly Dictionary<string, (PartnerType Type, string GoogleType)> CategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tow_truck"]    = (PartnerType.Towing,           "towing_service"),
        ["tire_service"] = (PartnerType.TireService,      "tire_shop"),
        ["car_service"]  = (PartnerType.AutoService,      "car_repair"),
        ["car_wash"]     = (PartnerType.CarWash,          "car_wash"),
        ["electrician"]  = (PartnerType.AutoElectrician,  "car_repair"),
        ["auto_service"] = (PartnerType.AutoService,      "car_repair"),
    };

    public static bool TryGetPartnerType(string category, out PartnerType partnerType)
    {
        if (CategoryMap.TryGetValue(category, out var entry))
        {
            partnerType = entry.Type;
            return true;
        }
        partnerType = default;
        return false;
    }

    public static string GetGooglePlaceType(string category)
    {
        if (CategoryMap.TryGetValue(category, out var entry))
            return entry.GoogleType;

        return "car_repair"; // safe fallback
    }
}
