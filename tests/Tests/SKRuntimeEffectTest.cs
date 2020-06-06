using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	public unsafe class SKRuntimeEffectTest : SKTest
	{
		[SkippableTheory]
		// Features that are only allowed in .fp files (key, in uniform, ctype, when, tracked).
		// Ensure that these fail, and the error messages contain the relevant keyword.
		[InlineData("layout(key) in bool Input;", "", "key")]
		[InlineData("in uniform float Input;", "", "in uniform")]
		[InlineData("layout(ctype=SkRect) float4 Input;", "", "ctype")]
		[InlineData("in bool Flag; layout(when=Flag) uniform float Input;", "", "when")]
		[InlineData("layout(tracked) uniform float Input;", "", "tracked")]
		// Runtime SkSL supports a limited set of uniform types. No samplers, for example:
		[InlineData("uniform sampler2D s;", "", "sampler2D")]
		// 'in' variables can't be arrays
		[InlineData("in int Input[2];", "", "array")]
		// Type specific restrictions:
		// 'bool', 'int' can't be 'uniform'
		[InlineData("uniform bool Input;", "", "'uniform'")]
		[InlineData("uniform int Input;", "", "'uniform'")]
		// vector and matrix types can't be 'in'
		[InlineData("in float2 Input;", "", "'in'")]
		[InlineData("in half3x3 Input;", "", "'in'")]
		[InlineData("half missing();", "color.r = missing();", "undefined function")]
		public void InvalidSourceFailsCreation(string hdr, string body, string expected)
		{
			var src = $"{hdr} void main(float2 p, inout half4 color) {{ {body} }}";

			using var effect = SKRuntimeEffect.Create(src, out var errorText);

			Assert.Null(effect);
			Assert.Contains(expected, errorText);
		}

		public static IEnumerable<object[]> ShadersTestCaseData
		{
			get
			{
				yield return new object[] { "", "color = half4(half2(p - 0.5), 0, 1);", new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF } };
			}
		}

		[SkippableTheory(Skip = "Shaders are not yet supported on raster surfaces.")]
		[MemberData(nameof(ShadersTestCaseData))]
		public void ShadersRunOnRaster(string hdr, string body, SKColor[] expected)
		{
			var info = new SKImageInfo(2, 2);
			using var surface = SKSurface.Create(info);

			RunShadersTest(surface, info, hdr, body, expected);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableTheory]
		[MemberData(nameof(ShadersTestCaseData))]
		public void ShadersRunOnGpu(string hdr, string body, SKColor[] expected)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(2, 2, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);

			RunShadersTest(surface, info, hdr, body, expected);
		}

		private static void RunShadersTest(SKSurface surface, SKImageInfo info, string hdr, string body, SKColor[] expected)
		{
			var src = $"{hdr} void main(float2 p, inout half4 color) {{ {body} }}";

			using var effect = SKRuntimeEffect.Create(src, out var errorText);

			Assert.Null(errorText);
			Assert.NotNull(effect);

			using var inputs = SKData.Create(effect.InputSize);

			using var shader = effect.ToShader(inputs, false);
			Assert.NotNull(shader);

			using var paint = new SKPaint
			{
				Shader = shader,
				BlendMode = SKBlendMode.Src
			};

			surface.Canvas.DrawPaint(paint);

			var actual = new SKColor[4];
			fixed (void* a = actual)
			{
				Assert.True(surface.ReadPixels(info, (IntPtr)a, info.RowBytes, 0, 0));
			}

			Assert.Equal(expected, actual);
		}
	}
}
