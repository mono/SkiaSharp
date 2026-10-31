using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class WindowsAppiumOperations : AppiumOperations
{
	protected override By Locator(string id) => MobileBy.AccessibilityId(id);

	internal override void SubmitSearch() => Find("gallery-search", "gallery-filter-search").SendKeys(Keys.Enter);

	internal override void SetSliderToMinimum(IWebElement slider) => slider.SendKeys(Keys.Home);

	internal override void Click(By selector)
	{
		((WindowsRunningApp)Owner).Activate();
		Wait(() =>
		{
			var element = Driver.FindElement(selector);
			if (!element.Displayed)
				return null;
			ClickAt(new Point(element.Location.X + element.Size.Width / 2,
				element.Location.Y + element.Size.Height / 2));
			return element;
		}, selector.ToString());
	}

	protected override void DragPoints(Point start, Point end)
	{
		((WindowsRunningApp)Owner).Activate();
		start = ToPhysical(start);
		end = ToPhysical(end);
		Driver.ExecuteScript("windows: clickAndDrag", new Dictionary<string, object>
		{
			["startX"] = start.X, ["startY"] = start.Y,
			["endX"] = end.X, ["endY"] = end.Y,
			["durationMs"] = 500
		});
	}

	internal override string Capture(string id, string name)
	{
		((WindowsRunningApp)Owner).Activate();
		return base.Capture(id, name);
	}

	private void ClickAt(Point point)
	{
		point = ToPhysical(point);
		Driver.ExecuteScript("windows: click", new Dictionary<string, object>
		{
			["x"] = point.X, ["y"] = point.Y
		});
	}

	private Point ToPhysical(Point point)
	{
		var app = (WindowsRunningApp)Owner;
		return WindowsWindow.ToPhysical(point, app.ReportedWindow, app.PointerWindow);
	}
}
