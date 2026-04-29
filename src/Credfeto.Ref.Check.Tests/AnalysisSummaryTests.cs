using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class AnalysisSummaryTests : TestBase
{
    [Fact]
    public void AnalysisSummaryHasTotalPackages()
    {
        AnalysisSummary summary = new()
        {
            TotalPackages = 5,
            PackagesWithUnusedTypes = 2,
            TotalUnusedTypes = 7,
        };

        Assert.Equal(expected: 5, actual: summary.TotalPackages);
    }

    [Fact]
    public void AnalysisSummaryHasPackagesWithUnusedTypes()
    {
        AnalysisSummary summary = new()
        {
            TotalPackages = 5,
            PackagesWithUnusedTypes = 2,
            TotalUnusedTypes = 7,
        };

        Assert.Equal(expected: 2, actual: summary.PackagesWithUnusedTypes);
    }

    [Fact]
    public void AnalysisSummaryHasTotalUnusedTypes()
    {
        AnalysisSummary summary = new()
        {
            TotalPackages = 5,
            PackagesWithUnusedTypes = 2,
            TotalUnusedTypes = 7,
        };

        Assert.Equal(expected: 7, actual: summary.TotalUnusedTypes);
    }
}
