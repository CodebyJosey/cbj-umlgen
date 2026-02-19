using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.Options;
using Cbj.UmlGen.Domain.Models;
using Cbj.UmlGen.Infrastructure.Roslyn;
using Cbj.UmlGen.PlantUml.Emitters;
using Xunit;

public sealed class PlantUmlSnapshotTests
{
    [Fact]
    public async Task ExampleApp_PackagesDiagram_MatchesSnapshot()
    {
        string? sln = GetExampleSolutionPath();

        RoslynCodebaseLoader? loader = new RoslynCodebaseLoader();
        Codebase? codebase = await loader.LoadAsync(sln);

        PackageDiagramEmitter? emitter = new PackageDiagramEmitter();
        UmlFile? file = emitter.Emit(codebase, new UmlEmitOptions()).Single();

        string? expectedPath = Path.Combine("Snapshots", "packages.snap.puml");
        AssertSnapshot(expectedPath, file.Content);
    }

    [Fact]
    public async Task ExampleApp_ClassDiagram_MatchesSnapshot()
    {
        string? sln = GetExampleSolutionPath();

        RoslynCodebaseLoader? loader = new RoslynCodebaseLoader();
        Codebase? codebase = await loader.LoadAsync(sln);

        ClassDiagramEmitter? emitter = new ClassDiagramEmitter();
        UmlFile? file = emitter.Emit(codebase, new UmlEmitOptions()).Single();

        string? expectedPath = Path.Combine("Snapshots", "classes.snap.puml");
        AssertSnapshot(expectedPath, file.Content);
    }

    private static string GetExampleSolutionPath()
    {
        // test output folder is: tests/Cbj.UmlGen.IntegrationTests/bin/...
        // So walk up to repo root:
        string? root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(root, "examples", "ExampleApp", "ExampleApp.sln");
    }

    private static void AssertSnapshot(string snapshotRelativePath, string actual)
    {
        string? snapshotFullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, snapshotRelativePath));

        // Snapshot file is copied to output by default? We'll load from project folder instead:
        // Better: resolve from repo root:
        string? projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string? fullPath = Path.Combine(projectDir, snapshotRelativePath);

        if (!File.Exists(fullPath))
        {
            // Create it once locally (first run) then commit.
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, Normalize(actual));
            throw new Xunit.Sdk.XunitException($"Snapshot created at {fullPath}. Re-run tests.");
        }

        string? expected = File.ReadAllText(fullPath);

        Assert.Equal(Normalize(expected), Normalize(actual));
    }

    private static string Normalize(string s) => s.Replace("\r\n", "\n").Trim();
}