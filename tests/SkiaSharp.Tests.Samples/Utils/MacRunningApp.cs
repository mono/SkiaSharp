using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacRunningApp(SampleWorkspace workspace, string artifact, bool catalyst)
	: AppiumRunningApp(workspace, artifact, new MacAppiumOperations())
{
	internal static string FindApp(string output)
	{
		var apps = Directory.Exists(output) ? Directory.GetDirectories(output, "*.app") : [];
		if (apps.Length != 1 || !File.Exists(Path.Combine(apps[0], "Contents", "Info.plist")))
			throw new FileNotFoundException($"Expected one built desktop Mac app in {output}. Run build mode first.");
		return apps[0];
	}

	internal override string DeviceProfile => catalyst ? "mac-catalyst" : "macos-desktop";
	private string PlatformName => catalyst ? "MacCatalyst" : "macOS";
	internal override GoldenCase Reference(string caseName) =>
		new(DeviceProfile, caseName, Environment.OSVersion.Version,
			RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

	internal override async Task<string> ReadApplicationId()
	{
		Assert.True(OperatingSystem.IsMacOS(), "Desktop Mac samples require macOS.");
		var plist = Path.Combine(Artifact, "Contents", "Info.plist");
		var id = (await Command("plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", plist])).Trim();
		if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsWhiteSpace))
			throw new InvalidOperationException($"Built desktop bundle has an invalid CFBundleIdentifier: {plist}");
		return id;
	}

	protected override async Task Start(Uri address)
	{
		Assert.True(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
			"Provide the exact built desktop .app bundle.");
		await InspectApplication("check");
		var options = new AppiumOptions
		{
			PlatformName = "mac",
			AutomationName = "mac2"
		};
		options.AddAdditionalAppiumOption("appPath", Artifact);
		options.AddAdditionalAppiumOption("bundleId", ApplicationId);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("skipAppKill", false);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("showServerLogs", true);
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		SetDriver(new DesktopAppiumDriver(address, options));

		Operations.Wait(() => Convert.ToInt32(Driver.ExecuteScript("macos: queryAppState",
			new Dictionary<string, object> { ["path"] = Artifact })) == 4 ? Artifact : null,
			$"exact built {PlatformName} app in the foreground");
	}

	protected override async Task StopPlatform() => await InspectApplication("stop");

	private async Task InspectApplication(string action)
	{
		const string script = """
			function run(argv) {
				ObjC.import('AppKit');
				var apps = $.NSWorkspace.sharedWorkspace.runningApplications;
				for (var i = 0; i < apps.count; i++) {
					var app = apps.objectAtIndex(i);
					if (app.bundleURL && ObjC.unwrap(app.bundleURL.path) === argv[0]) {
						if (argv[1] === 'check')
							throw new Error('The exact prepared app is already running: ' + argv[0]);
						if (!app.forceTerminate())
							throw new Error('Could not terminate the exact prepared app: ' + argv[0]);
					}
				}
			}
			""";
		await Command("osascript", ["-l", "JavaScript", "-e", script, Artifact, action]);
	}
}
