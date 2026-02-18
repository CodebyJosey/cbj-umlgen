namespace Cbj.UmlGen.Application.Abstractions;

/// <summary>
/// Represents one generated UML file.
/// </summary>
/// <param name="FileName">File name.</param>
/// <param name="Content">Text content.</param>
public sealed record UmlFile(string FileName, string Content);