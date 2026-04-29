using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Ref.Check;

[DebuggerDisplay("ActualUsage={ActualUsage.Count}, RegistrationOnly={RegistrationOnly.Count}")]
internal sealed record SymbolReferenceData(IReadOnlySet<ISymbol> ActualUsage, IReadOnlySet<ISymbol> RegistrationOnly);
