# cbj-umlgen

A .NET CLI tool that analyzes C# solutions and projects and
automatically generates PlantUML architecture diagrams.

------------------------------------------------------------------------

## 🚀 What is cbj-umlgen?

`cbj-umlgen` analyzes a C# codebase using Roslyn and generates
high-quality PlantUML diagrams that help you understand:

-   🧩 Package / namespace structure\
-   🏗 Class relationships (inheritance, interfaces, dependencies)\
-   🧱 Clean Architecture layers and cross-layer dependencies

It works with:

-   `.sln` files\
-   `.csproj` files\
-   Folders containing either of the above

------------------------------------------------------------------------

## 📦 Generated Diagrams

Running the tool generates:

  -----------------------------------------------------------------------
  File                    Description
  ----------------------- -----------------------------------------------
  `packages.puml`         Namespace/package overview

  `classes.puml`          Class diagram (types + relationships +
                          dependencies)

  `layers.puml`           Clean Architecture layer diagram
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## 🛠 Requirements

-   .NET SDK (latest stable recommended)
-   A C# solution or project that builds locally

------------------------------------------------------------------------

# ⚡ Quick Start

Clone the repository:

``` bash
git clone https://github.com/<your-username>/cbj-umlgen.git
cd cbj-umlgen
```

Build the project:

``` bash
dotnet build -c Release
```

Generate diagrams:

``` bash
dotnet run --project src/Cbj.UmlGen.Cli -- generate --source . --out docs/diagrams
```

Diagrams will appear in:

    docs/diagrams/

------------------------------------------------------------------------

# 🧪 Usage

## Command

``` bash
cbj-umlgen generate --source <path> --out <directory> [options]
```

## Required Options

  Option       Description
  ------------ --------------------------------------
  `--source`   Path to `.sln`, `.csproj`, or folder
  `--out`      Output directory for `.puml` files

## Optional Options

  -----------------------------------------------------------------------
  Option                       Description
  ---------------------------- ------------------------------------------
  `--include <prefix>`         Only include namespaces starting with this
                               prefix

  `--exclude <prefix>`         Exclude namespaces starting with this
                               prefix (repeatable)
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## 🧾 Examples

Analyze current directory:

``` bash
dotnet run --project src/Cbj.UmlGen.Cli -- generate --source . --out docs/diagrams
```

Analyze a solution file:

``` bash
dotnet run --project src/Cbj.UmlGen.Cli -- generate --source "C:\dev\MyApp\MyApp.sln" --out "C:\dev\MyApp\docs\diagrams"
```

Exclude test namespaces:

``` bash
dotnet run --project src/Cbj.UmlGen.Cli -- generate --source . --out docs/diagrams --exclude ".Tests" --exclude ".Test"
```

------------------------------------------------------------------------

# 🖼 Rendering the Diagrams

### Option 1 --- VS Code

Install the **PlantUML extension**, open any `.puml` file, and preview.

### Option 2 --- PlantUML Web Server

Paste the `.puml` content into the PlantUML web server and render.

### Option 3 --- Local PlantUML

``` bash
plantuml -tpng docs/diagrams/*.puml
```

------------------------------------------------------------------------

# 🧱 Project Structure

    src/
      Cbj.UmlGen.Domain
      Cbj.UmlGen.Application
      Cbj.UmlGen.Infrastructure
      Cbj.UmlGen.PlantUml
      Cbj.UmlGen.Cli
    tests/
    examples/

------------------------------------------------------------------------

# 🗺 Roadmap

-   Select specific diagrams via CLI flag
-   Namespace dependency diagram
-   Split class diagrams per namespace
-   GitHub Action for automatic diagram generation

------------------------------------------------------------------------

# 📄 License

MIT
