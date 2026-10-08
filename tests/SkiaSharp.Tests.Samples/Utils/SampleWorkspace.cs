namespace SkiaSharp.Tests.Samples.Utils;

public sealed class SampleWorkspace(string folder) : IDisposable
{
    private DotNet? profile;

    public DotNet Profile()
    {
        if (profile is not null)
            return profile;
        var created = DotNet.ForSamples();
        try
        {
            CopySample(DotNet.Setting("SamplesDirectory"), folder, Path.Combine(created.Root, "samples"));
        }
        catch
        {
            created.Dispose();
            throw;
        }
        profile = created;
        return created;
    }

    internal static void CopySample(string source, string folder, string destination)
    {
        if (string.IsNullOrWhiteSpace(folder) || Path.IsPathRooted(folder))
            throw new ArgumentException("A sample folder must be a nonempty relative path.", nameof(folder));
        source = Path.GetFullPath(source);
        var selected = Path.GetFullPath(Path.Combine(source, folder));
        var relative = Path.GetRelativePath(source, selected);
        if (relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("The sample folder must stay within the generated inputs.", nameof(folder));

        CopyTree(selected, Path.Combine(destination, relative));
        for (var parent = Directory.GetParent(selected); parent is not null; parent = parent.Parent)
        {
            var parentRelative = Path.GetRelativePath(source, parent.FullName);
            if (parentRelative == ".." || parentRelative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                break;
            var target = Path.Combine(destination, parentRelative);
            foreach (var file in Directory.EnumerateFiles(parent.FullName))
            {
                var name = Path.GetFileName(file);
                if (name.Equals("global.json", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Sample-local global.json would override the SDK pin: {file}");
                if (name is not ("Directory.Build.props" or "Directory.Build.targets" or "Directory.Packages.props") &&
                    !name.Equals("nuget.config", StringComparison.OrdinalIgnoreCase))
                    continue;
                Directory.CreateDirectory(target);
                File.Copy(file, Path.Combine(target, name));
            }
            if (parentRelative == ".")
                break;
        }
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

    public void Dispose()
    {
        profile?.Dispose();
    }
}
