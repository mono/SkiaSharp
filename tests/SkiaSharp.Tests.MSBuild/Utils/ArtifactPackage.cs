using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.MSBuild.Utils;

public sealed record ArtifactPackage(string Id, string Version, string ContentHash, Dictionary<string, string> Files)
{
    public Dictionary<string, string> NativeFiles => Files.Where(entry =>
        entry.Key.StartsWith("runtimes/", StringComparison.Ordinal) &&
        entry.Key.Contains("/native/", StringComparison.Ordinal) &&
        (entry.Key.EndsWith(".dll", StringComparison.Ordinal) ||
         entry.Key.EndsWith(".so", StringComparison.Ordinal) ||
         entry.Key.EndsWith(".dylib", StringComparison.Ordinal)))
        .ToDictionary();

    public static ArtifactPackage Read(string directory, string id)
    {
        var matches = new List<ArtifactPackage>();
        foreach (var file in Directory.EnumerateFiles(directory, id + ".*.nupkg", SearchOption.AllDirectories)
            .Where(file => !file.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase)))
        {
            using var zip = ZipFile.OpenRead(file);
            using var nuspec = zip.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
            var metadata = XDocument.Load(nuspec).Root!.Elements().Single(element => element.Name.LocalName == "metadata");
            if (metadata.Elements().Single(element => element.Name.LocalName == "id").Value != id)
                continue;
            var version = metadata.Elements().Single(element => element.Name.LocalName == "version").Value;
            var hashes = zip.Entries.Where(entry => !entry.FullName.EndsWith("/", StringComparison.Ordinal))
                .ToDictionary(entry => entry.FullName, entry =>
                {
                    using var stream = entry.Open();
                    return Convert.ToHexString(SHA256.HashData(stream));
                });
            using var packageStream = File.OpenRead(file);
            matches.Add(new(id, version, Convert.ToBase64String(SHA512.HashData(packageStream)), hashes));
        }
        Assert.True(matches.Count == 1, $"Expected one real {id} package in {directory}, found {matches.Count}");
        return matches[0];
    }

    public string CachePath(DotNet dotnet, string entry = "") => Path.GetFullPath(Path.Combine(
        dotnet.PackagesCacheDirectory, Id.ToLowerInvariant(), Version.ToLowerInvariant(),
        entry.Replace('/', Path.DirectorySeparatorChar)));

    public void AssertRestored(DotNet dotnet)
    {
        Assert.Equal(ContentHash, File.ReadAllText(CachePath(dotnet,
            $"{Id.ToLowerInvariant()}.{Version.ToLowerInvariant()}.nupkg.sha512")).Trim());
        using var metadata = JsonDocument.Parse(File.ReadAllText(CachePath(dotnet, ".nupkg.metadata")));
        var source = metadata.RootElement.GetProperty("source").GetString()!;
        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.IsFile)
            source = uri.LocalPath;
        Assert.Equal(Path.TrimEndingDirectorySeparator(dotnet.PackageDirectory),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(source)),
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    }

    public static void AssertFileHash(string file, string expected)
    {
        Assert.True(File.Exists(file), $"Missing native asset: {file}");
        using var stream = File.OpenRead(file);
        Assert.Equal(expected, Convert.ToHexString(SHA256.HashData(stream)));
    }
}
