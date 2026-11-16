using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMeshSpecificationTest : SKTest
	{
		private const string SimpleVertexShader = @"
			Varyings main(const Attributes attrs) {
				Varyings v;
				v.position = attrs.position;
				return v;
			}";

		private const string SimpleFragmentShader = @"
			float2 main(const Varyings varyings) {
				return varyings.position;
			}";

		private const string ColorFragmentShader = @"
			float2 main(const Varyings varyings, out half4 color) {
				color = half4(1.0, 0.0, 0.0, 1.0);
				return varyings.position;
			}";

		private static SKMeshSpecificationAttribute[] SimpleAttributes => new[]
		{
			new SKMeshSpecificationAttribute(SKMeshSpecificationAttributeType.Float2, 0, "position"),
		};

		private static SKMeshSpecificationVarying[] EmptyVaryings => Array.Empty<SKMeshSpecificationVarying>();

		private static SKMeshSpecification CreateSimpleSpec(out string errors) =>
			SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				null,
				SKAlphaType.Premul,
				out errors);

		[Fact]
		public void CanCreateSpecification()
		{
			var spec = CreateSimpleSpec(out var errors);

			Assert.Null(errors);
			Assert.NotNull(spec);
			Assert.Equal(sizeof(float) * 2, spec.Stride);

			spec.Dispose();
		}

		[Fact]
		public void SpecificationWithColorSpaceSucceeds()
		{
			using var cs = SKColorSpace.CreateSrgb();

			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				ColorFragmentShader,
				cs,
				SKAlphaType.Premul,
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(spec);

			spec.Dispose();
		}

		[Fact]
		public void SpecificationWithInvalidShaderReturnsError()
		{
			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				"invalid shader code",
				SimpleFragmentShader,
				null,
				SKAlphaType.Premul,
				out var errors);

			Assert.Null(spec);
			Assert.NotNull(errors);
		}

		[Fact]
		public void SpecificationStrideMatchesInput()
		{
			using var spec = CreateSimpleSpec(out _);

			Assert.NotNull(spec);
			Assert.Equal(sizeof(float) * 2, spec.Stride);
		}

		[Fact]
		public void SpecificationRequiresAttributes()
		{
			Assert.Throws<ArgumentException>(() =>
				SKMeshSpecification.Create(
					Array.Empty<SKMeshSpecificationAttribute>(),
					0,
					EmptyVaryings,
					SimpleVertexShader,
					SimpleFragmentShader,
					null,
					SKAlphaType.Premul,
					out _));
		}

		[Fact]
		public void SpecificationRequiresVertexShader()
		{
			Assert.Throws<ArgumentNullException>(() =>
				SKMeshSpecification.Create(
					SimpleAttributes,
					sizeof(float) * 2,
					EmptyVaryings,
					null,
					SimpleFragmentShader,
					null,
					SKAlphaType.Premul,
					out _));
		}

		[Fact]
		public void SpecificationBuilderReturnsNullAndErrorOnInvalidShader()
		{
			var specBuilder = new SKMeshSpecificationBuilder("invalid shader code", SimpleFragmentShader);
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);

			var spec = specBuilder.Build(out var errors);

			Assert.Null(spec);
			Assert.NotNull(errors);
		}

		[Fact]
		public void SpecificationBuilderComputesOffsetsAndStride()
		{
			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, SimpleFragmentShader);
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			specBuilder.AddAttribute("uv", SKMeshSpecificationAttributeType.Float2);
			specBuilder.AddAttribute("color", SKMeshSpecificationAttributeType.Ubyte4Unorm);

			Assert.Equal(20, specBuilder.VertexStride);
			Assert.Equal(0, specBuilder.Attributes[0].Offset);
			Assert.Equal(8, specBuilder.Attributes[1].Offset);
			Assert.Equal(16, specBuilder.Attributes[2].Offset);

			using var spec = specBuilder.Build(out var errors);
			Assert.Null(errors);
			Assert.NotNull(spec);
			Assert.Equal(20, spec.Stride);
		}

		[Fact]
		public void SpecificationBuilderDerivesStrideFromCollectionMutations()
		{
			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, SimpleFragmentShader);
			specBuilder.Attributes.Add(new SKMeshSpecificationAttribute(SKMeshSpecificationAttributeType.Float2, 0, "position"));

			Assert.Equal(8, specBuilder.VertexStride);
			using var spec = specBuilder.Build(out var errors);
			Assert.Null(errors);
			Assert.NotNull(spec);
			Assert.Equal(8, spec.Stride);
		}

		[Fact]
		public void UniformsAndChildrenProperties()
		{
			using var spec = CreateSimpleSpec(out _);

			Assert.NotNull(spec);
			Assert.NotNull(spec.Uniforms);
			Assert.NotNull(spec.Children);
		}

		[Fact]
		public void UniformsAreEnumeratedAndCanBeSetByName()
		{
			// Regression: SKMeshSpecification.Uniforms/Children must lazily enumerate the
			// native uniforms. A non-null field initializer previously defeated the `??=`
			// lazy-init, so Uniforms was always empty and setting a declared uniform by
			// name threw ArgumentOutOfRangeException.
			const string fragmentShaderWithUniform = @"
				uniform float2 uImageSize;
				float2 main(const Varyings varyings) {
					return varyings.position * uImageSize;
				}";

			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, fragmentShaderWithUniform);
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			using var spec = specBuilder.Build(out var errors);

			Assert.Null(errors);
			Assert.NotNull(spec);
			Assert.Equal(8, spec.UniformSize); // float2 == 8 bytes
			Assert.Single(spec.Uniforms);
			Assert.Equal("uImageSize", spec.Uniforms[0]);

			using var builder = new SKMeshBuilder(spec);
			Assert.Single(builder.Uniforms.Names);

			// Must not throw "Variable was not found".
			builder.Uniforms["uImageSize"] = (ReadOnlySpan<float>)new[] { 512f, 512f };
		}
	}
}
