namespace AutoHelper.Application.Common;

/// <summary>
/// Adds a JSON Schema "maxLength" constraint to a string property.
/// Picked up by OpenAiLlmProvider.BuildPropertySchema when generating
/// Structured Outputs schemas — instructs the LLM to cap the string length.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class JsonSchemaMaxLengthAttribute(int maxLength) : Attribute
{
    public int MaxLength { get; } = maxLength;
}
