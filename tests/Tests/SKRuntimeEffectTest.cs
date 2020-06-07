using System;
using System.Collections.Generic;
using System.IO;
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
				yield return new object[]
				{
					"",
					"color = half4(half2(p - 0.5), 0, 1);",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF }
				};
				yield return new object[]
				{
					"uniform float4 gColor;",
					"color = half4(gColor);",
					new Dictionary<string, object>
					{
						{ "gColor", new [] { 0.0f, 0.25f, 0.75f, 1.0f } },
					},
					new SKColor[] { 0xFFBF4000, 0xFFBF4000, 0xFFBF4000, 0xFFBF4000 }
				};
				yield return new object[]
				{
					"uniform float4 gColor;",
					"color = half4(gColor);",
					new Dictionary<string, object>
					{
						{ "gColor", new [] { 0.75f, 0.25f, 0.0f, 1.0f } },
					},
					new SKColor[] { 0xFF0040BF, 0xFF0040BF, 0xFF0040BF, 0xFF0040BF }
				};
				yield return new object[]
				{
					"in int flag; uniform half4 gColors[2];",
					"color = gColors[flag];",
					new Dictionary<string, object>
					{
						{ "flag", 0 },
						{ "gColors", new [] { new[] { 1.0f, 0.0f, 0.0f, 0.498f } , new[] { 0.0f, 1.0f, 0.0f, 1.0f } } },
					},
					new SKColor[] { 0x7F00007F, 0x7F00007F, 0x7F00007F, 0x7F00007F }
				};
				yield return new object[]
				{
					"in int flag; uniform half4 gColors[2];",
					"color = gColors[flag];",
					new Dictionary<string, object>
					{
						{ "flag", 1 },
						{ "gColors", new [] { new[] { 1.0f, 0.0f, 0.0f, 0.498f } , new[] { 0.0f, 1.0f, 0.0f, 1.0f } } },
					},
					new SKColor[] { 0xFF00FF00, 0xFF00FF00, 0xFF00FF00, 0xFF00FF00 }
				};
			}
		}

		[SkippableTheory(Skip = "Shaders are not yet supported on raster surfaces.")]
		[MemberData(nameof(ShadersTestCaseData))]
		public void ShadersRunOnRaster(string hdr, string body, Dictionary<string, object> inputs, SKColor[] expected)
		{
			var info = new SKImageInfo(2, 2);
			using var surface = SKSurface.Create(info);

			using var effect = new TestEffect(hdr, body);

			effect.SetInputs(inputs);

			effect.Test(surface, info, expected);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableTheory]
		[MemberData(nameof(ShadersTestCaseData))]
		public void ShadersRunOnGpu(string hdr, string body, Dictionary<string, object> inputs, SKColor[] expected)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(2, 2, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);

			using var effect = new TestEffect(hdr, body);

			effect.SetInputs(inputs);

			effect.Test(surface, info, expected);
		}

		class TestEffect : IDisposable
		{
			private readonly SKRuntimeEffect effect;
			private readonly SKRuntimeEffectInputs inputs;

			public TestEffect(string header, string body)
			{
				var src = $"{header} void main(float2 p, inout half4 color) {{ {body} }}";
				effect = SKRuntimeEffect.Create(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);

				inputs = new SKRuntimeEffectInputs(effect);
			}

			public void Test(SKSurface surface, SKImageInfo info, SKColor[] expected)
			{
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

			public void SetInputs(Dictionary<string, object> inputs)
			{
				if (inputs == null)
					return;

				foreach (var input in inputs)
				{
					if (input.Value is int intVal)
						this.inputs.Set(input.Key, intVal);
					else if (input.Value is float[] floatArray)
						this.inputs.Set(input.Key, floatArray);
					else if (input.Value is float[][] floatArrayArray)
						this.inputs.Set(input.Key, floatArrayArray);
					else if (input.Value is int[] intArray)
						this.inputs.Set(input.Key, intArray);
				}
			}

			public void Dispose()
			{
				effect?.Dispose();
			}
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"in fragmentProcessor color_map;", new string[] { "color_map" })]
		[InlineData(@"in fragmentProcessor color_map; in fragmentProcessor normal_map;", new string[] { "color_map", "normal_map" })]
		public void CorrectChildrenAreListed(string header, string[] children)
		{
			var src = $"{header} void main(float2 p, inout half4 color) {{ }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			Assert.Equal(children, effect.Children);
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"in fragmentProcessor color_map;", new string[] { "color_map" })]
		[InlineData(@"in fragmentProcessor color_map; in fragmentProcessor normal_map;", new string[] { "color_map", "normal_map" })]
		public void RuntimeEffectChildrenMatchesEffect(string header, string[] children)
		{
			var src = $"{header} void main(float2 p, inout half4 color) {{ }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var effectChildren = new SKRuntimeEffectChildren(effect);

			Assert.Equal(children, effectChildren.Names);
			Assert.Equal(new SKShader[children.Length], effectChildren.ToArray());
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform float scale;", new string[] { "scale" })]
		[InlineData(@"uniform float scale; uniform half exp; uniform float3 in_colors0;", new string[] { "scale", "exp", "in_colors0" })]
		public void CorrectInputsAreListed(string header, string[] inputs)
		{
			var src = $"{header} void main(float2 p, inout half4 color) {{ }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			Assert.Equal(inputs, effect.Inputs);
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform float scale;", new string[] { "scale" })]
		[InlineData(@"uniform float scale; uniform half exp; uniform float3 in_colors0;", new string[] { "scale", "exp", "in_colors0" })]
		public void RuntimeEffectInputsMatchesEffect(string header, string[] inputs)
		{
			var src = $"{header} void main(float2 p, inout half4 color) {{ }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var effectInputs = new SKRuntimeEffectInputs(effect);

			Assert.Equal(inputs, effectInputs.Names);
			Assert.Equal(effect.InputSize, effectInputs.ToData().Size);
		}

		[SkippableFact]
		public void RuntimeEffectChildrenWorksCorrectly()
		{
			var src = @"
in fragmentProcessor color_map;
in fragmentProcessor normal_map;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var effectChildren = new SKRuntimeEffectChildren(effect);

			Assert.Equal(2, effectChildren.Count);
			Assert.Equal(2, effectChildren.Names.Count);

			using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
			using var textureShader = blueShirt.ToShader();

			effectChildren.Set("color_map", textureShader);
			Assert.Equal(new SKShader[] { textureShader, null }, effectChildren.ToArray());

			effectChildren.Set("normal_map", textureShader);
			Assert.Equal(new SKShader[] { textureShader, textureShader }, effectChildren.ToArray());

			effectChildren.Set("color_map", null);
			Assert.Equal(new SKShader[] { null, textureShader }, effectChildren.ToArray());

			effectChildren.Reset();
			Assert.Equal(new SKShader[] { null, null }, effectChildren.ToArray());
		}
	}
}
