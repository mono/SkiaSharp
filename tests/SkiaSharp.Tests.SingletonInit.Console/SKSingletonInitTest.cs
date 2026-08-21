using System;
using SkiaSharp;
using Xunit;

namespace SkiaSharp.Tests.SingletonInit
{
	// Regression guard for https://github.com/mono/SkiaSharp/issues/3817.
	//
	// Touching SKFontManager.Default as the very first SkiaSharp type in a fresh
	// process used to throw a TypeInitializationException: the SKObject static
	// constructor eagerly initializes a cascade of singletons, and the SKTypeface
	// initializer in that cascade read the managed SKFontManager.Default and
	// SKFontStyle.Normal singletons. When SKFontManager was the first type touched,
	// that re-entered the still-running SKFontManager/SKFontStyle initializer on the
	// same thread and observed a not-yet-assigned (null) singleton, producing a
	// NullReferenceException surfaced as a TypeInitializationException.
	//
	// This assembly intentionally contains a single test so that this is genuinely
	// the first SkiaSharp access in the process.
	public class SKSingletonInitTest
	{
		[Fact]
		public void TouchingFontManagerDefaultFirstDoesNotThrow ()
		{
			var fontManager = SKFontManager.Default;

			Assert.NotNull (fontManager);
			Assert.NotEqual (IntPtr.Zero, fontManager.Handle);
			Assert.NotEqual (IntPtr.Zero, SKFontStyle.Normal.Handle);
			Assert.NotEqual (IntPtr.Zero, SKTypeface.Default.Handle);
		}
	}
}
