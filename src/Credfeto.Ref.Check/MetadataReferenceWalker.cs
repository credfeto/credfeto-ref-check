using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Credfeto.Ref.Check;

internal static class MetadataReferenceWalker
{
    public static async Task<HashSet<string>> CollectTypeReferencesAsync(
        IEnumerable<string> assemblyPaths
    )
    {
        HashSet<string> referenced = new(StringComparer.Ordinal);

        foreach (
            string path in System.Linq.Enumerable.Where(
                assemblyPaths,
                static path => File.Exists(path)
            )
        )
        {
            await TryCollectFromAssemblyAsync(path, referenced);
        }

        return referenced;
    }

    private static async Task<bool> TryCollectFromAssemblyAsync(
        string path,
        HashSet<string> referenced
    )
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

    private static async Task CollectTypeReferencesFromAssemblyAsync(
        string path,
        HashSet<string> referenced
    )
    {
        await using FileStream stream = File.OpenRead(path);
        using PEReader peReader = new(stream);

        if (!peReader.HasMetadata)
        {
            return;
        }

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

            if (string.IsNullOrEmpty(ns))
            {
                continue;
            }

            string name = metadata.GetString(typeRef.Name);
            referenced.Add($"{ns}.{name}");
        }
    }
}
