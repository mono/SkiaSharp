using System;
using SkiaSharp.Internals;

namespace SkiaSharp.Tests
{
	public static class Traits
	{
		public static class Category
		{
			public const string Key = "Category";

			public static class Values
			{
				public const string Api = "API";
				public const string Gpu = "GPU";
				public const string MatchCharacter = "MatchCharacter";
			}
		}

		public static class FailingOn
		{
			public const string Key = "FailingOn";

			public static class Values
			{
				public const string Android = "Android";
				public const string iOS = "iOS";
				public const string Linux = "Linux";
				public const string MacCatalyst = "MacCatalyst";
				public const string macOS = "macOS";
				public const string Tizen = "Tizen";
				public const string tvOS = "tvOS";
				public const string watchOS = "watchOS";
				public const string Windows = "Windows";

				public static string GetCurrent()
				{
#if NET5_0_OR_GREATER
					if (OperatingSystem.IsAndroid())
						return Android;
					if (OperatingSystem.IsIOS())
						return iOS;
					if (OperatingSystem.IsLinux())
						return Linux;
					if (OperatingSystem.IsMacCatalyst())
						return MacCatalyst;
					if (OperatingSystem.IsMacOS())
						return macOS;
					if (OperatingSystem.IsOSPlatform("Tizen"))
						return Tizen;
					if (OperatingSystem.IsTvOS())
						return tvOS;
					if (OperatingSystem.IsWatchOS())
						return watchOS;
					if (OperatingSystem.IsWindows())
						return Windows;
#else
					if (PlatformConfiguration.IsLinux)
						return Linux;
					if (PlatformConfiguration.IsMac)
						return macOS;
					if (PlatformConfiguration.IsWindows)
						return Windows;
#endif
					throw new InvalidOperationException("Unable to detect the current operating system.");
				}
			}
		}

		public static class SkipOn
		{
			public const string Key = "SkipOn";

			public static class Values
			{
				public const string Android = "Android";
				public const string iOS = "iOS";
				public const string Linux = "Linux";
				public const string MacCatalyst = "MacCatalyst";
				public const string macOS = "macOS";
				public const string Tizen = "Tizen";
				public const string tvOS = "tvOS";
				public const string watchOS = "watchOS";
				public const string Windows = "Windows";

				public static string GetCurrent()
				{
#if NET5_0_OR_GREATER
					if (OperatingSystem.IsAndroid())
						return Android;
					if (OperatingSystem.IsIOS())
						return iOS;
					if (OperatingSystem.IsLinux())
						return Linux;
					if (OperatingSystem.IsMacCatalyst())
						return MacCatalyst;
					if (OperatingSystem.IsMacOS())
						return macOS;
					if (OperatingSystem.IsOSPlatform("Tizen"))
						return Tizen;
					if (OperatingSystem.IsTvOS())
						return tvOS;
					if (OperatingSystem.IsWatchOS())
						return watchOS;
					if (OperatingSystem.IsWindows())
						return Windows;
#else
					if (PlatformConfiguration.IsLinux)
						return Linux;
					if (PlatformConfiguration.IsMac)
						return macOS;
					if (PlatformConfiguration.IsWindows)
						return Windows;
#endif
					throw new InvalidOperationException("Unable to detect the current operating system.");
				}
			}
		}
	}
}
