using System.Drawing;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacRunningApp(
	SampleWorkspace workspace, string artifact, string applicationId, string webDriverAgentMacRoot)
	: AppiumRunningApp(workspace, artifact, applicationId)
{
	internal override PointerKind PointerKind => PointerKind.Mouse;
	internal override void SubmitSearch() =>
		Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
		{
			["keys"] = new[] { "XCUIKeyboardKeyReturn" }
		});
	internal override void Drag(Point start, Point end) =>
		Driver.ExecuteScript("macos: clickAndDrag", new Dictionary<string, object>
		{
			["startX"] = start.X,
			["startY"] = start.Y,
			["endX"] = end.X,
			["endY"] = end.Y,
			["duration"] = 0.5
		});

	protected override Task Start(Uri address)
	{
		Require(OperatingSystem.IsMacOS(),
			"Mac2 tests require macOS, Xcode and XCTest Accessibility/automation permissions.");

		// Mac2 launches the exact appPath. Do not let its reset terminate an existing app/WDA.
		foreach (var process in Process.GetProcesses())
		{
			using (process)
				Require(process.ProcessName != Path.GetFileNameWithoutExtension(Artifact) &&
					!process.ProcessName.Contains("WebDriverAgent", StringComparison.OrdinalIgnoreCase),
					"A target app or WDA is already running; close it before selecting Mac tests.");
		}

		OwnAppData();

		var bootstrap = Path.Combine(Workspace.Root, "wda-mac");
		SampleWorkspace.CopyTree(webDriverAgentMacRoot, bootstrap);

		var options = Options("mac", "mac2");
		options.AddAdditionalAppiumOption("bundleId", ApplicationId);
		options.AddAdditionalAppiumOption("appPath", Artifact);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("skipAppKill", false);
		options.AddAdditionalAppiumOption("bootstrapRoot", bootstrap);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("systemHost", "127.0.0.1");
		options.AddAdditionalAppiumOption("showServerLogs", true);

		SetDriver(new MacDriver(address, options, TimeSpan.FromMinutes(3)));
		Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
			is { Displayed: true } element ? element : null, $"{ApplicationId} desktop UI");
		return Task.CompletedTask;
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
		// Appium terminates the app; remove only data that did not exist before this test.
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

}
