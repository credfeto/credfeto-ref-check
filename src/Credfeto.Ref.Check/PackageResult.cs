using System.Collections.Generic;
using System.Diagnostics;

namespace Credfeto.Ref.Check;

[DebuggerDisplay("{PackageId}: {UnusedTypes.Count} unused of {TotalPublicTypes}")]
public sealed record PackageResult
{
    public required string PackageId { get; init; }

    public required int TotalPublicTypes { get; init; }

    public required IReadOnlyList<TypeResult> UnusedTypes { get; init; }
}
