namespace SkiaSharp.Tests.Samples.Utils;

// Finds generated samples that can be built on the current host.
public static class SampleLookup
{
    [Flags]
    public enum EntryKind
    {
        None = 0,
        Basic = 1,
        Gallery = 2,
        Docker = 4,
        Host = 8,
        Device = 16,
        Http = 32
    }

    // Identifies a generated solution or Dockerfile relative to the samples root.
    public sealed record Entry(string Folder, string FileName, EntryKind Kind)
    {
        public string RelativePath => Path.Combine(Folder, FileName);
    }

    private static readonly string[] PlatformSuffixes = [".Windows", ".Mac", ".Linux"];

    private static readonly Dictionary<string, EntryKind> Targets = new()
    {
        [Path.Combine("Basic", "Console")] = EntryKind.Host,
        [Path.Combine("Basic", "Web")] = EntryKind.Host | EntryKind.Http,
        [Path.Combine("Basic", "DockerWebApi")] = EntryKind.Http,
        [Path.Combine("Basic", "Android")] = EntryKind.Device,
        [Path.Combine("Basic", "iOS")] = EntryKind.Device
    };

    public static string HostPlatform =>
        OperatingSystem.IsWindows()
            ? "Windows"
            : OperatingSystem.IsMacOS()
                ? "Mac"
                : "Linux";

    public static IReadOnlyList<Entry> Discover(string root, string platform, string filter = "")
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
        var entries = new List<Entry>();

        foreach (var path in solutions)
        {
            var relative = Path.GetRelativePath(root, path);
            if (!relative.Contains(filter, StringComparison.Ordinal))
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
            entries.Add(new(folder, Path.GetFileName(path), GetKind(folder, false)));
        }

        var hostDockerfile = platform == "Windows"
            ? "windows.Dockerfile"
            : "linux.Dockerfile";
        foreach (var group in dockerfiles.GroupBy(Path.GetDirectoryName))
        {
            var path =
                group.FirstOrDefault(file => Path.GetFileName(file) == hostDockerfile) ??
                group.FirstOrDefault(file => Path.GetFileName(file) == "Dockerfile");
            if (path is null)
                continue;

            var relative = Path.GetRelativePath(root, path);
            if (relative.Contains(filter, StringComparison.Ordinal))
            {
                var folder = Path.GetDirectoryName(relative)!;
                entries.Add(new(folder, Path.GetFileName(path), GetKind(folder, true)));
            }
        }

        if (entries.Count == 0)
            throw new InvalidOperationException($"No eligible samples match '{filter}' for {platform} in {root}");

        return entries.OrderBy(entry => entry.RelativePath, StringComparer.Ordinal).ToArray();
    }

    private static EntryKind GetKind(string folder, bool docker)
    {
        var kind = folder == "Gallery" || folder.StartsWith("Gallery" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? EntryKind.Gallery : EntryKind.Basic;
        kind |= docker ? EntryKind.Docker : Targets.GetValueOrDefault(folder);
        if (docker)
            kind |= Targets.GetValueOrDefault(folder) & EntryKind.Http;
        return kind;
    }

}
