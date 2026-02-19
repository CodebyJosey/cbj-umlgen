using System.CommandLine;
using System.CommandLine.Parsing;
using Cbj.UmlGen.Cli.Commands;

RootCommand? root = new RootCommand("cbj-umlgen - Generate PlantUML diagrams from C# codebases.");

root.Subcommands.Add(GenerateCommand.Create());

ParseResult result = root.Parse(args);
return result.Invoke();