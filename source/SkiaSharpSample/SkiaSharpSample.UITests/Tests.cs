using System;
using NUnit.Framework;
using Query = System.Func<Uno.UITest.IAppQuery, Uno.UITest.IAppQuery>;

namespace SkiaSharpSample.UITests
{
    public class SamplesTest : TestBase
	{
		[TestCase("Bitmap Decoder")]
		[TestCase("Bitmap Lattice (9-patch)")]
		[TestCase("Bitmap Shader")]
		[TestCase("Blur Image Filter")]
		[TestCase("Blur Mask Filter")]
		[TestCase("Chained Image Filter")]
		[TestCase("Color Matrix Color Filter")]
		[TestCase("Color Table Color Filter")]
		[TestCase("Compose Shader")]
		[TestCase("Dilate Image Filter")]
		[TestCase("Draw Matrix")]
		[TestCase("Erode Image Filter")]
		[TestCase("Filled Heptagram")]
		[TestCase("Fractal Perlin Noise Shader")]
		[TestCase("Gradient")]
		[TestCase("Luma Color Filter")]
		[TestCase("Magnifier Image Filter")]
		[TestCase("Bitmap Shader (Manipulated)")]
		[TestCase("Measure Text")]
		[TestCase("Path Bounds")]
		[TestCase("Path (conic to quads)")]
		[TestCase("2D Path Effect")]
		[TestCase("Path Effects")]
		[TestCase("Path Measure")]
		[TestCase("Sweep Gradient Shader")]
		[TestCase("Text")]
		[TestCase("3D Rotation (ortho)")]
		[TestCase("3D Rotation (perspective)")]
		[TestCase("Turbulence Perlin Noise Shader")]
		[TestCase("\"Xamagon\"")]
		[TestCase("Blend Mode Color Filter")]
		[TestCase("Blend Mode")]
		public void SkiaSample(string testName)
		{
			Query testSelector = q => q.Marked(testName);

			App.WaitForElement(testSelector);
			App.Tap(testSelector);

			TakeScreenshot(testName);
		}
	}
}
