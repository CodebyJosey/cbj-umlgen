using System.Text;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.PlantUml.Emitters;

/// <summary>
/// Emits a simple namespace/package overview diagram
/// </summary>
public sealed class PackageDiagramEmitter : IUmlEmitter
{
    /// <inheritdoc/>
    public string Name => "packages";

    /// <inheritdoc />
    public IReadOnlyList<UmlFile> Emit(Codebase codebase, UmlEmitOptions options)
    {
        var namespaces = codebase.Projects
            .SelectMany(p => p.Types)
            .Select(t => t.Namespace)
            .Where(ns => Filter(ns, options))
            .Distinct()
            .OrderBy(ns => ns)
            .ToList();

        StringBuilder? sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("hide empty members");
        sb.AppendLine("skinparam packageStyle rectangle");
        sb.AppendLine();

        foreach (var ns in namespaces)
        {
            sb.AppendLine($"package \"{Escape(ns)}\" {{ }}");
        }

        sb.AppendLine();
        sb.AppendLine("@enduml");

        return new[]
        {
            new UmlFile("packages.puml", sb.ToString())
        };
    }

    private static bool Filter(string ns, UmlEmitOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.IncludeNamespacePrefix) &&
            !ns.StartsWith(options.IncludeNamespacePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (options.ExcludeNamespacePrefixes is not null)
        {
            foreach (var ex in options.ExcludeNamespacePrefixes)
            {
                if (!string.IsNullOrWhiteSpace(ex) && ns.StartsWith(ex, StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}