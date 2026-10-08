namespace SkiaSharp.Tests.Samples.Utils;

// Distinguishes ordinary builds, shared Gallery builds, and Docker checks.
public enum SampleKind { Sample, Gallery, Docker }

// Identifies a generated solution or Dockerfile relative to the samples root.
public sealed record SampleEntry(string Folder, string FileName, SampleKind Kind)
{
    public string RelativePath => Path.Combine(Folder, FileName);
}

// Finds generated samples that can be built on the current host.
public static class SampleLookup
{
    private static readonly string[] PlatformSuffixes = [".Windows", ".Mac", ".Linux"];

    public static string HostPlatform =>
        OperatingSystem.IsWindows()
            ? "Windows"
            : OperatingSystem.IsMacOS()
                ? "Mac"
                : "Linux";

    public static IReadOnlyList<SampleEntry> Discover(string root, string platform, string filter = "")
    {
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Generated samples are missing: {root}. Run samples-generate first.");

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            MaxRecursionDepth = 2,
            IgnoreInaccessible = false,
            AttributesToSkip = 0
        };
        var solutions = Directory.EnumerateFiles(root, "*.slnx", options).ToArray();
        var dockerfiles = Directory.EnumerateFiles(root, "*Dockerfile", options)
            .Where(path => Path.GetFileName(path) is "Dockerfile" or "linux.Dockerfile" or "windows.Dockerfile")
            .ToArray();
        var dockerFolders = dockerfiles.Select(Path.GetDirectoryName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var entries = new List<SampleEntry>();

        foreach (var path in solutions)
        {
            var relative = Path.GetRelativePath(root, path);
            if (!relative.Contains(filter, StringComparison.Ordinal) || dockerFolders.Contains(Path.GetDirectoryName(path)))
                continue;

            var name = Path.GetFileNameWithoutExtension(path);
            var suffix = Path.GetExtension(name);
            var isPlatformVariant = PlatformSuffixes.Any(value => suffix.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (isPlatformVariant && !suffix.Equals("." + platform, StringComparison.OrdinalIgnoreCase))
                continue;

            // Host-specific solutions replace the common solution in the same folder.
            if (!isPlatformVariant && solutions.Any(other =>
                Path.GetDirectoryName(other) == Path.GetDirectoryName(path) &&
                PlatformSuffixes.Any(value => Path.GetFileNameWithoutExtension(other).Equals(name + value, StringComparison.OrdinalIgnoreCase))))
                continue;

            var folder = Path.GetDirectoryName(relative)!;
            var kind = folder == "Gallery" || folder.StartsWith("Gallery" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                ? SampleKind.Gallery : SampleKind.Sample;
            entries.Add(new(folder, Path.GetFileName(path), kind));
        }

        var hostDockerfile = platform == "Windows" ? "windows.Dockerfile" : "linux.Dockerfile";
        foreach (var group in dockerfiles.GroupBy(Path.GetDirectoryName))
        {
            var path = group.FirstOrDefault(file => Path.GetFileName(file) == hostDockerfile) ??
                group.FirstOrDefault(file => Path.GetFileName(file) == "Dockerfile");
            if (path is null)
                continue;
            var relative = Path.GetRelativePath(root, path);
            if (relative.Contains(filter, StringComparison.Ordinal))
                entries.Add(new(Path.GetDirectoryName(relative)!, Path.GetFileName(path), SampleKind.Docker));
        }

        if (entries.Count == 0)
            throw new InvalidOperationException($"No eligible samples match '{filter}' for {platform} in {root}");

        return entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToArray();
    }
}
