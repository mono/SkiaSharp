using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace SkiaSharp.Tests
{
	// Equivalence coverage for the managed port of the implicit SKColor -> SKColorF
	// conversion operator (previously sk_color4f_from_color via P/Invoke). Every
	// assertion compares the managed operator bit-for-bit against the native oracle.
	public class SKColorFConvertEquivalenceTest : SKTest
	{
		// The original native path the managed operator replaces.
		private static unsafe SKColorF NativeFromColor (SKColor color)
		{
			SKColorF f;
			SkiaApi.sk_color4f_from_color ((uint)color, &f);
			return f;
		}

		private static void AssertBitExact (SKColor src)
		{
			var native = NativeFromColor (src);
			SKColorF managed = src; // managed implicit operator under test

			Assert.True (
				BitConverter.SingleToInt32Bits (native.Red) == BitConverter.SingleToInt32Bits (managed.Red) &&
				BitConverter.SingleToInt32Bits (native.Green) == BitConverter.SingleToInt32Bits (managed.Green) &&
				BitConverter.SingleToInt32Bits (native.Blue) == BitConverter.SingleToInt32Bits (managed.Blue) &&
				BitConverter.SingleToInt32Bits (native.Alpha) == BitConverter.SingleToInt32Bits (managed.Alpha),
				$"SKColor {src}: managed ({managed.Red:R},{managed.Green:R},{managed.Blue:R},{managed.Alpha:R}) " +
				$"!= native ({native.Red:R},{native.Green:R},{native.Blue:R},{native.Alpha:R})");
		}

		[Fact]
		public void ManagedFromColorMatchesNativeForAllGrayscale ()
		{
			for (var c = 0; c <= 255; c++)
				AssertBitExact (new SKColor ((byte)c, (byte)c, (byte)c, (byte)c));
		}

		[Fact]
		public void ManagedFromColorMatchesNativeForEachChannelSweep ()
		{
			// Vary one channel across its full range while holding the others at
			// distinct values, so a swapped-channel port would diverge somewhere.
			for (var v = 0; v <= 255; v++) {
				AssertBitExact (new SKColor ((byte)v, 17, 200, 99));
				AssertBitExact (new SKColor (17, (byte)v, 200, 99));
				AssertBitExact (new SKColor (17, 200, (byte)v, 99));
				AssertBitExact (new SKColor (17, 200, 99, (byte)v));
			}
		}

		[Fact]
		public void ManagedFromColorMatchesNativeForDistinctChannels ()
		{
			var samples = new (byte r, byte g, byte b, byte a)[] {
				(0, 0, 0, 0), (255, 255, 255, 255), (255, 0, 0, 255),
				(0, 255, 0, 255), (0, 0, 255, 255), (10, 20, 30, 40),
				(1, 2, 3, 4), (128, 64, 32, 16), (127, 129, 126, 200),
				(254, 1, 253, 2), (100, 150, 200, 250),
			};
			foreach (var s in samples)
				AssertBitExact (new SKColor (s.r, s.g, s.b, s.a));
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static float NaiveDivide (int c) => c / 255f;

		[Fact]
		public void EquivalenceComparisonDetectsAWrongPort ()
		{
			// Guard: prove the bit-exact comparison the tests above rely on has teeth.

			// (a) A swapped R/B port (a plausible mistake given SkColor's BGRA memory
			// order) produces different bits and is caught.
			var src = new SKColor (10, 20, 30, 40);
			var native = NativeFromColor (src);
			var swapped = new SKColorF (native.Blue, native.Green, native.Red, native.Alpha);
			Assert.NotEqual (
				BitConverter.SingleToInt32Bits (native.Red),
				BitConverter.SingleToInt32Bits (swapped.Red));

			// (b) A naive divide-by-255 differs from the native *(1/255) scale at some
			// inputs (e.g. 127), confirming the exact formula matters.
			var grayNative = NativeFromColor (new SKColor (127, 127, 127, 127));
			Assert.NotEqual (
				BitConverter.SingleToInt32Bits (grayNative.Red),
				BitConverter.SingleToInt32Bits (NaiveDivide (127)));
		}
	}
}
