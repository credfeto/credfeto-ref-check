using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cocona;
using Microsoft.Extensions.Logging;

namespace Credfeto.Ref.Check;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        using CoconaApp app = CoconaApp.Create(args);

        app.AddCommand(
            async (CheckOptions options, ILogger<CheckOptions> logger) =>
            {
                CancellationToken cancellationToken = app.Lifetime.ApplicationStopping;

                if (string.IsNullOrWhiteSpace(options.Solution))
                {
                    logger.LogError("--solution is required");

                    return 1;
                }

                if (options.Packages.Length == 0)
                {
                    logger.LogError("At least one --packages prefix is required");

                    return 1;
                }

                AnalysisResult result = await ReferenceChecker.CheckAsync(
                    options.Solution,
                    options.Packages,
                    cancellationToken
                );

                if (options.Json)
                {
                    Console.WriteLine(JsonOutput.Serialise(result));
                }
                else
                {
                    PrintHumanReadable(result);
                }

                return 0;
            }
        );

        await app.RunAsync(app.Lifetime.ApplicationStopping);
    }

    private static void PrintHumanReadable(AnalysisResult result)
    {
        foreach (PackageResult package in result.Packages)
        {
            if (package.UnusedTypes.Count == 0)
            {
                Console.WriteLine(
                    $"{package.PackageId} (all {package.TotalPublicTypes} types used)"
                );
            }
            else
            {
                Console.WriteLine(
                    $"{package.PackageId} ({package.UnusedTypes.Count} of {package.TotalPublicTypes} types unused)"
                );

                foreach (TypeResult type in package.UnusedTypes)
                {
                    string registrationNote = type.RegistrationOnly
                        ? " (registration only)"
                        : string.Empty;

                    Console.WriteLine(
                        $"  ✗ {type.FullyQualifiedName} [{type.Kind.GetName()}]{registrationNote}"
                    );
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine(
            $"Summary: {result.Summary.TotalUnusedTypes} unused types across {result.Summary.PackagesWithUnusedTypes} of {result.Summary.TotalPackages} packages"
        );
    }
}
