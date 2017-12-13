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

		private static void TestToFromSvgPath (SKPath path)
		{
			var str = path.ToSvgPathData ();
			Assert.NotNull (str);

			var path2 = SKPath.ParseSvgPathData (str);
			Assert.NotNull (path2);

			var str2 = path2.ToSvgPathData ();
			Assert.NotNull (str2);

			Assert.Equal (str, str2);
		}

		[Fact]
		public void PathPointsAreCorrect()
		{
			using (var path = new SKPath ()) {
				// set up the xamagon
				path.MoveTo (71.4311121f, 56f);
				path.CubicTo (68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
				path.LineTo (43.0238921f, 97.5342563f);
				path.CubicTo (41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
				path.LineTo (64.5928855f, 143.034271f);
				path.CubicTo (65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
				path.LineTo (114.568946f, 147f);
				path.CubicTo (117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
				path.LineTo (142.976161f, 105.465744f);
				path.CubicTo (144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
				path.LineTo (121.407172f, 59.965729f);
				path.CubicTo (120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
				path.LineTo (71.4311121f, 56f);
				path.Close ();

				// the right number/count
				Assert.Equal (25, path.PointCount);
				Assert.Equal (25, path.Points.Length);

				// the right value
				Assert.Equal (new SKPoint (68.6763107f, 56.0058575f), path.GetPoint (1));
				Assert.Equal (new SKPoint (68.6763107f, 56.0058575f), path.Points [1]);

				// the right segment masks
				Assert.Equal (SKPathSegmentMask.Cubic | SKPathSegmentMask.Line, path.SegmentMasks);
			}
		}

		[Fact]
		public void PathContainsPoint()
		{
			using (var path = new SKPath ()) {
				path.AddRect (SKRect.Create (10, 10, 100, 100), SKPathDirection.Clockwise);

				Assert.True (path.Contains (30, 30));
				Assert.False (path.Contains (5, 30));
			}
		}

		[Fact]
		public void GetLastPoint()
		{
			using (var path = new SKPath ()) {
				path.MoveTo (0, 0);
				path.LineTo (10, 20);

				Assert.Equal (new SKPoint(10, 20), path.LastPoint);
			}
		}

		[Fact]
		public void ThumbtackShapeIsConcave ()
		{
			// based on test_skbug_3469
			
			using (var path = new SKPath ()) {
				path.MoveTo (20, 20);
				path.QuadTo (20, 50, 80, 50);
				path.QuadTo (20, 50, 20, 80);

				Assert.Equal (SKPathConvexity.Concave, path.Convexity);
			}
		}

		[Fact]
		public void PathConsistentAfterClose ()
		{
			// based on test_path_close_issue1474
			
			using (var path = new SKPath ()) {
				// This test checks that r{Line,Quad,Conic,Cubic}To following a close()
				// are relative to the point we close to, not relative to the point we close from.
				SKPoint last;

				// Test rLineTo().
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();          // Returns us back to 0,0.
				path.RLineTo (50, 50);  // This should go to 50,50.

				last = path.LastPoint;
				Assert.Equal (50, last.X);
				Assert.Equal (50, last.Y);

				// Test rQuadTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RQuadTo (50, 50, 75, 75);

				last = path.LastPoint;
				Assert.Equal (75, last.X);
				Assert.Equal (75, last.Y);

				// Test rConicTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RConicTo (50, 50, 85, 85, 2);

				last = path.LastPoint;
				Assert.Equal (85, last.X);
				Assert.Equal (85, last.Y);

				// Test rCubicTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RCubicTo (50, 50, 85, 85, 95, 95);

				last = path.LastPoint;
				Assert.Equal (95, last.X);
				Assert.Equal (95, last.Y);
			}
		}

		[Fact]
		public void ParsePathReturnsValidPath ()
		{
			// based on ParsePath

			foreach (var test in parsePathTestCases) {
				var path = SKPath.ParseSvgPathData (test.Item1);

				Assert.NotNull (path);
				Assert.Equal (test.Item2, path.Bounds);

				TestToFromSvgPath (path);
			}

			var r = SKRect.Create (0, 0, 10, 10.5f);
			using (var p = new SKPath ()) {
				p.AddRect (r);
				TestToFromSvgPath (p);

				p.AddOval (r);
				TestToFromSvgPath (p);
				
				p.AddRoundedRect (r, 4, 4.5f);
				TestToFromSvgPath (p);
			}
		}

		[Fact]
		public void PathBoundsAndRegionBoundsMatch ()
		{
			var path = new SKPath ();
			path.MoveTo (10, 10);
			path.LineTo (90, 90);

			var bounds = path.Bounds;
			Assert.Equal (10f, bounds.Left);
			Assert.Equal (10f, bounds.Top);
			Assert.Equal (90f, bounds.Right);
			Assert.Equal (90f, bounds.Bottom);

			var region = new SKRegion ();
			region.SetRect (new SKRectI (10, 10, 90, 90));

			var regionBounds = region.Bounds;
			Assert.Equal (10f, regionBounds.Left);
			Assert.Equal (10f, regionBounds.Top);
			Assert.Equal (90f, regionBounds.Right);
			Assert.Equal (90f, regionBounds.Bottom);
		}

		[Fact]
		public void BoundsAndTightBoundAreCorrect ()
		{
			const int Precision = 6;

			using (SKPath path = new SKPath ())
			{
				path.MoveTo (-6.2157825e-7f, -25.814698f);
				path.RCubicTo (-34.64102137842175f, 19.9999998f, 0f, 40f, 0f, 40f);

				var bounds = path.Bounds;
				Assert.Equal (-34.641022f, bounds.Left, Precision);
				Assert.Equal (-25.814698f, bounds.Top, Precision);
				Assert.Equal (-6.215782e-07f, bounds.Right, Precision);
				Assert.Equal (14.185303f, bounds.Bottom, Precision);

				var tightBounds = path.TightBounds;
				Assert.Equal (-15.396009f, tightBounds.Left, Precision);
				Assert.Equal (-25.814698f, tightBounds.Top, Precision);
				Assert.Equal (0f, tightBounds.Right, Precision);
				Assert.Equal (14.185303f, tightBounds.Bottom, Precision);
			}
		}

		[Fact]
		public void MeasuringSegementsWorks ()
		{
			using (SKPath path = new SKPath ())
			{
				path.MoveTo (10f, 10f);
				path.LineTo (110f, 10f);

				Assert.Equal (SKPathSegmentMask.Line, path.SegmentMasks);

				var measure = new SKPathMeasure (path);

				Assert.Equal (100f, measure.Length);

				var segment = new SKPath ();
				var result = measure.GetSegment (20, 50, segment, true);
				Assert.True (result);
				Assert.Equal (2, segment.PointCount);
				Assert.Equal (new SKPoint (30, 10), segment.Points [0]);
				Assert.Equal (new SKPoint (60, 10), segment.Points [1]);
			}
		}

		[Fact]
		public void TightBoundsForEnclosedPathIsNotZero ()
		{
			const int Precision = 3;

			var rect = SKRect.Create (10, 20, 28.889f, 28.889f);

			var path = new SKPath ();
			path.MoveTo (10, 20);
			path.CubicTo (10, 20, 30, 40, 30, 40);
			path.CubicTo (50, 60, 30, 40, 30, 40);

			var bounds = path.ComputeTightBounds ();

			Assert.Equal (rect.Left, bounds.Left);
			Assert.Equal (rect.Top, bounds.Top);
			Assert.Equal (rect.Right, bounds.Right, Precision);
			Assert.Equal (rect.Bottom, bounds.Bottom, Precision);
		}
	}
}
