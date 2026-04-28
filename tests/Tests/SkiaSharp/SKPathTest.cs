using System;
using System.Collections.Generic;
using Xunit;

using ParsePathTest = System.Tuple<System.String, SkiaSharp.SKRect>;

namespace SkiaSharp.Tests
{
	public class SKPathTest : SKTest
	{
		private static readonly List<ParsePathTest> parsePathTestCases = new List<ParsePathTest> {
			new ParsePathTest (
				"M1,1 l-2.58-2.828-3.82-0.113, 1.9-3.3223-1.08-3.6702, 3.75,0.7744,3.16-2.1551,0.42,3.8008,3.02,2.3384-3.48,1.574-1.29,3.601z",
				new SKRect (-5.39999962f, -10.3142f, 5.77000046f, 1.0f)),
			new ParsePathTest (
				"",
				new SKRect (0, 0, 0, 0)),
			new ParsePathTest (
				"M0,0L10,10",
				new SKRect (0, 0, 10f, 10f)),
			new ParsePathTest (
				"M-5.5,-0.5 Q 0 0 6,6.50",
				new SKRect (-5.5f, -0.5f, 6, 6.5f)),
		};

		private static void TestToFromSvgPath(SKPath path)
		{
			var str = path.ToSvgPathData();
			Assert.NotNull(str);

			var path2 = SKPath.ParseSvgPathData(str);
			Assert.NotNull(path2);

			var str2 = path2.ToSvgPathData();
			Assert.NotNull(str2);

			Assert.Equal(str, str2);
		}

		[SkippableFact]
		public void PathPointsAreCorrect()
		{
			using var builder = new SKPathBuilder();
			// set up the xamagon
			builder.MoveTo(71.4311121f, 56f);
			builder.CubicTo(68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
			builder.LineTo(43.0238921f, 97.5342563f);
			builder.CubicTo(41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
			builder.LineTo(64.5928855f, 143.034271f);
			builder.CubicTo(65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
			builder.LineTo(114.568946f, 147f);
			builder.CubicTo(117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
			builder.LineTo(142.976161f, 105.465744f);
			builder.CubicTo(144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
			builder.LineTo(121.407172f, 59.965729f);
			builder.CubicTo(120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
			builder.LineTo(71.4311121f, 56f);
			builder.Close();
			using var path = builder.Detach();

			// the right number/count
			Assert.Equal(25, path.PointCount);
			Assert.Equal(25, path.Points.Length);

			// the right value
			Assert.Equal(new SKPoint(68.6763107f, 56.0058575f), path.GetPoint(1));
			Assert.Equal(new SKPoint(68.6763107f, 56.0058575f), path.Points[1]);

			// the right segment masks
			Assert.Equal(SKPathSegmentMask.Cubic | SKPathSegmentMask.Line, path.SegmentMasks);
		}

		[SkippableFact]
		public void PathContainsPoint()
		{
			using var builder = new SKPathBuilder();
			builder.AddRect(SKRect.Create(10, 10, 100, 100), SKPathDirection.Clockwise);
			using var path = builder.Detach();

			Assert.True(path.Contains(30, 30));
			Assert.False(path.Contains(5, 30));
		}

		[SkippableFact]
		public void PathContainsPointInRoundRect()
		{
			using var builder = new SKPathBuilder();
			var rrect = new SKRoundRect(SKRect.Create(10, 10, 100, 100), 5, 5);
			builder.AddRoundRect(rrect);
			using var path = builder.Detach();

			Assert.True(path.Contains(30, 30));
			Assert.False(path.Contains(5, 30));
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void GetLastPoint()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(0, 0);
			builder.LineTo(10, 20);
			using var path = builder.Detach();

			Assert.Equal(new SKPoint(10, 20), path.LastPoint);
		}

		[SkippableFact]
		public void ThumbtackShapeIsConcave()
		{
			// based on test_skbug_3469

			using var builder = new SKPathBuilder();
			builder.MoveTo(20, 20);
			builder.QuadTo(20, 50, 80, 50);
			builder.QuadTo(20, 50, 20, 80);
			using var path = builder.Detach();

			Assert.Equal(SKPathConvexity.Concave, path.Convexity);
		}

		[SkippableFact]
		public void RawIteratorReturnsCorrectPointsAndVerb()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(20, 20);
			builder.QuadTo(20, 50, 80, 50);
			builder.QuadTo(20, 50, 20, 80);
			using var path = builder.Detach();

			using (var iter = path.CreateRawIterator())
			{
				var points = new SKPoint[4];

				var verb = iter.Next(points);
				Assert.Equal(SKPathVerb.Move, verb);
				Assert.Equal(new[] { new SKPoint(20, 20), SKPoint.Empty, SKPoint.Empty, SKPoint.Empty }, points);

				verb = iter.Next(points);
				Assert.Equal(SKPathVerb.Quad, verb);
				Assert.Equal(new[] { new SKPoint(20, 20), new SKPoint(20, 50), new SKPoint(80, 50), SKPoint.Empty }, points);

				verb = iter.Next(points);
				Assert.Equal(SKPathVerb.Quad, verb);
				Assert.Equal(new[] { new SKPoint(80, 50), new SKPoint(20, 50), new SKPoint(20, 80), SKPoint.Empty }, points);

				verb = iter.Next(points);
				Assert.Equal(SKPathVerb.Done, verb);
				// note: when the iteration is Done, it doesn't touch the points
				Assert.Equal(new[] { new SKPoint(80, 50), new SKPoint(20, 50), new SKPoint(20, 80), SKPoint.Empty }, points);
			}
		}

		[SkippableFact]
		public void PathConsistentAfterClose()
		{
			// based on test_path_close_issue1474

			// This test checks that r{Line,Quad,Conic,Cubic}To following a close()
			// are relative to the point we close to, not relative to the point we close from.
			SKPoint last;

			// Test rLineTo().
			using var builder1 = new SKPathBuilder();
			builder1.RLineTo(0, 100);
			builder1.RLineTo(100, 0);
			builder1.Close();          // Returns us back to 0,0.
			builder1.RLineTo(50, 50);  // This should go to 50,50.
			using var path1 = builder1.Detach();

			last = path1.LastPoint;
			Assert.Equal(50, last.X);
			Assert.Equal(50, last.Y);

			// Test rQuadTo().
			using var builder2 = new SKPathBuilder();
			builder2.RLineTo(0, 100);
			builder2.RLineTo(100, 0);
			builder2.Close();
			builder2.RQuadTo(50, 50, 75, 75);
			using var path2 = builder2.Detach();

			last = path2.LastPoint;
			Assert.Equal(75, last.X);
			Assert.Equal(75, last.Y);

			// Test rConicTo().
			using var builder3 = new SKPathBuilder();
			builder3.RLineTo(0, 100);
			builder3.RLineTo(100, 0);
			builder3.Close();
			builder3.RConicTo(50, 50, 85, 85, 2);
			using var path3 = builder3.Detach();

			last = path3.LastPoint;
			Assert.Equal(85, last.X);
			Assert.Equal(85, last.Y);

			// Test rCubicTo().
			using var builder4 = new SKPathBuilder();
			builder4.RLineTo(0, 100);
			builder4.RLineTo(100, 0);
			builder4.Close();
			builder4.RCubicTo(50, 50, 85, 85, 95, 95);
			using var path4 = builder4.Detach();

			last = path4.LastPoint;
			Assert.Equal(95, last.X);
			Assert.Equal(95, last.Y);
		}

		[SkippableFact]
		public void ParsePathReturnsValidPath()
		{
			// based on ParsePath

			foreach (var test in parsePathTestCases)
			{
				var path = SKPath.ParseSvgPathData(test.Item1);

				Assert.NotNull(path);
				Assert.Equal(test.Item2, path.Bounds);

				TestToFromSvgPath(path);
			}

			var r = SKRect.Create(0, 0, 10, 10.5f);
			using var builder = new SKPathBuilder();
			builder.AddRect(r);
			using var p1 = builder.Detach();
			TestToFromSvgPath(p1);

			builder.AddOval(r);
			using var p2 = builder.Detach();
			TestToFromSvgPath(p2);

			builder.AddRoundRect(r, 4, 4.5f);
			using var p3 = builder.Detach();
			TestToFromSvgPath(p3);
		}

		[SkippableFact]
		public void PathBoundsAndRegionBoundsMatch()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(10, 10);
			builder.LineTo(90, 90);
			var path = builder.Detach();

			var bounds = path.Bounds;
			Assert.Equal(10f, bounds.Left);
			Assert.Equal(10f, bounds.Top);
			Assert.Equal(90f, bounds.Right);
			Assert.Equal(90f, bounds.Bottom);

			var region = new SKRegion();
			region.SetRect(new SKRectI(10, 10, 90, 90));

			var regionBounds = region.Bounds;
			Assert.Equal(10f, regionBounds.Left);
			Assert.Equal(10f, regionBounds.Top);
			Assert.Equal(90f, regionBounds.Right);
			Assert.Equal(90f, regionBounds.Bottom);
		}

		[SkippableFact]
		public void BoundsAndTightBoundAreCorrect()
		{
			const int Precision = 6;

			using var builder = new SKPathBuilder();
			builder.MoveTo(-6.2157825e-7f, -25.814698f);
			builder.RCubicTo(-34.64102137842175f, 19.9999998f, 0f, 40f, 0f, 40f);
			using SKPath path = builder.Detach();

			var bounds = path.Bounds;
			Assert.Equal((double)-34.641022f, (double)bounds.Left, Precision);
			Assert.Equal((double)-25.814698f, (double)bounds.Top, Precision);
			Assert.Equal((double)-6.215782e-07f, (double)bounds.Right, Precision);
			Assert.Equal((double)14.185303f, (double)bounds.Bottom, Precision);

			var tightBounds = path.TightBounds;
			Assert.Equal((double)-15.39601f, (double)tightBounds.Left, Precision);
			Assert.Equal((double)-25.814698f, (double)tightBounds.Top, Precision);
			Assert.Equal((double)-6.2157825e-7f, (double)tightBounds.Right, Precision);
			Assert.Equal((double)14.185303f, (double)tightBounds.Bottom, Precision);
		}

		[SkippableFact]
		public void MeasuringSegementsWorks()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(10f, 10f);
			builder.LineTo(110f, 10f);
			using SKPath path = builder.Detach();

			Assert.Equal(SKPathSegmentMask.Line, path.SegmentMasks);

			var measure = new SKPathMeasure(path);

			Assert.Equal(100f, measure.Length);

			using var segBuilder = new SKPathBuilder();
			var result = measure.GetSegment(20, 50, segBuilder, true);
			Assert.True(result);
			using var segment = segBuilder.Detach();
			Assert.Equal(2, segment.PointCount);
			Assert.Equal(new SKPoint(30, 10), segment.Points[0]);
			Assert.Equal(new SKPoint(60, 10), segment.Points[1]);
		}

		[SkippableFact]
		public void TightBoundsForEnclosedPathIsNotZero()
		{
			const int Precision = 3;

			var rect = SKRect.Create(10, 20, 28.889f, 28.889f);

			using var builder = new SKPathBuilder();
			builder.MoveTo(10, 20);
			builder.CubicTo(10, 20, 30, 40, 30, 40);
			builder.CubicTo(50, 60, 30, 40, 30, 40);
			var path = builder.Detach();

			var bounds = path.ComputeTightBounds();

			Assert.Equal(rect.Left, bounds.Left);
			Assert.Equal(rect.Top, bounds.Top);
			Assert.Equal((double)rect.Right, (double)bounds.Right, Precision);
			Assert.Equal((double)rect.Bottom, (double)bounds.Bottom, Precision);
		}

		[SkippableFact]
		public void RectPathIsRect()
		{
			using var builder = new SKPathBuilder();
			var rect = SKRect.Create(10, 10, 100, 100);
			builder.AddRect(rect, SKPathDirection.CounterClockwise);
			using var path = builder.Detach();

			Assert.False(path.IsOval);
			Assert.False(path.IsLine);
			Assert.True(path.IsRect);
			Assert.False(path.IsRoundRect);
			Assert.Equal(rect, path.GetRect(out var isClosed, out var dir));
			Assert.True(isClosed);
			Assert.Equal(SKPathDirection.CounterClockwise, dir);
		}

		[SkippableFact]
		public void RoundRectPathIsRoundRect()
		{
			using var builder = new SKPathBuilder();
			var rrect = new SKRoundRect(SKRect.Create(10, 10, 100, 100), 5, 5);
			builder.AddRoundRect(rrect);
			using var path = builder.Detach();

			Assert.False(path.IsOval);
			Assert.False(path.IsLine);
			Assert.False(path.IsRect);
			Assert.True(path.IsRoundRect);
			Assert.Equal(rrect.Rect, path.GetRoundRect().Rect);
			Assert.Equal(rrect.Radii, path.GetRoundRect().Radii);
		}

		[SkippableFact]
		public void LinePathIsLine()
		{
			using var builder = new SKPathBuilder();
			builder.LineTo(new SKPoint(100, 100));
			using var path = builder.Detach();

			Assert.False(path.IsOval);
			Assert.True(path.IsLine);
			Assert.False(path.IsRect);
			Assert.False(path.IsRoundRect);
			Assert.Equal(new[] { SKPoint.Empty, new SKPoint(100, 100) }, path.GetLine());
		}

		[SkippableFact]
		public void OvalPathIsOval()
		{
			using var builder = new SKPathBuilder();
			var rect = SKRect.Create(10, 10, 100, 100);
			builder.AddOval(rect);
			using var path = builder.Detach();

			Assert.True(path.IsOval);
			Assert.False(path.IsLine);
			Assert.False(path.IsRect);
			Assert.False(path.IsRoundRect);
			Assert.Equal(rect, path.GetOvalBounds());
		}

		[SkippableFact]
		public void TrimPathEffectWorksInverted()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(0, 50);
			builder.LineTo(new SKPoint(100, 50));
			using var path = builder.Detach();

			using (var bitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(SKColors.White);

				// draw the base path
				using (var paint = new SKPaint())
				{
					paint.Style = SKPaintStyle.Stroke;
					paint.StrokeWidth = 20;
					paint.Color = SKColors.Black;

					canvas.DrawPath(path, paint);
				}

				// should be black
				Assert.Equal(SKColors.Black, bitmap.GetPixel(10, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(50, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(90, 50));

				// draw the path with an effect (reversed)
				using (var paint = new SKPaint())
				{
					paint.Style = SKPaintStyle.Stroke;
					paint.StrokeWidth = 20;
					paint.Color = SKColors.Red;

					// attach the effect
					paint.PathEffect = SKPathEffect.CreateTrim(0.3f, 0.7f, SKTrimPathEffectMode.Inverted);

					canvas.DrawPath(path, paint);
				}

				// should be red
				Assert.Equal(SKColors.Red, bitmap.GetPixel(10, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(50, 50));
				Assert.Equal(SKColors.Red, bitmap.GetPixel(90, 50));
			}
		}

		[SkippableFact]
		public void TrimPathEffectWorks()
		{
			using var builder = new SKPathBuilder();
			builder.MoveTo(0, 50);
			builder.LineTo(new SKPoint(100, 50));
			using var path = builder.Detach();

			using (var bitmap = new SKBitmap(new SKImageInfo(100, 100)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(SKColors.White);

				// draw the base path
				using (var paint = new SKPaint())
				{
					paint.Style = SKPaintStyle.Stroke;
					paint.StrokeWidth = 20;
					paint.Color = SKColors.Black;

					canvas.DrawPath(path, paint);
				}

				// should be black
				Assert.Equal(SKColors.Black, bitmap.GetPixel(10, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(50, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(90, 50));

				// draw the path with an effect
				using (var paint = new SKPaint())
				{
					paint.Style = SKPaintStyle.Stroke;
					paint.StrokeWidth = 20;
					paint.Color = SKColors.Red;

					// attach the effect
					paint.PathEffect = SKPathEffect.CreateTrim(0.3f, 0.7f);

					canvas.DrawPath(path, paint);
				}

				// should be red
				Assert.Equal(SKColors.Black, bitmap.GetPixel(10, 50));
				Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 50));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(90, 50));
			}
		}

		[SkippableFact]
		public void ToWinding()
		{
			using var builder = new SKPathBuilder();
			builder.AddRect(new SKRect(1, 2, 3, 4));
			using var path = builder.Detach();

			using var result = new SKPath();

			path.FillType = SKPathFillType.Winding;
			Assert.True(path.ToWinding(result));
			Assert.NotEqual(path, result);
			Assert.Equal(SKPathFillType.Winding, path.FillType);

			path.FillType = SKPathFillType.EvenOdd;
			Assert.True(path.ToWinding(result));
			Assert.NotEqual(path, result);
			Assert.Equal(SKPathFillType.Winding, result.FillType);
		}

		// --- Lazy-batching correctness -------------------------------------------------------
		// The following tests exercise the interaction between the [Obsolete] SKPath mutation
		// methods (which batch into an internal SKPathBuilder) and external/internal readers.
		// They fail if SKPath.Handle does not flush the pending builder, or if the backward-
		// compat [Obsolete] overloads that take SKPath dst don't correctly move geometry in.

#pragma warning disable CS0618 // Obsolete SKPath mutation/dst APIs are intentional here.

		[SkippableFact]
		public void BatchedMutationsVisibleThroughSKPathBuilderAddPath()
		{
			// SKPathBuilder.AddPath reads other.Handle directly in native code.
			// Without Handle-flush, the batched MoveTo/LineTo would be invisible.
			using var path = new SKPath();
			path.MoveTo(10, 20);
			path.LineTo(30, 40);

			using var builder = new SKPathBuilder();
			builder.AddPath(path);
			using var snapshot = builder.Snapshot();

			Assert.Equal(2, snapshot.PointCount);
			Assert.Equal(new SKPoint(10, 20), snapshot.Points[0]);
			Assert.Equal(new SKPoint(30, 40), snapshot.Points[1]);
		}

		[SkippableFact]
		public void BatchedMutationsVisibleToSKRegionSetPath()
		{
			// SKRegion.SetPath reads path.Handle in the C shim.
			using var path = new SKPath();
			path.AddRect(new SKRect(0, 0, 100, 100));

			using var clip = new SKRegion();
			clip.SetRect(new SKRectI(-10, -10, 200, 200));

			using var region = new SKRegion();
			Assert.True(region.SetPath(path, clip));
			Assert.Equal(new SKRectI(0, 0, 100, 100), region.Bounds);
		}

		[SkippableFact]
		public void BatchedMutationsVisibleToSKCanvasDrawPath()
		{
			// SKCanvas.DrawPath reads path.Handle. Without flush, the rect wouldn't be drawn.
			using var bitmap = new SKBitmap(new SKImageInfo(100, 100));
			using var canvas = new SKCanvas(bitmap);
			canvas.Clear(SKColors.White);

			using var path = new SKPath();
			path.AddRect(new SKRect(0, 0, 50, 50));

			using var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill };
			canvas.DrawPath(path, paint);

			Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 25));
			Assert.Equal(SKColors.White, bitmap.GetPixel(75, 75));
		}

		[SkippableFact]
		public void MutateReadMutateReadCycleIsConsistent()
		{
			// Exercises the flush/swap/rebuild-builder/flush chain.
			using var path = new SKPath();
			path.MoveTo(0, 0);
			path.LineTo(10, 0);
			var boundsBeforeFlush = path.Bounds;   // forces flush, builder disposed

			path.LineTo(10, 10);                    // EnsureBuilder copies flushed state into a new builder
			path.Close();
			var boundsAfterFlush = path.Bounds;

			Assert.Equal(new SKRect(0, 0, 10, 0), boundsBeforeFlush);
			Assert.Equal(new SKRect(0, 0, 10, 10), boundsAfterFlush);
		}

		[SkippableFact]
		public void FillTypeStaysConsistentAcrossFlush()
		{
			// FillType setter writes to both native path and builder (if any); getter reads
			// from the native path. The invariant must survive a flush.
			using var path = new SKPath();
			path.FillType = SKPathFillType.EvenOdd;
			Assert.Equal(SKPathFillType.EvenOdd, path.FillType);

			path.MoveTo(0, 0);      // creates builder, builder copies FillType from path
			path.LineTo(10, 0);
			Assert.Equal(SKPathFillType.EvenOdd, path.FillType);

			path.FillType = SKPathFillType.Winding;   // mirror to both
			_ = path.Bounds;                           // force flush
			Assert.Equal(SKPathFillType.Winding, path.FillType);
		}

		// --- SKPath-as-destination tests -----------------------------------------------------
		// A destination SKPath may hold pending batched mutations. The operation must
		// overwrite those mutations (matching the "destination is overwritten" contract).

		[SkippableFact]
		public void TransformWithDestinationOverwritesBatchedMutations()
		{
			using var src = new SKPath();
			src.MoveTo(0, 0);
			src.LineTo(10, 0);

			using var dst = new SKPath();
			dst.MoveTo(999, 999);   // batched; must be overwritten, not appended

			var matrix = SKMatrix.CreateTranslation(5, 5);
			src.Transform(matrix, dst);

			Assert.Equal(2, dst.PointCount);
			Assert.Equal(new SKPoint(5, 5), dst.Points[0]);
			Assert.Equal(new SKPoint(15, 5), dst.Points[1]);
		}

		[SkippableFact]
		public void OpWithDestinationOverwritesBatchedMutations()
		{
			using var a = new SKPath();
			a.AddRect(new SKRect(0, 0, 10, 10));

			using var b = new SKPath();
			b.AddRect(new SKRect(5, 5, 15, 15));

			using var result = new SKPath();
			result.MoveTo(999, 999);   // batched; must be overwritten

			Assert.True(a.Op(b, SKPathOp.Union, result));
			Assert.Equal(new SKRect(0, 0, 15, 15), result.Bounds);
		}

		[SkippableFact]
		public void SimplifyWithDestinationOverwritesBatchedMutations()
		{
			using var path = new SKPath();
			path.AddRect(new SKRect(0, 0, 10, 10));

			using var result = new SKPath();
			result.MoveTo(999, 999);

			Assert.True(path.Simplify(result));
			Assert.Equal(new SKRect(0, 0, 10, 10), result.Bounds);
		}

		[SkippableFact]
		public void ToWindingWithDestinationOverwritesBatchedMutations()
		{
			using var path = new SKPath();
			path.AddRect(new SKRect(0, 0, 10, 10));
			path.FillType = SKPathFillType.EvenOdd;

			using var result = new SKPath();
			result.MoveTo(999, 999);

			Assert.True(path.ToWinding(result));
			Assert.Equal(SKPathFillType.Winding, result.FillType);
			Assert.Equal(new SKRect(0, 0, 10, 10), result.Bounds);
		}

		[SkippableFact]
		public void SKPaintGetFillPathWithSKPathDstOverwritesBatchedMutations()
		{
			// Backward-compat overload restored as [Obsolete]. Goes through a temp
			// SKPathBuilder and SKPath.ReplaceFromBuilder. Pre-fix: CS1503 at compile time.
			using var paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2 };

			using var src = new SKPath();
			src.AddRect(new SKRect(0, 0, 10, 10));

			using var dst = new SKPath();
			dst.MoveTo(999, 999);

			Assert.True(paint.GetFillPath(src, dst));
			Assert.True(dst.VerbCount > 0);
			Assert.True(dst.Bounds.Width >= 10);
			// The stroke expanded the rect — the batched MoveTo(999,999) must be gone.
			Assert.DoesNotContain(new SKPoint(999, 999), dst.Points);
		}

		[SkippableFact]
		public void SKPathMeasureGetSegmentWithSKPathDstOverwritesBatchedMutations()
		{
			// Backward-compat overload restored as [Obsolete]. Pre-fix: CS1503 at compile time.
			using var path = new SKPath();
			path.MoveTo(0, 0);
			path.LineTo(100, 0);

			using var measure = new SKPathMeasure(path);

			using var dst = new SKPath();
			dst.MoveTo(999, 999);

			Assert.True(measure.GetSegment(0, 50, dst, true));
			Assert.True(dst.VerbCount > 0);
			Assert.DoesNotContain(new SKPoint(999, 999), dst.Points);
		}

		[SkippableFact]
		public void TransformInPlaceWhenSrcEqualsDst()
		{
			// Edge case: path.Transform(matrix, path) — native code computes the result
			// before the self-assignment overwrites, so it's safe.
			using var path = new SKPath();
			path.MoveTo(0, 0);
			path.LineTo(10, 0);
			_ = path.Bounds; // force flush

			var matrix = SKMatrix.CreateTranslation(5, 5);
			path.Transform(matrix, path);

			Assert.Equal(2, path.PointCount);
			Assert.Equal(new SKPoint(5, 5), path.Points[0]);
			Assert.Equal(new SKPoint(15, 5), path.Points[1]);
		}

#pragma warning restore CS0618
	}
}
