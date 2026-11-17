using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests
{
	[Collection(GpuRenderingCollection.Name)]
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

		// Mesh creation tests

		[Fact]
		public void CanCreateNonIndexedMesh()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			var bounds = new SKRect(0, 0, 100, 100);
			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				null,
				null,
				bounds,
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
			Assert.Equal(SKMeshMode.Triangles, mesh.Mode);
			Assert.Equal(3, mesh.VertexCount);
			Assert.Equal(0, mesh.VertexOffset);
			Assert.Equal(0, mesh.IndexCount);
			Assert.Equal(0, mesh.IndexOffset);
			Assert.Equal(bounds, mesh.Bounds);
		}

		[Fact]
		public void CanCreateIndexedMesh()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 100, 100, 0, 100 };
			var vData = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(vData);

			var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
			var iData = MemoryMarshal.AsBytes(indices.AsSpan());
			using var ib = SKMeshIndexBuffer.Create(iData);

			var bounds = new SKRect(0, 0, 100, 100);
			using var mesh = spec.ToMeshIndexed(
				SKMeshMode.Triangles,
				vb,
				4,
				0,
				ib,
				6,
				0,
				null,
				null,
				bounds,
				out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
			Assert.Equal(SKMeshMode.Triangles, mesh.Mode);
			Assert.Equal(4, mesh.VertexCount);
			Assert.Equal(0, mesh.VertexOffset);
			Assert.Equal(6, mesh.IndexCount);
			Assert.Equal(0, mesh.IndexOffset);
			Assert.Equal(bounds, mesh.Bounds);
		}

		[Fact]
		public void ToMeshWithoutUniformsReturnsErrorWhenSpecDeclaresUniforms()
		{
			const string fsWithUniform = @"
				uniform float2 uScale;
				float2 main(const Varyings varyings) {
					return varyings.position * uScale;
				}";

			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, fsWithUniform);
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			using var spec = specBuilder.Build(out var specErrors);
			Assert.NotNull(spec);
			Assert.Null(specErrors);

			using var vb = SKMeshVertexBuffer.Create(24);
			using var mesh = spec.ToMesh(
				SKMeshMode.Triangles,
				vb,
				3,
				0,
				null,
				null,
				new SKRect(0, 0, 100, 100),
				out var meshErrors);

			Assert.Null(mesh);
			Assert.NotNull(meshErrors);
			Assert.Contains("uniform", meshErrors, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void DisposingBorrowedMeshBuilderDoesNotDisposeSharedSpecification()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(new float[] { 0, 0, 100, 0, 50, 100 }.AsSpan()));

			using (var firstBuilder = new SKMeshBuilder(spec) { Bounds = new SKRect(0, 0, 100, 100) })
			{
				firstBuilder.SetVertexBuffer(vb);
				using var firstMesh = firstBuilder.Build();
				Assert.NotNull(firstMesh);
			}

			// Specification is still alive after firstBuilder.Dispose() and can be used by a second builder.
			Assert.NotEqual(IntPtr.Zero, spec.Handle);
			using var secondBuilder = new SKMeshBuilder(spec) { Bounds = new SKRect(0, 0, 100, 100) };
			secondBuilder.SetVertexBuffer(vb);
			using var secondMesh = secondBuilder.Build(out var errors);
			Assert.Null(errors);
			Assert.NotNull(secondMesh);
			Assert.True(secondMesh.IsValid);
		}

		[Fact]
		public void SpecificationAndToMeshValidateNegativeParameters()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				SKMeshSpecification.Create(SimpleAttributes, 0, EmptyVaryings, SimpleVertexShader, SimpleFragmentShader, null, SKAlphaType.Premul, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				SKMeshSpecification.Create(SimpleAttributes, -8, EmptyVaryings, SimpleVertexShader, SimpleFragmentShader, null, SKAlphaType.Premul, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				SKMeshSpecification.Create(
					new[] { new SKMeshSpecificationAttribute(SKMeshSpecificationAttributeType.Float2, -4, "position") },
					8,
					EmptyVaryings,
					SimpleVertexShader,
					SimpleFragmentShader,
					null,
					SKAlphaType.Premul,
					out _));

			using var spec = CreateSimpleSpec(out _);
			using var vb = SKMeshVertexBuffer.Create(24);
			using var ib = SKMeshIndexBuffer.Create(6);
			var bounds = new SKRect(0, 0, 100, 100);

			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMesh(SKMeshMode.Triangles, vb, 0, 0, null, null, bounds, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMesh(SKMeshMode.Triangles, vb, -1, 0, null, null, bounds, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMesh(SKMeshMode.Triangles, vb, 3, -8, null, null, bounds, out _));

			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMeshIndexed(SKMeshMode.Triangles, vb, 0, 0, ib, 3, 0, null, null, bounds, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMeshIndexed(SKMeshMode.Triangles, vb, 3, -8, ib, 3, 0, null, null, bounds, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMeshIndexed(SKMeshMode.Triangles, vb, 3, 0, ib, 0, 0, null, null, bounds, out _));
			Assert.Throws<ArgumentOutOfRangeException>(() => spec.ToMeshIndexed(SKMeshMode.Triangles, vb, 3, 0, ib, 3, -2, null, null, bounds, out _));
		}

		[Fact]
		public void ToMeshRequiresVertexBuffer()
		{
			using var spec = CreateSimpleSpec(out _);

			Assert.Throws<ArgumentNullException>(() =>
				spec.ToMesh(SKMeshMode.Triangles, null, 3, 0, null, null, new SKRect(0, 0, 100, 100), out _));
		}

		[Fact]
		public void ToMeshIndexedRequiresVertexBuffer()
		{
			using var spec = CreateSimpleSpec(out _);
			using var ib = SKMeshIndexBuffer.Create(6);

			Assert.Throws<ArgumentNullException>(() =>
				spec.ToMeshIndexed(SKMeshMode.Triangles, null, 3, 0, ib, 3, 0, null, null, new SKRect(0, 0, 100, 100), out _));
		}

		// Drawing tests

		[Fact]
		public void CanDrawMesh()
		{
			using var cs = SKColorSpace.CreateSrgb();
			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, ColorFragmentShader)
			{
				ColorSpace = cs,
				AlphaType = SKAlphaType.Premul,
			};
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			using var spec = specBuilder.Build(out _);

			Assert.NotNull(spec);
			Assert.NotNull(spec.ColorSpace);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			using var builder = new SKMeshBuilder(spec) { Bounds = new SKRect(0, 0, 100, 100) };
			builder.SetVertexBuffer(vb);
			using var mesh = builder.Build();

			Assert.NotNull(mesh);

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint();
			canvas.DrawMesh(mesh, paint);

			using var image = surface.Snapshot();
			Assert.NotNull(image);
		}

		[Fact]
		public void DrawMeshWithBlender()
		{
			using var cs = SKColorSpace.CreateSrgb();
			var specBuilder = new SKMeshSpecificationBuilder(SimpleVertexShader, ColorFragmentShader)
			{
				ColorSpace = cs,
				AlphaType = SKAlphaType.Premul,
			};
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			using var spec = specBuilder.Build(out _);

			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			using var builder = new SKMeshBuilder(spec) { Bounds = new SKRect(0, 0, 100, 100) };
			builder.SetVertexBuffer(vb);
			using var mesh = builder.Build();

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

		[Fact]
		public void DrawMeshRequiresMesh()
		{
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			using var paint = new SKPaint();

			Assert.Throws<ArgumentNullException>(() => canvas.DrawMesh(null, paint));
		}

		[Fact]
		public void DrawMeshRequiresPaint()
		{
			using var spec = CreateSimpleSpec(out _);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			using var builder = new SKMeshBuilder(spec) { Bounds = new SKRect(0, 0, 100, 100) };
			builder.SetVertexBuffer(vb);
			using var mesh = builder.Build();

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			Assert.Throws<ArgumentNullException>(() => canvas.DrawMesh(mesh, null));
		}

		[Fact]
		public void TriangleStripModeWorks()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			// 4 vertices forming a triangle strip (two triangles)
			var vertices = new float[] { 0, 0, 100, 0, 0, 100, 100, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			using var builder = new SKMeshBuilder(spec)
			{
				Mode = SKMeshMode.TriangleStrip,
				Bounds = new SKRect(0, 0, 100, 100),
			};
			builder.SetVertexBuffer(vb);
			using var mesh = builder.Build(out var errors);

			Assert.Null(errors);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[Fact]
		public void MeshBuilderCreatesNonIndexedMesh()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 50, 100 };
			var data = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(data);

			using var builder = spec.CreateMeshBuilder();
			builder.Mode = SKMeshMode.Triangles;
			builder.Bounds = new SKRect(0, 0, 100, 100);
			builder.SetVertexBuffer(vb);

			Assert.Equal(spec.VertexStride, builder.VertexStride);
			Assert.Equal(vb, builder.VertexBuffer);
			Assert.Equal(3, builder.VertexCount);
			Assert.Equal(0, builder.VertexOffset);

			using var mesh = builder.Build();

			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
		}

		[Fact]
		public void IndexedMeshBuilderCreatesIndexedMesh()
		{
			using var spec = CreateSimpleSpec(out _);
			Assert.NotNull(spec);

			var vertices = new float[] { 0, 0, 100, 0, 100, 100, 0, 100 };
			var vData = MemoryMarshal.AsBytes(vertices.AsSpan());
			using var vb = SKMeshVertexBuffer.Create(vData);

			var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
			var iData = MemoryMarshal.AsBytes(indices.AsSpan());
			using var ib = SKMeshIndexBuffer.Create(iData);

			using var builder = SKMesh.CreateIndexedBuilder(spec);
			builder.Mode = SKMeshMode.Triangles;
			builder.Bounds = new SKRect(0, 0, 100, 100);
			builder.SetVertexBuffer(vb);

			// Building an indexed mesh before setting the index buffer must throw.
			Assert.Throws<InvalidOperationException>(() => builder.Build());

			builder.SetIndexBuffer(ib);

			Assert.Equal(spec.VertexStride, builder.VertexStride);
			Assert.Equal(ib, builder.IndexBuffer);
			Assert.Equal(6, builder.IndexCount);
			Assert.Equal(0, builder.IndexOffset);

			// Polymorphic Build() through SKMeshBuilder base reference must call SKIndexedMeshBuilder.Build()
			SKMeshBuilder baseBuilder = builder;
			using var mesh = baseBuilder.Build();

			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);
			Assert.Equal(6, mesh.IndexCount);
		}

		// Full-canvas quad (two triangles) covering a 100x100 area.
		private static readonly float[] QuadVertices = { 0, 0, 100, 0, 100, 100, 0, 100 };
		private static readonly ushort[] QuadIndices = { 0, 1, 2, 0, 2, 3 };

		private SKMesh CreateFullCanvasRedMesh(SKMeshSpecification spec, SKIndexedMeshBuilder builder,
			SKMeshVertexBuffer vb, SKMeshIndexBuffer ib)
		{
			builder.Bounds = new SKRect(0, 0, 100, 100);
			builder.SetVertexBuffer(vb);
			builder.SetIndexBuffer(ib);
			return builder.Build();
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void MeshRendersRedPixelOnGpuSurface()
		{
			// SkMesh rendering only works on GPU-backed surfaces (SkBitmapDevice::drawMesh
			// is a no-op in upstream Skia), so this test requires a real GL context.
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();
			Assert.NotNull(grContext);

			using var cs = SKColorSpace.CreateSrgb();
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes, sizeof(float) * 2, EmptyVaryings,
				SimpleVertexShader, ColorFragmentShader, cs, SKAlphaType.Premul, out var errors);
			Assert.NotNull(spec);
			Assert.Null(errors);

			using var vb = SKMeshVertexBuffer.Create(grContext, MemoryMarshal.AsBytes(QuadVertices.AsSpan()));
			using var ib = SKMeshIndexBuffer.Create(grContext, MemoryMarshal.AsBytes(QuadIndices.AsSpan()));
			using var builder = spec.CreateIndexedMeshBuilder();
			using var mesh = CreateFullCanvasRedMesh(spec, builder, vb, ib);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);

			builder.Dispose();
			vb.Dispose();
			ib.Dispose();

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			Assert.NotNull(surface);
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Blue);

			// The mesh's fragment-shader colour (red) is modulated with the paint colour by the
			// default blender, so a white paint is required for the mesh's own colour to show —
			// a default (black) paint would render the mesh black. See DrawMeshDefaultPaintModulatesToBlack.
			using var paint = new SKPaint { Color = SKColors.White };
			canvas.DrawMesh(mesh, paint);
			canvas.Flush();

			using var img = surface.Snapshot();
			using var raster = img.ToRasterImage(true);
			using var bmp = SKBitmap.FromImage(raster);

			Assert.Equal(SKColors.Red, bmp.GetPixel(50, 50));
			Assert.Equal(SKColors.Red, bmp.GetPixel(10, 10));
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void MeshRetainsSpecBuffersUniformsAndChildrenAfterWrappersDisposed()
		{
			const string vs = @"
				Varyings main(const Attributes attrs) {
					Varyings v;
					v.position = attrs.position;
					return v;
				}";

			const string fs = @"
				uniform float uScale;
				uniform shader uChild;
				float2 main(const Varyings varyings, out half4 color) {
					color = uChild.eval(varyings.position * uScale);
					return varyings.position;
				}";

			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();
			Assert.NotNull(grContext);

			using var cs = SKColorSpace.CreateSrgb();
			var spec = SKMeshSpecification.Create(
				SimpleAttributes, sizeof(float) * 2, EmptyVaryings,
				vs, fs, cs, SKAlphaType.Premul, out var errors);
			Assert.NotNull(spec);
			Assert.Null(errors);

			var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(QuadVertices.AsSpan()));
			var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(QuadIndices.AsSpan()));
			var childShader = SKShader.CreateColor(SKColors.Lime);

			var specHandle = spec.Handle;
			var vbHandle = vb.Handle;
			var ibHandle = ib.Handle;
			var childHandle = childShader.Handle;

			Assert.Equal(1, specHandle.GetReferenceCount(false));
			Assert.Equal(1, vbHandle.GetReferenceCount(true));
			Assert.Equal(1, ibHandle.GetReferenceCount(true));
			Assert.Equal(1, childHandle.GetReferenceCount(true));

			var builder = new SKIndexedMeshBuilder(spec)
			{
				Bounds = new SKRect(0, 0, 100, 100),
			};
			builder.Uniforms["uScale"] = 1f;
			builder.Children["uChild"] = childShader;
			builder.SetVertexBuffer(vb);
			builder.SetIndexBuffer(ib);

			using var mesh = builder.Build();
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);

			Assert.Equal(2, specHandle.GetReferenceCount(false));
			Assert.Equal(2, vbHandle.GetReferenceCount(true));
			Assert.Equal(2, ibHandle.GetReferenceCount(true));
			Assert.Equal(2, childHandle.GetReferenceCount(true));

			// Dispose the builder, specification, buffers, and child shader before drawing.
			builder.Dispose();
			spec.Dispose();
			vb.Dispose();
			ib.Dispose();
			childShader.Dispose();

			Assert.Equal(1, specHandle.GetReferenceCount(false));
			Assert.Equal(1, vbHandle.GetReferenceCount(true));
			Assert.Equal(1, ibHandle.GetReferenceCount(true));
			Assert.Equal(1, childHandle.GetReferenceCount(true));

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Blue);

			using var paint = new SKPaint { Color = SKColors.White };
			canvas.DrawMesh(mesh, paint);
			canvas.Flush();

			using var img = surface.Snapshot();
			using var raster = img.ToRasterImage(true);
			using var bmp = SKBitmap.FromImage(raster);

			Assert.Equal(SKColors.Lime, bmp.GetPixel(50, 50));
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Fact]
		public void DrawMeshDefaultPaintModulatesToBlack()
		{
			// Documents the "black by default" gotcha: with a default (opaque black) paint,
			// the mesh colour is modulated to black rather than showing its own colour.
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();
			using var grContext = GRContext.CreateGl();

			using var cs = SKColorSpace.CreateSrgb();
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes, sizeof(float) * 2, EmptyVaryings,
				SimpleVertexShader, ColorFragmentShader, cs, SKAlphaType.Premul, out _);
			Assert.NotNull(spec);

			using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(QuadVertices.AsSpan()));
			using var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(QuadIndices.AsSpan()));
			using var builder = spec.CreateIndexedMeshBuilder();
			using var mesh = CreateFullCanvasRedMesh(spec, builder, vb, ib);
			Assert.NotNull(mesh);

			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.Blue);

			using var paint = new SKPaint (); // default: opaque black
			canvas.DrawMesh(mesh, paint);
			canvas.Flush();

			using var img = surface.Snapshot();
			using var raster = img.ToRasterImage(true);
			using var bmp = SKBitmap.FromImage(raster);

			Assert.Equal(SKColors.Black, bmp.GetPixel(50, 50));
		}

		[Fact]
		public void DrawMeshOnRasterSurfaceIsNoOp()
		{
			// On a CPU/raster surface DrawMesh is a documented no-op (upstream Skia limitation):
			// it must not crash and must not modify the canvas.
			using var cs = SKColorSpace.CreateSrgb();
			using var spec = SKMeshSpecification.Create(
				SimpleAttributes, sizeof(float) * 2, EmptyVaryings,
				SimpleVertexShader, ColorFragmentShader, cs, SKAlphaType.Premul, out var errors);
			Assert.NotNull(spec);
			Assert.Null(errors);

			using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(QuadVertices.AsSpan()));
			using var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(QuadIndices.AsSpan()));
			using var builder = spec.CreateIndexedMeshBuilder();
			using var mesh = CreateFullCanvasRedMesh(spec, builder, vb, ib);
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);

			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var paint = new SKPaint { Color = SKColors.White };
			canvas.DrawMesh(mesh, paint);
			canvas.Flush();

			using var img = surface.Snapshot();
			using var bmp = SKBitmap.FromImage(img);
			Assert.Equal(SKColors.White, bmp.GetPixel(50, 50)); // unchanged — mesh was not drawn
		}

		[Fact]
		public void MeshWithVaryingsAndPaintShaderDoesNotCrash()
		{
			// Verifies varyings + UV shader setup works (creation and draw don't crash)
			// Actual pixel output requires GPU backend (SkBitmapDevice::drawMesh is a no-op)
			const string vs = @"
				Varyings main(const Attributes attrs) {
					Varyings v;
					v.position = attrs.position;
					v.uv = attrs.uv;
					return v;
				}";

			const string fs = @"
				float2 main(const Varyings varyings) {
					return varyings.uv * float2(100.0, 100.0);
				}";

			var specBuilder = new SKMeshSpecificationBuilder(vs, fs);
			specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
			specBuilder.AddAttribute("uv", SKMeshSpecificationAttributeType.Float2);
			specBuilder.AddVarying("uv", SKMeshSpecificationVaryingType.Float2);
			using var spec = specBuilder.Build(out var errors);
			Assert.NotNull(spec);
			Assert.Null(errors);

			// Quad covering 0,0 to 100,100 with UV 0-1
			var verts = new float[] {
				0, 0, 0, 0,
				100, 0, 1, 0,
				100, 100, 1, 1,
				0, 100, 0, 1,
			};
			var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

			using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(verts.AsSpan()));
			using var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(indices.AsSpan()));

			using var builder = spec.CreateIndexedMeshBuilder();
			builder.Bounds = new SKRect(0, 0, 100, 100);
			builder.SetVertexBuffer(vb);
			builder.SetIndexBuffer(ib);

			using var mesh = builder.Build();
			Assert.NotNull(mesh);
			Assert.True(mesh.IsValid);

			// Draw on CPU surface — no pixels expected, must not crash
			using var surface = SKSurface.Create(new SKImageInfo(100, 100));
			var canvas = surface.Canvas;
			canvas.Clear(SKColors.White);

			using var shader = SKShader.CreateColor(SKColors.Green);
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawMesh(mesh, paint);
			canvas.Flush();
		}

		public static IEnumerable<object[]> GpuRendererNames()
		{
			foreach (var name in RendererCatalog.NamesIn(Assembly.GetExecutingAssembly()))
			{
				if (name != GpuBackends.Raster)
					yield return new object[] { name };
			}
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[Theory]
		[MemberData(nameof(GpuRendererNames))]
		public async Task MeshRendersAcrossAllGpuRenderersIncludingGraphite(string rendererName)
		{
			GpuPolicy.RequireOrSkip(rendererName);

			var renderer = RendererCatalog.Get(rendererName);
			var scene = new MeshRenderTestScene();
			var pixels = await renderer.RenderAsync(scene, scene.Info, TestContext.Current.CancellationToken);

			// Center of the 100x100 surface (50, 50) in RGBA8888 premul should be Lime (0, 255, 0, 255).
			var offset = (50 * scene.Info.Width + 50) * 4;
			Assert.Equal(0, pixels[offset + 0]);
			Assert.Equal(255, pixels[offset + 1]);
			Assert.Equal(0, pixels[offset + 2]);
			Assert.Equal(255, pixels[offset + 3]);
		}

		private sealed class MeshRenderTestScene : ISkiaScene
		{
			public string Name => "mesh-render-test";

			public SKImageInfo Info => new(100, 100);

			public void Draw(SKCanvas canvas)
			{
				const string vs = @"
					Varyings main(const Attributes attrs) {
						Varyings v;
						v.position = attrs.position;
						return v;
					}";

				const string fs = @"
					uniform float uScale;
					uniform shader uChild;
					float2 main(const Varyings varyings, out half4 color) {
						color = uChild.eval(varyings.position * uScale);
						return varyings.position;
					}";

				using var cs = SKColorSpace.CreateSrgb();
				var specBuilder = SKMeshSpecification.CreateBuilder(vs, fs);
				specBuilder.ColorSpace = cs;
				specBuilder.AlphaType = SKAlphaType.Premul;
				specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
				using var spec = specBuilder.Build(out var errors);
				Assert.NotNull(spec);
				Assert.Null(errors);

				using var vb = SKMeshVertexBuffer.Create(MemoryMarshal.AsBytes(QuadVertices.AsSpan()));
				using var ib = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(QuadIndices.AsSpan()));
				using var childShader = SKShader.CreateColor(SKColors.Lime);

				using var builder = spec.CreateIndexedMeshBuilder();
				builder.Bounds = new SKRect(0, 0, 100, 100);
				builder.Uniforms["uScale"] = 1f;
				builder.Children["uChild"] = childShader;
				builder.SetVertexBuffer(vb);
				builder.SetIndexBuffer(ib);

				using var mesh = builder.Build();
				Assert.NotNull(mesh);
				Assert.True(mesh.IsValid);

				canvas.Clear(SKColors.Blue);
				using var paint = new SKPaint { Color = SKColors.White };
				canvas.DrawMesh(mesh, paint);
			}
		}

	}
}
