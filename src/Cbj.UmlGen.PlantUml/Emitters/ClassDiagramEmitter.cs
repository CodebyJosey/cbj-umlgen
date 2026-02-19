using System.Security.Cryptography;
using System.Text;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.PlantUml.Emitters;

/// <summary>
/// Emits a basic class diagram (types + inheritance + interfaces + simple member associations).
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

        Dictionary<string, TypeModel>? byFullName = allTypes.ToDictionary(t => t.FullName, t => t);

        StringBuilder? sb = new StringBuilder();
        sb.AppendLine($"@startuml {Name}");
        sb.AppendLine("hide empty members");
        sb.AppendLine();

        // Declare types
        foreach (var t in allTypes.OrderBy(t => t.FullName))
        {
            string? keyword = t.Kind switch
            {
                TypeKind.Interface => "interface",
                TypeKind.Enum => "enum",
                _ => "class"
            };

            sb.AppendLine($"{keyword} \"{Escape(t.FullName)}\" as {Alias(t.FullName)}");
        }

        sb.AppendLine();

        // Relations
        foreach (TypeModel? t in allTypes)
        {
            // Inheritance
            if (!string.IsNullOrWhiteSpace(t.BaseTypeFullName))
            {
                string? baseName = NormalizeFullQualified(t.BaseTypeFullName);
                if (byFullName.ContainsKey(baseName))
                {
                    sb.AppendLine($"{Alias(t.FullName)} --|> {Alias(baseName)}");
                }
            }

            // Implements
            foreach (string? ifaceFq in t.InterfaceFullNames)
            {
                string? iface = NormalizeFullQualified(ifaceFq);
                if (byFullName.ContainsKey(iface))
                {
                    sb.AppendLine($"{Alias(t.FullName)} ..|> {Alias(iface)}");
                }
            }

            // Simple member associations (fields/properties)
            foreach (MemberTypeRef? m in t.MemberTypeRefs)
            {
                string? target = NormalizeFullQualified(m.TargetTypeFullName);

                if (byFullName.ContainsKey(target) && !string.Equals(target, t.FullName, StringComparison.Ordinal))
                {
                    sb.AppendLine($"{Alias(t.FullName)} --> {Alias(target)} : {Escape(m.MemberName)}");
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

    // Stable + collision resistant alias
    private static string Alias(string fullName)
    {
        byte[]? bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fullName));
        string? hex = Convert.ToHexString(bytes.AsSpan(0, 8)); // 16 chars
        return "T" + hex;
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
                    return false;
            }
        }

        return true;
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}