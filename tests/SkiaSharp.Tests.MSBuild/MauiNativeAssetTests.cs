using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using SkiaSharp.Tests.MSBuild.Utils;
using Xunit;

namespace SkiaSharp.Tests.MSBuild;

[Collection("Package consumers")]
public class MauiNativeAssetTests(DotNet dotnet)
{
    private static readonly string[] Families = ["SkiaSharp", "HarfBuzzSharp"];
    private static readonly string[] References =
        ["SkiaSharp", "HarfBuzzSharp", "SkiaSharp.HarfBuzz", "SkiaSharp.Views.Maui.Controls"];

    public static TheoryData<string, string, string> HostCases
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            var arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                var unsupported => throw new NotSupportedException($"Unsupported MAUI consumer host: {unsupported}")
            };
            foreach (var framework in new[] { "net10.0", "net11.0" })
            {
                data.Add(framework + "-android", $"android-{arch}", "Android");
                if (OperatingSystem.IsWindows())
                    data.Add(framework + "-windows10.0.19041.0", $"win-{arch}", "Win32");
                if (OperatingSystem.IsMacOS())
                {
                    data.Add(framework + "-ios", $"iossimulator-{arch}", "iOS");
                    data.Add(framework + "-maccatalyst", $"maccatalyst-{arch}", "MacCatalyst");
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(HostCases))]
    [Trait("Category", "Maui")]
    public async Task BuildMauiTemplateIncludesBothPackageFamilies(string framework, string rid, string platform)
    {
        var project = await CreateConsumer("build", framework, rid);
        await dotnet.Build(project, configuration: "Debug");
        await AssertConsumer(project, framework, rid, platform);
    }

    [Theory]
    [MemberData(nameof(HostCases))]
    [Trait("Category", "Maui")]
    public async Task PublishMauiTemplateIncludesBothPackageFamilies(string framework, string rid, string platform)
    {
        var project = await CreateConsumer("publish", framework, rid);
        await dotnet.Publish(project, configuration: "Debug");
        await AssertConsumer(project, framework, rid, platform);
    }

    private async Task<string> CreateConsumer(string name, string framework, string rid)
    {
        var project = await dotnet.NewTemplateProject($"maui-{framework}-{name}-{rid}", "maui", framework, xml =>
        {
            var properties = new XElement("PropertyGroup",
                new XElement("RuntimeIdentifier", rid));
            if (framework.Contains("-android", StringComparison.Ordinal))
                properties.Add(new XElement("AndroidPackageFormats", "apk"),
                    new XElement("EmbedAssembliesIntoApk", "true"),
                    new XElement("AndroidStripNativeLibraries", "false"));
            else if (framework.Contains("-windows", StringComparison.Ordinal))
                properties.Add(new XElement("WindowsPackageType", "None"));
            else
                properties.Add(new XElement("EnableCodeSigning", "false"),
                    new XElement("CodesignRequireProvisioningProfile", "false"));
            xml.Add(properties, new XElement("ItemGroup", References.Select(id =>
                new XElement("PackageReference", new XAttribute("Include", id),
                    new XAttribute("Version", $"[{dotnet.Packages.Get(id).Version}]")))),
                new XElement("Target", new XAttribute("Name", "RecordMauiPackageAssets"),
                    new XAttribute("AfterTargets", "ResolveReferences"),
                    new XElement("WriteLinesToFile", new XAttribute("File", "maui-package-assets.txt"),
                        new XAttribute("Overwrite", "true"),
                        new XAttribute("Lines", "TargetFramework=$(TargetFramework);RuntimeIdentifier=$(RuntimeIdentifier);" +
                            "UseMaui=$(UseMaui);IsExecutable=$(_IsExecutable)")),
                    new XElement("WriteLinesToFile", new XAttribute("File", "maui-package-assets.txt"),
                        new XAttribute("Lines", "@(ReferencePath->'%(FullPath)')"))));
        }, useFrameworkSdk: true);
        var startup = Path.Combine(project, "MauiProgram.cs");
        var original = File.ReadAllText(startup);
        Assert.Contains(".UseMauiApp<App>()", original, StringComparison.Ordinal);
        File.WriteAllText(startup, "using SkiaSharp.Views.Maui.Controls.Hosting;\n" +
            original.Replace(".UseMauiApp<App>()", ".UseMauiApp<App>().UseSkiaSharp()", StringComparison.Ordinal));
        File.WriteAllText(Path.Combine(project, "MainPage.xaml.cs"),
            """
            using SkiaSharp;
            using SkiaSharp.HarfBuzz;
            namespace Consumer;
            public partial class MainPage : ContentPage
            {
                public MainPage() => InitializeComponent();
                private void OnCounterClicked(object? sender, EventArgs e)
                {
                    using var bitmap = new SKBitmap(1, 1);
                    bitmap.SetPixel(0, 0, SKColors.Red);
                    using var buffer = new HarfBuzzSharp.Buffer();
                    buffer.AddUtf8("MAUI");
                    buffer.GuessSegmentProperties();
                    using var shaper = new SKShaper(SKTypeface.Default);
                    CounterBtn.Text = $"{bitmap.GetPixel(0, 0)}: {buffer.Length}";
                    SemanticScreenReader.Announce(CounterBtn.Text);
                }
            }
            """);
        return project;
    }

    private async Task AssertConsumer(string project, string framework, string rid, string platform)
    {
        var natives = Families.Select(family => dotnet.Packages.Get(family + ".NativeAssets." + platform)).ToArray();
        ArtifactPackage.AssertRestore(dotnet, project, framework, References.Select(dotnet.Packages.Get).Concat(natives));
        var evidence = File.ReadAllLines(Path.Combine(project, "maui-package-assets.txt"));
        Assert.Contains("TargetFramework=" + framework, evidence);
        Assert.Contains("RuntimeIdentifier=" + rid, evidence);
        Assert.Contains("UseMaui=true", evidence, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("IsExecutable=true", evidence, StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in References.Append("SkiaSharp.Views.Maui.Core"))
            Assert.Contains(evidence, line => Path.GetFileName(line) == assembly + ".dll");

        if (platform == "Android")
            AssertAndroidApplication(project, rid, natives);
        else if (platform == "Win32")
            AssertWindowsApplication(project, rid, natives);
        else
            await AssertAppleApplication(project, platform, natives);
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
                var path = $"runtimes/{rid}/native/lib{family}.so";
                var hash = package.Files[path];
                ArtifactPackage.AssertFileHash(package.CachePath(dotnet, path), hash);
                var entry = Assert.Single(zip.Entries, entry => entry.FullName == $"lib/{abi}/lib{family}.so");
                using var stream = entry.Open();
                Assert.Equal(hash, Convert.ToHexString(SHA256.HashData(stream)));
                Assert.Single(zip.Entries, entry => entry.FullName.EndsWith($"/lib{family}.so", StringComparison.Ordinal));
            }
        }
    }

    private void AssertWindowsApplication(string project, string rid, ArtifactPackage[] natives)
    {
        var output = Path.Combine(project, "output");
        var exe = Assert.Single(Directory.GetFiles(output, "Consumer.exe", SearchOption.AllDirectories));
        AssertPe(exe);
        foreach (var package in natives.Append(dotnet.Packages.Get("SkiaSharp.NativeAssets.WinUI")))
        {
            var selected = package.Files.Where(entry => entry.Key.StartsWith($"runtimes/{rid}/native/",
                StringComparison.Ordinal)).ToArray();
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
        var app = Assert.Single(Directory.GetDirectories(Path.Combine(project, "output"), "Consumer.app",
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
            var bundle = Assert.Single(Directory.GetDirectories(app, $"lib{family}.framework", SearchOption.AllDirectories));
            var binary = Path.Combine(bundle, "lib" + family);
            AssertMachO(binary);
            await dotnet.InspectAppleBinary(project, binary);
            Assert.Contains($"lib{family}.framework",
                File.ReadAllText(Path.Combine(project, "Consumer-link", "stdout.txt")), StringComparison.Ordinal);
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
        using var stream = File.OpenRead(file);
        var header = new byte[4];
        Assert.Equal(4, stream.Read(header));
        Assert.Contains(Convert.ToHexString(header),
            new[] { "CFFAEDFE", "FEEDFACF", "CEFAEDFE", "FEEDFACE", "CAFEBABE", "BEBAFECA", "CAFEBABF", "BFBAFECA" });
    }
}
