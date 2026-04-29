# Credfeto.Ref.Check

[![Build Status](https://github.com/credfeto/credfeto-ref-check/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-ref-check/actions/workflows/build-and-publish-pre-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Credfeto.Ref.Check.svg)](https://www.nuget.org/packages/Credfeto.Ref.Check)

Roslyn-based static analysis tool that finds public types from named NuGet packages that are not referenced anywhere in a .NET solution.

Use it to identify which parts of a dependency are unused so you can remove unnecessary packages, reduce binary size, and understand your actual dependency surface.

## Installation

```bash
dotnet tool install -g Credfeto.Ref.Check
```

## Usage

```bash
ref-check --solution <path-to.slnx> --packages <prefix> [--packages <prefix>] [--json]
```

### Options

| Option | Description |
|--------|-------------|
| `--solution` | Path to the `.slnx` or `.sln` solution file (required) |
| `--packages` | NuGet package name prefix to check (required, may be specified multiple times) |
| `--json` | Output results as JSON instead of human-readable text |

### Examples

Check a single package family:

```bash
ref-check --solution ./src/MyApp.slnx --packages FunFair.Common
```

Check multiple package families:

```bash
ref-check --solution ./src/MyApp.slnx --packages FunFair.Common --packages FunFair.Ethereum
```

Output as JSON (useful for CI or scripting):

```bash
ref-check --solution ./src/MyApp.slnx --packages FunFair.Common --json
```

## Output

### Human-readable (default)

```
FunFair.Common.BackgroundServices
  ✗ FunFair.Common.BackgroundServices.TimedHostedServiceBase
  ✗ FunFair.Common.BackgroundServices.IBackgroundServiceMonitor

FunFair.Common.Messaging (all types used)
```

### JSON

```json
{
  "packages": [
    {
      "name": "FunFair.Common.BackgroundServices",
      "unusedTypes": [
        "FunFair.Common.BackgroundServices.TimedHostedServiceBase",
        "FunFair.Common.BackgroundServices.IBackgroundServiceMonitor"
      ]
    }
  ]
}
```

## How it works

1. Loads the solution using the MSBuild workspace API (Roslyn).
2. For each project, inspects all referenced assemblies whose names match the given prefixes.
3. Enumerates all public types exported by those assemblies.
4. Walks all syntax trees in the solution, resolving identifiers to symbols.
5. Reports public types that were never referenced.

## Licence

This project is licensed under the MIT Licence — see the [LICENSE](LICENSE) file for details.
