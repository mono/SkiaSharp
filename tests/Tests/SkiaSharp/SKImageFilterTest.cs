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
		public void CreateEmptyCanBeUsedInRendering()
		{
			const int size = 64;
			var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);

			using var surface = SKSurface.Create(info);
			Assert.NotNull(surface);

			using var emptyFilter = SKImageFilter.CreateEmpty();
			using var paint = new SKPaint
			{
				ImageFilter = emptyFilter
			};

			// Use SaveLayer - image filters are applied to layer content
			surface.Canvas.SaveLayer(paint);
			surface.Canvas.Clear(SKColors.Red); // Fill layer with red
			surface.Canvas.Restore();

			using var image = surface.Snapshot();
			Assert.NotNull(image);
			using var pixmap = image.PeekPixels();
			Assert.NotNull(pixmap);
		
			// Empty filter should make the entire layer transparent black
			var pixel = pixmap.GetPixelColor(32, 32);
			Assert.Equal(new SKColor(0, 0, 0, 0), pixel);
		}
	}
}
