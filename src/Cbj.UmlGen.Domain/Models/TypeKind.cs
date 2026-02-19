namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents the kind of a declared type in a C# codebase.
/// </summary>
public enum TypeKind
{
    /// <summary>A class type.</summary>
    Class,

    /// <summary>An interface type.</summary>
    Interface,

    /// <summary>An enum type.</summary>
    Enum,

    /// <summary>A record type.</summary>
    Record,

    /// <summary>A struct type.</summary>
    Struct
}