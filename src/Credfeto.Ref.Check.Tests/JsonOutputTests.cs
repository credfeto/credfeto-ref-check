using System;
using System.Text.Json;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class JsonOutputTests : TestBase
{
    [Fact]
    public void SerialiseProducesValidJson()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        string json = JsonOutput.Serialise(result);

        JsonDocument.Parse(json).Dispose();
    }

    [Fact]
    public void SerialiseIncludesSolutionPath()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/my.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        string json = JsonOutput.Serialise(result);

        Assert.Contains("/path/to/my.slnx", json, StringComparison.Ordinal);
    }

    [Fact]
    public void SerialiseUsesKebabCasePropertyNames()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages = [],
            Summary = new AnalysisSummary { TotalPackages = 0, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        string json = JsonOutput.Serialise(result);

        Assert.Contains("\"solution\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void SerialiseIncludesPackages()
    {
        AnalysisResult result = new()
        {
            Solution = "/path/to/solution.slnx",
            AnalysedAt = new DateTimeOffset(year: 2026, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero),
            Packages =
            [
                new PackageResult { PackageId = "FunFair.Example", TotalPublicTypes = 2, UnusedTypes = [] },
            ],
            Summary = new AnalysisSummary { TotalPackages = 1, PackagesWithUnusedTypes = 0, TotalUnusedTypes = 0 },
        };

        string json = JsonOutput.Serialise(result);

        Assert.Contains("FunFair.Example", json, StringComparison.Ordinal);
    }
}
