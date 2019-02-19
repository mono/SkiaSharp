using System.IO;

namespace SkiaSharp.Tests
{
	using System;
	using System.Text;

	using HarfBuzzSharp;

	using SkiaSharp.HarfBuzz;

	using Xunit;

	using Buffer = HarfBuzzSharp.Buffer;

	public class HbBufferTests : SKTest
	{
		private static string SimpleText = "1234";

		[SkippableFact]
		public void ShouldHaveCorrectContentType()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			using (var buffer = new Buffer())
			{
				Assert.Equal(ContentType.Invalid, buffer.ContentType);

				buffer.AddUtf8(SimpleText);

				Assert.Equal(ContentType.Unicode, buffer.ContentType);

				font.Shape(buffer);

				Assert.Equal(ContentType.Glyphs, buffer.ContentType);
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultStateAfterReset()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8(SimpleText);

				buffer.Reset();

				Assert.Equal(ContentType.Invalid, buffer.ContentType);

				Assert.Equal(0, buffer.Length);
			}
		}

		[SkippableFact]
		public void ShouldContainNoGlyphInfoAfterClearContents()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8(SimpleText);

				Assert.Equal(SimpleText.Length, buffer.GlyphInfos.Length);

				buffer.ClearContents();

				Assert.Empty(buffer.GlyphInfos);
			}
		}

		[SkippableFact]
		public void ShouldAddUtfByString()
		{
			using (var buffer = new Buffer())
			{
				buffer.AddUtf8("A");

				Assert.Equal(1, buffer.Length);

				buffer.AddUtf8("B");

				Assert.Equal(2, buffer.Length);

				buffer.AddUtf8("C");

				Assert.Equal(3, buffer.Length);
			}
		}

		[SkippableFact]
		public void ShouldAddUtfBySpan()
		{
			using (var buffer = new Buffer())
			{
				var utf8 = Encoding.UTF8.GetBytes("A").AsSpan();

				buffer.AddUtf8(utf8);

				Assert.Equal(1, buffer.Length);

				var utf16 = "b".AsSpan();

				buffer.AddUtf16(utf16);

				Assert.Equal(2, buffer.Length);

				var utf32 = new[] { char.ConvertToUtf32("C", 0) }.AsSpan();

				buffer.AddUtf32(utf32);

				Assert.Equal(3, buffer.Length);
			}
		}
	}
}
