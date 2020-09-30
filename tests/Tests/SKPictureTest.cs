using System.IO;
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
			Assert.Equal(MagicBytes, span[0..8].ToArray());
		}

		[SkippableFact]
		public void CanSerializeToStream()
		{
			using var picture = CreateTestPicture();

			using var stream = new MemoryStream();
			picture.Serialize(stream);

			Assert.True(stream.Length > 8);
			Assert.Equal(MagicBytes, stream.ToArray()[0..8]);
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
	}
}
