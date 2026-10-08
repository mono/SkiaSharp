using System.Collections.Concurrent;
using Xunit;

namespace SkiaSharp.Tests.MSBuild.Utils;

public sealed class PackageInventory(string directory)
{
    private readonly ConcurrentDictionary<string, Lazy<ArtifactPackage>> packages = new(StringComparer.Ordinal);

    public ArtifactPackage Get(string id) =>
        packages.GetOrAdd(id, key => new Lazy<ArtifactPackage>(() => ArtifactPackage.Read(directory, key))).Value;
}

[CollectionDefinition("Package consumers")]
public sealed class PackageConsumerCollection : ICollectionFixture<DotNet>;
