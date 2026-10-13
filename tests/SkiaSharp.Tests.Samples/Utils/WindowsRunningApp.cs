using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal enum WindowsAppKind { Packaged, Unpackaged }

internal sealed class WindowsRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, string winAppDriverPath,
    WindowsAppKind kind = WindowsAppKind.Packaged)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private PackageMetadata? package;
    private string? packageFullName;
    private bool ownsPackage;

    internal override PointerKind PointerKind => PointerKind.Mouse;
    internal override void ConfigureServer(ProcessStartInfo start) =>
        start.Environment["APPIUM_WAD_PATH"] = Path.GetFullPath(winAppDriverPath);
    internal override void SubmitSearch() =>
        Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
        {
            ["actions"] = new[] { new Dictionary<string, object> { ["virtualKeyCode"] = 0x0D } }
        });
    internal override void Drag(Point start, Point end) =>
        Driver.ExecuteScript("windows: clickAndDrag", new Dictionary<string, object>
        {
            ["startX"] = start.X, ["startY"] = start.Y, ["endX"] = end.X, ["endY"] = end.Y,
            ["durationMs"] = 500
        });

    protected override async Task Start(Uri address)
    {
        Assert.True(OperatingSystem.IsWindows(), "Windows tests require a Windows host.");
        AssertInteractiveDesktop();
        Assert.True(File.Exists(winAppDriverPath),
            "Provide the preinstalled WinAppDriver.exe path. Tests never download or install drivers.");
        var extension = Path.GetExtension(Artifact).ToLowerInvariant();
        string executable;
        string app;
        if (kind == WindowsAppKind.Packaged)
        {
            Assert.True(extension is ".msix" or ".appx" || Path.GetFileName(Artifact).Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase),
                "Packaged Windows apps require the actual package or final generated manifest, not an unpackaged .exe.");
            package = ReadPackage();
            Assert.True((await InstalledPackages()).Length == 0,
                $"Package {package.Name} is already registered; refusing to overwrite its installation/data.");
            var packageData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
            Assert.True(!Directory.Exists(packageData) || !Directory.EnumerateDirectories(packageData)
                .Any(path => Path.GetFileName(path).StartsWith(package.Name + "_", StringComparison.OrdinalIgnoreCase)),
                $"Existing package state for {package.Name}; remove it explicitly before selecting the test.");
            ownsPackage = true;
            Own(RemovePackage);
            // WinAppDriver launches an installed AUMID; it has no MSIX registration API.
            await PowerShell(Path.GetFileName(Artifact).Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase)
                ? $"Add-AppxPackage -Register {Quote(Artifact)} -DisableDevelopmentMode -ErrorAction Stop"
                : $"Add-AppxPackage -Path {Quote(Artifact)} -ErrorAction Stop", TimeSpan.FromMinutes(2));
            var current = (await InstalledPackages()) is [var installed] ? installed :
                throw new InvalidOperationException($"Expected one newly registered {package.Name} package.");
            Assert.True(current.GetProperty("Publisher").GetString() == package.Publisher &&
                current.GetProperty("Version").GetString() == package.Version,
                "Registered package identity/version differs from the exact built artifact.");
            packageFullName = current.GetProperty("PackageFullName").GetString()!;
            executable = Path.Combine(current.GetProperty("InstallLocation").GetString()!, package.Executable);
            app = current.GetProperty("PackageFamilyName").GetString()! + "!" + package.AppId;
        }
        else
        {
            Assert.True(kind == WindowsAppKind.Unpackaged && extension == ".exe",
                "An explicitly unpackaged Windows app requires its actual executable.");
            Assert.True(!File.Exists(Path.Combine(Path.GetDirectoryName(Artifact)!, "AppxManifest.xml")),
                "This executable is packaged output; pass its package or generated manifest.");
            executable = app = Artifact;
        }
        Assert.True(File.Exists(executable), $"Built application executable is missing: {executable}");
        using var processes = JsonDocument.Parse(await PowerShell(
            $"ConvertTo-Json -InputObject @(Get-Process | Where-Object {{ $_.Path -eq {Quote(executable)} }}) -Compress"));
        Assert.True(processes.RootElement.GetArrayLength() == 0,
            $"The target executable is already running; refusing to relaunch or terminate it: {executable}");
        var options = Options("Windows", "Windows");
        options.App = app;
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
        options.AddAdditionalAppiumOption("ms:forcequit", true);
        SetDriver(new WindowsDriver(address, options, TimeSpan.FromMinutes(2)));
        Wait(() => Driver.FindElement(By.XPath("/*")) is { Displayed: true } element ? element : null, $"{app} desktop window");
    }

    private async Task<JsonElement[]> InstalledPackages()
    {
        using var json = JsonDocument.Parse(await PowerShell($$$"""
            ConvertTo-Json -InputObject @(Get-AppxPackage | Where-Object { $_.Name -eq {{{Quote(package!.Name)}}} } |
                Select-Object Name, Publisher, PackageFullName, PackageFamilyName, InstallLocation,
                    @{Name='Version';Expression={$_.Version.ToString()}}) -Compress
            """));
        return json.RootElement.EnumerateArray().Select(item => item.Clone()).ToArray();
    }

    private async Task RemovePackage()
    {
        if (!ownsPackage)
            return;
        foreach (var current in await InstalledPackages())
        {
            var fullName = current.GetProperty("PackageFullName").GetString()!;
            Assert.True(current.GetProperty("Publisher").GetString() == package!.Publisher &&
                current.GetProperty("Version").GetString() == package.Version &&
                (packageFullName is null || fullName == packageFullName),
                $"Package identity changed; refusing to remove foreign {fullName}.");
            await PowerShell($"Remove-AppxPackage -Package {Quote(fullName)} -ErrorAction Stop", TimeSpan.FromMinutes(2));
        }
        Assert.True((await InstalledPackages()).Length == 0, $"Owned {package!.Name} package was not removed.");
    }

    private PackageMetadata ReadPackage()
    {
        XDocument manifest;
        if (Path.GetExtension(Artifact).Equals(".xml", StringComparison.OrdinalIgnoreCase))
            manifest = XDocument.Load(Artifact);
        else
        {
            using var archive = ZipFile.OpenRead(Artifact);
            using var entry = (archive.GetEntry("AppxManifest.xml")
                ?? throw new InvalidOperationException("Built package has no AppxManifest.xml. Pass a single MSIX/AppX, not a bundle.")).Open();
            manifest = XDocument.Load(entry);
        }
        XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
        var identity = manifest.Root?.Element(ns + "Identity") ?? throw new InvalidOperationException("Built package identity is missing.");
        var apps = manifest.Root!.Element(ns + "Applications")?.Elements(ns + "Application")
            .Where(item => (string?)item.Attribute("Id") == ApplicationId).ToArray() ?? [];
        Assert.True(apps.Length == 1, $"Declare the exact Application/@Id from the built manifest: {ApplicationId}");
        var metadata = new PackageMetadata(Required(identity, "Name"), Required(identity, "Publisher"),
            Required(identity, "Version"), ApplicationId, Required(apps[0], "Executable"));
        Assert.True(!metadata.Executable.Contains('$') && !Path.IsPathRooted(metadata.Executable) &&
            !metadata.Executable.Split('\\', '/').Contains(".."), "Pass a final generated manifest, not source with MSBuild tokens.");
        return metadata;

        static string Required(XElement node, string attribute) =>
            (string?)node.Attribute(attribute) is { Length: > 0 } value ? value :
                throw new InvalidOperationException($"Built manifest {node.Name.LocalName}/@{attribute} is missing.");
    }

    private Task<string> PowerShell(string script, TimeSpan? timeout = null) =>
        Command("powershell.exe", ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
            "$ErrorActionPreference = 'Stop'; [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false); " + script],
            timeout, configure: start =>
            {
                start.StandardOutputEncoding = Encoding.UTF8;
                start.StandardErrorEncoding = Encoding.UTF8;
            });
    private static string Quote(string value) => "'" + value.Replace("'", "''") + "'";

    private void AssertInteractiveDesktop()
    {
        Assert.True(Environment.UserInteractive && Process.GetCurrentProcess().SessionId != 0,
            "Windows UI tests require a logged-in interactive desktop, not service/session 0.");
        var desktop = OpenInputDesktop(0, false, 1);
        Assert.True(desktop != IntPtr.Zero, "No accessible input desktop. Unlock and dedicate the host.");
        try
        {
            var name = new StringBuilder(256);
            Assert.True(GetUserObjectInformation(desktop, 2, name, name.Capacity * 2, out _) &&
                name.ToString().Equals("Default", StringComparison.OrdinalIgnoreCase),
                "Windows UI tests require an unlocked Default input desktop.");
        }
        finally
        {
            if (!CloseDesktop(desktop))
                throw new InvalidOperationException("Could not release the input-desktop inspection handle.");
        }
    }

    private sealed record PackageMetadata(string Name, string Publisher, string Version, string AppId, string Executable);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(IntPtr desktop);
    [DllImport("user32.dll", EntryPoint = "GetUserObjectInformationW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetUserObjectInformation(IntPtr handle, int index, StringBuilder value, int length, out int needed);
}
