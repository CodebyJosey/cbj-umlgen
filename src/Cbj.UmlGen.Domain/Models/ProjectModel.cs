namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents an analyzed project.
/// </summary>
/// <param name="Name">Project name.</param>
/// <param name="Types">Discovered types.</param>
public sealed record ProjectModel(
    string Name,
    IReadOnlyList<TypeModel> Types
);