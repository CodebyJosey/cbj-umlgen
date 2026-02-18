using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.Application.Abstractions;

/// <summary>
/// Loads a C# solution/project into an in-memory <see cref="Codebase"/> representation.
/// </summary>
public interface ICodebaseLoader
{
    /// <summary>
    /// Loads a codebase from a .sln, .csproj, or a folder containing one.
    /// </summary>
    /// <param name="sourcePath">Input path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The loaded <see cref="Codebase"/>.</returns>
    Task<Codebase> LoadAsync(string sourcePath, CancellationToken ct = default);
}