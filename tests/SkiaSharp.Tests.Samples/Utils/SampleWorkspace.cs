using System.Text.Json;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Owns the sample copy, private build environment, and disposable workspace.
public sealed class SampleWorkspace : IDisposable
{
    public string Root { get; }
    public string DiagnosticsRoot { get; }
    public string? SdkVersion { get; }

    internal static string? ConsumerSdkVersion =>
        Environment.GetEnvironmentVariable("SAMPLE_TEST_SDK_VERSION") is { Length: > 0 } version ? version : null;

    public SampleWorkspace(string folder)
        : this(folder, Repo.SamplesDir, Repo.PackagesDir, Repo.WorkspacesDir, Repo.ArtifactsDir)
    {
    }

    internal SampleWorkspace(string folder, string samplesDir, string packagesDir, string workspacesDir, string artifactsDir)
        : this(folder, samplesDir, packagesDir, workspacesDir, artifactsDir, ConsumerSdkVersion)
    {
    }

    internal SampleWorkspace(string folder, string samplesDir, string packagesDir, string workspacesDir, string artifactsDir, string? sdkVersion)
    {
        SdkVersion = sdkVersion;
        packagesDir = Path.GetFullPath(packagesDir);
        if (!Directory.Exists(packagesDir) || !Directory.EnumerateFiles(packagesDir, "*.nupkg", SearchOption.AllDirectories).Any())
            throw new InvalidOperationException($"Input NuGet packages are missing: {packagesDir}");

        var id = CreateIdentity(folder);
        Root = Path.Combine(Path.GetFullPath(workspacesDir), id);
        DiagnosticsRoot = Path.Combine(Path.GetFullPath(artifactsDir), id);

        Directory.CreateDirectory(Root);
        try
        {
            // Block repository imports; the consumer pin is independent of the runner SDK.
            File.WriteAllText(Path.Combine(Root, "Directory.Build.props"), "<Project />");
            File.WriteAllText(Path.Combine(Root, "Directory.Build.targets"), "<Project />");
            var sdk = new Dictionary<string, object> { ["paths"] = new[] { "$host$" }, ["allowPrerelease"] = true };
            if (SdkVersion is not null)
            {
                sdk["version"] = SdkVersion;
                sdk["rollForward"] = "disable";
            }
            File.WriteAllText(Path.Combine(Root, "global.json"), JsonSerializer.Serialize(new { sdk }));
            DotNet.WriteNuGetConfig(Path.Combine(Root, "NuGet.Config"), packagesDir);
            CopySample(samplesDir, folder, Path.Combine(Root, "samples"));
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal void RetargetProject(string relativeProject, string expectedFramework, string targetFramework)
    {
        var project = Path.GetFullPath(Path.Combine(Root, "samples", relativeProject));
        var relative = Path.GetRelativePath(Path.Combine(Root, "samples"), project);
        if (Path.IsPathRooted(relativeProject) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("The project must stay within the owned sample copy.", nameof(relativeProject));

        var document = XDocument.Load(project);
        var frameworks = document.Descendants().Where(e => e.Name.LocalName is "TargetFramework" or "TargetFrameworks").ToArray();
        if (frameworks.Length != 1 || frameworks[0].Name.LocalName != "TargetFramework" ||
            frameworks[0].Value != expectedFramework || frameworks[0].Attribute("Condition") is not null ||
            frameworks[0].Parent?.Attribute("Condition") is not null)
            throw new InvalidOperationException($"Expected one unconditional {expectedFramework} TargetFramework in {project}.");

        frameworks[0].Value = targetFramework;
        document.Save(project);
    }

    internal static string CreateIdentity(string folder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        var name = new string(folder.Select(c => char.IsAsciiLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-').ToArray());
        return $"{name}-{Guid.NewGuid():N}";
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
        => DeleteDirectory(path, directory => Directory.Delete(directory, recursive: true));

    internal static void DeleteDirectory(string path, Action<string> delete)
    {
        const int attempts = 20;
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                delete(path);
                return;
            }
            catch (IOException error) when (
                OperatingSystem.IsMacOS() &&
                attempt == 0 &&
                error.HResult == 66) // ENOTEMPTY on macOS.
            {
                // macOS metadata can appear after recursive deletion enumerates a directory.
                var directories = new Stack<DirectoryInfo>();
                var metadata = new List<string>();
                directories.Push(new DirectoryInfo(path));
                while (directories.TryPop(out var directory))
                {
                    foreach (var entry in directory.EnumerateFileSystemInfos())
                    {
                        if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                            throw;
                        if (entry is DirectoryInfo child)
                            directories.Push(child);
                        else if (entry.Name == ".DS_Store")
                            metadata.Add(entry.FullName);
                        else
                            throw;
                    }
                }
                if (metadata.Count == 0)
                    throw;

                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Removing macOS metadata before one owned directory cleanup retry: {path}\n" +
                    $"HResult=0x{error.HResult:X8}: {error.Message}\n{string.Join(Environment.NewLine, metadata)}");
                foreach (var file in metadata)
                    File.Delete(file);
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
