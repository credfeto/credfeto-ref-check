using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Credfeto.Ref.Check;

public static class ReferenceChecker
{
    public static async ValueTask<AnalysisResult> CheckAsync(string solutionPath, IReadOnlyList<string> packagePrefixes, CancellationToken cancellationToken)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        string absolutePath = Path.GetFullPath(solutionPath);

        using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        Solution solution = await workspace.OpenSolutionAsync(absolutePath, cancellationToken: cancellationToken);

        SymbolReferenceData references = await ReferenceWalker.CollectReferencedSymbolsAsync(solution, cancellationToken);

        IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)> typesByPackage =
            await CollectPublicTypesByPackageAsync(solution, packagePrefixes, cancellationToken);

        return BuildAnalysisResult(absolutePath, typesByPackage, references);
    }

    private static async ValueTask<IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)>> CollectPublicTypesByPackageAsync(
        Solution solution,
        IReadOnlyList<string> packagePrefixes,
        CancellationToken cancellationToken)
    {
        Dictionary<string, List<INamedTypeSymbol>> typesByPackage = new(StringComparer.OrdinalIgnoreCase);

        foreach (Project project in solution.Projects)
        {
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);

            if (compilation is null)
            {
                continue;
            }

            foreach (MetadataReference reference in compilation.References)
            {
                if (reference.Display is null)
                {
                    continue;
                }

                string packageId = Path.GetFileNameWithoutExtension(reference.Display);

                if (!packagePrefixes.Any(prefix => packageId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly)
                {
                    continue;
                }

                if (!typesByPackage.TryGetValue(packageId, out List<INamedTypeSymbol>? types))
                {
                    types = [];
                    typesByPackage[packageId] = types;
                }

                types.AddRange(TypeEnumerator.GetPublicTypes(assembly));
            }
        }

        return typesByPackage
               .OrderBy(static kv => kv.Key, StringComparer.OrdinalIgnoreCase)
               .Select(static kv => (kv.Key, (IReadOnlyList<INamedTypeSymbol>)kv.Value))
               .ToList();
    }

    private static AnalysisResult BuildAnalysisResult(
        string solutionPath,
        IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)> typesByPackage,
        SymbolReferenceData references)
    {
        List<PackageResult> packages = [];

        foreach ((string packageId, IReadOnlyList<INamedTypeSymbol> allTypes) in typesByPackage)
        {
            List<TypeResult> unused = [];

            foreach (INamedTypeSymbol type in allTypes)
            {
                if (references.ActualUsage.Contains(type.OriginalDefinition))
                {
                    continue;
                }

                bool registrationOnly = references.RegistrationOnly.Contains(type.OriginalDefinition);

                unused.Add(new TypeResult
                           {
                               TypeName = type.Name,
                               Namespace = type.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                               FullyQualifiedName = type.ToDisplayString(),
                               Kind = MapTypeKind(type),
                               RegistrationOnly = registrationOnly,
                           });
            }

            unused.Sort(static (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullyQualifiedName, b.FullyQualifiedName));

            packages.Add(new PackageResult
                         {
                             PackageId = packageId,
                             TotalPublicTypes = allTypes.Count,
                             UnusedTypes = unused,
                         });
        }

        int totalUnused = packages.Sum(static p => p.UnusedTypes.Count);
        int packagesWithUnused = packages.Count(static p => p.UnusedTypes.Count > 0);

        return new AnalysisResult
               {
                   Solution = solutionPath,
                   AnalysedAt = TimeProvider.System.GetUtcNow(),
                   Packages = packages,
                   Summary = new AnalysisSummary
                             {
                                 TotalPackages = packages.Count,
                                 PackagesWithUnusedTypes = packagesWithUnused,
                                 TotalUnusedTypes = totalUnused,
                             },
               };
    }

    private static TypeKind MapTypeKind(INamedTypeSymbol type)
    {
        if (type.IsRecord)
        {
            return TypeKind.Record;
        }

        return type.TypeKind switch
        {
            Microsoft.CodeAnalysis.TypeKind.Class => TypeKind.Class,
            Microsoft.CodeAnalysis.TypeKind.Interface => TypeKind.Interface,
            Microsoft.CodeAnalysis.TypeKind.Struct => TypeKind.Struct,
            Microsoft.CodeAnalysis.TypeKind.Enum => TypeKind.Enum,
            Microsoft.CodeAnalysis.TypeKind.Delegate => TypeKind.Delegate,
            _ => TypeKind.Class,
        };
    }
}
