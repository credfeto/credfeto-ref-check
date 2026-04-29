using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Ref.Check;

internal static class ReferenceWalker
{
    public static async ValueTask<SymbolReferenceData> CollectReferencedSymbolsAsync(Solution solution, CancellationToken cancellationToken)
    {
        HashSet<ISymbol> actualUsage = new(SymbolEqualityComparer.Default);
        HashSet<ISymbol> registrationOnly = new(SymbolEqualityComparer.Default);

        foreach (Project project in solution.Projects)
        {
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);

            if (compilation is null)
            {
                continue;
            }

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                SemanticModel model = compilation.GetSemanticModel(tree);
                SyntaxNode root = await tree.GetRootAsync(cancellationToken);

                foreach (IdentifierNameSyntax identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    ISymbol? symbol = model.GetSymbolInfo(identifier, cancellationToken).Symbol;

                    if (symbol is null)
                    {
                        continue;
                    }

                    ISymbol original = symbol.OriginalDefinition;

                    if (IsDiRegistrationTypeArgument(identifier, model, cancellationToken))
                    {
                        registrationOnly.Add(original);
                    }
                    else
                    {
                        actualUsage.Add(original);
                    }
                }
            }
        }

        return new SymbolReferenceData(ActualUsage: actualUsage, RegistrationOnly: registrationOnly);
    }

    private static bool IsDiRegistrationTypeArgument(IdentifierNameSyntax identifier, SemanticModel model, in CancellationToken cancellationToken)
    {
        if (identifier.Parent is not TypeArgumentListSyntax typeArgList)
        {
            return false;
        }

        if (typeArgList.Parent is not GenericNameSyntax genericName)
        {
            return false;
        }

        if (!IsServiceRegistrationMethodName(genericName.Identifier.Text))
        {
            return false;
        }

        if (genericName.Parent is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        if (memberAccess.Parent is not InvocationExpressionSyntax)
        {
            return false;
        }

        ITypeSymbol? receiverType = model.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;

        return receiverType is not null && IsServiceCollectionType(receiverType);
    }

    private static bool IsServiceRegistrationMethodName(string name)
    {
        return name.StartsWith(value: "Add", System.StringComparison.Ordinal)
            || name.StartsWith(value: "TryAdd", System.StringComparison.Ordinal)
            || name.StartsWith(value: "Register", System.StringComparison.Ordinal);
    }

    private static bool IsServiceCollectionType(ITypeSymbol type)
    {
        if (string.Equals(type.Name, "IServiceCollection", StringComparison.Ordinal))
        {
            return true;
        }

        return type.AllInterfaces.Any(static iface => string.Equals(iface.Name, "IServiceCollection", StringComparison.Ordinal));
    }
}
