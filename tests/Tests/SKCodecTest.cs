using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKCodecTest : SKTest
	{
		[Test]
		public void MinBufferedBytesNeededHasAValue ()
		{
			Assert.IsTrue (SKCodec.MinBufferedBytesNeeded > 0);
		}

		[Test]
		public void CanCreateStreamCodec ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "color-wheel.png"));
			using (var codec = SKCodec.Create (stream)) {
				Assert.AreEqual (SKEncodedFormat.Png, codec.EncodedFormat);
				Assert.AreEqual (128, codec.Info.Width);
				Assert.AreEqual (128, codec.Info.Height);
				Assert.AreEqual (SKAlphaType.Unpremul, codec.Info.AlphaType);
				if (IsUnix) {
					Assert.AreEqual (SKColorType.Rgba8888, codec.Info.ColorType);
				} else {
					Assert.AreEqual (SKColorType.Bgra8888, codec.Info.ColorType);
				}
			}
		}

		[Test]
		public void CanGetPixels ()
		{
			var stream = new SKFileStream (Path.Combine (PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create (stream)) {
				var pixels = codec.Pixels;
				Assert.AreEqual (codec.Info.BytesSize, pixels.Length);
			}
		}

		[Test]
		public void BitmapDecodesCorrectly ()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			using (var codec = SKCodec.Create (new SKFileStream (Path.Combine (PathToImages, "baboon.png")))) {
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode (Path.Combine (PathToImages, "baboon.png"))) {
				bitmapPixels = bitmap.Bytes;
			}

			CollectionAssert.AreEqual (codecPixels, bitmapPixels);
		}

		[Test]
		public void BitmapDecodesCorrectlyWithManagedStream ()
		{
			byte[] codecPixels;
			byte[] bitmapPixels;

			var stream = File.OpenRead (Path.Combine(PathToImages, "baboon.png"));
			using (var codec = SKCodec.Create (new SKManagedStream (stream))) {
				codecPixels = codec.Pixels;
			}

			using (var bitmap = SKBitmap.Decode (Path.Combine (PathToImages, "baboon.png"))) {
				bitmapPixels = bitmap.Bytes;
			}

			CollectionAssert.AreEqual (codecPixels, bitmapPixels);
		}
	}
}
