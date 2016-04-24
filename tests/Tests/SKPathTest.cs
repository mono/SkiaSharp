using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKPathTest : SKTest
	{
		private void AssertPt(SKPoint point, float x, float y)
		{
			Assert.AreEqual (x, point.X);
			Assert.AreEqual (y, point.Y);
		}

		[Test]
		public void EmptyPathPeekException ()
		{
			SKPoint [] points = new SKPoint [4];
			using (SKPath path = new SKPath ()) {
				using (SKPath.Iter iter = new SKPath.Iter (path)) {
					Assert.Throws (typeof (InvalidOperationException), () => iter.Peek());
				}
			}
		}

		[Test]
		public void TestIter ()
		{
			SKPoint [] points = new SKPoint [4];
			using (SKPath path = new SKPath ()) {
				using (SKPath.Iter iter = new SKPath.Iter (path)) {
					Assert.AreEqual (SKPathVerb.Done, iter.Next (points));
				}

				path.MoveTo (0, 0);
				path.LineTo (10, 10);
				using (SKPath.Iter iter = new SKPath.Iter (path)) {
					Assert.AreEqual (SKPathVerb.Move, iter.Next (points));
					AssertPt (points [0], 0, 0);
					Assert.AreEqual (SKPathVerb.Line, iter.Next (points));
					AssertPt (points [0], 0, 0);
					AssertPt (points [1], 10, 10);
					Assert.AreEqual (SKPathVerb.Done, iter.Next (points));
				}

				path.RLineTo (5, 5);
				using (SKPath.Iter iter = new SKPath.Iter (path)) {
					Assert.AreEqual (SKPathVerb.Move, iter.Next (points));
					AssertPt (points [0], 0, 0);
					Assert.AreEqual (SKPathVerb.Line, iter.Next (points));
					AssertPt (points [0], 0, 0);
					AssertPt (points [1], 10, 10);
					Assert.AreEqual (SKPathVerb.Line, iter.Next (points));
					AssertPt (points [0], 10, 10);
					AssertPt (points [1], 15, 15);
					Assert.AreEqual (SKPathVerb.Done, iter.Next (points));
				}
			}

			using (SKPath path = new SKPath ()) {
				path.MoveTo (0, 0);
				for (int i = 0; i < 10; i++) {
					path.LineTo (i, i);
					path.MoveTo (i, i);
				}
				using (SKPath.Iter iter = new SKPath.Iter (path)) {
					int numVerbs = 0;
					while (iter.Next (points) != SKPathVerb.Done) numVerbs++;
					Assert.AreEqual (21, numVerbs);
					Assert.AreEqual (21, path.CountVerbs ());
				}
			}
		}
	}
}
