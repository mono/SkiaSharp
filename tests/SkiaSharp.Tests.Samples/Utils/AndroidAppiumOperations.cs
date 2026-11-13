using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidAppiumOperations : AppiumOperations
{
	protected override By Locator(string id) => ElementLocator(ApplicationId, id);

	internal static By ElementLocator(string applicationId, string id) =>
		By.Id($"{applicationId}:id/{id}");

	internal override void SubmitSearch() => ((AndroidDriver)Driver).HideKeyboard();
}
