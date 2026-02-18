namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents a discovered type (class/interface/enum/...) in the analyzed codebase.
/// </summary>
/// <param name="Name">Simple type name.</param>
/// <param name="Namespace">Namespace of the type.</param>
/// <param name="Kind">Type kind.</param>
/// <param name="FullName">Full name (Namespace + Name).</param>
/// <param name="BaseTypeFullName">Base type full name if applicable.</param>
/// <param name="InterfaceFullNames">Implemented interface full names.</param>
/// <param name="MemberTypeRefs">Referenced types from members.</param>
public sealed record TypeModel(
    string Name,
    string Namespace,
    TypeKind Kind,
    string FullName,
    string? BaseTypeFullName,
    IReadOnlyList<string> InterfaceFullNames,
    IReadOnlyList<MemberTypeRef> MemberTypeRefs
);