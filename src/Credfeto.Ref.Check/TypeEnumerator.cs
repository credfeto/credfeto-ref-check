using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Credfeto.Ref.Check;

internal static class TypeEnumerator
{
    public static IReadOnlyList<INamedTypeSymbol> GetPublicTypes(IAssemblySymbol assembly)
    {
        List<INamedTypeSymbol> types = [];
        CollectPublicTypes(assembly.GlobalNamespace, types);

        return types;
    }

    private static void CollectPublicTypes(INamespaceSymbol ns, List<INamedTypeSymbol> types)
    {
        foreach (INamedTypeSymbol type in ns.GetTypeMembers())
        {
            if (type.DeclaredAccessibility == Accessibility.Public)
            {
                types.Add(type);
            }
        }

        foreach (INamespaceSymbol nested in ns.GetNamespaceMembers())
        {
            CollectPublicTypes(nested, types);
        }
    }
}
