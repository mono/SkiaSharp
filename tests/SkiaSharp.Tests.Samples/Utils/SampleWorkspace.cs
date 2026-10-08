using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Owns the sample copy, private build environment, and disposable workspace.
public sealed class SampleWorkspace : IDisposable
{
    public string Root { get; }
    public string DiagnosticsRoot { get; }

    public SampleWorkspace(string folder)
        : this(folder, Repo.SamplesDir, Repo.PackagesDir, Repo.WorkspacesDir, Repo.ArtifactsDir)
    {
    }

    internal SampleWorkspace(string folder, string samplesDir, string packagesDir, string workspacesDir, string artifactsDir)
    {
        packagesDir = Path.GetFullPath(packagesDir);
        if (!Directory.Exists(packagesDir) || !Directory.EnumerateFiles(packagesDir, "*.nupkg", SearchOption.AllDirectories).Any())
            throw new InvalidOperationException($"Input NuGet packages are missing: {packagesDir}");

        var id = $"samples-{Guid.NewGuid():N}";
        Root = Path.Combine(Path.GetFullPath(workspacesDir), id);
        DiagnosticsRoot = Path.Combine(Path.GetFullPath(artifactsDir), id);
        Directory.CreateDirectory(Root);
        try
        {
            // Block repository imports and SDK pins; let dotnet on PATH select the SDK.
            File.WriteAllText(Path.Combine(Root, "Directory.Build.props"), "<Project />");
            File.WriteAllText(Path.Combine(Root, "Directory.Build.targets"), "<Project />");
            File.WriteAllText(Path.Combine(Root, "global.json"), """{"sdk":{"paths":["$host$"],"allowPrerelease":true}}""");
            DotNet.WriteNuGetConfig(Path.Combine(Root, "NuGet.Config"), packagesDir);
            CopySample(samplesDir, folder, Path.Combine(Root, "samples"));
        }
        catch
        {
            Dispose();
            throw;
        }
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
        // Preserve ancestor build configuration without copying unrelated samples.
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
        if (Directory.Exists(Root))
            DeleteDirectory(Root);
    }

    internal static void DeleteDirectory(string path)
    {
        const int attempts = 20;
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (Exception error) when (
                OperatingSystem.IsWindows() &&
                attempt + 1 < attempts &&
                (error is IOException or UnauthorizedAccessException) &&
                (error.HResult & 0xffff) is 5 or 32 or 33)
            {
                // Terminated descendants can briefly retain DLL handles on Windows.
                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Retrying owned directory cleanup ({attempt + 2}/{attempts}): {path}\n{error.Message}");
                Thread.Sleep(250);
            }
        }
    }
}
