using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMeshTest : SKTest
	{
		// Simple SkSL programs for testing
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

		// Specification tests

		[SkippableFact]
		public void CanCreateSpecification()
		{
			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(spec);
			Assert.Equal(sizeof(float) * 2, spec.Stride);

			spec.Dispose();
		}

		[SkippableFact]
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

		[SkippableFact]
		public void SpecificationWithInvalidShaderReturnsError()
		{
			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				"invalid shader code",
				SimpleFragmentShader,
				out var errors);

			Assert.Null(spec);
			Assert.NotNull(errors);
		}

		[SkippableFact]
		public void SpecificationStrideMatchesInput()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);
			Assert.Equal(sizeof(float) * 2, spec.Stride);
		}

		[SkippableFact]
		public void SpecificationRequiresAttributes()
		{
			Assert.Throws<ArgumentException>(() =>
				SKMeshSpecification.Create(
					Array.Empty<SKMeshSpecificationAttribute>(),
					0,
					EmptyVaryings,
					SimpleVertexShader,
					SimpleFragmentShader,
					out _));
		}

		[SkippableFact]
		public void SpecificationRequiresVertexShader()
		{
			Assert.Throws<ArgumentNullException>(() =>
				SKMeshSpecification.Create(
					SimpleAttributes,
					sizeof(float) * 2,
					EmptyVaryings,
					null,
					SimpleFragmentShader,
					out _));
		}

		[SkippableFact]
		public void BuildThrowsOnInvalidShader()
		{
			Assert.Throws<SKMeshSpecificationException>(() =>
				SKMeshSpecification.Build(
					SimpleAttributes,
					sizeof(float) * 2,
					EmptyVaryings,
					"invalid shader code",
					SimpleFragmentShader));
		}

		[SkippableFact]
		public void BuildReturnsMeshBuilder()
		{
			using var builder = SKMeshSpecification.Build(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader);

			Assert.NotNull(builder);
			Assert.NotNull(builder.Specification);
			Assert.NotNull(builder.Uniforms);
			Assert.NotNull(builder.Children);
		}

		[SkippableFact]
		public void UniformsAndChildrenProperties()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);
			Assert.NotNull(spec.Uniforms);
			Assert.NotNull(spec.Children);
		}

		// Buffer tests

		[SkippableFact]
		public void CanCreateVertexBuffer()
		{
			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());

			using var vb = SKMeshVertexBuffer.Make(data);

			Assert.NotNull(vb);
			Assert.Equal(vertices.Length * sizeof(float), vb.Size);
		}

		[SkippableFact]
		public void CanCreateZeroInitializedVertexBuffer()
		{
			using var vb = SKMeshVertexBuffer.Make(24);

			Assert.NotNull(vb);
			Assert.Equal(24, vb.Size);
		}

		[SkippableFact]
		public void CanCreateIndexBuffer()
		{
			var indices = new ushort[] { 0, 1, 2 };
			var data = MemoryMarshal.AsBytes(indices.AsSpan());

			using var ib = SKMeshIndexBuffer.Make(data);

			Assert.NotNull(ib);
			Assert.Equal(indices.Length * sizeof(ushort), ib.Size);
		}

		// Mesh creation tests

		[SkippableFact]
		public void CanCreateNonIndexedMesh()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				new SKRect(0, 0, 100, 100),
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[SkippableFact]
		public void CanCreateIndexedMesh()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 100, 100, 0, 100 };
			var vData = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(vData);

			var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
			var iData = MemoryMarshal.AsBytes(indices.AsSpan());
			using var ib = SKMeshIndexBuffer.Make(iData);

			using var mesh = spec.ToMeshIndexed(
				SKMeshMode.Triangles,
				vb,
				4,
				0,
				ib,
				6,
				0,
				new SKRect(0, 0, 100, 100),
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[SkippableFact]
		public void ToMeshRequiresVertexBuffer()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.Throws<ArgumentNullException>(() =>
				spec.ToMesh(SKMeshMode.Triangles, null, 3, 0, new SKRect(0, 0, 100, 100)));
		}

		[SkippableFact]
		public void ToMeshIndexedRequiresVertexBuffer()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			using var ib = SKMeshIndexBuffer.Make(6);

			Assert.Throws<ArgumentNullException>(() =>
				spec.ToMeshIndexed(SKMeshMode.Triangles, null, 3, 0, ib, 3, 0, new SKRect(0, 0, 100, 100)));
		}

		// Drawing tests

		[SkippableFact]
		public void CanDrawMesh()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				ColorFragmentShader,
				SKColorSpace.CreateSrgb(),
				SKAlphaType.Premul,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				new SKRect(0, 0, 100, 100));

			Assert.NotNull(mesh);

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			canvas.DrawMesh(mesh, paint);

			using var image = surface.Snapshot();
			Assert.NotNull(image);
		}

		[SkippableFact]
		public void DrawMeshWithBlender()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				ColorFragmentShader,
				SKColorSpace.CreateSrgb(),
				SKAlphaType.Premul,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				new SKRect(0, 0, 100, 100));

			Assert.NotNull(mesh);

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			using var blender = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			canvas.DrawMesh(mesh, blender, paint);

			using var image = surface.Snapshot();
			Assert.NotNull(image);
		}

		[SkippableFact]
		public void DrawMeshRequiresMesh()
		{
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			using var paint = new SKPaint();

			Assert.Throws<ArgumentNullException>(() => canvas.DrawMesh(null, paint));
		}

		[SkippableFact]
		public void DrawMeshRequiresPaint()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				new SKRect(0, 0, 100, 100));

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			Assert.Throws<ArgumentNullException>(() => canvas.DrawMesh(mesh, null));
		}

		[SkippableFact]
		public void TriangleStripModeWorks()
		{
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);

			// 4 vertices forming a triangle strip (two triangles)
			var vertices = new float[] { 0, 0, 100, 0, 0, 100, 100, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var mesh = spec.ToMesh(
				SKMeshMode.TriangleStrip,
				vb,
				4,
				0,
				new SKRect(0, 0, 100, 100),
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[SkippableFact]
		public void MeshBuilderCreatesNonIndexedMesh()
		{
			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(data);

			using var builder = new SKMeshBuilder(spec);
			builder.Mode = SKMeshMode.Triangles;
			builder.Bounds = new SKRect(0, 0, 100, 100);

			using var mesh = builder.Build(vb, 3, 0);

			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[SkippableFact]
		public void MeshBuilderCreatesIndexedMesh()
		{
			var spec = SKMeshSpecification.Create(
				SimpleAttributes,
				sizeof(float) * 2,
				EmptyVaryings,
				SimpleVertexShader,
				SimpleFragmentShader,
				out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 100, 100, 0, 100 };
			var vData = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Make(vData);

			var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
			var iData = MemoryMarshal.AsBytes(indices.AsSpan());
			using var ib = SKMeshIndexBuffer.Make(iData);

			using var builder = new SKMeshBuilder(spec);
			builder.Mode = SKMeshMode.Triangles;
			builder.Bounds = new SKRect(0, 0, 100, 100);

			using var mesh = builder.BuildIndexed(vb, 4, 0, ib, 6, 0);

			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}
	}
}
