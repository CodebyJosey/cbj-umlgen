namespace Cbj.UmlGen.Application.UseCases.GenerateDiagrams;

/// <summary>
/// Request parameters for generating diagrams.
/// </summary>
/// <param name="Source">Input path or repo path.</param>
/// <param name="OutputDirectory">Output directory.</param>
/// <param name="IncludeNamespacePrefix">Optional include filter.</param>
/// <param name="ExcludeNamespacePrefixes">Optional exclude filters.</param>
public sealed record GenerateDiagramsRequest(
    string Source,
    string OutputDirectory,
    string? IncludeNamespacePrefix = null,
    IReadOnlyList<string>? ExcludeNamespacePrefixes = null
);