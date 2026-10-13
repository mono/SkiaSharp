using System.Drawing;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, string webDriverAgentMacRoot)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private const string LaunchServices =
        "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister";
    private string? executable;
    private readonly DateTime started = DateTime.Now;
    private bool sessionRequested;

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
        Require(OperatingSystem.IsMacOS(), "Mac2 tests require macOS, Xcode and configured XCTest Accessibility/automation permissions.");
        Require(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase), "Mac2 requires the actual built macOS or MacCatalyst .app bundle.");
        Require(Regex.IsMatch(ApplicationId, @"^[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+$"), $"Invalid Mac bundle ID: {ApplicationId}");
        Require((await Command("/usr/bin/stat", ["-f", "%Su", "/dev/console"])).Trim() == Environment.UserName, "Mac2 requires this user to own an unlocked interactive desktop.");
        var info = Path.Combine(Artifact, "Contents", "Info.plist");
        Require(File.Exists(info), $"Built bundle metadata is missing: {info}");
        Require((await Command("/usr/bin/plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", info])).Trim() == ApplicationId, $"Built bundle does not declare {ApplicationId}.");
        executable = Path.Combine(Artifact, "Contents", "MacOS", (await Command("/usr/bin/plutil", ["-extract", "CFBundleExecutable", "raw", "-o", "-", info])).Trim());
        Require(File.Exists(executable), $"Built bundle executable is missing: {executable}");
        var conflicts = await Script($$"""
            ObjC.import('AppKit');
            var apps = $.NSWorkspace.sharedWorkspace.runningApplications, result = [];
            for (var i = 0; i < apps.count; i++) {
                var app = apps.objectAtIndex(i);
                if (!app.bundleIdentifier.isNil()) {
                    var id = ObjC.unwrap(app.bundleIdentifier);
                    if (id === {{JsonSerializer.Serialize(ApplicationId)}} || /WebDriverAgent/i.test(id))
                        result.push(id);
                }
            }
            JSON.stringify(result);
            """);
        using (var apps = JsonDocument.Parse(conflicts))
            Require(apps.RootElement.GetArrayLength() == 0,
                $"A target app or WDA is already running; refusing to reuse or terminate it: {conflicts}");
        var registered = (await Script($"""
            ObjC.import('AppKit');
            var url = $.NSWorkspace.sharedWorkspace.URLForApplicationWithBundleIdentifier({JsonSerializer.Serialize(ApplicationId)});
            url.isNil() ? '' : ObjC.unwrap(url.path);
            """)).Trim();
        Require(registered.Length == 0 || Path.GetFullPath(registered) == Artifact,
            $"Bundle {ApplicationId} is registered at {registered}; refusing existing installation/data reuse.");
        OwnAppData();
        Own(() => Command(LaunchServices, ["-u", Artifact]));
        AppiumFixture.RequireFreePort(AppiumFixture.MacPort);
        Require(File.Exists(Path.Combine(webDriverAgentMacRoot, "WebDriverAgentMac.xcodeproj", "project.pbxproj")),
            "Provide the preinstalled Mac2 WebDriverAgentMac source. Tests never install drivers.");
        var bootstrap = Path.Combine(Workspace.Root, "wda-mac");
        SampleWorkspace.CopyTree(webDriverAgentMacRoot, bootstrap);
        var options = Options("mac", "mac2");
        options.AddAdditionalAppiumOption("bundleId", ApplicationId);
        options.AddAdditionalAppiumOption("appPath", Artifact);
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("skipAppKill", false);
        options.AddAdditionalAppiumOption("bootstrapRoot", bootstrap);
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.MacPort);
        options.AddAdditionalAppiumOption("systemHost", "127.0.0.1");
        options.AddAdditionalAppiumOption("showServerLogs", true);
        sessionRequested = true;
        SetDriver(new MacDriver(address, options, TimeSpan.FromMinutes(3)));
        Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
            is { Displayed: true } element ? element : null, $"{ApplicationId} desktop UI");
    }

    protected override async Task CapturePlatformDiagnostics()
    {
        if (sessionRequested)
            await Command("/usr/bin/log", ["show", "--style", "compact", "--start", started.ToString("yyyy-MM-dd HH:mm:ss"),
                "--predicate", $"processImagePath == {JsonSerializer.Serialize(executable)}"]);
    }

    private void OwnAppData()
    {
        var library = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library");
        string[] paths = [Path.Combine(library, "Containers", ApplicationId),
            Path.Combine(library, "Preferences", ApplicationId + ".plist"),
            Path.Combine(library, "Saved Application State", ApplicationId + ".savedState")];
        foreach (var path in paths)
            Require(!File.Exists(path) && !Directory.Exists(path),
                $"Existing app state at {path}; remove it explicitly before selecting this desktop test.");
        // Mac2 terminates the app; filesystem/LaunchServices state is owned separately.
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
}
