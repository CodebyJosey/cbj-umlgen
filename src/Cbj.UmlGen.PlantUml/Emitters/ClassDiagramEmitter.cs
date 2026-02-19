using System.Security.Cryptography;
using System.Text;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.PlantUml.Emitters;

/// <summary>
/// Emits a readable class diagram grouped by namespace with sensible dependency relations.
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
            .Where(t => !IsNoiseType(t))
            .ToList();

        Dictionary<string, TypeModel>? byFullName = allTypes
            .GroupBy(t => t.FullName, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        StringBuilder? sb = new StringBuilder();
        sb.AppendLine($"@startuml {Name}");
        sb.AppendLine("hide empty members");
        sb.AppendLine("skinparam shadowing false");
        sb.AppendLine("skinparam classAttributeIconSize 0");
        sb.AppendLine("skinparam linetype ortho");
        sb.AppendLine();

        // Layer colors (subtle)
        sb.AppendLine("skinparam package {");
        sb.AppendLine("  BorderColor #888888");
        sb.AppendLine("  BackgroundColor #FFFFFF");
        sb.AppendLine("}");
        sb.AppendLine();

        // Group by namespace (package)
        foreach (IGrouping<string, TypeModel>? group in allTypes
                     .GroupBy(t => t.Namespace ?? string.Empty)
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            string? ns = string.IsNullOrWhiteSpace(group.Key) ? "(global)" : group.Key;
            string? layer = GuessLayer(ns);

            sb.AppendLine($"package \"{Escape(ns)}\" <<{layer}>> {{");

            foreach (TypeModel? t in group.OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                sb.AppendLine($"  {DeclareType(t)}");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Legend (nice for docs)
        sb.AppendLine("legend left");
        sb.AppendLine("|= Layer |= Meaning |");
        sb.AppendLine("|<<Domain>>|Domain model|");
        sb.AppendLine("|<<Application>>|Use-cases + abstractions|");
        sb.AppendLine("|<<Infrastructure>>|IO/Roslyn/Adapters|");
        sb.AppendLine("|<<Cli>>|CLI host|");
        sb.AppendLine("|<<Tests>>|Test projects|");
        sb.AppendLine("endlegend");
        sb.AppendLine();

        // Relations
        // 1) Inheritance + interfaces
        foreach (TypeModel? t in allTypes)
        {
            if (!string.IsNullOrWhiteSpace(t.BaseTypeFullName))
            {
                string? baseName = NormalizeFullQualified(t.BaseTypeFullName);
                if (byFullName.ContainsKey(baseName))
                {
                    sb.AppendLine($"{Alias(t.FullName)} --|> {Alias(baseName)}");
                }
            }

            foreach (string? ifaceFq in t.InterfaceFullNames)
            {
                string? iface = NormalizeFullQualified(ifaceFq);
                if (byFullName.ContainsKey(iface))
                {
                    sb.AppendLine($"{Alias(t.FullName)} ..|> {Alias(iface)}");
                }
            }
        }

        sb.AppendLine();

        // 2) Dependencies
        // Prefer constructor deps (DI) as dotted arrows
        foreach (TypeModel? t in allTypes)
        {
            foreach (DependencyRef? dep in t.Dependencies)
            {
                string? target = NormalizeFullQualified(dep.TargetTypeFullName);
                if (!byFullName.ContainsKey(target) || target == t.FullName)
                    continue;

                if (dep.Kind == DependencyKind.Constructor)
                {
                    sb.AppendLine($"{Alias(t.FullName)} ..> {Alias(target)} : {Escape(dep.Label ?? "ctor")}");
                }
            }
        }

        sb.AppendLine();

        // 3) Optional: member associations (can be noisy; keep it for now, but we can make it a flag later)
        foreach (TypeModel? t in allTypes)
        {
            foreach (DependencyRef? dep in t.Dependencies)
            {
                if (dep.Kind != DependencyKind.Member)
                {
                    continue;
                }

                string? target = NormalizeFullQualified(dep.TargetTypeFullName);
                if (!byFullName.ContainsKey(target) || target == t.FullName)
                {
                    continue;
                }

                sb.AppendLine($"{Alias(t.FullName)} --> {Alias(target)} : {Escape(dep.Label ?? "member")}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("@enduml");

        return new[] { new UmlFile("classes.puml", sb.ToString()) };
    }

    private static string DeclareType(TypeModel t)
    {
        string? keyword = t.Kind switch
        {
            TypeKind.Interface => "interface",
            TypeKind.Enum => "enum",
            _ => "class"
        };

        // Show readable label, keep alias stable
        return $"{keyword} \"{Escape(t.Name)}\" as {Alias(t.FullName)}";
    }

    private static string NormalizeFullQualified(string fq)
        => fq.StartsWith("global::", StringComparison.Ordinal) ? fq["global::".Length..] : fq;

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
            foreach (string? ex in options.ExcludeNamespacePrefixes)
            {
                if (!string.IsNullOrWhiteSpace(ex) && ns.StartsWith(ex, StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsNoiseType(TypeModel t)
    {
        // Avoid compiler/auto-generated noise
        if (t.Name is "AutoGeneratedProgram")
        {
            return true;
        }

        // Skip weird compiler-generated names
        if (t.Name.StartsWith("<", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string GuessLayer(string ns)
    {
        // Simple heuristics based on your solution naming
        if (ns.Contains(".Domain", StringComparison.OrdinalIgnoreCase)) return "Domain";
        if (ns.Contains(".Application", StringComparison.OrdinalIgnoreCase)) return "Application";
        if (ns.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)) return "Infrastructure";
        if (ns.Contains(".Cli", StringComparison.OrdinalIgnoreCase)) return "Cli";
        if (ns.Contains(".Test", StringComparison.OrdinalIgnoreCase) || ns.Contains(".Tests", StringComparison.OrdinalIgnoreCase)) return "Tests";
        return "Application";
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");
}