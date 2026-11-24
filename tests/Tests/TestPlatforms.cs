using System;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// The hosts the test suite runs on, as flags so a capability can declare the
	/// whole set of platforms it applies to in one value. Names double as the
	/// lowercase platform tag (see <see cref="TestConfig.PlatformName"/>).
	/// </summary>
	[Flags]
	public enum TestPlatforms
	{
		None = 0,

		Windows = 1 << 0,
		MacOS = 1 << 1,
		Linux = 1 << 2,
		Android = 1 << 3,
		IOS = 1 << 4,
		MacCatalyst = 1 << 5,
		TvOS = 1 << 6,
		Browser = 1 << 7,

		// Kept apart from Windows because native/nanoserver/build.cake builds without
		// Vulkan or Direct3D: the OS is Windows but the library is not.
		NanoServer = 1 << 8,

		Apple = MacOS | IOS | MacCatalyst | TvOS,
		Desktop = Windows | MacOS | Linux,
		AnyWindows = Windows | NanoServer,
		All = Windows | MacOS | Linux | Android | IOS | MacCatalyst | TvOS | Browser | NanoServer,
	}
}
