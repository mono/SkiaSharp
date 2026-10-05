using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

public class MauiNativeAssetTests(DotNet dotnet) : IClassFixture<DotNet>
{
    private const string ApplicationTitle = "Package Consumer";
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] References =
        ["SkiaSharp", "HarfBuzzSharp", "SkiaSharp.HarfBuzz", "SkiaSharp.Views.Maui.Controls"];

    public static TheoryData<string, string, string> HostCases
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            var framework = DotNet.ConsumerTargetFramework;
            var arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                var unsupported => throw new NotSupportedException($"Unsupported MAUI consumer host: {unsupported}")
            };
            data.Add(framework + "-android", $"android-{arch}", "Android");
            if (OperatingSystem.IsWindows())
                data.Add(framework + "-windows10.0.19041.0", $"win-{arch}", "Win32");
            if (OperatingSystem.IsMacOS())
            {
                data.Add(framework + "-ios", $"iossimulator-{arch}", "iOS");
                data.Add(framework + "-maccatalyst", $"maccatalyst-{arch}", "MacCatalyst");
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(HostCases))]
    [Trait("Category", "Maui")]
    public async Task ApplicationBuildIncludesBothRealNativeFamilies(string framework, string rid, string platform)
    {
        var packages = References.ToDictionary(id => id, id => ArtifactPackage.Read(dotnet.PackageDirectory, id));
        var natives = Families.Select(family => ArtifactPackage.Read(dotnet.PackageDirectory,
            family + ".NativeAssets." + platform)).ToArray();
        foreach (var native in natives)
            Assert.Equal(packages[native.Id.Split('.')[0]].Version, native.Version);

        var project = await dotnet.NewMauiProject($"maui-{rid}", ProjectXml(framework, rid, packages), App, MauiProgram);
        // Debug creates real apps without release signing or AOT; retain diagnostics before the runner's hang detector.
        await dotnet.Build(project, configuration: "Debug", timeoutMinutes: 10);
        AssertRestore(project, rid, packages.Values.Concat(natives));
        AssertManagedReferences(project);

        if (platform == "Android")
            AssertAndroidApplication(project, rid, natives);
        else if (platform == "Win32")
            AssertWindowsApplication(project, rid, natives);
        else
            await AssertAppleApplication(project, platform, natives);
    }

    private void AssertRestore(string project, string rid, IEnumerable<ArtifactPackage> required)
    {
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(project, "obj", "project.assets.json")));
        var libraries = assets.RootElement.GetProperty("libraries");
        var target = Assert.Single(assets.RootElement.GetProperty("targets").EnumerateObject(),
            target => target.Name.EndsWith("/" + rid, StringComparison.Ordinal)).Value;
        foreach (var package in required)
            Assert.True(target.TryGetProperty(package.Id + "/" + package.Version, out _),
                $"Missing {package.Id}/{package.Version} in the {rid} restore target");
        foreach (var library in libraries.EnumerateObject().Where(library =>
            library.Name.StartsWith("SkiaSharp", StringComparison.Ordinal) ||
            library.Name.StartsWith("HarfBuzzSharp", StringComparison.Ordinal)))
        {
            var identity = library.Name.Split('/');
            var package = ArtifactPackage.Read(dotnet.PackageDirectory, identity[0]);
            Assert.Equal(package.Version, identity[1]);
            Assert.Equal(package.ContentHash, library.Value.GetProperty("sha512").GetString());
            package.AssertRestored(dotnet);
        }
        var restoreSources = assets.RootElement.GetProperty("project").GetProperty("restore").GetProperty("sources")
            .EnumerateObject().Select(source => source.Name).ToArray();
        var libraryPacks = File.ReadAllLines(Path.Combine(project, "maui-package-assets.txt"))
            .Single(line => line.StartsWith("LibraryPacksDirectory=", StringComparison.Ordinal))
            ["LibraryPacksDirectory=".Length..];
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        Assert.InRange(restoreSources.Length, 2, 3);
        Assert.All(restoreSources, source => Assert.Contains(source,
            new[] { dotnet.PackageDirectory, libraryPacks,
                "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" }, comparer));
        Assert.Contains(dotnet.PackageDirectory, restoreSources, comparer);
        Assert.Contains("https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json",
            restoreSources);
    }

    private static void AssertManagedReferences(string project)
    {
        var evidence = File.ReadAllLines(Path.Combine(project, "maui-package-assets.txt"));
        Assert.Contains("UseMaui=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("IsExecutable=true", evidence, StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in new[] { "SkiaSharp", "HarfBuzzSharp", "SkiaSharp.HarfBuzz",
            "SkiaSharp.Views.Maui.Controls", "SkiaSharp.Views.Maui.Core" })
            Assert.Contains(evidence, line => line.EndsWith("/" + assembly + ".dll", StringComparison.OrdinalIgnoreCase) ||
                line.EndsWith("\\" + assembly + ".dll", StringComparison.OrdinalIgnoreCase));
    }

    private void AssertAndroidApplication(string project, string rid, ArtifactPackage[] natives)
    {
        var abi = rid == "android-arm64" ? "arm64-v8a" : "x86_64";
        var apks = Directory.GetFiles(Path.Combine(project, "output"), "*.apk", SearchOption.AllDirectories);
        Assert.NotEmpty(apks);
        foreach (var apk in apks)
        {
            using var zip = ZipFile.OpenRead(apk);
            Assert.Contains(zip.Entries, entry => entry.FullName == "AndroidManifest.xml" && entry.Length > 0);
            Assert.Contains(zip.Entries, entry => entry.FullName == "classes.dex" && entry.Length > 0);
            foreach (var package in natives)
            {
                var family = package.Id.Split('.')[0];
                var native = Assert.Single(package.NativeFiles,
                    entry => entry.Key == $"runtimes/{rid}/native/lib{family}.so");
                ArtifactPackage.AssertFileHash(package.CachePath(dotnet, native.Key), native.Value);
                var entry = Assert.Single(zip.Entries, entry => entry.FullName == $"lib/{abi}/lib{family}.so");
                using var stream = entry.Open();
                Assert.Equal(native.Value, Convert.ToHexString(SHA256.HashData(stream)));
                Assert.Single(zip.Entries, entry => entry.FullName.EndsWith($"/lib{family}.so", StringComparison.Ordinal));
            }
        }
    }

    private void AssertWindowsApplication(string project, string rid, ArtifactPackage[] natives)
    {
        var output = Path.Combine(project, "output");
        var exe = Assert.Single(Directory.GetFiles(output, "Consumer.exe", SearchOption.AllDirectories));
        AssertPe(exe);
        var winui = ArtifactPackage.Read(dotnet.PackageDirectory, "SkiaSharp.NativeAssets.WinUI");
        winui.AssertRestored(dotnet);
        foreach (var package in natives.Append(winui))
        {
            var selected = package.NativeFiles.Where(entry =>
                entry.Key.StartsWith($"runtimes/{rid}/native/", StringComparison.Ordinal)).ToArray();
            Assert.NotEmpty(selected);
            foreach (var (path, hash) in selected)
            {
                ArtifactPackage.AssertFileHash(package.CachePath(dotnet, path), hash);
                var binary = Assert.Single(Directory.GetFiles(output, Path.GetFileName(path), SearchOption.AllDirectories));
                ArtifactPackage.AssertFileHash(binary, hash);
                AssertPe(binary);
            }
        }
    }

    private async Task AssertAppleApplication(string project, string platform, ArtifactPackage[] natives)
    {
        // Mac Catalyst names the bundle after ApplicationTitle, not the managed assembly.
        var appName = platform == "iOS" ? "Consumer" : ApplicationTitle;
        var app = Assert.Single(Directory.GetDirectories(Path.Combine(project, "output"), appName + ".app",
            SearchOption.AllDirectories));
        var executable = platform == "iOS" ? Path.Combine(app, "Consumer") : Path.Combine(app, "Contents", "MacOS", "Consumer");
        AssertMachO(executable);
        await dotnet.InspectAppleBinary(project, executable);
        foreach (var package in natives)
        {
            var family = package.Id.Split('.')[0];
            var prefix = $"runtimes/{(platform == "iOS" ? "iossimulator" : "maccatalyst")}/native/";
            var selected = package.Files.Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            Assert.NotEmpty(selected);
            foreach (var (path, hash) in selected)
                ArtifactPackage.AssertFileHash(package.CachePath(dotnet, path), hash);
            var framework = Assert.Single(Directory.GetDirectories(app, $"lib{family}.framework", SearchOption.AllDirectories));
            var binary = Path.Combine(framework, $"lib{family}");
            AssertMachO(binary);
            await dotnet.InspectAppleBinary(project, binary);
            Assert.Contains($"lib{family}.framework", File.ReadAllText(Path.Combine(project, $"lib{family}-link-stdout.txt")),
                StringComparison.Ordinal);
        }
    }

    private static void AssertPe(string file)
    {
        using var stream = File.OpenRead(file);
        Assert.Equal('M', stream.ReadByte());
        Assert.Equal('Z', stream.ReadByte());
    }

    private static void AssertMachO(string file)
    {
        Assert.True(File.Exists(file), $"Missing native Apple application binary: {file}");
        using var stream = File.OpenRead(file);
        var header = new byte[4];
        Assert.Equal(4, stream.Read(header));
        Assert.Contains(Convert.ToHexString(header),
            new[] { "CFFAEDFE", "FEEDFACF", "CEFAEDFE", "FEEDFACE", "CAFEBABE", "BEBAFECA", "CAFEBABF", "BFBAFECA" });
    }

    private static string ProjectXml(string framework, string rid, Dictionary<string, ArtifactPackage> packages)
    {
        var properties = new XElement("PropertyGroup",
            new XElement("TargetFramework", framework),
            new XElement("RuntimeIdentifier", rid),
            new XElement("OutputType", "Exe"),
            new XElement("RootNamespace", "Consumer"),
            new XElement("UseMaui", "true"),
            new XElement("SingleProject", "true"),
            new XElement("ImplicitUsings", "enable"),
            new XElement("Nullable", "enable"),
            new XElement("ApplicationTitle", ApplicationTitle),
            new XElement("ApplicationId", "com.skiasharp.packageconsumer"),
            new XElement("ApplicationDisplayVersion", "1.0"),
            new XElement("ApplicationVersion", "1"));
        if (framework.Contains("-android", StringComparison.Ordinal))
            properties.Add(new XElement("SupportedOSPlatformVersion", DotNet.ConsumerSdkMajor == 11 ? "24.0" : "21.0"),
                new XElement("AndroidPackageFormats", "apk"),
                new XElement("EmbedAssembliesIntoApk", "true"),
                new XElement("AndroidStripNativeLibraries", "false"));
        else if (framework.Contains("-windows", StringComparison.Ordinal))
            properties.Add(new XElement("SupportedOSPlatformVersion", "10.0.17763.0"),
                new XElement("TargetPlatformMinVersion", "10.0.17763.0"),
                new XElement("WindowsPackageType", "None"));
        else
            properties.Add(new XElement("SupportedOSPlatformVersion", "15.0"),
                new XElement("EnableCodeSigning", "false"),
                new XElement("CodesignRequireProvisioningProfile", "false"));
        var snapshot = new XElement("Target", new XAttribute("Name", "RecordMauiPackageAssets"),
            new XAttribute("AfterTargets", "ResolveReferences"),
            new XElement("WriteLinesToFile", new XAttribute("File", "maui-package-assets.txt"),
                new XAttribute("Lines", "UseMaui=$(UseMaui);OutputType=$(OutputType);IsExecutable=$(_IsExecutable);LibraryPacksDirectory=$(_WorkloadLibraryPacksFolder)"),
                new XAttribute("Overwrite", "true")),
            new XElement("WriteLinesToFile", new XAttribute("File", "maui-package-assets.txt"),
                new XAttribute("Lines", "@(ReferencePath->'%(FullPath)')")));
        return new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"), properties,
            new XElement("ItemGroup",
                new XElement("MauiIcon", new XAttribute("Include", "Resources\\AppIcon\\appicon.svg"),
                    new XAttribute("ForegroundFile", "Resources\\AppIcon\\appiconfg.svg"), new XAttribute("Color", "#512BD4")),
                new XElement("MauiSplashScreen", new XAttribute("Include", "Resources\\Splash\\splash.svg"),
                    new XAttribute("Color", "#512BD4"), new XAttribute("BaseSize", "128,128"))),
            new XElement("ItemGroup",
                new XElement("PackageReference", new XAttribute("Include", "Microsoft.Maui.Controls"),
                    new XAttribute("Version", "$(MauiVersion)")),
                packages.Select(package => new XElement("PackageReference", new XAttribute("Include", package.Key),
                    new XAttribute("Version", $"[{package.Value.Version}]")))), snapshot).ToString();
    }

    private const string MauiProgram = """
        using Microsoft.Maui.Hosting;
        using SkiaSharp.Views.Maui.Controls.Hosting;

        namespace Consumer;

        public static class MauiProgram
        {
            public static MauiApp CreateMauiApp() =>
                MauiApp.CreateBuilder().UseMauiApp<App>().UseSkiaSharp().Build();
        }
        """;

    private const string App = """
        using Microsoft.Maui;
        using Microsoft.Maui.Controls;
        using SkiaSharp;
        using SkiaSharp.HarfBuzz;
        using SkiaSharp.Views.Maui.Controls;

        namespace Consumer;

        public class App : Application
        {
            protected override Window CreateWindow(IActivationState? activationState)
            {
                var canvas = new SKCanvasView();
                canvas.PaintSurface += (_, args) =>
                {
                    using var buffer = new HarfBuzzSharp.Buffer();
                    buffer.AddUtf8("MAUI");
                    buffer.GuessSegmentProperties();
                    using var shaper = new SKShaper(SKTypeface.Default);
                    using var font = new SKFont(SKTypeface.Default, 20);
                    using var paint = new SKPaint { Color = SKColors.Red };
                    args.Surface.Canvas.Clear(SKColors.White);
                    args.Surface.Canvas.DrawShapedText(shaper, "MAUI", 10, 30, SKTextAlign.Left, font, paint);
                };
                return new Window(new ContentPage { Content = canvas });
            }
        }
        """;
}
