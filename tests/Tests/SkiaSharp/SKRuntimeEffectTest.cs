using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public unsafe class SKRuntimeEffectTest : SKTest
	{
		private const string EmptyMain = "half4 main() { return half4(0); }";

		[SkippableTheory]
		// Features that are only allowed in .fp files (key, in uniform, ctype, when, tracked).
		// Ensure that these fail, and the error messages contain the relevant keyword.
		[InlineData("layout(key) in bool Uniform;" + EmptyMain, "key")]
		[InlineData("in uniform float Uniform;" + EmptyMain, "in uniform")]
		[InlineData("layout(ctype=SkRect) float4 Uniform;" + EmptyMain, "ctype")]
		[InlineData("in bool Flag; layout(when=Flag) uniform float Uniform;" + EmptyMain, "when")]
		[InlineData("layout(tracked) uniform float Uniform;" + EmptyMain, "tracked")]
		// GLSL types like sampler2D and texture2D are not allowed anywhere:
		[InlineData("uniform sampler2D s;" + EmptyMain, "no type named 'sampler2D'")]
		[InlineData("uniform texture2D s;" + EmptyMain, "no type named 'texture2D'")]
		// Runtime SkSL supports a limited set of uniform types. No bool, or int, for example:
		[InlineData("uniform bool b;" + EmptyMain, "uniform")]
		[InlineData("uniform int i;" + EmptyMain, "uniform")]
		// 'in' variables aren't allowed at all:
		[InlineData("in bool b;" + EmptyMain, "'in'")]
		[InlineData("in float f;" + EmptyMain, "'in'")]
		[InlineData("in float2 v;" + EmptyMain, "'in'")]
		[InlineData("in half3x3 m;" + EmptyMain, "'in'")]
		// 'marker' is only permitted on float4x4 uniforms
		[InlineData("layout(marker=local_to_world) uniform float3x3 localToWorld;" + EmptyMain, "float4x4")]
		[InlineData("half4 missing(); half4 main() { return missing(); }", "undefined function")]
		// Shouldn't be possible to create an SkRuntimeEffect without "main"
		[InlineData("", "main")]
		// Various places that shaders (fragmentProcessors) should not be allowed
		[InlineData("half4 main() { shader child; return sample(child); }", "must be global")]
		[InlineData(
			"uniform shader child;" +
			"half4 helper(shader fp) { return sample(fp); }" +
			"half4 main() { return helper(child); }",
			"parameter")]
		[InlineData(
			"uniform shader child;" +
			"shader get_child() { return child; }" +
			"half4 main() { return sample(get_child()); }",
			"return")]
		[InlineData(
			"uniform shader child;" +
			"half4 main() { return sample(shader(child)); }",
			"construct")]
		[InlineData(
			"uniform shader child1; uniform shader child2;" +
			"half4 main(float2 p) { return sample(p.x > 10 ? child1 : child2); }",
			"expression")]
		// sk_Caps is an internal system. It should not be visible to runtime effects
		[InlineData(
			"half4 main() { return sk_Caps.integerSupport ? half4(1) : half4(0); }",
			"unknown identifier 'sk_Caps'")]
		// Errors that aren't caught until later in the compilation process (during optimize())
		[InlineData("half4 main() { return half4(1); return half4(0); }", "unreachable")]
		[InlineData(
			"half4 badFunc() {}" +
			"half4 main() { return badFunc(); }",
			"without returning")]
		public void InvalidSourceFailsCreation(string src, string expected)
		{
			using var effect = SKRuntimeEffect.Create(src, out var errorText);

			Assert.Null(effect);
			Assert.Contains(expected, errorText);
		}

		[SkippableTheory]
		// Runtime effects that use sample coords or sk_FragCoord are valid shaders,
		// but not valid color filters
		[InlineData("half4 main(float2 p) { return half2(p).xy01; }")]
		[InlineData("half4 main(float2 p) { return half2(sk_FragCoord.xy).xy01; }")]
		// We also can't use layout(marker), which would give the runtime color filter CTM information
		[InlineData(
			"layout(marker=ctm) uniform float4x4 ctm;" +
			"half4 main(float2 p) { return half4(half(ctm[0][0]), 0, 0, 1); }")]
		public void InvalidColorFilters(string src)
		{
			using var effect = SKRuntimeEffect.Create(src, out var errorText);

			var uniforms = new SKRuntimeEffectUniforms(effect);

			Assert.NotNull(effect.ToShader(false, uniforms));
			Assert.Null(effect.ToColorFilter(uniforms));
		}

		public static IEnumerable<object[]> ShadersUniformTestCaseData
		{
			get
			{
				// Local coords
				yield return new object[]
				{
					"half4 main(float2 p) { return half4(half2(p - 0.5), 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				// Use of a simple uniform. (Draw twice with two values to ensure it's updated).
				yield return new object[]
				{
					"uniform float4 gColor; half4 main() { return half4(gColor); }",
					new Dictionary<string, object>
					{
						{ "gColor", new [] { new[] { 0.0f, 0.25f, 0.75f, 1.0f } } },
					},
					new SKColor[] { 0xFFBF4000, 0xFFBF4000, 0xFFBF4000, 0xFFBF4000 },
					null
				};
				// Tests that we clamp to valid premul
				yield return new object[]
				{
					"uniform float4 gColor; half4 main() { return half4(gColor); }",
					new Dictionary<string, object>
					{
						{ "gColor", new [] { new[] { 1.0f, 0.0f, 0.0f, 0.498f } } },
					},
					new SKColor[] { 0x7F00007F, 0x7F00007F, 0x7F00007F, 0x7F00007F },
					null
				};
				// Test sk_FragCoord (device coords). Rotate the canvas to be sure we're seeing device coords.
				// Since the surface is 2x2, we should see (0,0), (1,0), (0,1), (1,1). Multiply by 0.498 to
				// make sure we're not saturating unexpectedly.
				yield return new object[]
				{
					"half4 main() { return half4(0.498 * (half2(sk_FragCoord.xy) - 0.5), 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF00007F, 0xFF007F00, 0xFF007F7F },
					new Action<SKCanvas, SKPaint>((canvas, paint) => canvas.RotateDegrees(45)),
				};
				// Runtime effects should use relaxed precision rules by default
				yield return new object[]
				{
					"half4 main(float2 p) { return float4(p - 0.5, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				// ... and support GLSL type names
				yield return new object[]
				{
					"half4 main(float2 p) { return vec4(p - 0.5, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				// ... and support *returning* float4 (aka vec4), not just half4
				yield return new object[]
				{
					"float4 main(float2 p) { return float4(p - 0.5, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				yield return new object[]
				{
					"vec4 main(float2 p) { return float4(p - 0.5, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				// Mutating coords should work. (skbug.com/10918)
				yield return new object[]
				{
					"vec4 main(vec2 p) { p -= 0.5; return vec4(p, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				yield return new object[]
				{
					"void moveCoords(inout vec2 p) { p -= 0.5; } vec4 main(vec2 p) { moveCoords(p); return vec4(p, 0, 1); }",
					null,
					new SKColor[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF },
					null
				};
				// Test case for inlining in the pipeline-stage and fragment-shader passes (skbug.com/10526):
				yield return new object[]
				{
					"float2 helper(float2 x) { return x + 1; }" +
					"half4 main(float2 p) { float2 v = helper(p); return half4(half2(v), 0, 1); }",
					null,
					new SKColor[] { 0xFF00FFFF, 0xFF00FFFF, 0xFF00FFFF, 0xFF00FFFF },
					null,
				};
			}
		}

		// Produces a 2x2 bitmap shader, with opaque colors:
		// [ Red,  Lime  ]
		// [ Blue, White ]
		private static SKShader RedGreenBlueWhiteShader
		{
			get
			{
				using var bmp = new SKBitmap(new SKImageInfo(2, 2, SKColorType.Rgba8888));
				bmp.SetPixel(0, 0, 0xFFFF0000);
				bmp.SetPixel(1, 0, 0xFF00FF00);
				bmp.SetPixel(0, 1, 0xFF0000FF);
				bmp.SetPixel(1, 1, 0xFFFFFFFF);
				return bmp.ToShader();
			}
		}

		public static IEnumerable<object[]> ShadersChildrenTestCaseData
		{
			get
			{
				// Sampling a null child should return the paint color
				yield return new object[]
				{
					"uniform shader child; half4 main() { return sample(child); }",
					new Dictionary<string, SKShader>
					{
						{ "child",  null },
					},
					new SKColor[] { 0xFF00FFFF, 0xFF00FFFF, 0xFF00FFFF, 0xFF00FFFF },
					new Action<SKCanvas, SKPaint>((canvas, paint) => paint.ColorF = new SKColorF(1.0f, 1.0f, 0.0f, 1.0f)),
				};
				// Sampling a simple child at our coordinates (implicitly)
				yield return new object[]
				{
					"uniform shader child; half4 main() { return sample(child); }",
					new Dictionary<string, SKShader>
					{
						{ "child",  RedGreenBlueWhiteShader },
					},
					new SKColor[] { 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF },
					null,
				};
				// Sampling with explicit coordinates (reflecting about the diagonal)
				yield return new object[]
				{
					"uniform shader child; half4 main(float2 p) { return sample(child, p.yx); }",
					new Dictionary<string, SKShader>
					{
						{ "child",  RedGreenBlueWhiteShader },
					},
					new SKColor[] { 0xFF0000FF, 0xFFFF0000, 0xFF00FF00, 0xFFFFFFFF },
					null,
				};
				// Sampling with a matrix (again, reflecting about the diagonal)
				yield return new object[]
				{
					"uniform shader child; half4 main() { return sample(child, float3x3(0, 1, 0, 1, 0, 0, 0, 0, 1)); }",
					new Dictionary<string, SKShader>
					{
						{ "child",  RedGreenBlueWhiteShader },
					},
					new SKColor[] { 0xFF0000FF, 0xFFFF0000, 0xFF00FF00, 0xFFFFFFFF },
					null,
				};
				// Legacy behavior - shaders can be declared 'in' rather than 'uniform'
				yield return new object[]
				{
					"in shader child; half4 main() { return sample(child); }",
					new Dictionary<string, SKShader>
					{
						{ "child",  RedGreenBlueWhiteShader },
					},
					new SKColor[] { 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF },
					null,
				};
			}
		}

		private class TestEffect : IDisposable
		{
			private readonly SKRuntimeEffect effect;
			private readonly SKRuntimeEffectUniforms uniforms;
			private readonly SKRuntimeEffectChildren children;

			public TestEffect(string src)
			{
				effect = SKRuntimeEffect.Create(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);

				uniforms = new SKRuntimeEffectUniforms(effect);
				children = new SKRuntimeEffectChildren(effect);
			}

			public void Test(SKSurface surface, SKImageInfo info, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
			{
				using var shader = effect.ToShader(false, uniforms, children);
				Assert.NotNull(shader);

				using var paint = new SKPaint
				{
					Shader = shader,
					BlendMode = SKBlendMode.Src
				};

				preTestCallback?.Invoke(surface.Canvas, paint);

				surface.Canvas.DrawPaint(paint);

				var actual = new SKColor[4];
				fixed (void* a = actual)
				{
					Assert.True(surface.ReadPixels(info, (IntPtr)a, info.RowBytes, 0, 0));
				}

				Assert.Equal(expected, actual);
			}

			public void SetUniforms(Dictionary<string, object> uniforms)
			{
				if (uniforms == null)
					return;

				foreach (var uniform in uniforms)
				{
					if (uniform.Value is float floatVal)
						this.uniforms[uniform.Key] = floatVal;
					else if (uniform.Value is float[] floatArray)
						this.uniforms[uniform.Key] = floatArray;
					else if (uniform.Value is float[][] floatArrayArray)
						this.uniforms[uniform.Key] = floatArrayArray;
				}
			}

			public void SetChildren(Dictionary<string, SKShader> children)
			{
				if (children == null)
					return;

				foreach (var child in children)
				{
					this.children[child.Key] = child.Value;
				}
			}

			public void Dispose() =>
				effect?.Dispose();
		}

		[SkippableTheory(Skip = "Shaders are not yet supported on raster surfaces.")]
		[MemberData(nameof(ShadersUniformTestCaseData))]
		public void ShadersWithUniformRunOnRaster(string src, Dictionary<string, object> uniforms, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
		{
			var info = new SKImageInfo(2, 2);
			using var surface = SKSurface.Create(info);

			using var effect = new TestEffect(src);

			effect.SetUniforms(uniforms);

			effect.Test(surface, info, expected, preTestCallback);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableTheory]
		[MemberData(nameof(ShadersUniformTestCaseData))]
		public void ShadersWithUniformRunOnGpu(string src, Dictionary<string, object> uniforms, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(2, 2, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);

			using var effect = new TestEffect(src);

			effect.SetUniforms(uniforms);

			effect.Test(surface, info, expected, preTestCallback);
		}

		[SkippableTheory(Skip = "Shaders are not yet supported on raster surfaces.")]
		[MemberData(nameof(ShadersChildrenTestCaseData))]
		public void ShadersWithChildrenRunOnRaster(string src, Dictionary<string, SKShader> children, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
		{
			var info = new SKImageInfo(2, 2);
			using var surface = SKSurface.Create(info);

			using var effect = new TestEffect(src);

			effect.SetChildren(children);

			effect.Test(surface, info, expected, preTestCallback);
		}

		[Trait(CategoryKey, GpuCategory)]
		[SkippableTheory]
		[MemberData(nameof(ShadersChildrenTestCaseData))]
		public void ShadersWithChildrenRunOnGpu(string src, Dictionary<string, SKShader> children, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();

			var info = new SKImageInfo(2, 2, SKColorType.Rgba8888);
			using var surface = SKSurface.Create(grContext, false, info);

			using var effect = new TestEffect(src);

			effect.SetChildren(children);

			effect.Test(surface, info, expected, preTestCallback);
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform shader color_map;", new string[] { "color_map" })]
		[InlineData(@"uniform shader color_map; uniform shader normal_map;", new string[] { "color_map", "normal_map" })]
		public void CorrectChildrenAreListed(string header, string[] children)
		{
			var src = $"{header} half4 main() {{ return half4(0); }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			Assert.Equal(children, effect.Children);
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform shader color_map;", new string[] { "color_map" })]
		[InlineData(@"uniform shader color_map; uniform shader normal_map;", new string[] { "color_map", "normal_map" })]
		public void RuntimeEffectChildrenMatchesEffect(string header, string[] children)
		{
			var src = $"{header} half4 main() {{ return half4(0); }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var effectChildren = new SKRuntimeEffectChildren(effect);

			Assert.Equal(children, effectChildren.Names);
			Assert.Equal(new SKShader[children.Length], effectChildren.ToArray());
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform float scale;", new string[] { "scale" })]
		[InlineData(@"uniform float scale; uniform half exp; uniform float3 in_colors0;", new string[] { "scale", "exp", "in_colors0" })]
		public void CorrectUniformsAreListed(string header, string[] uniforms)
		{
			var src = $"{header} half4 main() {{ return half4(0); }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			Assert.Equal(uniforms, effect.Uniforms);
		}

		[SkippableTheory]
		[InlineData(@"", new string[0])]
		[InlineData(@"uniform float scale;", new string[] { "scale" })]
		[InlineData(@"uniform float scale; uniform half exp; uniform float3 in_colors0;", new string[] { "scale", "exp", "in_colors0" })]
		public void RuntimeEffectUniformsMatchesEffect(string header, string[] uniforms)
		{
			var src = $"{header} half4 main() {{ return half4(0); }}";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var effectUniforms = new SKRuntimeEffectUniforms(effect);

			Assert.Equal(uniforms, effectUniforms.Names);
			Assert.Equal(effect.UniformSize, effectUniforms.ToData().Size);
			Assert.Equal(uniforms, effectUniforms);
		}

		[SkippableFact]
		public void RuntimeEffectInitializesCorrectChildren()
		{
			var src = @"
uniform shader color_map;
uniform shader normal_map;
half4 main() { return half4(0); }";

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
uniform shader color_map;
uniform shader normal_map;
half4 main() { return half4(0); }";

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
uniform shader color_map;
uniform shader normal_map;
half4 main() { return half4(0); }";

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
uniform shader color_map;
uniform shader normal_map;
half4 main() { return half4(0); }";

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
		public void RuntimeEffectUniformsWorksCorrectly()
		{
			var src = @"
uniform float uniform_float;
uniform float uniform_float_array[2];
uniform float2 uniform_float2;
half4 main() { return half4(0); }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var uniforms = new SKRuntimeEffectUniforms(effect);

			uniforms.Add("uniform_float", 1f);
			uniforms.Add("uniform_float_array", new[] { 1f, 2f });
			uniforms.Add("uniform_float2", new[] { 1f, 2f });
			uniforms.Reset();
		}

		[SkippableFact]
		public void RuntimeEffectUniformsThrowsWhithInvalidName()
		{
			var src = @"
uniform float uniform_float;
half4 main() { return half4(0); }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var uniforms = new SKRuntimeEffectUniforms(effect);

			Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add("invalid", 1f));
		}

		[SkippableTheory]
		//[InlineData("uniform_float", 1)]
		//[InlineData("uniform_float", new[] { 1f })]
		[InlineData("uniform_float_array", 1)]
		[InlineData("uniform_float_array", 1f)]
		[InlineData("uniform_float_array", new[] { 1f })]
		public void RuntimeEffectUniformsThrowsCorrectly(string name, object value)
		{
			var src = @"
uniform float uniform_float;
uniform float uniform_float_array[2];
half4 main() { return half4(0); }";

			using var effect = SKRuntimeEffect.Create(src, out _);
			var uniforms = new SKRuntimeEffectUniforms(effect);

			if (value is int intValue)
				Assert.Throws<ArgumentException>(() => uniforms.Add(name, intValue));
			else if (value is float floatValue)
				Assert.Throws<ArgumentException>(() => uniforms.Add(name, floatValue));
			else if (value is float[] floatArray)
				Assert.Throws<ArgumentException>(() => uniforms.Add(name, floatArray));
			else
				throw new ArgumentException($"Invalid test data type {value}");
		}

		[SkippableFact]
		public void RuntimeEffectUniformsWorksCorrectlyWithCollectionInitializer()
		{
			var src = @"
uniform float uniform_float;
uniform float uniform_float_array[2];
uniform float2 uniform_float2;
half4 main() { return half4(0); }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var uniforms = new SKRuntimeEffectUniforms(effect)
			{
				{ "uniform_float", 1f },
				{ "uniform_float_array", new [] { 1f, 2f } },
				{ "uniform_float2", new [] { 1f, 2f } },
			};
		}

		[SkippableFact]
		public void RuntimeEffectUniformsWorksCorrectlyWithCollectionIndexer()
		{
			var src = @"
uniform float uniform_float;
uniform float uniform_float_array[2];
uniform float2 uniform_float2;
half4 main() { return half4(0); }";

			using var effect = SKRuntimeEffect.Create(src, out _);

			var uniforms = new SKRuntimeEffectUniforms(effect)
			{
				["uniform_float"] = 1f,
				["uniform_float_array"] = new[] { 1f, 2f },
				["uniform_float2"] = new[] { 1f, 2f }
			};
		}

		[SkippableFact]
		public void UniformIsConvertedFromBasicTypes()
		{
			var uniform = SKRuntimeEffectUniform.Empty;
			Assert.True(uniform.IsEmpty);
			Assert.Equal(0, uniform.Size);

			var data = new byte[4];

			uniform = 3;
			Assert.False(uniform.IsEmpty);
			Assert.Equal(4, uniform.Size);
			uniform.WriteTo(data);
			Assert.Equal(new byte[] { 0, 0, 64, 64 }, data);

			uniform = 3f;
			Assert.False(uniform.IsEmpty);
			Assert.Equal(4, uniform.Size);
			uniform.WriteTo(data);
			Assert.Equal(new byte[] { 0, 0, 64, 64 }, data);

			data = new byte[8];

			uniform = new[] { 6f, 9f };
			Assert.False(uniform.IsEmpty);
			Assert.Equal(8, uniform.Size);
			uniform.WriteTo(data);
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
uniform shader color_map;
uniform float scale;
uniform half exp;
uniform float3 in_colors0;
half4 main(float2 p) {
    half4 texColor = sample(color_map, p);
    if (length(abs(in_colors0 - pow(texColor.rgb, half3(exp)))) < scale)
        discard;
    return texColor;
}";

			using var effect = SKRuntimeEffect.Create(src, out var errorText);
			Assert.Null(errorText);
			Assert.NotNull(effect);

			var uniformSize = effect.UniformSize;
			Assert.Equal(5 * sizeof(float), uniformSize);

			var uniforms = new SKRuntimeEffectUniforms(effect)
			{
				["scale"] = threshold,
				["exp"] = exponent,
				["in_colors0"] = new[] { 1f, 1f, 1f },
			};

			var children = new SKRuntimeEffectChildren(effect)
			{
				["color_map"] = textureShader
			};

			using var shader = effect.ToShader(true, uniforms, children);

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
uniform shader input;
half4 main() {
    return sample(input);
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
