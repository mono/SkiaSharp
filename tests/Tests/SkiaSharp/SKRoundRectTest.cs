using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKRoundRectTest : SKTest
	{
		[SkippableFact]
		public void CanConstructEmpty()
		{
			var rrect = new SKRoundRect();

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Empty, rrect.Type);

			Assert.Equal(0f, rrect.Width);
			Assert.Equal(0f, rrect.Height);
			Assert.Equal(SKRect.Empty, rrect.Rect);
		}

		[SkippableFact]
		public void CanConstructRect()
		{
			var rect = SKRect.Create(10, 10, 100, 100);

			var rrect = new SKRoundRect(rect);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Rect, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);
		}

		[SkippableFact]
		public void CanConstructOval()
		{
			var rect = SKRect.Create(10, 10, 100, 100);

			var rrect = new SKRoundRect();
			rrect.SetOval(rect);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Oval, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);
		}

		[SkippableFact]
		public void CanConstructSimple()
		{
			var rect = SKRect.Create(10, 10, 100, 100);

			var rrect = new SKRoundRect(rect, 5, 5);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Simple, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);
		}

		[SkippableFact]
		public void CanConstructNinePatch()
		{
			var rect = SKRect.Create(10, 10, 100, 100);

			var rrect = new SKRoundRect();
			rrect.SetNinePatch(rect, 5, 10, 5, 5);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.NinePatch, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);
		}

		[SkippableFact]
		public void CanNotConstructRectWithInvalidRadii()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var radii = new[]
			{
				new SKPoint(1, 2),
				new SKPoint(3, 4)
			};

			var rrect = new SKRoundRect();
			Assert.Throws<ArgumentException>(() => rrect.SetRectRadii(rect, radii));
		}

		[SkippableFact]
		public void CanConstructRectWithRadii()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var radii = new[]
			{
				new SKPoint(1, 2),
				new SKPoint(3, 4),
				new SKPoint(5, 6),
				new SKPoint(7, 8)
			};

			var rrect = new SKRoundRect();
			rrect.SetRectRadii(rect, radii);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Complex, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);

			Assert.Equal(radii[0], rrect.GetRadii(SKRoundRectCorner.UpperLeft));
			Assert.Equal(radii[1], rrect.GetRadii(SKRoundRectCorner.UpperRight));
			Assert.Equal(radii[2], rrect.GetRadii(SKRoundRectCorner.LowerRight));
			Assert.Equal(radii[3], rrect.GetRadii(SKRoundRectCorner.LowerLeft));
		}

		[SkippableFact]
		public void CanCopy()
		{
			var rect = SKRect.Create(10, 10, 100, 100);

			var original = new SKRoundRect(rect, 5, 5);
			var rrect = new SKRoundRect(original);

			Assert.NotNull(rrect);
			Assert.True(rrect.IsValid);

			Assert.Equal(SKRoundRectType.Simple, rrect.Type);

			Assert.Equal(100f, rrect.Width);
			Assert.Equal(100f, rrect.Height);
			Assert.Equal(rect, rrect.Rect);
		}

		[SkippableFact]
		public void ContainsRect()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var rrect = new SKRoundRect(rect, 5, 5);

			var contains = rrect.Contains(SKRect.Create(20, 20, 20, 20));
			Assert.True(contains);
		}

		[SkippableFact]
		public void CanGetRadii()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var rrect = new SKRoundRect(rect, 5, 7);

			Assert.Equal(new SKPoint(5, 7), rrect.GetRadii(SKRoundRectCorner.UpperLeft));
			Assert.Equal(new SKPoint(5, 7), rrect.GetRadii(SKRoundRectCorner.UpperRight));
			Assert.Equal(new SKPoint(5, 7), rrect.GetRadii(SKRoundRectCorner.LowerRight));
			Assert.Equal(new SKPoint(5, 7), rrect.GetRadii(SKRoundRectCorner.LowerLeft));
		}

		[SkippableFact]
		public void CheckAllCornersCircular()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var rrect = new SKRoundRect(rect, 5, 5);

			Assert.True(rrect.AllCornersCircular);
		}

		[SkippableFact]
		public void CheckAllCornersCircularWithTolerance()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var rrect = new SKRoundRect(rect, 5, 7);

			Assert.False(rrect.AllCornersCircular);
			Assert.True(rrect.CheckAllCornersCircular(2f));
		}

		[SkippableFact]
		public void CanInflate()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var inflated = SKRect.Inflate(rect, 2, 2);

			var rrect = new SKRoundRect(rect, 5, 5);
			rrect.Inflate(2, 2);

			Assert.Equal(inflated, rrect.Rect);
		}

		[SkippableFact]
		public void CanOffset()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var offset = rect;
			offset.Offset(2, 2);

			var rrect = new SKRoundRect(rect, 5, 5);
			rrect.Offset(2, 2);

			Assert.Equal(offset, rrect.Rect);
		}

		[SkippableFact]
		public void WillFailToTransformWithInvalidTransformation()
		{
			var rect = SKRect.Create(10, 10, 100, 100);
			var offset = rect;
			offset.Offset(2, 2);

			var rrect = new SKRoundRect(rect, 5, 5);
			var transformed = rrect.Transform(SKMatrix.CreateRotationDegrees(30));

			Assert.Null(transformed);
		}

		[SkippableFact]
		public void CanTransform()
		{
			var radii = new[] { new SKPoint(5, 5), new SKPoint(5, 5), new SKPoint(5, 5), new SKPoint(5, 5) };

			var rect = SKRect.Create(10, 10, 100, 100);
			var offset = rect;
			offset.Offset(2, 2);

			var rrect = new SKRoundRect(rect, 5, 5);
			var transformed = rrect.Transform(SKMatrix.CreateTranslation(2, 2));

			Assert.Equal(offset, transformed.Rect);
			Assert.Equal(radii, transformed.Radii);
		}
	}
}
