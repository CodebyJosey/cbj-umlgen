using System.CommandLine;
using System.CommandLine.Parsing;
using Cbj.UmlGen.Application.Abstractions;
using Cbj.UmlGen.Application.UseCases.GenerateDiagrams;
using Cbj.UmlGen.Infrastructure.IO;
using Cbj.UmlGen.Infrastructure.Roslyn;
using Cbj.UmlGen.PlantUml.Emitters;

namespace Cbj.UmlGen.Cli.Commands;

/// <summary>
/// Factory for the 'generate' command.
/// </summary>
public static class GenerateCommand
{
    /// <summary>
    /// Creates the 'generate' command.
    /// </summary>
    public static Command Create()
    {
        Command? cmd = new Command("generate", "Generate PlantUML diagrams from a C# solution/project.");

        Option<string>? sourceOption = new Option<string>("--source")
        {
            Description = "Path to a .sln/.csproj or a folder containing one.",
            Arity = ArgumentArity.ExactlyOne
        };

        Option<string>? outOption = new Option<string>("--out")
        {
            Description = "Output directory for generated diagrams.",
            Arity = ArgumentArity.ExactlyOne
        };

        Option<string?> includeOption = new Option<string?>("--include")
        {
            Description = "Include only namespaces starting with this prefix.",
            Arity = ArgumentArity.ZeroOrOne
        };

        Option<string[]>? excludeOption = new Option<string[]>("--exclude")
        {
            Description = "Exclude namespaces starting with these prefixes (repeatable).",
            Arity = ArgumentArity.ZeroOrMore
        };

        // v3 preview: add options via Options.Add
        cmd.Options.Add(sourceOption);
        cmd.Options.Add(outOption);
        cmd.Options.Add(includeOption);
        cmd.Options.Add(excludeOption);

        // v3 preview: SetAction(ParseResult => int)
        cmd.SetAction(parseResult =>
        {
            string? source = parseResult.GetValue(sourceOption);
            string? output = parseResult.GetValue(outOption);
            string? include = parseResult.GetValue(includeOption);
            string[]? exclude = parseResult.GetValue(excludeOption) ?? Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(source))
            {
                Console.Error.WriteLine("Missing required option: --source");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                Console.Error.WriteLine("Missing required option: --out");
                return 1;
            }

            try
            {
                ICodebaseLoader loader = new RoslynCodebaseLoader();
                IFileWriter writer = new FileWriter();

                IUmlEmitter[] emitters = new IUmlEmitter[]
                {
                    new PackageDiagramEmitter(),
                    new ClassDiagramEmitter()
                };

                GenerateDiagramsUseCase? useCase = new GenerateDiagramsUseCase(loader, emitters, writer);

                GenerateDiagramsRequest? request = new GenerateDiagramsRequest(
                    Source: source,
                    OutputDirectory: output,
                    IncludeNamespacePrefix: include,
                    ExcludeNamespacePrefixes: exclude.Length == 0 ? null : exclude);

                // v3 SetAction is sync -> bridge to async
                useCase.ExecuteAsync(request).GetAwaiter().GetResult();

                Console.WriteLine($"Generated diagrams in: {output}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }
        });

        return cmd;
    }
}