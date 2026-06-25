using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageFilterTest : SKTest
	{
		[Fact]
		public void MergeFilterAcceptsNullFilterArray()
		{
			var filter = SKImageFilter.CreateMerge(new SKImageFilter[] { null });
			Assert.NotNull(filter);
		}

		[Fact]
		public void MergeFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateMerge((SKImageFilter)null, null);
			Assert.NotNull(filter);
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void ShaderFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateShader(null);
			Assert.NotNull(filter);
		}

		[Fact]
		public void CreateEmptyReturnsNonNull()
		{
			using var filter = SKImageFilter.CreateEmpty();
			Assert.NotNull(filter);
		}

		[Fact]
		public void CreateEmptyReturnsNewInstanceEachTime()
		{
			using var filter1 = SKImageFilter.CreateEmpty();
			using var filter2 = SKImageFilter.CreateEmpty();
			Assert.NotNull(filter1);
			Assert.NotNull(filter2);
			Assert.NotEqual(filter1.Handle, filter2.Handle);
		}

		[Fact]
		public void CreateEmptyCanBeComposedWithOtherFilters()
		{
			using var empty = SKImageFilter.CreateEmpty();
			using var blur = SKImageFilter.CreateBlur(5, 5);
			using var composed = SKImageFilter.CreateCompose(blur, empty);
			Assert.NotNull(composed);
		}

		[Fact]
		public void CreateEmptyCanBeUsedAsInputToCompose()
		{
			using var empty1 = SKImageFilter.CreateEmpty();
			using var empty2 = SKImageFilter.CreateEmpty();
			using var composed = SKImageFilter.CreateCompose(empty1, empty2);
			Assert.NotNull(composed);
		}

		[Fact]
		public void CreateEmptyDiscardsDrawnContent()
		{
			const int size = 64;
			var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
			var rect = new SKRect(10, 10, 50, 50);

			// Draws a red rectangle on a blue background using the given image
			// filter and returns the resulting pixel at the rectangle's center.
			// The surface is explicitly cleared to blue so the result never relies
			// on the surface being zero-initialized.
			SKColor RenderCenterPixel(SKImageFilter filter)
			{
				using var surface = SKSurface.Create(info);
				Assert.NotNull(surface);

				surface.Canvas.Clear(SKColors.Blue);

				using var paint = new SKPaint
				{
					Color = SKColors.Red,
					ImageFilter = filter
				};
				surface.Canvas.DrawRect(rect, paint);

				using var image = surface.Snapshot();
				Assert.NotNull(image);
				using var pixmap = image.PeekPixels();
				Assert.NotNull(pixmap);

				return pixmap.GetPixelColor(32, 32);
			}

			// Without a filter the red rectangle is drawn normally, proving the
			// draw itself produces visible content.
			Assert.Equal(SKColors.Red, RenderCenterPixel(null));

			// With the empty filter the red rectangle is replaced by transparent
			// black, so the blue background shows through unchanged.
			using var emptyFilter = SKImageFilter.CreateEmpty();
			Assert.Equal(SKColors.Blue, RenderCenterPixel(emptyFilter));
		}
	}
}
