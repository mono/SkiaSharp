namespace SkiaSharp.Tests.Samples.Utils;

public sealed class SampleWorkspace : IDisposable
{
    private DotNet? profile;

    public DotNet Profile()
    {
        if (profile is not null)
            return profile;
        var created = DotNet.ForSamples();
        try
        {
            CopyTree(DotNet.Setting("SamplesDirectory"), Path.Combine(created.Root, "samples"));
        }
        catch
        {
            created.Dispose();
            throw;
        }
        profile = created;
        return created;
    }

    public static void CopyTree(string source, string destination)
    {
        if (!Directory.Exists(source))
            throw new DirectoryNotFoundException($"Sample inputs are missing: {source}");
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            if (relative.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj" or "AppPackages" or ".vs"))
                continue;
            if (Path.GetFileName(file).Equals("global.json", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Sample-local global.json would override the SDK pin: {file}");
            var target = Path.Combine(destination, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }

    public static void CleanProducts(string samples)
    {
        foreach (var directory in Directory.EnumerateDirectories(samples, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path) is "bin" or "obj" or "AppPackages" or ".vs")
            .OrderByDescending(path => path.Length).ToArray())
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
    }

    public void Dispose()
    {
        if (profile is null)
            return;
        var samples = Path.Combine(profile.Root, "samples");
        if (Directory.Exists(samples))
            Directory.Delete(samples, recursive: true);
        profile.Dispose();
    }
}
