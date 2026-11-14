using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPathMeasureTest : SKTest
	{
		[Fact]
		public void ConstructorThrowsOnNullPathArgument()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => new SKPathMeasure(null));
			Assert.Equal("path", ex.ParamName);
		}

		[Fact]
		public void ConstructorDoesNotThrownOnNonNullPathArgument()
		{
			var path = new SKPath();
			var pm = new SKPathMeasure(path);
			Assert.NotNull(pm);
		}

		[Fact]
		public void EmptyConstructorDoesNotThrow()
		{
			var pm = new SKPathMeasure();
			Assert.NotNull(pm);
		}

		[Fact]
		public void PathRemainsAliveForMeasureLifetime()
		{
			using var measure = CreatePathMeasure(out var path);

			CollectGarbage();

			Assert.True(path.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static SKPathMeasure CreatePathMeasure(out WeakReference pathReference)
		{
			var path = new SKPath();
			path.LineTo(100, 100);

			var measure = new SKPathMeasure(path);
			pathReference = new WeakReference(path);
			return measure;
		}

	}
}
