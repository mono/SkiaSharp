using System;

namespace SkiaSharpSample
{
	[Flags]
	public enum SamplePlatforms
	{
		iOS = 1 << 0,
		Android = 1 << 1,
		OSX = 1 << 2,
		WindowsDesktop = 1 << 3,
		UWP = 1 << 4,
		tvOS = 1 << 5,
		MAUI = 1 << 6,
		Blazor = 1 << 7,

		All = iOS | Android | OSX | WindowsDesktop | UWP | tvOS | MAUI | Blazor,

		AllWindows = WindowsDesktop | UWP,
		AllAndroid = Android,
		AlliOS = iOS | tvOS,
		AllApple = iOS | tvOS | OSX,
		AllMobile = iOS | tvOS | Android | UWP | MAUI,
		AllDesktop = WindowsDesktop | OSX,
		AllModern = MAUI | Blazor,
	}
}
