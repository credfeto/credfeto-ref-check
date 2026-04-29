using System.Diagnostics;

namespace Credfeto.Ref.Check;

[DebuggerDisplay("{TotalUnusedTypes} unused in {TotalPackages} packages")]
public sealed record AnalysisSummary
{
    public required int TotalPackages { get; init; }

    public required int PackagesWithUnusedTypes { get; init; }

    public required int TotalUnusedTypes { get; init; }
}
