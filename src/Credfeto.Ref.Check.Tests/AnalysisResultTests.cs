using System;
using System.Collections.Generic;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class AnalysisResultTests : TestBase
{
    [Fact]
    public void AnalysisResultHasSolutionPath()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 4, day: 29, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        Assert.Equal(expected: "/path/to/solution.slnx", actual: result.Solution);
    }

    [Fact]
    public void AnalysisResultHasAnalysedAt()
    {
        DateTimeOffset timestamp = new(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);

        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = timestamp,
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        Assert.Equal(expected: timestamp, actual: result.AnalysedAt);
    }

    [Fact]
    public void AnalysisResultHasEmptyPackagesWhenNoneProvided()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 4, day: 29, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        Assert.Empty(result.Packages);
    }

    [Fact]
    public void AnalysisResultHasPackagesWhenProvided()
    {
        IReadOnlyList<PackageResult> packages =
        [
            new PackageResult { PackageId = "Test.Package", TotalPublicTypes = 3, UnusedTypes = [] },
        ];

        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 4, day: 29, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = packages,
            Summary = new AnalysisSummary { TotalPackages = 1, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        Assert.Single(result.Packages);
    }

    [Fact]
    public void AnalysisResultHasSummary()
    {
        AnalysisSummary summary = new()
        {
            TotalPackages = 3,
            PackagesWithUnusedTypes = 1,
            TotalUnusedTypes = 4,
        };

        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 4, day: 29, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = summary,
        };

        Assert.Equal(expected: summary, actual: result.Summary);
    }
}
