using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Ref.Check;

public static class MetadataReferenceWalker
{
    public static async ValueTask<HashSet<string>> CollectTypeReferencesAsync(
        IEnumerable<string> assemblyPaths,
        CancellationToken cancellationToken
    )
    {
        HashSet<string> referenced = new(StringComparer.Ordinal);

        foreach (string path in assemblyPaths.Where(static path => File.Exists(path)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await TryCollectFromAssemblyAsync(path, referenced);
        }

        return referenced;
    }

    private static async ValueTask<bool> TryCollectFromAssemblyAsync(string path, HashSet<string> referenced)
    {
        try
        {
            await CollectTypeReferencesFromAssemblyAsync(path, referenced);

            return true;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }

    private static async ValueTask CollectTypeReferencesFromAssemblyAsync(string path, HashSet<string> referenced)
    {
        await using FileStream stream = File.OpenRead(path);
        using PEReader peReader = new(stream);

        MetadataReader metadata = peReader.GetMetadataReader();

        foreach (TypeReferenceHandle handle in metadata.TypeReferences)
        {
            TypeReference typeRef = metadata.GetTypeReference(handle);

            // Nested types have a TypeReference as ResolutionScope — their namespace is inherited from the parent
            if (typeRef.ResolutionScope.Kind == HandleKind.TypeReference)
            {
                continue;
            }

            string ns = metadata.GetString(typeRef.Namespace);
            string name = metadata.GetString(typeRef.Name);
            referenced.Add($"{ns}.{name}");
        }
    }
}
