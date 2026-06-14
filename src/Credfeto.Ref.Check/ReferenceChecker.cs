using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Credfeto.Ref.Check;

public static class ReferenceChecker
{
    public static async ValueTask<AnalysisResult> CheckAsync(
        string solutionPath,
        IReadOnlyList<string> packagePrefixes,
        CancellationToken cancellationToken
    )
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        string absolutePath = Path.GetFullPath(solutionPath);

        using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        Solution solution = absolutePath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)
            ? await OpenSlnxAsync(workspace, absolutePath, cancellationToken)
            : await workspace.OpenSolutionAsync(absolutePath, cancellationToken: cancellationToken);

        SymbolReferenceData references = await ReferenceWalker.CollectReferencedSymbolsAsync(
            solution,
            cancellationToken
        );

        IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)> typesByPackage =
            await CollectPublicTypesByPackageAsync(solution, packagePrefixes, cancellationToken);

        IReadOnlySet<string> nonTargetPaths = await CollectNonTargetAssemblyPathsAsync(
            solution,
            packagePrefixes,
            cancellationToken
        );
        HashSet<string> metadataReferencedTypeKeys = await MetadataReferenceWalker.CollectTypeReferencesAsync(
            nonTargetPaths,
            cancellationToken
        );

        return BuildAnalysisResult(absolutePath, typesByPackage, references, metadataReferencedTypeKeys);
    }

    private static async ValueTask<IReadOnlySet<string>> CollectNonTargetAssemblyPathsAsync(
        Solution solution,
        IReadOnlyList<string> packagePrefixes,
        CancellationToken cancellationToken
    )
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);

        foreach (Project project in solution.Projects)
        {
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);

            if (compilation is null)
            {
                continue;
            }

            foreach (string? display in compilation.References.Select(static r => r.Display))
            {
                if (display is null)
                {
                    continue;
                }

                string packageId = Path.GetFileNameWithoutExtension(display);

                if (packagePrefixes.Any(prefix => packageId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                paths.Add(display);
            }
        }

        return paths;
    }

    private static async ValueTask<Solution> OpenSlnxAsync(
        MSBuildWorkspace workspace,
        string slnxPath,
        CancellationToken cancellationToken
    )
    {
        string solutionDir = Path.GetDirectoryName(slnxPath) ?? Path.GetPathRoot(slnxPath) ?? string.Empty;
        XDocument doc = XDocument.Load(slnxPath);

        IEnumerable<string> projectPaths = doc.Descendants("Project")
            .Select(static e => e.Attribute("Path")?.Value)
            .OfType<string>()
            .Select(p => Path.GetFullPath(Path.Combine(solutionDir, p)))
            .Where(File.Exists);

        foreach (string projectPath in projectPaths)
        {
            bool alreadyLoaded = workspace.CurrentSolution.Projects.Any(p =>
                string.Equals(p.FilePath, projectPath, StringComparison.OrdinalIgnoreCase)
            );

            if (!alreadyLoaded)
            {
                await workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);
            }
        }

        return workspace.CurrentSolution;
    }

    private static async ValueTask<
        IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)>
    > CollectPublicTypesByPackageAsync(
        Solution solution,
        IReadOnlyList<string> packagePrefixes,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, List<INamedTypeSymbol>> typesByPackage = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> seenAssemblyPaths = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> seenAssemblyIdentities = new(StringComparer.OrdinalIgnoreCase);

        foreach (Project project in solution.Projects)
        {
            Compilation? compilation = await project.GetCompilationAsync(cancellationToken);

            if (compilation is null)
            {
                continue;
            }

            ProcessCompilationReferences(
                compilation,
                packagePrefixes,
                seenAssemblyPaths,
                seenAssemblyIdentities,
                typesByPackage
            );
        }

        return typesByPackage
            .OrderBy(static kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(static kv => (kv.Key, (IReadOnlyList<INamedTypeSymbol>)kv.Value))
            .ToList();
    }

    private static void ProcessCompilationReferences(
        Compilation compilation,
        IReadOnlyList<string> packagePrefixes,
        HashSet<string> seenAssemblyPaths,
        HashSet<string> seenAssemblyIdentities,
        Dictionary<string, List<INamedTypeSymbol>> typesByPackage
    )
    {
        foreach (MetadataReference reference in compilation.References)
        {
            if (reference.Display is null)
            {
                continue;
            }

            if (!seenAssemblyPaths.Add(reference.Display))
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

            if (!seenAssemblyIdentities.Add(assembly.Identity.GetDisplayName()))
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

    private static AnalysisResult BuildAnalysisResult(
        string solutionPath,
        IReadOnlyList<(string PackageId, IReadOnlyList<INamedTypeSymbol> Types)> typesByPackage,
        SymbolReferenceData references,
        IReadOnlySet<string> metadataReferencedTypeKeys
    )
    {
        List<PackageResult> packages =
        [
            .. typesByPackage.Select(entry =>
                BuildPackageResult(entry.PackageId, entry.Types, references, metadataReferencedTypeKeys)
            ),
        ];

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

    private static PackageResult BuildPackageResult(
        string packageId,
        IReadOnlyList<INamedTypeSymbol> allTypes,
        SymbolReferenceData references,
        IReadOnlySet<string> metadataReferencedTypeKeys
    )
    {
        IReadOnlyList<INamedTypeSymbol> distinctTypes =
        [
            .. allTypes.GroupBy(static t => t.ToDisplayString(), StringComparer.Ordinal).Select(static g => g.First()),
        ];
        List<TypeResult> unused = [];

        foreach (INamedTypeSymbol type in distinctTypes)
        {
            if (IsTypeUsed(type, references, metadataReferencedTypeKeys))
            {
                continue;
            }

            bool registrationOnly = references.RegistrationOnly.Contains(type.OriginalDefinition);

            unused.Add(
                new TypeResult
                {
                    TypeName = type.Name,
                    Namespace = type.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    FullyQualifiedName = type.ToDisplayString(),
                    Kind = MapTypeKind(type),
                    RegistrationOnly = registrationOnly,
                }
            );
        }

        unused.Sort(
            static (a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.FullyQualifiedName, b.FullyQualifiedName)
        );

        return new PackageResult
        {
            PackageId = packageId,
            TotalPublicTypes = distinctTypes.Count,
            UnusedTypes = unused,
        };
    }

    private static bool IsTypeUsed(
        INamedTypeSymbol type,
        SymbolReferenceData references,
        IReadOnlySet<string> metadataReferencedTypeKeys
    )
    {
        if (references.ActualUsage.Contains(type.OriginalDefinition))
        {
            return true;
        }

        string metadataKey = $"{type.ContainingNamespace?.ToDisplayString() ?? string.Empty}.{type.MetadataName}";

        return metadataReferencedTypeKeys.Contains(metadataKey);
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
