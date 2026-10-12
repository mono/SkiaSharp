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

namespace SkiaSharp.Tests.Samples.Utils;

internal enum WindowsAppKind { Packaged, Unpackaged }

internal sealed class WindowsRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, string winAppDriverPath,
    WindowsAppKind kind = WindowsAppKind.Packaged)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private PackageMetadata? package;
    private string? packageFullName;
    private string? executable;
    private bool ownsLaunch;
    private bool ownsPackage;

    internal override PointerKind PointerKind => PointerKind.Mouse;
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
        Require(OperatingSystem.IsWindows(), "Selected Windows tests require a Windows host.");
        RequireInteractiveDesktop();
        Require(File.Exists(winAppDriverPath),
            "Pass the explicitly installed WinAppDriver.exe path. Appium's Windows driver, Developer Mode, dependencies, and signing trust must be provisioned beforehand.");
        var extension = Path.GetExtension(Artifact).ToLowerInvariant();
        string app;
        if (kind == WindowsAppKind.Packaged)
        {
            Require(extension is ".msix" or ".appx" || Path.GetFileName(Artifact).Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase),
                "Packaged Windows apps require the actual MSIX/AppX or final generated AppxManifest.xml, not their .exe.");
            package = ReadPackage();
            Require((await InstalledPackages()).Length == 0,
                $"Package {package.Name} is already registered for this user; refusing to overwrite its installation/data.");
            var packageData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
            Require(!Directory.Exists(packageData) || !Directory.EnumerateDirectories(packageData)
                .Any(path => Path.GetFileName(path).StartsWith(package.Name + "_", StringComparison.OrdinalIgnoreCase)),
                $"Existing package state for {package.Name} under {packageData}; remove it explicitly before selecting the test.");
            ownsPackage = true;
            Own(RemovePackage);
            await PowerShell(Path.GetFileName(Artifact).Equals("AppxManifest.xml", StringComparison.OrdinalIgnoreCase)
                ? $"Add-AppxPackage -Register {Quote(Artifact)} -DisableDevelopmentMode -ErrorAction Stop"
                : $"Add-AppxPackage -Path {Quote(Artifact)} -ErrorAction Stop", TimeSpan.FromMinutes(2));
            var installed = await InstalledPackages();
            Require(installed.Length == 1, $"Expected one newly installed {package.Name} package, found {installed.Length}.");
            var current = installed[0];
            Require(current.GetProperty("Publisher").GetString() == package.Publisher &&
                current.GetProperty("Version").GetString() == package.Version,
                "Installed package identity/version does not match the exact built artifact.");
            packageFullName = current.GetProperty("PackageFullName").GetString()!;
            executable = Path.Combine(current.GetProperty("InstallLocation").GetString()!, package.Executable);
            app = current.GetProperty("PackageFamilyName").GetString()! + "!" + package.AppId;
        }
        else
        {
            Require(kind == WindowsAppKind.Unpackaged && extension == ".exe",
                "Explicitly unpackaged Windows apps require the actual native .exe.");
            Require(!File.Exists(Path.Combine(Path.GetDirectoryName(Artifact)!, "AppxManifest.xml")),
                "This executable belongs to packaged output. Pass its actual MSIX/AppX or generated AppxManifest.xml; do not run it as an unpackaged app.");
            executable = Artifact;
            app = Artifact;
        }
        Require(File.Exists(executable), $"The built application's actual executable is missing: {executable}");
        Require((await ApplicationProcesses()).Length == 0,
            $"The selected executable is already running: {executable}. Refusing foreign app/process reuse.");

        // wadUrl prevents driver-side installation, port fallback, or foreign server reuse.
        AppiumFixture.RequireFreePort(AppiumFixture.WindowsPort);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var wad = ProcessRunner.Start(winAppDriverPath,
            ["127.0.0.1", AppiumFixture.WindowsPort.ToString(), "/forcequit"], Workspace.Root,
            start =>
            {
                start.StandardOutputEncoding = Encoding.Unicode;
                start.StandardErrorEncoding = Encoding.Unicode;
            });
        var stdout = ReadServerOutput(wad.StandardOutput, ready);
        var stderr = ReadServerOutput(wad.StandardError, ready);
        Own(() => StopProcess(wad, stdout, stderr, "winappdriver"));
        await WaitForServer(wad, ready.Task);
        var options = Options("Windows", "Windows");
        options.App = app;
        options.AddAdditionalAppiumOption("wadUrl", $"http://127.0.0.1:{AppiumFixture.WindowsPort}");
        options.AddAdditionalAppiumOption("createSessionTimeout", 60000);
        ownsLaunch = true;
        Own(() => CloseNativeSessions(wad, app));
        SetDriver(new WindowsDriver(address, options, TimeSpan.FromMinutes(2)));
        Wait(() => Driver.FindElement(By.XPath("/*")) is { Displayed: true } element ? element : null,
            $"{app} desktop window");
    }

    protected override async Task StopApplication()
    {
        if (!ownsLaunch || executable is null)
            return;
        List<Exception> failures = [];
        foreach (var pid in await ApplicationProcesses())
        {
            // A PID must still belong to the exact newly-owned executable before it can be stopped.
            await Attempt(failures, async () =>
            {
                Process process;
                try { process = Process.GetProcessById(pid); }
                catch (ArgumentException) { return; } // The enumerated process has already exited.
                using (process)
                {
                    if (process.HasExited)
                        return;
                    Require(string.Equals(process.MainModule?.FileName, executable, StringComparison.OrdinalIgnoreCase),
                        $"Owned application PID {pid} changed identity; refusing to terminate it.");
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
                }
            });
        }
        await Attempt(failures, async () =>
            Require((await ApplicationProcesses()).Length == 0, $"Owned executable did not stop: {executable}"));
        ThrowFailures(failures);
    }

    private async Task CloseNativeSessions(Process server, string app)
    {
        // The Windows driver's wadUrl mode does not own/stop WAD. Its native sessions
        // (including partially created ones) belong to this independently owned listener.
        Require(!server.HasExited, "Owned WinAppDriver exited before native-session cleanup.");
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(15) };
        var address = new Uri($"http://127.0.0.1:{AppiumFixture.WindowsPort}");
        using var response = await client.GetAsync(new Uri(address, "/sessions"));
        response.EnsureSuccessStatusCode();
        using var sessions = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Require(sessions.RootElement.GetProperty("status").GetInt32() == 0, "WinAppDriver could not list owned native sessions.");
        List<Exception> failures = [];
        foreach (var session in sessions.RootElement.GetProperty("value").EnumerateArray())
        {
            await Attempt(failures, async () =>
            {
                Require(!server.HasExited && session.GetProperty("capabilities").GetProperty("app").GetString() == app,
                    "Unexpected app/session on the owned WinAppDriver listener; refusing to terminate a foreign target.");
                var id = Uri.EscapeDataString(session.GetProperty("id").GetString()!);
                using var deleted = await client.DeleteAsync(new Uri(address, "/session/" + id));
                deleted.EnsureSuccessStatusCode();
                using var result = JsonDocument.Parse(await deleted.Content.ReadAsStringAsync());
                Require(result.RootElement.GetProperty("status").GetInt32() == 0, $"WinAppDriver did not remove owned session {id}.");
            });
        }
        ThrowFailures(failures);
    }

    private async Task<int[]> ApplicationProcesses()
    {
        using var json = JsonDocument.Parse(await PowerShell(
            $"ConvertTo-Json -InputObject @(Get-Process | Where-Object {{ $_.Path -eq {Quote(executable!)} }} | ForEach-Object {{ $_.Id }}) -Compress"));
        return json.RootElement.EnumerateArray().Select(item => item.GetInt32()).ToArray();
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
            Require(current.GetProperty("Publisher").GetString() == package!.Publisher &&
                current.GetProperty("Version").GetString() == package.Version &&
                (packageFullName is null || fullName == packageFullName),
                $"Package identity changed during the test; refusing to remove foreign {fullName}.");
            await PowerShell($"Remove-AppxPackage -Package {Quote(fullName)} -ErrorAction Stop", TimeSpan.FromMinutes(2));
        }
        Require((await InstalledPackages()).Length == 0, $"Owned {package!.Name} package was not removed.");
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
        Require(apps.Length == 1, $"Pass the exact Application/@Id from the built manifest (not an invented executable): {ApplicationId}");
        var metadata = new PackageMetadata(Required(identity, "Name"), Required(identity, "Publisher"),
            Required(identity, "Version"), ApplicationId, Required(apps[0], "Executable"));
        Require(!metadata.Executable.Contains('$') && !Path.IsPathRooted(metadata.Executable) &&
            !metadata.Executable.Split('\\', '/').Contains(".."),
            "Pass the final generated manifest or package. Source manifests with MSBuild tokens are not runnable artifacts.");
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

    private void RequireInteractiveDesktop()
    {
        Require(Environment.UserInteractive && Process.GetCurrentProcess().SessionId != 0,
            "Windows UI tests require a logged-in interactive desktop, not a CI service/session 0.");
        var desktop = OpenInputDesktop(0, false, 1);
        Require(desktop != IntPtr.Zero, "No accessible input desktop. Unlock and dedicate the Windows host before selecting UI tests.");
        try
        {
            var name = new StringBuilder(256);
            Require(GetUserObjectInformation(desktop, 2, name, name.Capacity * 2, out _) &&
                name.ToString().Equals("Default", StringComparison.OrdinalIgnoreCase),
                "Windows UI tests require the unlocked Default input desktop, not the lock/security desktop.");
        }
        finally
        {
            if (!CloseDesktop(desktop))
                throw new InvalidOperationException("Could not release the input-desktop inspection handle.");
        }
    }

    private static async Task<string> ReadServerOutput(StreamReader reader, TaskCompletionSource ready)
    {
        var text = new StringBuilder();
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            text.AppendLine(line);
            if (line.Contains($"Windows Application Driver listening for requests at: http://127.0.0.1:{AppiumFixture.WindowsPort}",
                StringComparison.Ordinal))
                ready.TrySetResult();
        }
        return text.ToString();
    }

    private async Task WaitForServer(Process process, Task ready)
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            Require(!process.HasExited, "Owned WinAppDriver exited before readiness; inspect attached server logs.");
            if (ready.IsCompletedSuccessfully)
            {
                try
                {
                    using var response = await client.GetAsync($"http://127.0.0.1:{AppiumFixture.WindowsPort}/status");
                    if (response.IsSuccessStatusCode && !process.HasExited)
                        return;
                }
                catch (Exception error) when (error is HttpRequestException or TaskCanceledException) { }
            }
            await Task.Delay(250);
        }
        throw new TimeoutException("Owned WinAppDriver did not report its listener and /status within 30 seconds.");
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
