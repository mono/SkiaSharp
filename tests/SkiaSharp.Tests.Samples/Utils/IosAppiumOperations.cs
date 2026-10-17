using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosAppiumOperations : AppiumOperations
{
	protected override By Locator(string id) => MobileBy.AccessibilityId(id);

	internal override void SubmitSearch() =>
		Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();
}
