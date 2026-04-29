using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Credfeto.Ref.Check;

[DebuggerDisplay("{Solution}")]
public sealed record AnalysisResult
{
    public required string Solution { get; init; }

    public required DateTimeOffset AnalysedAt { get; init; }

    public required IReadOnlyList<PackageResult> Packages { get; init; }

    public required AnalysisSummary Summary { get; init; }
}
