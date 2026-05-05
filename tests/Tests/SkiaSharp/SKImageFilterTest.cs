using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageFilterTest : SKTest
	{
		[SkippableFact]
		public void MergeFilterAcceptsNullFilterArray()
		{
			var filter = SKImageFilter.CreateMerge(new SKImageFilter[] { null });
			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void MergeFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateMerge((SKImageFilter)null, null);
			Assert.NotNull(filter);
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void ShaderFilterAcceptsNullParams()
		{
			var filter = SKImageFilter.CreateShader(null);
			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void RuntimeShaderFilterIsCreatedWithSingleChild()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return child.eval(p);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var filter = builder.BuildImageFilter("child");

			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void RuntimeShaderFilterIsCreatedWithAutoDetectedChild()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return child.eval(p);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			// empty string lets C++ auto-detect the single child
			using var filter = builder.BuildImageFilter("");

			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void RuntimeShaderFilterIsCreatedWithInputFilter()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return child.eval(p);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var blur = SKImageFilter.CreateBlur(5, 5);
			using var filter = builder.BuildImageFilter("child", blur);

			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void RuntimeShaderFilterIsCreatedWithMaxSampleRadius()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					half4 c = child.eval(p);
					c += child.eval(p + float2(1, 0));
					c += child.eval(p + float2(-1, 0));
					return c / 3;
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var filter = builder.BuildImageFilter(1.0f, "child");

			Assert.NotNull(filter);
		}

		[SkippableFact]
		public void RuntimeShaderFilterRendersCorrectly()
		{
			// Shader that tints everything red
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return half4(1, 0, 0, 1);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var filter = builder.BuildImageFilter("child");

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.ImageFilter = filter;
			canvas.DrawPaint(paint);

			using var snapshot = surface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			var pixel = pixmap.GetPixelColor(50, 50);

			Assert.Equal(SKColors.Red, pixel);
		}

		[SkippableFact]
		public void RuntimeShaderFilterWithUniformsRendersCorrectly()
		{
			var src = """
				uniform shader child;
				uniform float4 tintColor;
				half4 main(float2 p) {
					return half4(tintColor);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			builder.Uniforms["tintColor"] = new float[] { 0, 0, 1, 1 };
			using var filter = builder.BuildImageFilter("child");

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.ImageFilter = filter;
			canvas.DrawPaint(paint);

			using var snapshot = surface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			var pixel = pixmap.GetPixelColor(50, 50);

			Assert.Equal(SKColors.Blue, pixel);
		}

		[SkippableFact]
		public void RuntimeShaderFilterChainsWithOtherFilters()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return child.eval(p);
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var blur = SKImageFilter.CreateBlur(2, 2);
			using var filter = builder.BuildImageFilter("child", blur);

			Assert.NotNull(filter);

			// Verify it can be used in a paint and renders without crashing
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Green);

			using var paint = new SKPaint();
			paint.ImageFilter = filter;
			canvas.DrawPaint(paint);
		}

		[SkippableFact]
		public void RuntimeShaderFilterWithMaxSampleRadiusRendersCorrectly()
		{
			// Simple averaging shader that samples nearby pixels
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					half4 c = child.eval(p);
					c += child.eval(p + float2(1, 0));
					c += child.eval(p + float2(-1, 0));
					c += child.eval(p + float2(0, 1));
					c += child.eval(p + float2(0, -1));
					return c / 5;
				}
				""";

			using var builder = SKRuntimeEffect.BuildShader(src);
			using var filter = builder.BuildImageFilter(1.0f, "child", null);

			Assert.NotNull(filter);

			// Draw a red rectangle, then apply the filter via a saveLayer
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Transparent);

			using var paint = new SKPaint();
			paint.ImageFilter = filter;

			// Draw source content within a layer so the child shader receives it
			canvas.SaveLayer(paint);
			canvas.Clear(SKColors.Red);
			canvas.Restore();

			using var snapshot = surface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			var pixel = pixmap.GetPixelColor(50, 50);

			// The center pixel should be red (averaging all-red neighbors = red)
			Assert.Equal(255, pixel.Red);
			Assert.Equal(0, pixel.Green);
			Assert.Equal(0, pixel.Blue);
		}

		[SkippableFact]
		public void ToImageFilterWorksDirectlyOnEffect()
		{
			var src = """
				uniform shader child;
				half4 main(float2 p) {
					return half4(0, 1, 0, 1);
				}
				""";

			using var effect = SKRuntimeEffect.CreateShader(src, out var errors);
			Assert.Null(errors);

			var uniforms = new SKRuntimeEffectUniforms(effect);
			var children = new SKRuntimeEffectChildren(effect);
			using var filter = effect.ToImageFilter(uniforms, children, "child");

			Assert.NotNull(filter);

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			paint.ImageFilter = filter;
			canvas.DrawPaint(paint);

			using var snapshot = surface.Snapshot();
			using var pixmap = snapshot.PeekPixels();
			var pixel = pixmap.GetPixelColor(50, 50);

			Assert.Equal(new SKColor(0, 255, 0), pixel);
		}
	}
}
