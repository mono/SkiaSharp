using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests;

public class SKBlenderTest
{
	[SkippableFact]
	public void SameBlendModeReturnsSameBlenderInstance()
	{
		var blender1 = SKBlender.CreateBlendMode(SKBlendMode.Src);
		var blender2 = SKBlender.CreateBlendMode(SKBlendMode.Src);

		Assert.Same(blender1, blender2);
	}

	[SkippableFact]
	public void BlendModeBlenderIsNotDisposed()
	{
		var blender = SKBlender.CreateBlendMode(SKBlendMode.Src);
		Assert.True(SKObject.GetInstance<SKBlender>(blender.Handle, out _));

		blender.Dispose();
		Assert.True(SKObject.GetInstance<SKBlender>(blender.Handle, out _));
	}

	[SkippableFact]
	public void ArithmeticBlendModeBlenderIsBlendModeBlender()
	{
		var blendmode = SKBlender.CreateBlendMode(SKBlendMode.Src);

		var arithmetic = SKBlender.CreateArithmetic(0, 1, 0, 0, false);

		Assert.Same(blendmode, arithmetic);
	}

	[SkippableFact]
	public void InvalidBlendModeThrowsArgumentException()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => SKBlender.CreateBlendMode((SKBlendMode)100));
	}

	public abstract class SurfaceTestBase : SKTest
	{
		protected SKSurface Surface { get; set; }

		protected SKImageInfo Info { get; set; }

		protected abstract void CreateSurface(int width, int height);

		[SkippableTheory]
		[MemberData(nameof(GetAllBlendModes))]
		public void BlenderMatchesBlendModeWhenUsingOpaqueColor(SKBlendMode mode)
		{
			var blendModeColor = GetColor(p => ApplyColor(p, false), p => ApplyBlendMode(p, mode));
			var blenderColor = GetColor(p => ApplyColor(p, false), p => ApplyBlender(p, mode));
			Assert.Equal(blendModeColor, blenderColor);
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllBlendModes))]
		public void BlenderMatchesBlendModeWhenUsingransparentColor(SKBlendMode mode)
		{
			var blendModeColor = GetColor(p => ApplyColor(p, true), p => ApplyBlendMode(p, mode));
			var blenderColor = GetColor(p => ApplyColor(p, true), p => ApplyBlender(p, mode));
			Assert.Equal(blendModeColor, blenderColor);
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllBlendModes))]
		public void BlenderMatchesBlendModeWhenUsingOpaqueShader(SKBlendMode mode)
		{
			var blendModeColor = GetColor(p => ApplyColorShader(p, false), p => ApplyBlendMode(p, mode));
			var blenderColor = GetColor(p => ApplyColorShader(p, false), p => ApplyBlender(p, mode));
			Assert.Equal(blendModeColor, blenderColor);
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllBlendModes))]
		public void BlenderMatchesBlendModeWhenUsingransparentShader(SKBlendMode mode)
		{
			var blendModeColor = GetColor(p => ApplyColorShader(p, true), p => ApplyBlendMode(p, mode));
			var blenderColor = GetColor(p => ApplyColorShader(p, true), p => ApplyBlender(p, mode));
			Assert.Equal(blendModeColor, blenderColor);
		}

		private SKColor GetColor(Action<SKPaint> applyColor, Action<SKPaint> applyBlend)
		{
			// Draw a solid red pixel.
			using var paint = new SKPaint
			{
				Shader = null,
				Color = SKColors.Red,
				Blender = null,
				BlendMode = SKBlendMode.Src,
			};
			Surface.Canvas.DrawRect(SKRect.Create(1, 1), paint);

			// Draw a blue pixel on top of it, using the passed-in blend mode.
			applyColor(paint);
			applyBlend(paint);
			Surface.Canvas.DrawRect(SKRect.Create(1, 1), paint);

			// Read the pixels out of the surface and into the bitmap.
			using var bmp = new SKBitmap(new SKImageInfo(1, 1));
			Surface.ReadPixels(bmp.Info, bmp.GetPixels(), bmp.RowBytes, 0, 0);

			// Get the pixel color.
			return bmp.GetPixel(0, 0);
		}

		private static void ApplyBlendMode(SKPaint paint, SKBlendMode mode) =>
			paint.BlendMode = mode;

		private static void ApplyBlender(SKPaint paint, SKBlendMode mode) =>
			paint.Blender = GetRuntimeBlenderForBlendMode(mode);

		private static void ApplyColor(SKPaint paint, bool useTransparent)
		{
			var alpha = GetAlpha(useTransparent);
			paint.Color = new SKColor(0x00, 0x00, 0xFF, alpha);
		}

		private static void ApplyColorShader(SKPaint paint, bool useTransparent)
		{
			// Install a different color in the paint, to ensure we're using the shader
			paint.Color = SKColors.Green;

			var alpha = GetAlpha(useTransparent);
			paint.Shader = SKShader.CreateColor(new SKColor(0x00, 0x00, 0xFF, alpha));
		}

		private static byte GetAlpha(bool useTransparent) =>
			useTransparent ? (byte)0x80 : (byte)0xFF;

		public static IEnumerable<object[]> GetAllBlendModes()
		{
			foreach (SKBlendMode mode in Enum.GetValues(typeof(SKBlendMode)))
				yield return new object[] { mode };
		}

		private static SKBlender GetRuntimeBlenderForBlendMode(SKBlendMode mode)
		{
			using var builder = SKRuntimeEffect.BuildBlender(
				"""
				uniform blender b;
				half4 main(half4 src, half4 dst) {
					return b.eval(src, dst);
				}
				""");

			Assert.NotNull(builder.Effect);

			builder.Children["b"] = SKBlender.CreateBlendMode(mode);

			return builder.Build();
		}
	}

	[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
	public unsafe class Gpu : SurfaceTestBase, IDisposable
	{
		GlContext glContext;
		GRContext grContext;

		public Gpu()
		{
			glContext = CreateGlContext();
			glContext.MakeCurrent();

			grContext = GRContext.CreateGl();

			CreateSurface(1, 1);
		}

		protected override void CreateSurface(int width, int height)
		{
			Surface?.Dispose();

			Info = new SKImageInfo(width, height, SKColorType.Rgba8888);
			Surface = SKSurface.Create(grContext, false, Info);
		}

		public void Dispose()
		{
			Surface.Dispose();
			grContext.Dispose();
			glContext.Destroy();
		}
	}

	public unsafe class Raster : SurfaceTestBase, IDisposable
	{
		public Raster()
		{
			CreateSurface(1, 1);
		}

		protected override void CreateSurface(int width, int height)
		{
			Surface?.Dispose();

			Info = new SKImageInfo(width, height, SKColorType.Rgba8888);
			Surface = SKSurface.Create(Info);
		}

		public void Dispose()
		{
			Surface.Dispose();
		}
	}
}
