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
						this.inputs[input.Key] = intVal;
					else if (input.Value is float[] floatArray)
						this.inputs[input.Key] = floatArray;
					else if (input.Value is float[][] floatArrayArray)
						this.inputs[input.Key] = floatArrayArray;
					else if (input.Value is int[] intArray)
						this.inputs[input.Key] = intArray;
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
			Assert.Equal(inputs, effectInputs);
		}

		[SkippableFact]
		public void RuntimeEffectInitializesCorrectChildren()
		{
			var src = @"
in fragmentProcessor color_map;
in fragmentProcessor normal_map;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var effectChildren = new SKRuntimeEffectChildren(effect);

			Assert.Equal(2, effectChildren.Count);
			Assert.Equal(2, effectChildren.Names.Count);
			Assert.Equal(new[] { "color_map", "normal_map" }, effectChildren.Names);
			Assert.Equal(new[] { "color_map", "normal_map" }, effectChildren);
		}

		[SkippableFact]
		public void RuntimeEffectChildrenWorksCorrectly()
		{
			using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
			using var textureShader = blueShirt.ToShader();

			var src = @"
in fragmentProcessor color_map;
in fragmentProcessor normal_map;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var children = new SKRuntimeEffectChildren(effect);

			children.Add("color_map", textureShader);
			Assert.Equal(new SKShader[] { textureShader, null }, children.ToArray());

			children.Add("normal_map", textureShader);
			Assert.Equal(new SKShader[] { textureShader, textureShader }, children.ToArray());

			children.Add("color_map", null);
			Assert.Equal(new SKShader[] { null, textureShader }, children.ToArray());

			children.Reset();
			Assert.Equal(new SKShader[] { null, null }, children.ToArray());
		}

		[SkippableFact]
		public void RuntimeEffectChildrenWorksCorrectlyWithCollectionInitializer()
		{
			using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
			using var textureShader = blueShirt.ToShader();

			var src = @"
in fragmentProcessor color_map;
in fragmentProcessor normal_map;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var children = new SKRuntimeEffectChildren(effect)
			{
				{ "color_map", textureShader },
			};
			Assert.Equal(new SKShader[] { textureShader, null }, children.ToArray());

			children = new SKRuntimeEffectChildren(effect)
			{
				{ "color_map", textureShader },
				{ "normal_map", textureShader },
			};
			Assert.Equal(new SKShader[] { textureShader, textureShader }, children.ToArray());

			children = new SKRuntimeEffectChildren(effect)
			{
				{ "normal_map", textureShader },
				{ "color_map", null },
			};
			Assert.Equal(new SKShader[] { null, textureShader }, children.ToArray());

			children = new SKRuntimeEffectChildren(effect)
			{
				{ "normal_map", null },
				{ "color_map", null },
			};
			Assert.Equal(new SKShader[] { null, null }, children.ToArray());
		}

		[SkippableFact]
		public void RuntimeEffectChildrenWorksCorrectlyWithCollectionIndexer()
		{
			using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
			using var textureShader = blueShirt.ToShader();

			var src = @"
in fragmentProcessor color_map;
in fragmentProcessor normal_map;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var childrent = new SKRuntimeEffectChildren(effect)
			{
				["color_map"] = textureShader,
			};
			Assert.Equal(new SKShader[] { textureShader, null }, childrent.ToArray());

			childrent = new SKRuntimeEffectChildren(effect)
			{
				["color_map"] = textureShader,
				["normal_map"] = textureShader,
			};
			Assert.Equal(new SKShader[] { textureShader, textureShader }, childrent.ToArray());

			childrent = new SKRuntimeEffectChildren(effect)
			{
				["normal_map"] = textureShader,
				["color_map"] = null,
			};
			Assert.Equal(new SKShader[] { null, textureShader }, childrent.ToArray());

			childrent = new SKRuntimeEffectChildren(effect)
			{
				["normal_map"] = null,
				["color_map"] = null,
			};
			Assert.Equal(new SKShader[] { null, null }, childrent.ToArray());
		}

		[SkippableFact]
		public void RuntimeEffectInputsWorksCorrectly()
		{
			var src = @"
uniform float input_float;
in int input_int;
uniform float input_float_array[2];
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var inputs = new SKRuntimeEffectInputs(effect);

			inputs.Add("input_float", 1f);
			inputs.Add("input_int", 1);
			inputs.Add("input_float_array", new[] { 1f, 2f });
			inputs.Reset();
		}

		[SkippableFact]
		public void RuntimeEffectInputsThrowsWhithInvalidName()
		{
			var src = @"
uniform float input_float;
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var inputs = new SKRuntimeEffectInputs(effect);

			Assert.Throws<ArgumentOutOfRangeException>(() => inputs.Add("invalid", 1f));
		}

		[SkippableTheory]
		[InlineData("input_float", true)]
		//[InlineData("input_float", 1)]
		//[InlineData("input_float", new[] { 1f })]
		[InlineData("input_int", true)]
		//[InlineData("input_int", 1f)]
		//[InlineData("input_int", new[] { 1f })]
		[InlineData("input_float_array", true)]
		[InlineData("input_float_array", 1)]
		[InlineData("input_float_array", 1f)]
		[InlineData("input_float_array", new[] { 1f })]
		public void RuntimeEffectInputsThrowsCorrectly(string name, object value)
		{
			var src = @"
uniform float input_float;
in int input_int;
uniform float input_float_array[2];
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var inputs = new SKRuntimeEffectInputs(effect);

			if (value is bool boolValue)
				Assert.Throws<ArgumentException>(() => inputs.Add(name, boolValue));
			else if (value is int intValue)
				Assert.Throws<ArgumentException>(() => inputs.Add(name, intValue));
			else if (value is float floatValue)
				Assert.Throws<ArgumentException>(() => inputs.Add(name, floatValue));
			else if (value is float[] floatArray)
				Assert.Throws<ArgumentException>(() => inputs.Add(name, floatArray));
			else
				throw new ArgumentException($"Invalid test data type {value}");
		}

		[SkippableFact]
		public void RuntimeEffectInputsWorksCorrectlyWithCollectionInitializer()
		{
			var src = @"
uniform float input_float;
in int input_int;
uniform float input_float_array[2];
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var inputs = new SKRuntimeEffectInputs(effect)
			{
				{ "input_float", 1f },
				{ "input_int", 1 },
				{ "input_float_array", new [] { 1f, 2f } },
			};
		}

		[SkippableFact]
		public void RuntimeEffectInputsWorksCorrectlyWithCollectionIndexer()
		{
			var src = @"
uniform float input_float;
in int input_int;
uniform float input_float_array[2];
void main(float2 p, inout half4 color) { }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var inputs = new SKRuntimeEffectInputs(effect)
			{
				["input_float"] = 1f,
				["input_int"] = 1,
				["input_float_array"] = new[] { 1f, 2f }
			};
		}

		[SkippableFact]
		public void InputIsConvertedFromBasicTypes()
		{
			var input = SKRuntimeEffectInput.Empty;
			Assert.True(input.IsEmpty);
			Assert.Equal(0, input.Size);

			var data = new byte[4];

			input = true;
			Assert.False(input.IsEmpty);
			Assert.Equal(1, input.Size);
			input.WriteTo(data);
			Assert.Equal(new byte[] { 1, 0, 0, 0 }, data);

			input = false;
			Assert.False(input.IsEmpty);
			Assert.Equal(1, input.Size);
			input.WriteTo(data);
			Assert.Equal(new byte[] { 0, 0, 0, 0 }, data);

			input = 5;
			Assert.False(input.IsEmpty);
			Assert.Equal(4, input.Size);
			input.WriteTo(data);
			Assert.Equal(new byte[] { 5, 0, 0, 0 }, data);

			input = 3f;
			Assert.False(input.IsEmpty);
			Assert.Equal(4, input.Size);
			input.WriteTo(data);
			Assert.Equal(new byte[] { 0, 0, 64, 64 }, data);

			data = new byte[8];

			input = new[] { 6f, 9f };
			Assert.False(input.IsEmpty);
			Assert.Equal(8, input.Size);
			input.WriteTo(data);
			Assert.Equal(new byte[] { 0, 0, 192, 64, 0, 0, 16, 65 }, data);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableTheory]
		[InlineData(1.05f, 1.5f, 0xFF000000, 0xFFE98404)]
		[InlineData(1.26f, 1.35f, 0xFF000000, 0xFF000000)]
		[InlineData(0f, 6.5f, 0xFFFFFFFF, 0xFFE98404)]
		public void ImageCanClearBackground(float threshold, float exponent, uint backgroundColor, uint shirtColor)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(500, 500, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Black);

			using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
			using var textureShader = blueShirt.ToShader();

			var src = @"
in fragmentProcessor color_map;

uniform float scale;
uniform half exp;
uniform float3 in_colors0;

void main(float2 p, inout half4 color) {
    half4 texColor = sample(color_map, p);
    if (length(abs(in_colors0 - pow(texColor.rgb, half3(exp)))) < scale)
        discard;
    color = texColor;
}";

			using var effect = SKRuntimeEffect.Create(src, out var errorText);
			Assert.Null(errorText);
			Assert.NotNull(effect);

			var inputSize = effect.InputSize;
			Assert.Equal(5 * sizeof(float), inputSize);

			var inputs = new SKRuntimeEffectInputs(effect)
			{
				["scale"] = threshold,
				["exp"] = exponent,
				["in_colors0"] = new[] { 1f, 1f, 1f },
			};

			var children = new SKRuntimeEffectChildren(effect)
			{
				["color_map"] = textureShader
			};

			using var shader = effect.ToShader(inputs, children, true);

			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(SKRect.Create(400, 400), paint);

			var actual = new SKColor[500 * 500];
			fixed (void* a = actual)
				Assert.True(surface.ReadPixels(info, (IntPtr)a, info.RowBytes, 0, 0));

			Assert.Equal(backgroundColor, actual[100 * info.Width + 100]);
			Assert.Equal(shirtColor, actual[230 * info.Width + 300]);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void ColorFiltersRunOnGpu()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(512, 256, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);
			using var canvas = surface.Canvas;

			var src = @"
void main(inout half4 color) {
    color.a = color.r*0.3 + color.g*0.6 + color.b*0.1;
    color.r = 0;
    color.g = 0;
    color.b = 0;
}";

			using var img = SKImage.FromEncodedData(Path.Combine(PathToImages, "baboon.jpg"));
			canvas.DrawImage(img, 0, 0);

			using var effect = SKRuntimeEffect.Create(src, out var errorText);
			Assert.Null(errorText);
			Assert.NotNull(effect);

			var cf1 = effect.ToColorFilter();
			using var p = new SKPaint
			{
				ColorFilter = cf1,
			};
			canvas.DrawImage(img, 256, 0, p);
		}
	}
}
