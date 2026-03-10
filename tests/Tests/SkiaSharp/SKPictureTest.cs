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

		[SkippableFact]
		public void CanSerializeToData()
		{
			using var picture = CreateTestPicture();

			using var data = picture.Serialize();
			Assert.NotNull(data);

			var span = data.AsSpan();
			Assert.True(span.Length > 8);
			Assert.Equal(MagicBytes, span.Slice(0, 8).ToArray());
		}

		[SkippableFact]
		public void CanSerializeToStream()
		{
			using var picture = CreateTestPicture();

			using var stream = new MemoryStream();
			picture.Serialize(stream);

			Assert.True(stream.Length > 8);
			Assert.Equal(MagicBytes, stream.ToArray().Take(8));
		}

		[SkippableFact]
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

		[SkippableFact]
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

		[SkippableFact]
		public void CanPlayback()
		{
			using var picture = CreateTestPicture();

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			picture.Playback(cnv);

			ValidateTestBitmap(bmp);
		}

		[SkippableFact]
		public void CanDrawPicture()
		{
			using var picture = CreateTestPicture();

			using var bmp = new SKBitmap(40, 40);
			using var cnv = new SKCanvas(bmp);

			cnv.DrawPicture(picture);

			ValidateTestBitmap(bmp);
		}

		[SkippableFact]
		public void CanGetApproximateOperationCount()
		{
			using var picture = CreateTestPicture();

			Assert.Equal(5, picture.ApproximateOperationCount);
		}

		[SkippableFact]
		public void CanGetApproximateBytesUsed()
		{
			using var picture = CreateTestPicture();

			Assert.True(picture.ApproximateBytesUsed > 0);
		}

		[SkippableFact]
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
