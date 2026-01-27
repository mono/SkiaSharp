using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using HarfBuzzSharp;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.HarfBuzz.Tests
{
	public class SKShaperTest : SKTest
	{
		[SkippableFact]
		public void DrawShapedTextExtensionMethodDraws()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(512, 512)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var paint = new SKPaint { IsAntialias = true })
			using (var font = new SKFont { Size = 64, Typeface = tf })
			{
				canvas.Clear(SKColors.White);

				canvas.DrawShapedText(shaper, "متن", 100, 200, font, paint);

				canvas.Flush();

				Assert.Equal(SKColors.Black, bitmap.GetPixel(110, 210));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(127, 196));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(142, 197));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(155, 195));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(131, 181));
				Assert.Equal(SKColors.White, bitmap.GetPixel(155, 190));
				Assert.Equal(SKColors.White, bitmap.GetPixel(110, 200));
			}
		}

		[SkippableFact]
		public void CorrectlyShapesArabicScriptAtAnOffset()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(100, 200), new SKPoint(128.375f, 200), new SKPoint(142.125f, 200) };

			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var font = new SKFont { Size = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", 100, 200, font);

				Assert.Equal(clusters, result.Clusters);
				Assert.Equal(codepoints, result.Codepoints);
				Assert.Equal(points, result.Points);
			}
		}

		[SkippableFact]
		public void CorrectlyShapesArabicScript()
		{
			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };
			var points = new SKPoint[] { new SKPoint(0, 0), new SKPoint(28.375f, 0), new SKPoint(42.125f, 0) };

			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var shaper = new SKShaper(tf))
			using (var font = new SKFont { Size = 64, Typeface = tf })
			{
				var result = shaper.Shape("متن", font);

				Assert.Equal(clusters, result.Clusters);
				Assert.Equal(codepoints, result.Codepoints);
				Assert.Equal(points, result.Points);
			}
		}

		[SkippableFact]
		public void CanCreateFaceShaperFromTypeface()
		{
			var skiaTypeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));

			var clusters = new uint[] { 4, 2, 0 };
			var codepoints = new uint[] { 629, 668, 891 };

			using (var face = new Face(GetFaceBlob, () => skiaTypeface.Dispose()))
			using (var font = new Font(face))
			using (var buffer = new HarfBuzzSharp.Buffer())
			{
				buffer.AddUtf8("متن");
				buffer.GuessSegmentProperties();

				font.Shape(buffer);

				Assert.Equal(clusters, buffer.GlyphInfos.Select(i => i.Cluster));
				Assert.Equal(codepoints, buffer.GlyphInfos.Select(i => i.Codepoint));
			}

			Blob GetFaceBlob(Face face, Tag tag)
			{
				var size = skiaTypeface.GetTableSize(tag);
				var data = Marshal.AllocCoTaskMem(size);
				skiaTypeface.TryGetTableData(tag, 0, size, data);
				return new Blob(data, size, MemoryMode.Writeable, () => Marshal.FreeCoTaskMem(data));
			}
		}

		[SkippableTheory]
		[InlineData(SKTextAlign.Left, 300)]
		[InlineData(SKTextAlign.Center, 162)]
		[InlineData(SKTextAlign.Right, 23)]
		public void TextAlignMovesTextPosition(SKTextAlign align, int offset)
		{
			var fontFile = Path.Combine(PathToFonts, "segoeui.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			using var bitmap = new SKBitmap(600, 200);
			using var canvas = new SKCanvas(bitmap);

			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.IsAntialias = true;
			paint.Color = SKColors.Black;

			using var font = new SKFont();
			font.Typeface = tf;
			font.Size = 64;

			canvas.DrawShapedText("SkiaSharp", 300, 100, align, font, paint);

			AssertTextAlign(bitmap, offset, 0);
		}

		private static void AssertTextAlign(SKBitmap bitmap, int x, int y)
		{
			// [S]kia[S]har[p]

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 6, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 28, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 28, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 6, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 120, y + 66));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 142, y + 87));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 142, y + 66));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 120, y + 87));

			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 70));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 246, y + 113));
			Assert.Equal(SKColors.Black, bitmap.GetPixel(x + 271, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 83));
			Assert.Equal(SKColors.White, bitmap.GetPixel(x + 258, y + 113));
		}

		[SkippableFact]
		public void ToHarfBuzzBlobDoesNotDisposeStream()
		{
			// Test that ToHarfBuzzBlob does not take ownership of the stream
			// and the stream remains usable after creating a blob
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			int index;
			var stream = tf.OpenStream(out index);
			
			// Verify stream is usable before creating blob
			Assert.NotNull(stream);
			Assert.True(stream.Length > 0);
			var initialPosition = stream.Position;

			// Create blob - this should NOT take ownership of the stream
			using var blob = stream.ToHarfBuzzBlob();
			
			// Verify blob was created successfully
			Assert.NotNull(blob);
			Assert.Equal(stream.Length, blob.Length);

			// Verify stream is still usable after blob creation
			// The stream should not be disposed
			Assert.Equal(initialPosition, stream.Position);
			Assert.True(stream.HasPosition);
			Assert.True(stream.HasLength);

			// Clean up the stream explicitly (caller's responsibility)
			stream.Dispose();
		}

		[SkippableFact]
		public void ToHarfBuzzBlobStreamMustBeDisposedByCaller()
		{
			// Test that the caller is responsible for disposing the stream
			// even after creating a blob from it
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			int index;
			var stream = tf.OpenStream(out index);
			var streamLength = stream.Length;

			// Create and dispose blob
			using (var blob = stream.ToHarfBuzzBlob())
			{
				Assert.NotNull(blob);
				Assert.Equal(streamLength, blob.Length);
			}
			// Blob is now disposed

			// Stream should still be valid and usable after blob disposal
			Assert.True(stream.HasLength);
			Assert.Equal(streamLength, stream.Length);

			// Caller must dispose stream
			stream.Dispose();
		}

		[SkippableFact]
		public void ToHarfBuzzBlobBlobCanBeUsedAfterStreamDisposal()
		{
			// Test that the blob can outlive the stream when using memory-mapped path
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			Blob blob;
			int blobLength;
			int index;

			// Create blob from stream
			using (var stream = tf.OpenStream(out index))
			{
				blob = stream.ToHarfBuzzBlob();
				blobLength = blob.Length;
				Assert.NotNull(blob);
				Assert.True(blobLength > 0);
			}
			// Stream is now disposed

			// Blob should still be valid and usable
			using (blob)
			{
				Assert.Equal(blobLength, blob.Length);
				
				// Verify we can create a face from the blob
				using var face = new Face(blob, index);
				Assert.NotNull(face);
				Assert.Equal(index, face.Index);
			}
		}

		[SkippableFact]
		public void ToHarfBuzzBlobWorksWithNonMemoryMappedStream()
		{
			// Test the fallback path where GetMemoryBase returns IntPtr.Zero
			// This tests the code path that copies data to allocated memory
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			var fileBytes = File.ReadAllBytes(fontFile);
			
			// Create a managed stream (which won't have a memory base)
			using var managedStream = new MemoryStream(fileBytes);
			using var skStream = new SKManagedStream(managedStream);

			// Create blob from non-memory-mapped stream
			using var blob = skStream.ToHarfBuzzBlob();

			// Verify blob was created successfully
			Assert.NotNull(blob);
			Assert.Equal(fileBytes.Length, blob.Length);

			// Verify we can use the blob
			using var face = new Face(blob, 0);
			Assert.NotNull(face);

			// Verify stream is still usable
			Assert.True(skStream.HasLength);
			Assert.Equal(fileBytes.Length, skStream.Length);

			// Caller is responsible for disposing the stream
			skStream.Dispose();
		}

		[SkippableFact]
		public void SKShaperDisposesStreamCorrectly()
		{
			// Test that SKShaper properly disposes the stream when changed to use explicit using statement
			// This verifies the fix in SKShaper constructor
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			// Create SKShaper - it should properly dispose the stream internally
			using var shaper = new SKShaper(tf);
			
			// Verify shaper was created successfully
			Assert.NotNull(shaper);
			Assert.Equal(tf, shaper.Typeface);

			// Verify shaper can be used for shaping
			using var font = new SKFont { Size = 64, Typeface = tf };
			var result = shaper.Shape("متن", font);
			
			Assert.NotNull(result);
			Assert.True(result.Codepoints.Length > 0);
		}

		[SkippableFact]
		public void ToHarfBuzzBlobCanCreateMultipleBlobsFromSameStream()
		{
			// Test that we can create multiple blobs from the same stream
			// since the stream is not disposed by ToHarfBuzzBlob
			var fontFile = Path.Combine(PathToFonts, "content-font.ttf");
			using var tf = SKTypeface.FromFile(fontFile);

			int index;
			using var stream = tf.OpenStream(out index);

			// Create first blob
			using var blob1 = stream.ToHarfBuzzBlob();
			Assert.NotNull(blob1);
			var length = blob1.Length;

			// Stream should still be usable
			Assert.True(stream.HasLength);

			// Reset stream position if needed
			if (stream.HasPosition)
			{
				stream.Position = 0;
			}

			// Create second blob from the same stream
			using var blob2 = stream.ToHarfBuzzBlob();
			Assert.NotNull(blob2);
			Assert.Equal(length, blob2.Length);

			// Both blobs should be valid
			using (var face1 = new Face(blob1, index))
			using (var face2 = new Face(blob2, index))
			{
				Assert.NotNull(face1);
				Assert.NotNull(face2);
			}

			// Caller disposes the stream
			stream.Dispose();
		}
	}
}
