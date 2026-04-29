using System.Collections.Generic;
using Cocona;

namespace Credfeto.Ref.Check;

public sealed class CheckOptions : ICommandParameterSet
{
    [Option('s', Description = "Path to the .slnx or .sln solution file")]
    [HasDefaultValue]
    public string Solution { get; init; } = string.Empty;

    [Option('p', Description = "NuGet package name prefix to check (may be specified multiple times)")]
    [HasDefaultValue]
    public IReadOnlyList<string> Packages { get; init; } = [];

    [Option('j', Description = "Output results as JSON")]
    [HasDefaultValue]
    public bool Json { get; init; }
}
