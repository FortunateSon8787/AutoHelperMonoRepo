namespace AutoHelper.Application.Common;

/// <summary>
/// Represents a structured application error with a machine-readable code and a human-readable description.
/// All error instances are defined in <see cref="AppErrors"/> — do not create ad-hoc errors.
/// </summary>
public sealed record AppError(string Code, string Description)
{
    public override string ToString() => $"{Code}: {Description}";
}
