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
			var filter = SKImageFilter.CreateEmpty();
			Assert.NotNull(filter);
		}

		[Fact]
		public void CreateEmptyReturnsNewInstanceEachTime()
		{
			var filter1 = SKImageFilter.CreateEmpty();
			var filter2 = SKImageFilter.CreateEmpty();
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
	}
}
