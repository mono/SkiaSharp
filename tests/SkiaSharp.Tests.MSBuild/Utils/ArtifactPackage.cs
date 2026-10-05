using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.MSBuild.Utils;

public sealed record ArtifactPackage(string Id, string Version, string RepositoryCommit, string ContentHash,
    Dictionary<string, string> Files)
{
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
            var commit = metadata.Elements().Single(element => element.Name.LocalName == "repository").Attribute("commit")!.Value;
            Assert.True(commit.Length == 40 && commit.All(Uri.IsHexDigit), $"Missing producer commit in {file}");
            var hashes = zip.Entries.Where(entry => !entry.FullName.EndsWith("/", StringComparison.Ordinal))
                .ToDictionary(entry => entry.FullName, entry =>
                {
                    using var stream = entry.Open();
                    return Convert.ToHexString(SHA256.HashData(stream));
                });
            using var packageStream = File.OpenRead(file);
            matches.Add(new(id, version, commit, Convert.ToBase64String(SHA512.HashData(packageStream)), hashes));
        }
        Assert.True(matches.Count == 1, $"Expected one real {id} package in {directory}, found {matches.Count}");
        return matches[0];
    }

    public string CachePath(DotNet dotnet, string entry = "") => Path.GetFullPath(Path.Combine(
        dotnet.PackagesCacheDirectory, Id.ToLowerInvariant(), Version.ToLowerInvariant(),
        entry.Replace('/', Path.DirectorySeparatorChar)));

    public static void AssertRestore(DotNet dotnet, string project, string framework, IEnumerable<ArtifactPackage> required)
    {
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        Assert.Equal(framework, Assert.Single(assets.RootElement.GetProperty("project").GetProperty("frameworks")
            .EnumerateObject()).Name);
        var restored = assets.RootElement.GetProperty("libraries");
        var packages = required.ToArray();
        Assert.Single(packages.Select(package => package.RepositoryCommit).Distinct());
        foreach (var package in packages)
            Assert.True(restored.TryGetProperty(package.Id + "/" + package.Version, out _),
                $"Missing {package.Id}/{package.Version} in {framework}");
        var provenance = new List<object>();
        foreach (var library in restored.EnumerateObject().Where(library =>
            library.Name.StartsWith("SkiaSharp", StringComparison.Ordinal) ||
            library.Name.StartsWith("HarfBuzzSharp", StringComparison.Ordinal)))
        {
            var identity = library.Name.Split('/');
            var package = dotnet.Packages.Get(identity[0]);
            Assert.Equal(package.Version, identity[1]);
            Assert.Equal(packages[0].RepositoryCommit, package.RepositoryCommit);
            Assert.Equal(package.ContentHash, library.Value.GetProperty("sha512").GetString());
            Assert.Equal(package.ContentHash, File.ReadAllText(package.CachePath(dotnet,
                $"{package.Id.ToLowerInvariant()}.{package.Version.ToLowerInvariant()}.nupkg.sha512")).Trim());
            using var metadata = JsonDocument.Parse(File.ReadAllText(package.CachePath(dotnet, ".nupkg.metadata")));
            var source = metadata.RootElement.GetProperty("source").GetString()!;
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.IsFile)
                source = uri.LocalPath;
            Assert.Equal(Path.TrimEndingDirectorySeparator(dotnet.PackageDirectory),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(source)), PathComparer);
            provenance.Add(new { package.Id, package.Version, package.RepositoryCommit, package.ContentHash, source });
        }
        File.WriteAllText(Path.Combine(project, "package-provenance.json"), JsonSerializer.Serialize(provenance));
        File.Copy(Path.Combine(project, "obj", "project.assets.json"), Path.Combine(project, "project.assets.json"));
    }

    public static StringComparer PathComparer =>
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static void AssertFileHash(string file, string expected)
    {
        Assert.True(File.Exists(file), $"Missing native asset: {file}");
        using var stream = File.OpenRead(file);
        Assert.Equal(expected, Convert.ToHexString(SHA256.HashData(stream)));
    }
}
