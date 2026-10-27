using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using System.Drawing;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MacAppiumOperations : AppiumOperations
{
	protected override By Locator(string id) => MobileBy.AccessibilityId(id);

	internal override void SubmitSearch() =>
		Driver.ExecuteScript("macos: keys",
			new Dictionary<string, object> { ["keys"] = new[] { "XCUIKeyboardKeyReturn" } });

	protected override void DragPoints(Point start, Point end) =>
		Driver.ExecuteScript("macos: clickAndDrag", new Dictionary<string, object>
		{
			["startX"] = start.X, ["startY"] = start.Y,
			["endX"] = end.X, ["endY"] = end.Y,
			["duration"] = 0.5
		});
}
