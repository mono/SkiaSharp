using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

using ParsePathTest = System.Tuple<System.String, SkiaSharp.SKRect>;

namespace SkiaSharp.Tests
{
	[TestFixture]
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
			Assert.IsNotNull (str);

			var path2 = SKPath.ParseSvgPathData (str);
			Assert.IsNotNull (path2);

			var str2 = path2.ToSvgPathData ();
			Assert.IsNotNull (str2);

			Assert.AreEqual (str, str2);
		}

		[Test]
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
				Assert.AreEqual (25, path.PointCount);
				Assert.AreEqual (25, path.Points.Length);

				// the right value
				Assert.AreEqual (new SKPoint (68.6763107f, 56.0058575f), path.GetPoint (1));
				Assert.AreEqual (new SKPoint (68.6763107f, 56.0058575f), path.Points [1]);
			}
		}

		[Test]
		public void PathContainsPoint()
		{
			using (var path = new SKPath ()) {
				path.AddRect (SKRect.Create (10, 10, 100, 100), SKPathDirection.Clockwise);

				Assert.IsTrue (path.Contains (30, 30));
				Assert.IsFalse (path.Contains (5, 30));
			}
		}

		[Test]
		public void GetLastPoint()
		{
			using (var path = new SKPath ()) {
				path.MoveTo (0, 0);
				path.LineTo (10, 20);

				Assert.AreEqual (new SKPoint(10, 20), path.LastPoint);
			}
		}

		[Test]
		public void ThumbtackShapeIsConcave ()
		{
			// based on test_skbug_3469
			
			using (var path = new SKPath ()) {
				path.MoveTo (20, 20);
				path.QuadTo (20, 50, 80, 50);
				path.QuadTo (20, 50, 20, 80);

				Assert.AreEqual (SKPathConvexity.Concave, path.Convexity);
			}
		}

		[Test]
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
				Assert.AreEqual (50, last.X);
				Assert.AreEqual (50, last.Y);

				// Test rQuadTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RQuadTo (50, 50, 75, 75);

				last = path.LastPoint;
				Assert.AreEqual (75, last.X);
				Assert.AreEqual (75, last.Y);

				// Test rConicTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RConicTo (50, 50, 85, 85, 2);

				last = path.LastPoint;
				Assert.AreEqual (85, last.X);
				Assert.AreEqual (85, last.Y);

				// Test rCubicTo().
				path.Rewind ();
				path.RLineTo (0, 100);
				path.RLineTo (100, 0);
				path.Close ();
				path.RCubicTo (50, 50, 85, 85, 95, 95);

				last = path.LastPoint;
				Assert.AreEqual (95, last.X);
				Assert.AreEqual (95, last.Y);
			}
		}

		[Test]
		public void ParsePathReturnsValidPath ()
		{
			// based on ParsePath

			foreach (var test in parsePathTestCases) {
				var path = SKPath.ParseSvgPathData (test.Item1);

				Assert.IsNotNull (path);
				Assert.AreEqual (test.Item2, path.Bounds);

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

		[Test]
		public void BoundsAndTightBoundAreCorrect ()
		{
			const float EPSILON = 0.000001f;

			using (SKPath path = new SKPath ())
			{
				path.MoveTo (-6.2157825e-7f, -25.814698f);
				path.RCubicTo (-34.64102137842175f, 19.9999998f, 0f, 40f, 0f, 40f);

				var bounds = path.Bounds;
				Assert.AreEqual (-34.641022f, bounds.Left, EPSILON);
				Assert.AreEqual (-25.814698f, bounds.Top, EPSILON);
				Assert.AreEqual (-6.2157825e-7f, bounds.Right, EPSILON);
				Assert.AreEqual (14.185303f, bounds.Bottom, EPSILON);

				var tightBounds = path.TightBounds;
				Assert.AreEqual (-15.396009f, tightBounds.Left, EPSILON);
				Assert.AreEqual (-25.814698f, tightBounds.Top, EPSILON);
				Assert.AreEqual (-6.2157825e-7f, tightBounds.Right, EPSILON);
				Assert.AreEqual (14.185303f, tightBounds.Bottom, EPSILON);
			}
		}

		[Test]
		public void MeasuringSegementsWorks ()
		{
			const float EPSILON = 0.000001f;

			using (SKPath path = new SKPath ())
			{
				path.MoveTo (10f, 10f);
				path.LineTo (110f, 10f);

				var measure = new SKPathMeasure (path);

				Assert.AreEqual (100f, measure.Length, EPSILON);

				var segment = new SKPath ();
				var result = measure.GetSegment (20, 50, segment, true);
				Assert.IsTrue (result);
				Assert.AreEqual (2, segment.PointCount);
				Assert.AreEqual (new SKPoint (30, 10), segment.Points [0]);
				Assert.AreEqual (new SKPoint (60, 10), segment.Points [1]);
			}
		}
	}
}
