using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidAppiumOperations(bool nativeResourceIds = false) : AppiumOperations
{
	protected override By Locator(string id) => ElementLocator(ApplicationId, id, nativeResourceIds);

	internal static By ElementLocator(string applicationId, string id, bool nativeResourceIds) =>
		nativeResourceIds ? By.Id($"{applicationId}:id/{id}") : MobileBy.AccessibilityId(id);

	internal override void SubmitSearch() => ((AndroidDriver)Driver).HideKeyboard();
}
