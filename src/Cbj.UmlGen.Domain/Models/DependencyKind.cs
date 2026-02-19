namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Categorizes a dependency relationship.
/// </summary>
public enum DependencyKind
{
    /// <summary>Constructor parameter dependency (DI style).</summary>
    Constructor,

    /// <summary>Field/property association.</summary>
    Member
}