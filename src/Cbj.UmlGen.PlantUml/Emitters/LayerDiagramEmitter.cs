using System.Text;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;

namespace Cbj.UmlGen.PlantUml.Emitters;

/// <summary>
/// Emits a high-level layer diagram (Clean Architecture style) based on detected dependencies between layers.
/// </summary>
public sealed class LayerDiagramEmitter : IUmlEmitter
{
    /// <inheritdoc />
    public string Name => "layers";

    /// <inheritdoc />
    public IReadOnlyList<UmlFile> Emit(Codebase codebase, UmlEmitOptions options)
    {
        List<TypeModel>? types = codebase.Projects
            .SelectMany(p => p.Types)
            .Where(t => Filter(t.Namespace, options))
            .Where(t => !IsNoiseType(t))
            .ToList();

        // Build a quick lookup for "is this type part of the codebase?"
        Dictionary<string, TypeModel>? byFullName = types
            .GroupBy(t => t.FullName, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        // Collect layer dependencies (LayerA -> LayerB)
        Dictionary<(Layer From, Layer To), int>? edges = new Dictionary<(Layer From, Layer To), int>();

        foreach (TypeModel? t in types)
        {
            Layer fromLayer = DetectLayer(t);

            // 1) Constructor + member dependencies (preferred signal)
            foreach (DependencyRef? dep in t.Dependencies)
            {
                string? target = NormalizeFullQualified(dep.TargetTypeFullName);
                if (!byFullName.TryGetValue(target, out TypeModel? targetType))
                {
                    continue;
                }

                Layer toLayer = DetectLayer(targetType);
                if (fromLayer == toLayer)
                {
                    continue;
                }

                AddEdge(edges, fromLayer, toLayer);
            }

            // 2) Inheritance
            if (!string.IsNullOrWhiteSpace(t.BaseTypeFullName))
            {
                string? baseName = NormalizeFullQualified(t.BaseTypeFullName);
                if (byFullName.TryGetValue(baseName, out TypeModel? baseType))
                {
                    Layer toLayer = DetectLayer(baseType);
                    if (fromLayer != toLayer)
                    {
                        AddEdge(edges, fromLayer, toLayer);
                    }
                }
            }

            // 3) Implements
            foreach (string? ifaceFq in t.InterfaceFullNames)
            {
                string? iface = NormalizeFullQualified(ifaceFq);
                if (byFullName.TryGetValue(iface, out TypeModel? ifaceType))
                {
                    Layer toLayer = DetectLayer(ifaceType);
                    if (fromLayer != toLayer)
                    {
                        AddEdge(edges, fromLayer, toLayer);
                    }
                }
            }
        }

        StringBuilder? sb = new StringBuilder();
        sb.AppendLine("@startuml");
        sb.AppendLine("skinparam shadowing false");
        sb.AppendLine("skinparam linetype ortho");
        sb.AppendLine();

        // Force a stable diagram interpretation in most servers/renderers
        sb.AppendLine("' Force class/component-style parsing");
        sb.AppendLine("skinparam componentStyle rectangle");
        sb.AppendLine();

        // Define nodes as rectangles (server-proof)
        foreach (Layer layer in Enum.GetValues<Layer>())
        {
            sb.AppendLine($"rectangle \"{LayerName(layer)}\" as {LayerId(layer)}");
        }

        sb.AppendLine();

        // Render edges
        foreach (KeyValuePair<(Layer From, Layer To), int> kv in edges.OrderBy(e => e.Key.From).ThenBy(e => e.Key.To))
        {
            var (from, to) = kv.Key;
            int count = kv.Value;

            sb.AppendLine($"{LayerId(from)} ..> {LayerId(to)} : {count}");
        }

        sb.AppendLine();
        sb.AppendLine("legend left");
        sb.AppendLine("Numbers on arrows = amount of cross-layer references detected.");
        sb.AppendLine("endlegend");
        sb.AppendLine("@enduml");

        return new[] { new UmlFile("layers.puml", sb.ToString()) };
    }

    private static void AddEdge(Dictionary<(Layer From, Layer To), int> edges, Layer from, Layer to)
    {
        (Layer from, Layer to) key = (from, to);
        edges.TryGetValue(key, out int current);
        edges[key] = current + 1;
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
        if (t.Name is "AutoGeneratedProgram")
        {
            return true;
        }

        if (t.Name.StartsWith("<", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static string NormalizeFullQualified(string fq)
        => fq.StartsWith("global::", StringComparison.Ordinal) ? fq["global::".Length..] : fq;

    private static Layer DetectLayer(TypeModel t)
    {
        // Prefer namespace-based detection (works for most solutions)
        string? ns = t.Namespace ?? string.Empty;

        if (Contains(ns, ".Domain")) return Layer.Domain;
        if (Contains(ns, ".Application")) return Layer.Application;
        if (Contains(ns, ".Infrastructure")) return Layer.Infrastructure;
        if (Contains(ns, ".Cli")) return Layer.Cli;

        // fallbacks
        if (Contains(ns, ".Test") || Contains(ns, ".Tests")) return Layer.Tests;

        // default bucket
        return Layer.Other;
    }

    private static bool Contains(string value, string token)
        => value.Contains(token, StringComparison.OrdinalIgnoreCase);

    private static string LayerId(Layer layer) => layer switch
    {
        Layer.Domain => "L_Domain",
        Layer.Application => "L_Application",
        Layer.Infrastructure => "L_Infrastructure",
        Layer.Cli => "L_Cli",
        Layer.Tests => "L_Tests",
        _ => "L_Other"
    };

    private static string LayerName(Layer layer) => layer switch
    {
        Layer.Domain => "Domain",
        Layer.Application => "Application",
        Layer.Infrastructure => "Infrastructure",
        Layer.Cli => "CLI",
        Layer.Tests => "Tests",
        _ => "Other"
    };

    private enum Layer
    {
        Domain,
        Application,
        Infrastructure,
        Cli,
        Tests,
        Other
    }
}