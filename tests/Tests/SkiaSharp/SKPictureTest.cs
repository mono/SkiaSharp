using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPictureTest : SKTest
	{
		private static readonly byte[] MagicBytes = {
			(byte)'s', (byte)'k', (byte)'i', (byte)'a', (byte)'p', (byte)'i', (byte)'c', (byte)'t'
		};

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void CanSerializeToData()
		{
			using var picture = CreateTestPicture();

			using var data = picture.Serialize();
			Assert.NotNull(data);

			var span = data.AsSpan();
			Assert.True(span.Length > 8);
			Assert.Equal(MagicBytes, span.Slice(0, 8).ToArray());
		}

		[Fact]
		public void CanSerializeToStream()
		{
			using var picture = CreateTestPicture();

			using var stream = new MemoryStream();
			picture.Serialize(stream);

			Assert.True(stream.Length > 8);
			Assert.Equal(MagicBytes, stream.ToArray().Take(8));
		}

		[Fact]
		public void CanDeserializeFromData()
		{
			using var picture = CreateTestPicture();
			using var data = picture.Serialize();

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			using var deserialized = SKPicture.Deserialize(data);
			Assert.Equal(SKRect.Create(0, 0, 40, 40), deserialized.CullRect);
			cnv.DrawPicture(deserialized);

			ValidateTestBitmap(bmp);
		}

		[Fact]
		public void CanDeserializeFromStream()
		{
			using var picture = CreateTestPicture();
			using var data = picture.Serialize();
			using var stream = new MemoryStream(data.ToArray());

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			using var deserialized = SKPicture.Deserialize(stream);
			Assert.Equal(SKRect.Create(0, 0, 40, 40), deserialized.CullRect);
			cnv.DrawPicture(deserialized);

			ValidateTestBitmap(bmp);
		}

		[Fact]
		public void CanPlayback()
		{
			using var picture = CreateTestPicture();

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			picture.Playback(cnv);

			ValidateTestBitmap(bmp);
		}

		[Fact]
		public void CanDrawPicture()
		{
			using var picture = CreateTestPicture();

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			cnv.DrawPicture(picture);

			ValidateTestBitmap(bmp);
		}

		[Fact]
		public void CanGetApproximateOperationCount()
		{
			using var picture = CreateTestPicture();

			Assert.Equal(5, picture.ApproximateOperationCount);
		}

		[Fact]
		public void CanGetApproximateBytesUsed()
		{
			using var picture = CreateTestPicture();

			Assert.True(picture.ApproximateBytesUsed > 0);
		}

		[Fact]
		public void CanRoundtripNestedPictureThroughSerialization()
		{
			// Guards against regressions from the M154 SkBigPicture -> SkPicture refactor
			// (upstream b392fb672d). Records an outer picture that draws a nested SKPicture,
			// serializes / deserializes it through the stable C API, and asserts observable
			// contract parity + raster playback fidelity. Uses only stable public APIs and
			// avoids byte-exact serialization or exact byte-count assertions.

			var cullRect = SKRect.Create(0, 0, 40, 40);

			using var nested = CreateTestPicture();

			using var recorder = new SKPictureRecorder();
			var recCanvas = recorder.BeginRecording(cullRect);
			recCanvas.DrawPicture(nested);
			using var original = recorder.EndRecording();

			var originalOpCount = original.ApproximateOperationCount;
			var originalOpCountWithNested = original.GetApproximateOperationCount(true);

			using var data = original.Serialize();
			Assert.NotNull(data);

			using var deserialized = SKPicture.Deserialize(data);
			Assert.NotNull(deserialized);

			// CullRect parity (exact — CullRect is round-tripped through the format).
			Assert.Equal(original.CullRect, deserialized.CullRect);
			Assert.Equal(cullRect, deserialized.CullRect);

			// Approximate operation-count parity for both the shallow and nested variants.
			// Cross-milestone-safe: assert equality between original and deserialized,
			// not a specific magic number that could shift with internal storage changes.
			Assert.Equal(originalOpCount, deserialized.ApproximateOperationCount);
			Assert.Equal(originalOpCountWithNested, deserialized.GetApproximateOperationCount(true));

			// The nested variant must not be smaller than the shallow variant.
			Assert.True(deserialized.GetApproximateOperationCount(true) >= deserialized.ApproximateOperationCount);

			// ApproximateBytesUsed is internal storage — only assert positivity, never an
			// exact value: the M154 refactor may shift internal byte accounting.
			Assert.True(deserialized.ApproximateBytesUsed > 0);

			// Raster playback parity: the deserialized picture must reproduce the same
			// pixels as the nested test bitmap.
			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);
			deserialized.Playback(cnv);

			ValidateTestBitmap(bmp);
		}

		[Fact]
		public void EncodesImageIntoPicture()
		{
			// create an image
			using var sourceBitmap = CreateTestBitmap();

			// create a picture that has an image in it
			using var picRecorder = new SKPictureRecorder();
			using var picCanvas = picRecorder.BeginRecording(SKRect.Create(0, 0, 40, 40));
			picCanvas.DrawBitmap(sourceBitmap, 0, 0);
			using var picture = picRecorder.EndRecording();

			// serialize and then deserialize the picture
			using var serialized = picture.Serialize();
			using var deserialized = SKPicture.Deserialize(serialized);

			// draw the picture into a new bitmap
			using var desBitmap = new SKBitmap(40, 40);
			using var destCanvas = new SKCanvas(desBitmap);
			destCanvas.DrawPicture(deserialized);

			// make sure the bitmap made it through the serialization
			ValidateTestBitmap(desBitmap);
		}
	}
}
