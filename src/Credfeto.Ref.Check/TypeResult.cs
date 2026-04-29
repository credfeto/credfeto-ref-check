using System.Diagnostics;

namespace Credfeto.Ref.Check;

[DebuggerDisplay("{FullyQualifiedName} ({Kind})")]
public sealed record TypeResult
{
    public required string TypeName { get; init; }

    public required string Namespace { get; init; }

    public required string FullyQualifiedName { get; init; }

    public required TypeKind Kind { get; init; }

    public required bool RegistrationOnly { get; init; }
}
