using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.Application.Abstractions;

/// <summary>
/// Emits UML files for a given codebase.
/// </summary>
public interface IUmlEmitter
{
    /// <summary>
    /// The emitter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Emits one or more UML files.
    /// </summary>
    /// <param name="codebase">Codebase model.</param>
    /// <param name="options">Emit options.</param>
    /// <returns>UML files.</returns>
    IReadOnlyList<UmlFile> Emit(Codebase codebase, UmlEmitOptions options);
}