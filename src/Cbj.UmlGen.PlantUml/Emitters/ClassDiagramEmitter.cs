using System.Text;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.PlantUml.Emitters;

/// <summary>
/// Emits a basic class diagram with inheritance and interface implementation.
/// </summary>
public sealed class ClassDiagramEmitter : IUmlEmitter
{
    /// <inheritdoc />
    public string Name => "classes";

    /// <inheritdoc />
    public IReadOnlyList<UmlFile> Emit(Codebase codebase, UmlEmitOptions options)
    {
        List<TypeModel>? allTypes = codebase.Projects
            .SelectMany(p => p.Types)
            .Where(t => Filter(t.Namespace, options))
            .ToList();

        Dictionary<string, TypeModel>? byFullname = allTypes.ToDictionary(t => t.FullName, t => t);

        StringBuilder? sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("hide empty members");
        sb.AppendLine();

        // Declare types
        foreach (TypeModel? t in allTypes.OrderBy(t => t.FullName))
        {
            string? keyword = t.Kind switch
            {
                TypeKind.Interface => "interface",
                TypeKind.Enum => "enum",
                TypeKind.Struct => "class",
                TypeKind.Record => "class",
                _ => "class"
            };

            sb.AppendLine($"{keyword} \"{Escape(t.FullName)}\" as {Alias(t.FullName)}");
        }

        sb.AppendLine();

        // Inheritance + interfaces
        foreach (TypeModel? t in allTypes)
        {
            if (!string.IsNullOrWhiteSpace(t.BaseTypeFullName))
            {
                string? baseName = NormalizeFullQualified(t.BaseTypeFullName);
                if (byFullname.ContainsKey(baseName))
                {
                    sb.AppendLine($"{Alias(t.FullName)} --|> {Alias(baseName)}");
                }
            }

            foreach (string? ifaceFq in t.InterfaceFullNames)
            {
                string? iface = NormalizeFullQualified(ifaceFq);
                if (byFullname.ContainsKey(iface))
                {
                    sb.AppendLine($"{Alias(t.FullName)} ..|> {Alias(iface)}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("@enduml");

        return new[]
        {
            new UmlFile("classes.puml", sb.ToString())
        };
    }

    private static string NormalizeFullQualified(string fq)
        => fq.StartsWith("global::", StringComparison.Ordinal) ? fq["global::".Length..] : fq;

    private static string Alias(string fullName)
        => "T" + Math.Abs(fullName.GetHashCode()).ToString();

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
                    return false;
            }
        }

        return true;
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}