namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents a reference from a member (field/property) to another type.
/// </summary>
/// <param name="MemberName">The member name.</param>
/// <param name="TargetTypeFullName">The fully qualified name of the referenced type.</param>
public sealed record MemberTypeRef(
    string MemberName,
    string TargetTypeFullName
);