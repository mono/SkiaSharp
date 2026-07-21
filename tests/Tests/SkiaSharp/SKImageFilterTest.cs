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

		[Fact]
		public void CropFilterReturnsNonNull()
		{
			var rect = new SKRect(10, 10, 100, 100);
			using var filter = SKImageFilter.CreateCrop(rect);
			Assert.NotNull(filter);
		}

		[Fact]
		public void CropFilterWithTileModeReturnsNonNull()
		{
			var rect = new SKRect(10, 10, 100, 100);
			using var filter = SKImageFilter.CreateCrop(rect, SKShaderTileMode.Clamp);
			Assert.NotNull(filter);
		}

		[Fact]
		public void CropFilterAcceptsNullInput()
		{
			var rect = new SKRect(10, 10, 100, 100);
			using var filter = SKImageFilter.CreateCrop(rect, SKShaderTileMode.Decal, null);
			Assert.NotNull(filter);
		}

		[Fact]
		public void CropFilterComposesWithBlur()
		{
			using var blur = SKImageFilter.CreateBlur(5, 5);
			var rect = new SKRect(10, 10, 100, 100);
			using var crop = SKImageFilter.CreateCrop(rect, SKShaderTileMode.Decal, blur);
			Assert.NotNull(crop);
		}

		[Fact]
		public void CropFilterWithDifferentTileModes()
		{
			using var bitmap = new SKBitmap(200, 200);
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint();
	
			var cropRect = new SKRect(50, 50, 150, 150);
	
			// Test Decal mode - outside crop rect should be transparent (background color preserved)
			canvas.Clear(SKColors.Blue);
			using (var decal = SKImageFilter.CreateCrop(cropRect, SKShaderTileMode.Decal, null))
			{
				paint.ImageFilter = decal;
				paint.Color = SKColors.Red;
				canvas.DrawRect(new SKRect(0, 0, 200, 200), paint);
			}
			Assert.Equal(SKColors.Blue, bitmap.GetPixel(10, 10)); // Outside = blue (Decal transparency)
			Assert.Equal(SKColors.Red, bitmap.GetPixel(100, 100)); // Inside = red
	
			// Test Clamp mode - edges should extend beyond crop rect
			canvas.Clear(SKColors.Blue);
			using (var clamp = SKImageFilter.CreateCrop(cropRect, SKShaderTileMode.Clamp, null))
			{
				paint.ImageFilter = clamp;
				paint.Color = SKColors.Green;
				canvas.DrawRect(new SKRect(0, 0, 200, 200), paint);
			}
			Assert.Equal(SKColors.Green, bitmap.GetPixel(10, 10)); // Outside = green (Clamp extends)
			Assert.Equal(SKColors.Green, bitmap.GetPixel(100, 100)); // Inside = green
	
			// Test Repeat mode - creates non-null filter
			using var repeat = SKImageFilter.CreateCrop(cropRect, SKShaderTileMode.Repeat, null);
			Assert.NotNull(repeat);
	
			// Test Mirror mode - creates non-null filter
			using var mirror = SKImageFilter.CreateCrop(cropRect, SKShaderTileMode.Mirror, null);
			Assert.NotNull(mirror);
		}

		[Fact]
		public void CropFilterRendersCorrectly()
		{
			using var bitmap = new SKBitmap(200, 200);
			using var canvas = new SKCanvas(bitmap);
			using var paint = new SKPaint();
	
			canvas.Clear(SKColors.White);
	
			// Create a crop rect that only includes the center 100x100 area
			var cropRect = new SKRect(50, 50, 150, 150);
			using var crop = SKImageFilter.CreateCrop(cropRect, SKShaderTileMode.Decal, null);
			paint.ImageFilter = crop;
			paint.Color = SKColors.Red;
	
			// Draw a large rect, but it should be cropped
			canvas.DrawRect(new SKRect(0, 0, 200, 200), paint);
	
			// Check that pixels outside the crop rect are white (transparent/not drawn)
			Assert.Equal(SKColors.White, bitmap.GetPixel(25, 25));  // Top-left, outside crop
			Assert.Equal(SKColors.White, bitmap.GetPixel(175, 175));  // Bottom-right, outside crop
	
			// Pixels inside the crop rect should be red
			Assert.Equal(SKColors.Red, bitmap.GetPixel(100, 100));  // Center, inside crop
		}
	}
}
