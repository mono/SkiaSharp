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
	}
}
