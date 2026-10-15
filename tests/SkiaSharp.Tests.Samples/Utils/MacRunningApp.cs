using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacRunningApp(SampleWorkspace workspace, string artifact)
	: AppiumRunningApp(workspace, artifact)
{
	internal static string HostArchitecture => RuntimeInformation.OSArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Mac sample tests require an arm64 or x64 host.")
	};

	internal override string DeviceProfile => HostArchitecture;

	internal override PointerKind PointerKind => PointerKind.Mouse;

	internal override Task<string> ReadApplicationId() => ReadAppleApplicationId();

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

	internal override void Scroll(Rectangle container, int amount) =>
		Driver.ExecuteScript("macos: scroll", new Dictionary<string, object>
		{
			["x"] = container.Left + container.Width / 2,
			["y"] = container.Top + container.Height / 2,
			["deltaX"] = 0,
			["deltaY"] = -amount
		});

	protected override Task Start(Uri address)
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"Mac2 tests require macOS, Xcode and XCTest Accessibility/automation permissions.");

		// Mac2 launches the exact appPath. Do not let its reset terminate an existing app/WDA.
		foreach (var process in Process.GetProcesses())
		{
			using (process)
				Assert.True(process.ProcessName != Path.GetFileNameWithoutExtension(Artifact) &&
					!process.ProcessName.Contains("WebDriverAgent", StringComparison.OrdinalIgnoreCase),
					"A target app or WDA is already running; close it before selecting Mac tests.");
		}

		var options = Options("mac", "mac2");
		options.AddAdditionalAppiumOption("bundleId", ApplicationId);
		options.AddAdditionalAppiumOption("appPath", Artifact);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("skipAppKill", false);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("systemHost", "127.0.0.1");
		options.AddAdditionalAppiumOption("showServerLogs", true);

		SetDriver(new MacDriver(address, options, TimeSpan.FromMinutes(3)));
		Wait(() => Driver.FindElements(By.XPath("//XCUIElementTypeWindow"))
			.FirstOrDefault(window => window.Displayed && window.Size.Width > 0 && window.Size.Height > 0),
			$"{ApplicationId} desktop window");

		return Task.CompletedTask;
	}
}
