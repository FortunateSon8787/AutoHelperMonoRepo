using AutoHelper.Domain.Exceptions;

namespace AutoHelper.Domain.Partners;

/// <summary>
/// Describes a partner's working hours for a given period.
/// </summary>
public sealed record WorkingSchedule
{
    /// <summary>Opening time (e.g. 09:00).</summary>
    public TimeOnly OpenFrom { get; }

    /// <summary>Closing time (e.g. 18:00).</summary>
    public TimeOnly OpenTo { get; }

    /// <summary>Human-readable working days description (e.g. "Mon-Fri", "Mon-Sun").</summary>
    public string WorkDays { get; }

    private WorkingSchedule(TimeOnly openFrom, TimeOnly openTo, string workDays)
    {
        OpenFrom = openFrom;
        OpenTo = openTo;
        WorkDays = workDays;
    }

    /// <summary>
    /// Creates a validated <see cref="WorkingSchedule"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when work days description is empty.</exception>
    public static WorkingSchedule Create(TimeOnly openFrom, TimeOnly openTo, string workDays)
    {
        if (string.IsNullOrWhiteSpace(workDays))
            throw new DomainException("Work days description must not be empty.");

        return new WorkingSchedule(openFrom, openTo, workDays.Trim());
    }
}
