using OpenQA.Selenium.Appium;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class DesktopAppiumDriver(Uri address, AppiumOptions options)
	: AppiumDriver(address, options.ToCapabilities(), TimeSpan.FromMinutes(6));
