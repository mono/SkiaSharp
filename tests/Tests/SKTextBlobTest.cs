using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKTextBlobTest : SKTest
	{
		[SkippableFact]
		public void TestEmptyBuilderReturnsNull()
		{
			var builder = new SKTextBlobBuilder();

			var blob = builder.Build();

			Assert.Null(blob);
		}

		[SkippableFact]
		public void TestExplicitBounds()
		{
			var builder = new SKTextBlobBuilder();
			var font = new SKPaint();
			font.TextEncoding = SKTextEncoding.GlyphId;

			{
				var blob = builder.Build();
				Assert.Null(blob);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocateRun(font, 16, 0, 0, 0, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocateRunHorizontal(font, 16, 0, 0, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocateRunPositioned(font, 16, 0, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				var r2 = SKRect.Create(15, 20, 50, 50);
				var r3 = SKRect.Create(0, 5, 10, 5);

				builder.AllocateRun(font, 16, 0, 0, 0, r1);
				builder.AllocateRunHorizontal(font, 16, 0, 0, r2);
				builder.AllocateRunPositioned(font, 16, 0, r3);

				var blob = builder.Build();
				Assert.Equal(SKRect.Create(0, 5, 65, 65), blob.Bounds);
			}

			{
				var blob = builder.Build();
				Assert.Null(blob);
			}
		}

		[SkippableFact]
		public void TestImplicitBounds()
		{
			var builder = new SKTextBlobBuilder();

			var font = new SKPaint();
			font.TextSize = 0;

			var txt = "BOOO";
			var glyphs = font.GetGlyphs(txt);

			font.TextEncoding = SKTextEncoding.GlyphId;
			builder.AddPositionedRun(font, glyphs, new SKPoint[glyphs.Length]);

			var blob = builder.Build();
			Assert.True(blob.Bounds.IsEmpty);
		}
	}
}
