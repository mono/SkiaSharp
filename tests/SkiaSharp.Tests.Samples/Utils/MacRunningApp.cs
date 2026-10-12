using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

// Mac2 automates both AppKit/macOS and MacCatalyst bundles, including MAUI.
internal sealed class MacRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, string webDriverAgentMacRoot)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private const string LaunchServices =
        "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister";
    private bool ownsLaunch;
    private string? executable;
    private readonly DateTime started = DateTime.Now;
    private readonly string wdaBundleId = $"com.skiasharp.samples.tests.macwda{Guid.NewGuid():N}";

    internal override PointerKind PointerKind => PointerKind.Mouse;
    internal override void SubmitSearch() =>
        Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
        {
            ["keys"] = new[] { "XCUIKeyboardKeyReturn" }
        });
    internal override void Drag(Point start, Point end) =>
        Driver.ExecuteScript("macos: clickAndDrag", new Dictionary<string, object>
        {
            ["startX"] = start.X, ["startY"] = start.Y, ["endX"] = end.X, ["endY"] = end.Y,
            ["duration"] = 0.5
        });

    protected override async Task Start(Uri address)
    {
        Require(OperatingSystem.IsMacOS(), "Selected Mac2 tests require macOS, Xcode, and preconfigured XCTest Accessibility/automation permissions.");
        Require(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
            "Mac2 requires the exact built macOS or MacCatalyst .app bundle.");
        Require(Regex.IsMatch(ApplicationId, @"^[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+$"),
            $"Invalid Mac bundle ID: {ApplicationId}");
        Require((await Command("/usr/bin/stat", ["-f", "%Su", "/dev/console"])).Trim() == Environment.UserName,
            "Mac2 requires this user to own an unlocked interactive macOS desktop; a headless/service host is not supported.");
        var info = Path.Combine(Artifact, "Contents", "Info.plist");
        Require(File.Exists(info), $"Desktop bundle Info.plist is missing: {info}");
        Require((await Command("/usr/bin/plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", info])).Trim() == ApplicationId,
            $"Built bundle identity does not match {ApplicationId}.");
        executable = Path.Combine(Artifact, "Contents", "MacOS",
            (await Command("/usr/bin/plutil", ["-extract", "CFBundleExecutable", "raw", "-o", "-", info])).Trim());
        Require(File.Exists(executable), $"Built bundle executable is missing: {executable}");
        Require((await RunningApplications(ApplicationId)).Count == 0,
            $"An application with bundle ID {ApplicationId} is already running; refusing to attach, relaunch, or stop it.");
        var drivers = await Script("""
            ObjC.import('AppKit');
            var apps = $.NSWorkspace.sharedWorkspace.runningApplications, result = [];
            for (var i = 0; i < apps.count; i++) {
                var app = apps.objectAtIndex(i);
                if (!app.bundleIdentifier.isNil()) {
                    var id = ObjC.unwrap(app.bundleIdentifier);
                    if (/WebDriverAgent|^com\.skiasharp\.samples\.tests\.(mac)?wda/i.test(id)) result.push(id);
                }
            }
            JSON.stringify(result);
            """);
        using (var runningDrivers = JsonDocument.Parse(drivers))
            Require(runningDrivers.RootElement.GetArrayLength() == 0,
                $"An existing WDA application owns desktop focus: {drivers}. Stop it explicitly; tests will not reuse or terminate it.");
        var registered = (await Script($"""
            ObjC.import('AppKit');
            var url = $.NSWorkspace.sharedWorkspace.URLForApplicationWithBundleIdentifier({JsonSerializer.Serialize(ApplicationId)});
            url.isNil() ? '' : ObjC.unwrap(url.path);
            """)).Trim();
        Require(registered.Length == 0 || Path.GetFullPath(registered) == Artifact,
            $"Bundle {ApplicationId} is already registered at {registered}; refusing existing installation/data reuse.");
        OwnAppData(ApplicationId);
        Own(() => Command(LaunchServices, ["-u", Artifact]));

        // Mac2 has no derivedDataPath capability. Own the XCTest process ourselves, and
        // use its documented webDriverAgentMacUrl rather than its shared default build.
        Require(Directory.Exists(webDriverAgentMacRoot),
            "Pass the explicitly installed Mac2 driver's WebDriverAgentMac source directory. Tests never install Appium/drivers.");
        var wdaRoot = Path.Combine(Workspace.Root, "wda-mac-source");
        SampleWorkspace.CopyTree(webDriverAgentMacRoot, wdaRoot);
        var project = Path.Combine(wdaRoot, "WebDriverAgentMac.xcodeproj", "project.pbxproj");
        Require(File.Exists(project), $"WebDriverAgentMac.xcodeproj is missing from {webDriverAgentMacRoot}.");
        var text = await File.ReadAllTextAsync(project);
        var identity = new Regex(@"PRODUCT_BUNDLE_IDENTIFIER = io\.appium\.WebDriverAgentRunner;");
        Require(identity.Matches(text).Count == 2,
            "Installed Mac2 WDA runner build settings differ from the supported project. Supply a compatible driver; do not mutate the shared driver installation.");
        await File.WriteAllTextAsync(project, identity.Replace(text, $"PRODUCT_BUNDLE_IDENTIFIER = {wdaBundleId};"));
        var derivedData = Path.Combine(Workspace.Root, "wda-mac-derived");
        OwnAppData(wdaBundleId + ".xctrunner");
        Own(async () =>
        {
            if (!Directory.Exists(derivedData))
                return;
            foreach (var runner in Directory.EnumerateDirectories(derivedData, "WebDriverAgentRunner-Runner.app", SearchOption.AllDirectories))
            {
                var bundle = (await Command("/usr/bin/plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-",
                    Path.Combine(runner, "Contents", "Info.plist")])).Trim();
                Require(bundle == wdaBundleId + ".xctrunner", $"Unexpected WDA identity under owned build output: {bundle}");
                await Command(LaunchServices, ["-u", runner]);
            }
        });
        Own(() => StopBundle(wdaBundleId + ".xctrunner", path =>
        {
            var relative = Path.GetRelativePath(Workspace.Root, path);
            return !Path.IsPathRooted(relative) && relative != ".." &&
                !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
        }));
        AppiumFixture.RequireFreePort(AppiumFixture.MacPort);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var wda = ProcessRunner.Start("/usr/bin/xcodebuild",
            ["build-for-testing", "test-without-building", "-project", Path.GetDirectoryName(project)!,
                "-scheme", "WebDriverAgentRunner", "-destination", "platform=macOS",
                "-derivedDataPath", derivedData, "COMPILER_INDEX_STORE_ENABLE=NO"],
            wdaRoot, start =>
            {
                start.Environment["USE_PORT"] = AppiumFixture.MacPort.ToString();
                start.Environment["USE_HOST"] = "127.0.0.1";
            });
        var stdout = ReadWdaOutput(wda.StandardOutput, ready);
        var stderr = ReadWdaOutput(wda.StandardError, ready);
        Own(() => StopProcess(wda, stdout, stderr, "wda-mac"));
        await WaitForWda(wda, ready.Task);

        var options = Options("mac", "mac2");
        options.AddAdditionalAppiumOption("bundleId", ApplicationId);
        options.AddAdditionalAppiumOption("appPath", Artifact);
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("skipAppKill", false);
        options.AddAdditionalAppiumOption("webDriverAgentMacUrl", $"http://127.0.0.1:{AppiumFixture.MacPort}");
        ownsLaunch = true;
        SetDriver(new MacDriver(address, options, TimeSpan.FromMinutes(3)));
        Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
            is { Displayed: true } element ? element : null, $"{ApplicationId} desktop UI");
    }

    protected override async Task StopApplication()
    {
        if (!ownsLaunch)
            return;
        await StopBundle(ApplicationId, path => path == Artifact);
    }

    protected override async Task CapturePlatformDiagnostics()
    {
        if (ownsLaunch)
            await Command("/usr/bin/log", ["show", "--style", "compact", "--start", started.ToString("yyyy-MM-dd HH:mm:ss"),
                "--predicate", $"processImagePath == {JsonSerializer.Serialize(executable)}"]);
    }

    private async Task StopBundle(string bundleId, Func<string, bool> expectedPath)
    {
        List<Exception> failures = [];
        foreach (var app in await RunningApplications(bundleId))
        {
            await Attempt(failures, async () =>
            {
                Require(expectedPath(app.Path),
                    $"Bundle {bundleId} appeared at foreign path {app.Path}; refusing to stop PID {app.Pid}.");
                await Script($"""
                    ObjC.import('AppKit');
                    var app = $.NSRunningApplication.runningApplicationWithProcessIdentifier({app.Pid});
                    if (!app.isNil() && (ObjC.unwrap(app.bundleIdentifier) !== {JsonSerializer.Serialize(bundleId)} ||
                        ObjC.unwrap(app.bundleURL.path) !== {JsonSerializer.Serialize(app.Path)})) throw Error('PID identity changed');
                    if (!app.isNil() && !app.forceTerminate) throw Error('Owned app could not be terminated');
                    """);
            });
        }
        await Attempt(failures, async () =>
        {
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while ((await RunningApplications(bundleId)).Any(app => expectedPath(app.Path)))
            {
                Require(DateTime.UtcNow < deadline, $"Owned {bundleId} did not terminate within 15 seconds.");
                await Task.Delay(200);
            }
        });
        ThrowFailures(failures);
    }

    private void OwnAppData(string bundleId)
    {
        var library = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library");
        string[] paths = [Path.Combine(library, "Containers", bundleId), Path.Combine(library, "Preferences", bundleId + ".plist"),
            Path.Combine(library, "Saved Application State", bundleId + ".savedState")];
        foreach (var path in paths)
            Require(!File.Exists(path) && !Directory.Exists(path),
                $"Existing app state at {path}; remove it explicitly before selecting this desktop test.");
        foreach (var path in paths)
            Own(() =>
            {
                if (Directory.Exists(path))
                    SampleWorkspace.DeleteDirectory(path);
                else if (File.Exists(path))
                    File.Delete(path);
                return Task.CompletedTask;
            });
    }

    private Task<string> Script(string javascript) =>
        Command("/usr/bin/osascript", ["-l", "JavaScript", "-e", javascript]);

    private async Task<List<(int Pid, string Path)>> RunningApplications(string bundleId)
    {
        var output = await Script($$"""
            ObjC.import('AppKit');
            var apps = $.NSWorkspace.sharedWorkspace.runningApplications, result = [];
            for (var i = 0; i < apps.count; i++) {
                var app = apps.objectAtIndex(i);
                if (!app.bundleIdentifier.isNil() && ObjC.unwrap(app.bundleIdentifier) === {{JsonSerializer.Serialize(bundleId)}})
                    result.push({pid: Number(app.processIdentifier), path: ObjC.unwrap(app.bundleURL.path)});
            }
            JSON.stringify(result);
            """);
        using var json = JsonDocument.Parse(output);
        return json.RootElement.EnumerateArray().Select(item =>
            (item.GetProperty("pid").GetInt32(), Path.GetFullPath(item.GetProperty("path").GetString()!))).ToList();
    }

    private static async Task<string> ReadWdaOutput(StreamReader reader, TaskCompletionSource ready)
    {
        var text = new StringBuilder();
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            text.AppendLine(line);
            if (line.Contains($"ServerURLHere->http://localhost:{AppiumFixture.MacPort}<-ServerURLHere", StringComparison.Ordinal))
                ready.TrySetResult();
        }
        return text.ToString();
    }

    private async Task WaitForWda(Process process, Task ready)
    {
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddMinutes(5);
        while (DateTime.UtcNow < deadline)
        {
            Require(!process.HasExited, "Owned Mac WDA exited before readiness; inspect attached XCTest logs.");
            if (ready.IsCompletedSuccessfully)
            {
                try
                {
                    using var response = await client.GetAsync($"http://127.0.0.1:{AppiumFixture.MacPort}/status");
                    if (response.IsSuccessStatusCode && !process.HasExited)
                        return;
                }
                catch (Exception error) when (error is HttpRequestException or TaskCanceledException) { }
            }
            await Task.Delay(250);
        }
        throw new TimeoutException("Owned Mac WDA did not report its listener and /status within five minutes. Verify interactive host and pre-granted XCTest permissions.");
    }
}
