using System;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests
{
	public abstract class BaseTest
	{
		protected static bool IsLinux = TestConfig.Current.IsLinux;
		protected static bool IsMac = TestConfig.Current.IsMac;
		protected static bool IsUnix = TestConfig.Current.IsUnix;
		protected static bool IsWindows = TestConfig.Current.IsWindows;

		protected static string[] UnicodeFontFamilies => TestConfig.Current.UnicodeFontFamilies;
		protected static string DefaultFontFamily => TestConfig.Current.DefaultFontFamily;

		public static string PathRoot => TestConfig.Current.PathRoot;
		public static string PathToFonts => TestConfig.Current.PathToFonts;
		public static string PathToImages => TestConfig.Current.PathToImages;

		private readonly ITestOutputHelper _output;

		public BaseTest()
		{
		}

		public BaseTest(ITestOutputHelper output)
		{
			_output = output;
		}

		protected void WriteOutput(string message)
		{
			Assert.True(_output is not null, "Output writer was null, you should be using the constructor that accepts an ITestOutputHelper.");

			_output.WriteLine(message);
		}

		public static void CollectGarbage()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		protected static bool IsAndroid =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsAndroid();
#else
			false;
#endif

		protected static bool IsIOS =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsIOS();
#else
			false;
#endif

		protected static bool IsMacCatalyst =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsMacCatalyst();
#else
			false;
#endif

		protected static void SkipOnMono(string reason = "Mono does not guarantee finalizers are invoked immediately")
		{
			Skip.If(IsAndroid || IsIOS || IsMacCatalyst, reason);
		}

		protected static void SkipOnNonWindows(string reason = "Exceptions cannot be thrown in native delegates on non-Windows platforms")
		{
			Skip.If(!IsWindows, reason);
		}

		protected static void SkipOnPlatform(bool condition, string reason)
		{
			Skip.If(condition, reason);
		}
	}
}
