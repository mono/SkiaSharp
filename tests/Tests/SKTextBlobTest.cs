using System;
using System.Runtime.InteropServices;
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
			var font = new SKFont();

			{
				var blob = builder.Build();
				Assert.Null(blob);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocateRun(font, 16, 0, 0, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocateHorizontalRun(font, 16, 0, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				builder.AllocatePositionedRun(font, 16, r1);
				var blob = builder.Build();
				Assert.Equal(r1, blob.Bounds);
			}

			{
				var r1 = SKRect.Create(10, 10, 20, 20);
				var r2 = SKRect.Create(15, 20, 50, 50);
				var r3 = SKRect.Create(0, 5, 10, 5);

				builder.AllocateRun(font, 16, 0, 0, r1);
				builder.AllocateHorizontalRun(font, 16, 0, r2);
				builder.AllocatePositionedRun(font, 16, r3);

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

			var font = new SKFont();
			font.Size = 0;

			var txt = "BOOO";
			var glyphs = font.GetGlyphs(txt);

			builder.AddPositionedRun(glyphs, font, new SKPoint[glyphs.Length]);

			var blob = builder.Build();
			Assert.True(blob.Bounds.IsEmpty);
		}

		[SkippableFact]
		public unsafe void TestPositionedRunIsBothPointsAndFloats()
		{
			var font = new SKFont();

			var builder = new SKTextBlobBuilder();
			var run = builder.AllocatePositionedRun(font, 3);

			var positions = new[] { new SKPoint(1, 2), new SKPoint(3, 4), new SKPoint(5, 6) };
			var positionsRaw = new float[] { 1, 2, 3, 4, 5, 6 };

			run.SetPositions(positions);

			var span = run.GetPositionSpan();
			Assert.Equal(positions, span.ToArray());

			var floats = new float[6];
			Marshal.Copy((IntPtr)run.internalBuffer.pos, floats, 0, 6);
			Assert.Equal(positionsRaw, floats);
		}
	}
}
