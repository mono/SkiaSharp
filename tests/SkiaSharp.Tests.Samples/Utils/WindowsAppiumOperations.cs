using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class WindowsAppiumOperations : AppiumOperations
{
	protected override By Locator(string id) => MobileBy.AccessibilityId(id);

	internal override void SubmitSearch() => Find("gallery-search", "gallery-filter-search").SendKeys(Keys.Enter);

	protected override void DragPoints(Point start, Point end)
	{
		var app = (WindowsRunningApp)Owner;
		var reported = app.ReportedWindow;
		var physical = app.PointerWindow;
		start = WindowsWindow.ToPhysical(start, reported, physical);
		end = WindowsWindow.ToPhysical(end, reported, physical);
		Driver.ExecuteScript("windows: clickAndDrag", new Dictionary<string, object>
		{
			["startX"] = start.X, ["startY"] = start.Y,
			["endX"] = end.X, ["endY"] = end.Y,
			["durationMs"] = 500
		});
	}
}
