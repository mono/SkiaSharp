using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKPathTest : SKTest
	{
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
	}
}
