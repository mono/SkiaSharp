using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	// Regression guard for PR #3997.
	//
	// SKImage.FromEncodedData(SKData, SKRectI) decodes an intermediate full-size image and
	// then calls image.Subset(subset). When the subset is strictly smaller than the image,
	// Subset() allocates a NEW image, so the intermediate must be disposed. The pre-fix code
	//
	//     return FromEncodedData (data)?.Subset (subset);
	//
	// dropped the intermediate wrapper on the floor, leaking the native SkImage until a GC
	// eventually finalized it. The fix applies the standard same-instance disposal pattern.
	//
	// These tests lock in two things:
	//   * Behavioural parity - the fix must NOT change the produced image (before == after).
	//   * The leak fix - the intermediate image is disposed deterministically (no GC).
	//
	// The leak assertion inspects the global HandleDictionary registry, so this class lives in the
	// serialized HandleDictionary threading collection: it runs in xUnit's non-parallel phase where
	// nothing else is creating or disposing native wrappers concurrently, making the alive-instance
	// count deterministic.
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class Pr3997ResourceLeakTest : SKTest
	{
		// The produced subset must be byte-for-byte identical to decoding and subsetting by
		// hand. This is the "same behaviour before and after the fix" guarantee: the fix only
		// adds disposal of the intermediate, it must not alter the returned pixels.
		[Fact]
		public void FromEncodedDataWithSubsetMatchesManualDecodeThenSubset ()
		{
			using var data = SKData.Create (Path.Combine (PathToImages, "baboon.jpg"));

			SKRectI subset;
			using (var full = SKImage.FromEncodedData (data))
				subset = new SKRectI (0, 0, full.Width / 2, full.Height / 2);

			// The exact work the pre-fix one-liner did, kept alive explicitly.
			using var manualFull = SKImage.FromEncodedData (data);
			using var manualSubset = manualFull.Subset (subset);

			using var combined = SKImage.FromEncodedData (data, subset);

			Assert.NotNull (manualSubset);
			Assert.NotNull (combined);

			Assert.Equal (subset.Width, combined.Width);
			Assert.Equal (subset.Height, combined.Height);
			Assert.Equal (manualSubset.Width, combined.Width);
			Assert.Equal (manualSubset.Height, combined.Height);

			Assert.Equal (ReadPixels (manualSubset), ReadPixels (combined));
		}

		// A subset that equals the full bounds exercises the "same instance" branch of the fix
		// (Subset() can hand back the source image, so nothing is disposed). Output must still be
		// the untouched full image.
		[Fact]
		public void FromEncodedDataWithFullBoundsSubsetReturnsFullImage ()
		{
			using var data = SKData.Create (Path.Combine (PathToImages, "baboon.jpg"));

			int width, height;
			SKColor[] expectedPixels;
			using (var full = SKImage.FromEncodedData (data))
			{
				width = full.Width;
				height = full.Height;
				expectedPixels = ReadPixels (full);
			}

			using var combined = SKImage.FromEncodedData (data, new SKRectI (0, 0, width, height));

			Assert.NotNull (combined);
			Assert.Equal (width, combined.Width);
			Assert.Equal (height, combined.Height);
			Assert.Equal (expectedPixels, ReadPixels (combined));
		}

		// The actual leak fix: after the call only the returned subset image should remain
		// registered as a live native wrapper. Pre-fix, the intermediate full-size image was
		// still alive (delta of 2); the fix disposes it deterministically (delta of 1).
		[Fact]
		public void FromEncodedDataWithSubsetDoesNotLeakIntermediateImage ()
		{
			using var data = SKData.Create (Path.Combine (PathToImages, "baboon.jpg"));

			// A subset strictly smaller than the image forces Subset() to allocate a new image,
			// which is exactly when the intermediate needs disposing.
			SKRectI subset;
			using (var full = SKImage.FromEncodedData (data))
				subset = new SKRectI (0, 0, full.Width / 2, full.Height / 2);

			// Drain any dead-but-not-yet-finalized wrappers left by earlier tests so the
			// baseline count is stable. (Nothing runs in parallel with this collection.)
			CollectGarbage ();

			var before = CountAliveImages ();

			var result = SKImage.FromEncodedData (data, subset);

			// Deliberately NO GC between the two snapshots: a leaked intermediate would still be
			// alive here, so a buggy build reports a delta of 2. The fix disposes it immediately,
			// leaving only the returned image -> delta of exactly 1.
			var after = CountAliveImages ();

			Assert.NotNull (result);
			Assert.Equal (before + 1, after);

			result.Dispose ();
		}

		[Fact]
		public void FromEncodedDataWithSubsetThrowsForNullData ()
		{
			Assert.Throws<ArgumentNullException> (() => SKImage.FromEncodedData ((SKData)null, new SKRectI (0, 0, 10, 10)));
		}

		private static int CountAliveImages ()
		{
			HandleDictionary.instancesLock.EnterReadLock ();
			try
			{
				return HandleDictionary.instances.Values
					.Select (w => w.Target as SKImage)
					.Count (img => img is not null && !img.IsDisposed);
			}
			finally
			{
				HandleDictionary.instancesLock.ExitReadLock ();
			}
		}

		private static SKColor[] ReadPixels (SKImage image)
		{
			using var bitmap = new SKBitmap (image.Width, image.Height);
			using var canvas = new SKCanvas (bitmap);
			canvas.Clear (SKColors.Transparent);
			canvas.DrawImage (image, 0, 0);
			return bitmap.Pixels;
		}
	}
}
