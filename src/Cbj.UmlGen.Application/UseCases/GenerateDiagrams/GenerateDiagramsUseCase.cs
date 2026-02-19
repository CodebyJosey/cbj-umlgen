using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.Application.UseCases.GenerateDiagrams;

/// <summary>
/// Orchestrates codebase loading and UML emitting.
/// </summary>
public sealed class GenerateDiagramsUseCase
{
    private readonly ICodebaseLoader _loader;
    private readonly IReadOnlyList<IUmlEmitter> _emitters;
    private readonly IFileWriter _writer;

    /// <summary>
    /// Creates a new instance of the use case.
    /// </summary>
    /// <param name="loader">Codebase loader.</param>
    /// <param name="emitters">Emitters.</param>
    /// <param name="writer">File writer.</param>
    public GenerateDiagramsUseCase(
        ICodebaseLoader loader,
        IReadOnlyList<IUmlEmitter> emitters,
        IFileWriter writer)
    {
        _loader = loader;
        _emitters = emitters;
        _writer = writer;
    }

    /// <summary>
    /// Executes diagram generation.
    /// </summary>
    public async Task ExecuteAsync(GenerateDiagramsRequest request, CancellationToken ct = default)
    {
        Codebase? codebase = await _loader.LoadAsync(request.Source, ct);

        UmlEmitOptions? options = new UmlEmitOptions(
            request.IncludeNamespacePrefix,
            request.ExcludeNamespacePrefixes);

        foreach (IUmlEmitter? emitter in _emitters)
        {
            IReadOnlyList<UmlFile>? files = emitter.Emit(codebase, options);

            foreach (UmlFile? file in files)
            {
                await _writer.WriteTextAsync(
                    Path.Combine(request.OutputDirectory, file.FileName),
                    file.Content,
                    ct);
            }
        }
    }
}