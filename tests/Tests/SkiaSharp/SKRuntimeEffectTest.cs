using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public unsafe abstract class SKRuntimeEffectTest : SKTest
	{
		private const string EmptyMain =
			"""
			half4 main(float2 p) {
				return half4(0);
			}
			""";

		public unsafe class Generic : SKRuntimeEffectTest
		{
			[SkippableFact]
			public void UniformIsConvertedFromEmpty()
			{
				var uniform = SKRuntimeEffectUniform.Empty;

				Assert.True(uniform.IsEmpty);
				Assert.Equal(0, uniform.Size);

				var data = new byte[4];
				uniform.WriteTo(data);

				Assert.Equal(new byte[] { 0, 0, 0, 0 }, data);
			}

			[SkippableTheory]
			[InlineData(3, new byte[] { 3, 0, 0, 0 })]
			[InlineData(255, new byte[] { 255, 0, 0, 0 })]
			[InlineData(511, new byte[] { 255, 1, 0, 0 })]
			public void UniformIsConvertedFromInt32(int value, byte[] expectedData)
			{
				SKRuntimeEffectUniform uniform = value;

				Assert.False(uniform.IsEmpty);
				Assert.Equal(4, uniform.Size);

				var data = new byte[4];
				uniform.WriteTo(data);

				Assert.Equal(expectedData, data);
			}

			[SkippableTheory]
			[InlineData(3f, new byte[] { 0, 0, 64, 64 })]
			[InlineData(255f, new byte[] { 0, 0, 127, 67 })]
			[InlineData(511f, new byte[] { 0, 128, 255, 67 })]
			public void UniformIsConvertedFromFloat(float value, byte[] expectedData)
			{
				SKRuntimeEffectUniform uniform = value;

				Assert.False(uniform.IsEmpty);
				Assert.Equal(4, uniform.Size);

				var data = new byte[4];
				uniform.WriteTo(data);

				Assert.Equal(expectedData, data);
			}

			[SkippableTheory]
			[InlineData(new[] { 6, 9 }, new byte[] { 6, 0, 0, 0, 9, 0, 0, 0 })]
			[InlineData(new[] { 255, 511 }, new byte[] { 255, 0, 0, 0, 255, 1, 0, 0 })]
			public void UniformIsConvertedFromInt32Array(int[] value, byte[] expectedData)
			{
				SKRuntimeEffectUniform uniform = value;

				Assert.False(uniform.IsEmpty);
				Assert.Equal(8, uniform.Size);

				var data = new byte[8];
				uniform.WriteTo(data);

				Assert.Equal(expectedData, data);
			}

			[SkippableTheory]
			[InlineData(new[] { 6f, 9f }, new byte[] { 0, 0, 192, 64, 0, 0, 16, 65 })]
			[InlineData(new[] { 255f, 511f }, new byte[] { 0, 0, 127, 67, 0, 128, 255, 67 })]
			public void UniformIsConvertedFromFloatArray(float[] value, byte[] expectedData)
			{
				SKRuntimeEffectUniform uniform = value;

				Assert.False(uniform.IsEmpty);
				Assert.Equal(8, uniform.Size);

				var data = new byte[8];
				uniform.WriteTo(data);

				Assert.Equal(expectedData, data);
			}

			[SkippableTheory]
			[InlineData(0xFFFF0000, 3, new byte[] { 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0 })]
			[InlineData(0xFFFF0000, 4, new byte[] { 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63 })]
			[InlineData(0xFFFF69B4, 3, new byte[] { 0, 0, 128, 63, 212, 210, 210, 62, 181, 180, 52, 63 })]
			[InlineData(0xFFFF69B4, 4, new byte[] { 0, 0, 128, 63, 212, 210, 210, 62, 181, 180, 52, 63, 0, 0, 128, 63 })]
			[InlineData(0xFF228B22, 3, new byte[] { 137, 136, 8, 62, 140, 139, 11, 63, 137, 136, 8, 62 })]
			[InlineData(0xFF228B22, 4, new byte[] { 137, 136, 8, 62, 140, 139, 11, 63, 137, 136, 8, 62, 0, 0, 128, 63 })]
			public void UniformIsConvertedFromColor(uint value, int components, byte[] expectedData)
			{
				SKColor color = value;
				SKRuntimeEffectUniform uniform = color;

				Assert.False(uniform.IsEmpty);
				Assert.Equal(4 * sizeof(float), uniform.Size); // size is always the full color

				var data = new byte[components * sizeof(float)]; // but we can write to smaller
				uniform.WriteTo(data);

				Assert.Equal(expectedData, data);
			}

			[SkippableTheory]
			[InlineData(0xFFFF0000, 0)]
			[InlineData(0xFFFF0000, 1)]
			[InlineData(0xFFFF0000, 2)]
			[InlineData(0xFFFF0000, 5)]
			public void UniformDoesNotSupportInvalidColorComponentCounts(uint value, int components)
			{
				SKColor color = value;
				var data = new byte[components * sizeof(float)];

				Assert.Throws<ArgumentOutOfRangeException>(() =>
				{
					SKRuntimeEffectUniform uniform = color;
					uniform.WriteTo(data);
				});
			}
		}

		public unsafe class Shaders : SKRuntimeEffectTest
		{
			[SkippableTheory]
			[InlineData("in bool b;", "'in'")]
			public void InVariablesFailCreation(string prefix, string errorContains)
			{
				var src = $"""
				{prefix}
				{EmptyMain}
				""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(effect);
				Assert.Contains(errorContains, errorText);
			}

			[SkippableFact]
			public void UndefinedFunctionsFailsCreation()
			{
				using var effect = SKRuntimeEffect.CreateShader("", out var errorText);

				Assert.Null(effect);
				Assert.Contains("main", errorText);
			}

			[SkippableFact]
			public void MissingMainFailsCreation()
			{
				var src = """
				half4 missing();
				half4 main(float2 p) {
					return missing();
				}
				""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(effect);
				Assert.Contains("function 'half4 missing()' is not defined", errorText);
			}

			[SkippableTheory]
			[InlineData("", "construction of array type")]
			[InlineData("#version 100", "construction of array type")]
			[InlineData("#version 300", null)]
			public void VersionsCanControlFeatures(string version, string errorContains)
			{
				var src = $"""
				{version}
				float f[2] = float[2](0, 1);
				{EmptyMain}
				""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				if (errorContains is null)
				{
					Assert.NotNull(effect);
					Assert.Null(errorText);
				}
				else
				{
					Assert.Null(effect);
					Assert.Contains(errorContains, errorText);
				}
			}

			[SkippableTheory]
			[InlineData("half4  main(float2 p) { return p.xyxy; }")]
			[InlineData("float4 main(float2 p) { return p.xyxy; }")]
			[InlineData("vec4   main(float2 p) { return p.xyxy; }")]
			[InlineData("half4  main(vec2   p) { return p.xyxy; }")]
			[InlineData("vec4   main(vec2   p) { return p.xyxy; }")]
			public void SupportAllSize2InMainReturnSize4(string src)
			{
				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);
			}

			[SkippableTheory]
			// The 'half4 main(float2, half4|float4)' signature is disallowed
			[InlineData("half4  main(float2 p, half4  c) { return c; }", "'main' parameter")]
			[InlineData("half4  main(float2 p, float4 c) { return c; }", "'main' parameter")]
			[InlineData("half4  main(float2 p, vec4   c) { return c; }", "'main' parameter")]
			[InlineData("float4 main(float2 p, half4  c) { return c; }", "'main' parameter")]
			[InlineData("vec4   main(float2 p, half4  c) { return c; }", "'main' parameter")]
			[InlineData("vec4   main(vec2   p, vec4   c) { return c; }", "'main' parameter")]
			// Invalid return types
			[InlineData("void  main(float2 p) {}", "'main' must return")]
			[InlineData("half3 main(float2 p) { return p.xy1; }", "'main' must return")]
			// Invalid argument types (some are valid as color filters, but not shaders)
			[InlineData("half4 main() { return half4(1); }", "'main' parameter")]
			[InlineData("half4 main(half4 c) { return c; }", "'main' parameter")]
			// sk_FragCoord should be available
			[InlineData("half4 main(float2 p) { return sk_FragCoord.xy01; }", "unknown identifier 'sk_FragCoord'")]
			public void InvalidFailCreation(string src, string errorContains)
			{
				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(effect);
				Assert.Contains(errorContains, errorText);
			}

			[SkippableTheory]
			[InlineData("shader", "p")]
			[InlineData("colorFilter", "half4(1)")]
			[InlineData("blender", "half4(0.5), half4(0.6)")]
			public void SupportDifferentUniforms(string uniform, string usage)
			{
				var src = $$"""
				uniform {{uniform}} child;
				half4 main(float2 p) {
					return child.eval({{usage}});
				}
				""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);
			}

			[SkippableTheory]
			[InlineData(@"", new string[0])]
			[InlineData(@"uniform shader color_map;", new string[] { "color_map" })]
			[InlineData(@"uniform shader color_map; uniform shader normal_map;", new string[] { "color_map", "normal_map" })]
			public void CorrectChildrenAreListed(string header, string[] children)
			{
				var src = $"""
					{header}
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(errorText);
				Assert.Equal(children, effect.Children);
			}

			[SkippableTheory]
			[InlineData(@"", new string[0])]
			[InlineData(@"uniform shader color_map;", new string[] { "color_map" })]
			[InlineData(@"uniform shader color_map; uniform shader normal_map;", new string[] { "color_map", "normal_map" })]
			public void ChildrenMatchesEffect(string header, string[] children)
			{
				var src = $"""
					{header}
					{EmptyMain}
					""";

				using var builder = SKRuntimeEffect.BuildShader(src);

				Assert.Equal(children, builder.Children.Names);
				Assert.Equal(new SKShader[children.Length], builder.Children.ToArray());
			}

			[SkippableTheory]
			[InlineData(@"", new string[0])]
			[InlineData(@"uniform float scale;", new string[] { "scale" })]
			[InlineData(@"uniform float scale; uniform half in_exp; uniform float3 in_colors0;", new string[] { "scale", "in_exp", "in_colors0" })]
			public void CorrectUniformsAreListed(string header, string[] uniforms)
			{
				var src = $"""
					{header}
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);

				Assert.Null(errorText);
				Assert.Equal(uniforms, effect.Uniforms);
			}

			[SkippableTheory]
			[InlineData(@"", new string[0])]
			[InlineData(@"uniform float scale;", new string[] { "scale" })]
			[InlineData(@"uniform float scale; uniform half in_exp; uniform float3 in_colors0;", new string[] { "scale", "in_exp", "in_colors0" })]
			public void UniformsMatchesEffect(string header, string[] uniforms)
			{
				var src = $"""
					{header}
					{EmptyMain}
					""";

				using var builder = SKRuntimeEffect.BuildShader(src);

				Assert.Equal(uniforms, builder.Uniforms.Names);
				Assert.Equal(builder.Effect.UniformSize, builder.Uniforms.Size);
				Assert.Equal(builder.Effect.UniformSize, builder.Uniforms.ToData().Size);
				Assert.Equal(uniforms, builder.Uniforms);
			}

			[SkippableFact]
			public void ChildrenWorksCorrectly()
			{
				using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
				using var textureShader = blueShirt.ToShader();

				var src = $"""
					uniform shader color_map;
					uniform shader normal_map;
					{EmptyMain}
					""";

				using var builder = SKRuntimeEffect.BuildShader(src);

				builder.Children.Add("color_map", textureShader);
				Assert.Equal(new SKObject[] { textureShader, null }, builder.Children.ToArray());

				builder.Children.Add("normal_map", textureShader);
				Assert.Equal(new SKObject[] { textureShader, textureShader }, builder.Children.ToArray());

				builder.Children.Add("color_map", null);
				Assert.Equal(new SKObject[] { null, textureShader }, builder.Children.ToArray());

				builder.Children.Reset();
				Assert.Equal(new SKObject[] { null, null }, builder.Children.ToArray());
			}

			[SkippableFact]
			public void ChildrenWorksCorrectlyWithCollectionInitializer()
			{
				using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
				using var textureShader = blueShirt.ToShader();

				var src = $"""
					uniform shader color_map;
					uniform shader normal_map;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var children = new SKRuntimeEffectChildren(effect)
				{
					{ "color_map", textureShader },
				};
				Assert.Equal(new SKObject[] { textureShader, null }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					{ "color_map", textureShader },
					{ "normal_map", textureShader },
				};
				Assert.Equal(new SKObject[] { textureShader, textureShader }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					{ "normal_map", textureShader },
					{ "color_map", null },
				};
				Assert.Equal(new SKObject[] { null, textureShader }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					{ "normal_map", null },
					{ "color_map", null },
				};
				Assert.Equal(new SKObject[] { null, null }, children.ToArray());
			}

			[SkippableFact]
			public void ChildrenWorksCorrectlyWithCollectionIndexer()
			{
				using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
				using var textureShader = blueShirt.ToShader();

				var src = $"""
					uniform shader color_map;
					uniform shader normal_map;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var children = new SKRuntimeEffectChildren(effect)
				{
					["color_map"] = textureShader,
				};
				Assert.Equal(new SKObject[] { textureShader, null }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					["color_map"] = textureShader,
					["normal_map"] = textureShader,
				};
				Assert.Equal(new SKObject[] { textureShader, textureShader }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					["normal_map"] = textureShader,
					["color_map"] = null,
				};
				Assert.Equal(new SKObject[] { null, textureShader }, children.ToArray());

				children = new SKRuntimeEffectChildren(effect)
				{
					["normal_map"] = null,
					["color_map"] = null,
				};
				Assert.Equal(new SKObject[] { null, null }, children.ToArray());
			}

			[SkippableTheory]
			[InlineData("uniform_int", 1)]
			[InlineData("uniform_int_array", new[] { 1, 1 })]
			[InlineData("uniform_int2", new[] { 1, 1 })]
			[InlineData("uniform_float", 1f)]
			[InlineData("uniform_float_array", new[] { 1f, 1f })]
			[InlineData("uniform_float2", new[] { 1f, 1f })]
			public void UniformsWorksCorrectly(string name, object value)
			{
				var src = $"""
					uniform int uniform_int;
					uniform int uniform_int_array[2];
					uniform int2 uniform_int2;
					uniform float uniform_float;
					uniform float uniform_float_array[2];
					uniform float2 uniform_float2;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var uniforms = new SKRuntimeEffectUniforms(effect);

				if (value is int intValue)
					uniforms.Add(name, intValue);
				else if (value is int[] intArray)
					uniforms.Add(name, intArray);
				else if (value is float floatValue)
					uniforms.Add(name, floatValue);
				else if (value is float[] floatArray)
					uniforms.Add(name, floatArray);
				else
					throw new ArgumentException($"Invalid test data type {value}");
			}

			[SkippableFact]
			public void UniformsThrowsWithInvalidName()
			{
				var src = $"""
					uniform float uniform_float;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var uniforms = new SKRuntimeEffectUniforms(effect);

				Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add("invalid", 1f));
			}

			[SkippableTheory]
			[InlineData("uniform_int", 1f)]
			[InlineData("uniform_int", new[] { 1 })]
			[InlineData("uniform_int", new[] { 1f })]
			[InlineData("uniform_int_array", 1)]
			[InlineData("uniform_int_array", 1f)]
			[InlineData("uniform_int_array", new[] { 1 })]
			[InlineData("uniform_int_array", new[] { 1, 1, 1 })]
			[InlineData("uniform_int_array", new[] { 1f })]
			[InlineData("uniform_int_array", new[] { 1f, 1f })]
			[InlineData("uniform_int_array", new[] { 1f, 1f, 1f })]
			[InlineData("uniform_int2", 1)]
			[InlineData("uniform_int2", 1f)]
			[InlineData("uniform_int2", new[] { 1 })]
			[InlineData("uniform_int2", new[] { 1, 1, 1 })]
			[InlineData("uniform_int2", new[] { 1f })]
			[InlineData("uniform_int2", new[] { 1f, 1f })]
			[InlineData("uniform_int2", new[] { 1f, 1f, 1f })]
			[InlineData("uniform_float", 1)]
			[InlineData("uniform_float", new[] { 1 })]
			[InlineData("uniform_float", new[] { 1f })]
			[InlineData("uniform_float_array", 1)]
			[InlineData("uniform_float_array", 1f)]
			[InlineData("uniform_float_array", new[] { 1f })]
			[InlineData("uniform_float_array", new[] { 1f, 1f, 1f })]
			[InlineData("uniform_float_array", new[] { 1 })]
			[InlineData("uniform_float_array", new[] { 1, 1 })]
			[InlineData("uniform_float_array", new[] { 1, 1, 1 })]
			[InlineData("uniform_float2", 1)]
			[InlineData("uniform_float2", 1f)]
			[InlineData("uniform_float2", new[] { 1f })]
			[InlineData("uniform_float2", new[] { 1f, 1f, 1f })]
			[InlineData("uniform_float2", new[] { 1 })]
			[InlineData("uniform_float2", new[] { 1, 1 })]
			[InlineData("uniform_float2", new[] { 1, 1, 1 })]
			public void UniformsThrowsCorrectly(string name, object value)
			{
				var src = $"""
					uniform int uniform_int;
					uniform int uniform_int_array[2];
					uniform int2 uniform_int2;
					uniform float uniform_float;
					uniform float uniform_float_array[2];
					uniform float2 uniform_float2;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var uniforms = new SKRuntimeEffectUniforms(effect);

				if (value is int intValue)
					Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add(name, intValue));
				else if (value is int[] intArray)
					Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add(name, intArray));
				else if (value is float floatValue)
					Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add(name, floatValue));
				else if (value is float[] floatArray)
					Assert.Throws<ArgumentOutOfRangeException>(() => uniforms.Add(name, floatArray));
				else
					throw new ArgumentException($"Invalid test data type {value}");
			}

			[SkippableFact]
			public void UniformsWorksCorrectlyWithCollectionInitializer()
			{
				var src = $"""
					uniform float uniform_float;
					uniform float uniform_float_array[2];
					uniform float2 uniform_float2;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var uniforms = new SKRuntimeEffectUniforms(effect)
				{
					{ "uniform_float", 1f },
					{ "uniform_float_array", new [] { 1f, 2f } },
					{ "uniform_float2", new [] { 1f, 2f } },
				};
			}

			[SkippableFact]
			public void UniformsWorksCorrectlyWithCollectionIndexer()
			{
				var src = $"""
					uniform float uniform_float;
					uniform float uniform_float_array[2];
					uniform float2 uniform_float2;
					{EmptyMain}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);

				var uniforms = new SKRuntimeEffectUniforms(effect)
				{
					["uniform_float"] = 1f,
					["uniform_float_array"] = new[] { 1f, 2f },
					["uniform_float2"] = new[] { 1f, 2f }
				};
			}
		}

		public unsafe class ColorFilters : SKRuntimeEffectTest
		{
			[SkippableTheory]
			[InlineData("half4  main(half4  c) { return c; }")]
			[InlineData("float4 main(half4  c) { return c; }")]
			[InlineData("half4  main(float4 c) { return c; }")]
			[InlineData("float4 main(float4 c) { return c; }")]
			[InlineData("vec4   main(half4  c) { return c; }")]
			[InlineData("half4  main(vec4   c) { return c; }")]
			[InlineData("vec4   main(vec4   c) { return c; }")]
			public void SupportAllSize4InMainReturnSize4(string src)
			{
				using var effect = SKRuntimeEffect.CreateColorFilter(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);
			}

			[SkippableTheory]
			// Invalid return types
			[InlineData("void  main(half4 c) {}", "'main' must return")]
			[InlineData("half3 main(half4 c) { return c.rgb; }", "'main' must return")]
			// Invalid argument types (some are valid as shaders, but not color filters)
			[InlineData("half4 main() { return half4(1); }", "'main' parameter")]
			[InlineData("half4 main(float2 p) { return half4(1); }", "'main' parameter")]
			[InlineData("half4 main(float2 p, half4 c) { return c; }", "'main' parameter")]
			// sk_FragCoord should not be available
			[InlineData("half4 main(half4 c) { return sk_FragCoord.xy01; }", "unknown identifier")]
			public void InvalidFailCreation(string src, string errorContains)
			{
				using var effect = SKRuntimeEffect.CreateColorFilter(src, out var errorText);

				Assert.Null(effect);
				Assert.Contains(errorContains, errorText);
			}

			[SkippableTheory]
			[InlineData("shader", "c.rg")]
			[InlineData("colorFilter", "c")]
			[InlineData("blender", "c, c")]
			public void SupportDifferentUniforms(string uniform, string usage)
			{
				var src = $$"""
				uniform {{uniform}} child;
				half4 main(half4 c) {
					return child.eval({{usage}});
				}
				""";

				using var effect = SKRuntimeEffect.CreateColorFilter(src, out var errorText);

				Assert.Null(errorText);
				Assert.NotNull(effect);
			}
		}

		public abstract class TestEffectTests : SKRuntimeEffectTest
		{
			protected SKSurface Surface { get; set; }

			protected SKImageInfo Info { get; set; }

			protected abstract void CreateSurface(int width, int height);

			[SkippableFact]
			public void LocalCoordinatesAreSuccessful()
			{
				var src = """
					half4 main(float2 p) {
						return half4(half2(p - 0.5), 0, 1);
					}
					""";

				using var builder = new TestShaderBuilder(src);
				Test(builder, new[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF });
			}

			[SkippableFact]
			public void SimpleFloatUniform()
			{
				var src = """
					uniform float4 gColor;
					half4 main(float2 p) {
						return half4(gColor);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Uniforms.Add("gColor", new[] { 0.0f, 0.25f, 0.75f, 1.0f });

				Test(builder, 0xFFBF4000);

				builder.Uniforms.Add("gColor", new[] { 1.0f, 0.0f, 0.0f, 0.498f });

				Test(builder, 0x7F0000FF);
			}

			[SkippableFact]
			public void SimpleInt32Uniform()
			{
				var src = """
					uniform int4 gColor;
					half4 main(float2 p) {
						return half4(gColor) / 255.0;
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Uniforms.Add("gColor", new[] { 0x00, 0x40, 0xBF, 0xFF });

				Test(builder, 0xFFBF4000);

				builder.Uniforms.Add("gColor", new[] { 0xFF, 0x00, 0x00, 0x7F });

				Test(builder, 0x7F0000FF);
			}

			[SkippableTheory]
			[InlineData("half4")]
			[InlineData("float4")]
			[InlineData("vec4")]
			public void AllReturnTypesAreSuccessful(string returnType)
			{
				var src = $$"""
					{{returnType}} main(float2 p) {
						return float4(p - 0.5, 0, 1);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				Test(builder, new[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF });
			}

			[SkippableFact]
			public void CoordinatesCanBeMutated()
			{
				var src = """
					vec4 main(vec2 p) {
						p -= 0.5;
						return vec4(p, 0, 1);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				Test(builder, new[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF });
			}

			[SkippableFact]
			public void CoordinatesCanBeMutatedInASeparateMethod()
			{
				var src = """
					void moveCoords(inout vec2 p) {
						p -= 0.5;
					}
					vec4 main(vec2 p) {
						moveCoords(p);
						return vec4(p, 0, 1);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				Test(builder, new[] { 0xFF000000, 0xFF0000FF, 0xFF00FF00, 0xFF00FFFF });
			}

			[SkippableFact]
			public void NullShaderChildDrawsPaintColor()
			{
				var src = """
					uniform shader child;
					half4 main(float2 p) {
						return child.eval(p);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Children["child"] = null;

				Test(builder, 0x00000000, (c, p) => p.ColorF = new SKColorF(1.0f, 1.0f, 0.0f, 1.0f));
			}

			[SkippableFact]
			public void NullColorFilterChildDrawsPaintColor()
			{
				var src = """
					uniform colorFilter child;
					half4 main(float2 p) {
						return child.eval(half4(1, 1, 0, 1));
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Children["child"] = null;

				Test(builder, 0xFF00FFFF);
			}

			[SkippableFact]
			public void SamplingChildAtLocalCoordinates()
			{
				var src = """
					uniform shader child;
					half4 main(float2 p) {
						return child.eval(p);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Children["child"] = CreateTestShader();

				Test(builder, new[] { 0xFF0000FF, 0xFF00FF00, 0xFFFF0000, 0xFFFFFFFF });
			}

			[SkippableFact]
			public void SamplingChildAtExplicitCoordinates()
			{
				var src = """
					uniform shader child;
					half4 main(float2 p) {
						return child.eval(p.yx);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Children["child"] = CreateTestShader();

				Test(builder, new[] { 0xFF0000FF, 0xFFFF0000, 0xFF00FF00, 0xFFFFFFFF });
			}

			[SkippableFact]
			public void ChildrenAreNotRequiredToBeUsed()
			{
				var src = """
					uniform shader child;
					half4 main(float2 p) {
						return half4(0, 1, 0, 1);
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Children["child"] = CreateTestShader();

				Test(builder, 0xFF00FF00);
			}

			[SkippableTheory]
			[InlineData(1.05f, 1.5f, 0xFF000000, 0xFFE98404)]
			[InlineData(1.26f, 1.35f, 0xFF000000, 0xFF000000)]
			[InlineData(0f, 6.5f, 0xFFFFFFFF, 0xFFE98404)]
			public void ImageCanClearBackground(float threshold, float exponent, uint backgroundColor, uint shirtColor)
			{
				CreateSurface(500, 500);

				var canvas = Surface.Canvas;
				canvas.Clear(SKColors.Black);

				using var blueShirt = SKImage.FromEncodedData(Path.Combine(PathToImages, "blue-shirt.jpg"));
				using var textureShader = blueShirt.ToShader();

				var src = """
					uniform shader color_map;

					uniform float scale;
					uniform half in_exp;
					uniform float3 in_colors0;

					half4 main(float2 p) {
						half4 texColor = color_map.eval(p);
						if (length(abs(in_colors0 - pow(texColor.rgb, half3(in_exp)))) < scale)
							return half4(0, 0, 0, 1);
						return texColor;
					}
					""";

				using var effect = SKRuntimeEffect.CreateShader(src, out var errorText);
				Assert.Null(errorText);
				Assert.NotNull(effect);

				var uniformSize = effect.UniformSize;
				Assert.Equal(5 * sizeof(float), uniformSize);

				var uniforms = new SKRuntimeEffectUniforms(effect)
				{
					["scale"] = threshold,
					["in_exp"] = exponent,
					["in_colors0"] = SKColors.White,
				};

				var children = new SKRuntimeEffectChildren(effect)
				{
					["color_map"] = textureShader
				};

				using var shader = effect.ToShader(uniforms, children);

				using var paint = new SKPaint { Shader = shader };
				canvas.DrawRect(SKRect.Create(400, 400), paint);

				var actual = new SKColor[500 * 500];
				fixed (void* a = actual)
					Assert.True(Surface.ReadPixels(Info, (IntPtr)a, Info.RowBytes, 0, 0));

				Assert.Equal(backgroundColor, actual[100 * Info.Width + 100]);
				Assert.Equal(shirtColor, actual[230 * Info.Width + 300]);
			}

			[SkippableFact]
			public void CoolEffectCanBeSeen()
			{
				CreateSurface(256, 256);

				var src = """
					// Source: @notargs https://twitter.com/notargs/status/1250468645030858753
					uniform float3 iResolution;      // Viewport resolution (pixels)
					uniform float  iTime;            // Shader playback time (s)

					float f(vec3 p) {
						p.z -= iTime * 10.;
						float a = p.z * .1;
						p.xy *= mat2(cos(a), sin(a), -sin(a), cos(a));
						return .1 - length(cos(p.xy) + sin(p.yz));
					}

					half4 main(vec2 fragcoord) {
						vec3 d = .5 - fragcoord.xy1 / iResolution.y;
						vec3 p=vec3(0);
						for (int i = 0; i < 32; i++) {
							p += f(p) * d;
						}
						return ((sin(p) + vec3(2, 5, 9)) / length(p)).xyz1;
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Uniforms["iResolution"] = new SKPoint3(256, 256, 0);
				builder.Uniforms["iTime"] = 0f;

				Draw(builder);
			}

			[SkippableFact]
			public void CoolLightsSphereCanBeSeen()
			{
				CreateSurface(256, 256);

				var src = """
					// Source: @XorDev https://twitter.com/XorDev/status/1475524322785640455
					uniform float3 iResolution;      // Viewport resolution (pixels)
					uniform float  iTime;            // Shader playback time (s)

					vec4 main(vec2 FC) {
						vec4 o = vec4(0);
						vec2 p = vec2(0), c=p, u=FC.xy*2.-iResolution.xy;
						float a;
						for (float i=0; i<4e2; i++) {
							a = i/2e2-1.;
							p = cos(i*2.4+iTime+vec2(0,11))*sqrt(1.-a*a);
							c = u/iResolution.y+vec2(p.x,a)/(p.y+2.);
							o += (cos(i+vec4(0,2,4,0))+1.)/dot(c,c)*(1.-p.y)/3e4;
						}
						return o;
					}
					""";

				using var builder = new TestShaderBuilder(src);

				builder.Uniforms["iResolution"] = new SKPoint3(256, 256, 0);
				builder.Uniforms["iTime"] = 0f;

				Draw(builder);
			}

			/// <summary>
			/// Produces a shader which will paint these opaque colors in a 2x2 rectangle:<br/>
			/// [  Red, Green ]<br/>
			/// [ Blue, White ]
			/// </summary>
			/// <returns></returns>
			private static SKShader CreateTestShader() =>
				SKShader.CreateSweepGradient(
					new SKPoint(1, 1),
					new[]
					{
						(SKColor)0xFFFFFFFF, (SKColor)0xFFFFFFFF,
						(SKColor)0xFF0000FF, (SKColor)0xFF0000FF,
						(SKColor)0xFFFF0000, (SKColor)0xFFFF0000,
						(SKColor)0xFF00FF00, (SKColor)0xFF00FF00
					},
					new[]
					{
						0.0f, 0.25f,
						0.25f, 0.50f,
						0.50f, 0.75f,
						0.75f, 1.0f
					});

			protected void Draw(TestShaderBuilder builder, Action<SKCanvas, SKPaint> preTestCallback = null) =>
				builder.Draw(Surface, preTestCallback);

			protected void Test(TestShaderBuilder builder, uint expected, Action<SKCanvas, SKPaint> preTestCallback = null) =>
				Test(builder, new[] { (SKColor)expected, (SKColor)expected, (SKColor)expected, (SKColor)expected }, preTestCallback);

			protected void Test(TestShaderBuilder builder, SKColor expected, Action<SKCanvas, SKPaint> preTestCallback = null) =>
				Test(builder, new[] { expected, expected, expected, expected }, preTestCallback);

			protected void Test(TestShaderBuilder builder, uint[] expected, Action<SKCanvas, SKPaint> preTestCallback = null) =>
				Test(builder, expected.Select(e => (SKColor)e).ToArray(), preTestCallback);

			protected void Test(TestShaderBuilder builder, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null) =>
				builder.Test(Surface, Info, expected, preTestCallback);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		public unsafe class Gpu : TestEffectTests, IDisposable
		{
			GlContext glContext;
			GRContext grContext;

			public Gpu()
			{
				glContext = CreateGlContext();
				glContext.MakeCurrent();

				grContext = GRContext.CreateGl();

				CreateSurface(2, 2);
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

		public unsafe class Raster : TestEffectTests, IDisposable
		{
			public Raster()
			{
				CreateSurface(2, 2);
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

		public class TestShaderBuilder : IDisposable
		{
			public TestShaderBuilder(string src)
			{
				Builder = SKRuntimeEffect.BuildShader(src);

				Assert.NotNull(Builder);
				Assert.NotNull(Effect);
				Assert.NotNull(Uniforms);
				Assert.NotNull(Children);
			}

			public SKRuntimeShaderBuilder Builder { get; }

			public SKRuntimeEffect Effect => Builder.Effect;

			public SKRuntimeEffectUniforms Uniforms => Builder.Uniforms;

			public SKRuntimeEffectChildren Children => Builder.Children;

			public void Draw(SKSurface surface, Action<SKCanvas, SKPaint> preTestCallback = null)
			{
				// build
				using var shader = Builder.Build();
				Assert.NotNull(shader);

				// draw
				using var paint = new SKPaint
				{
					Shader = shader,
					BlendMode = SKBlendMode.Src
				};
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.Black);
				canvas.Save();
				preTestCallback?.Invoke(canvas, paint);
				canvas.DrawPaint(paint);
				canvas.Restore();
			}

			public void Test(SKSurface surface, SKImageInfo info, SKColor[] expected, Action<SKCanvas, SKPaint> preTestCallback = null)
			{
				// build & draw
				Draw(surface, preTestCallback);

				// test
				var actual = new SKColor[4];
				fixed (void* a = actual)
					Assert.True(surface.ReadPixels(info, (IntPtr)a, info.RowBytes, 0, 0));
				Assert.Equal(expected, actual);
			}

			public void Dispose() =>
				Builder.Dispose();
		}
	}
}
