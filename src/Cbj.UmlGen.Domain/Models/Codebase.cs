namespace Cbj.UmlGen.Domain.Models;

/// <summary>
/// Represents an analyzed codebase (solution containing one or more projects).
/// </summary>
/// <param name="Projects">Projects in the solution.</param>
public sealed record Codebase(
    IReadOnlyList<ProjectModel> Projects
);