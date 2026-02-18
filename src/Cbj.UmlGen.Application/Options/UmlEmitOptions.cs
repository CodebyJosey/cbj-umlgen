namespace Cbj.UmlGen.Application.Options;

/// <summary>
/// Options that influence UML generation output.
/// </summary>
/// <param name="IncludeNamespacePrefix">Optional namespace prefix include filter.</param>
/// <param name="ExcludeNamespacePrefixes">Optional namespace prefix exclude filters.</param>
public sealed record UmlEmitOptions(
    string? IncludeNamespacePrefix = null,
    IReadOnlyList<string>? ExcludeNamespacePrefixes = null
);