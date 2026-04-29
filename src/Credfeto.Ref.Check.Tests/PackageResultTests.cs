using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class PackageResultTests : TestBase
{
    [Fact]
    public void PackageResultHasPackageId()
    {
        PackageResult result = new()
        {
            PackageId = "FunFair.Common.Example",
            TotalPublicTypes = 0,
            UnusedTypes = [],
        };

        Assert.Equal(expected: "FunFair.Common.Example", actual: result.PackageId);
    }

    [Fact]
    public void PackageResultHasEmptyUnusedTypesWhenNoneProvided()
    {
        PackageResult result = new()
        {
            PackageId = "FunFair.Common.Example",
            TotalPublicTypes = 0,
            UnusedTypes = [],
        };

        Assert.Empty(result.UnusedTypes);
    }

    [Fact]
    public void PackageResultTracksTotalPublicTypes()
    {
        PackageResult result = new()
        {
            PackageId = "FunFair.Common.Example",
            TotalPublicTypes = 5,
            UnusedTypes = [],
        };

        Assert.Equal(expected: 5, actual: result.TotalPublicTypes);
    }
}
