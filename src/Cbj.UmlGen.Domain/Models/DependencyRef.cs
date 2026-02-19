namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents a dependency from one type to another (e.g., constructor parameter).
/// </summary>
/// <param name="SourceTypeFullName">The depending type.</param>
/// <param name="TargetTypeFullName">The depended-on type.</param>
/// <param name="Kind">Dependency kind.</param>
/// <param name="Label">Optional label (e.g., parameter name).</param>
public sealed record DependencyRef(
    string SourceTypeFullName,
    string TargetTypeFullName,
    DependencyKind Kind,
    string? Label = null
);