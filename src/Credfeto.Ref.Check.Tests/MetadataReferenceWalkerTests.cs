using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Xunit;

namespace Credfeto.Ref.Check.Tests;

public sealed class MetadataReferenceWalkerTests : TestBase
{
    [Fact]
    public async Task EmptyPathListReturnsEmptySet()
    {
        HashSet<string> result = await MetadataReferenceWalker.CollectTypeReferencesAsync([], this.CancellationToken());

        Assert.Empty(result);
    }

    [Fact]
    public async Task NonExistentPathIsSkipped()
    {
        HashSet<string> result = await MetadataReferenceWalker.CollectTypeReferencesAsync(
            ["/nonexistent/path/to/assembly.dll"],
            this.CancellationToken()
        );

        Assert.Empty(result);
    }

    [Fact]
    public async Task NonPeFileReturnsEmptySet()
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "not a PE file", this.CancellationToken());

            HashSet<string> result = await MetadataReferenceWalker.CollectTypeReferencesAsync(
                [tempFile],
                this.CancellationToken()
            );

            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ValidAssemblyReturnsTypeReferences()
    {
        string assemblyPath = typeof(MetadataReferenceWalkerTests).Assembly.Location;

        HashSet<string> result = await MetadataReferenceWalker.CollectTypeReferencesAsync(
            [assemblyPath],
            this.CancellationToken()
        );

        Assert.NotEmpty(result);
    }

    [Fact]
    public Task CancelledTokenThrowsOperationCanceledException()
    {
        CancellationToken cancelledToken = new(canceled: true);
        string validPath = typeof(MetadataReferenceWalkerTests).Assembly.Location;

        return Assert.ThrowsAsync<OperationCanceledException>(() =>
            MetadataReferenceWalker.CollectTypeReferencesAsync([validPath], cancelledToken).AsTask()
        );
    }
}
