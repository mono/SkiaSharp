using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures the per-frame hot path for dynamic SKMesh rendering:
//   1. Updating a pre-allocated SKMeshVertexBuffer with new vertex positions/UVs;
//   2. Updating uniforms on a reused SKMeshBuilder / SKIndexedMeshBuilder;
//   3. Building the SKMesh and disposing it at the end of the frame.
//
// Also compares:
//   - Allocating a new SKMeshBuilder + SKMeshVertexBuffer every frame ("AllocatePerFrame")
//     vs reusing the builder and vertex buffer across frames ("ReuseBuilderAndBuffer").
//   - RentedUtf8Strings vs GetEncodedText + MemoryHandle.Pin for attribute/varying name marshalling.
[MemoryDiagnoser]
public class SKMeshBenchmark
{
	private const string VertexShader = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			v.uv = attrs.uv;
			return v;
		}";

	private const string FragmentShader = @"
		uniform float uTime;
		uniform float4 uTint;
		uniform shader uTex;
		float2 main(const Varyings v, out half4 color) {
			color = uTex.eval(v.uv) * half4(uTint);
			return v.position + float2(uTime, 0.0);
		}";

	private const int QuadCount = 256;
	private const int VertexCount = QuadCount * 4;
	private const int IndexCount = QuadCount * 6;
	private const int FloatsPerVertex = 4; // position(2) + uv(2)

	private float[] vertices;
	private ushort[] indices;
	private SKColorSpace colorSpace;
	private SKShader childShader;
	private SKMeshSpecification spec;
	private SKMeshVertexBuffer vertexBuffer;
	private SKMeshIndexBuffer indexBuffer;
	private SKIndexedMeshBuilder reusableBuilder;
	private float frameTime;

	[GlobalSetup]
	public void Setup ()
	{
		vertices = new float[VertexCount * FloatsPerVertex];
		indices = new ushort[IndexCount];

		for (var q = 0; q < QuadCount; q++) {
			var vBase = (ushort)(q * 4);
			var iBase = q * 6;
			indices[iBase + 0] = vBase;
			indices[iBase + 1] = (ushort)(vBase + 1);
			indices[iBase + 2] = (ushort)(vBase + 2);
			indices[iBase + 3] = vBase;
			indices[iBase + 4] = (ushort)(vBase + 2);
			indices[iBase + 5] = (ushort)(vBase + 3);
		}

		colorSpace = SKColorSpace.CreateSrgb ();
		childShader = SKShader.CreateColor (SKColors.CornflowerBlue);

		var specBuilder = SKMeshSpecification.CreateBuilder (VertexShader, FragmentShader);
		specBuilder.ColorSpace = colorSpace;
		specBuilder.AlphaType = SKAlphaType.Premul;
		specBuilder.AddAttribute ("position", SKMeshSpecificationAttributeType.Float2);
		specBuilder.AddAttribute ("uv", SKMeshSpecificationAttributeType.Float2);
		specBuilder.AddVarying ("uv", SKMeshSpecificationVaryingType.Float2);
		spec = specBuilder.Build (out var errors)
			?? throw new InvalidOperationException ("Failed to compile mesh specification: " + errors);

		vertexBuffer = SKMeshVertexBuffer.Create (MemoryMarshal.AsBytes (vertices.AsSpan ()));
		indexBuffer = SKMeshIndexBuffer.Create (MemoryMarshal.AsBytes (indices.AsSpan ()));

		reusableBuilder = spec.CreateIndexedMeshBuilder ();
		reusableBuilder.Bounds = new SKRect (0, 0, 512, 512);
		reusableBuilder.Children["uTex"] = childShader;
		reusableBuilder.SetVertexBuffer (vertexBuffer);
		reusableBuilder.SetIndexBuffer (indexBuffer);
	}

	[GlobalCleanup]
	public void Cleanup ()
	{
		reusableBuilder?.Dispose ();
		indexBuffer?.Dispose ();
		vertexBuffer?.Dispose ();
		spec?.Dispose ();
		childShader?.Dispose ();
		colorSpace?.Dispose ();
	}

	[Benchmark (Baseline = true)]
	public bool PerFrameAllocateBuilderAndBuffers ()
	{
		frameTime += 0.016f;
		using var vb = SKMeshVertexBuffer.Create (MemoryMarshal.AsBytes (vertices.AsSpan ()));
		using var ib = SKMeshIndexBuffer.Create (MemoryMarshal.AsBytes (indices.AsSpan ()));
		using var builder = spec.CreateIndexedMeshBuilder ();
		builder.Bounds = new SKRect (0, 0, 512, 512);
		builder.Uniforms["uTime"] = frameTime;
		builder.Uniforms["uTint"] = new SKColorF (1f, 0.9f, 0.8f, 1f);
		builder.Children["uTex"] = childShader;
		builder.SetVertexBuffer (vb);
		builder.SetIndexBuffer (ib);
		using var mesh = builder.Build ();
		return mesh.IsValid;
	}

	[Benchmark]
	public bool PerFrameReuseBuilderAndUpdateBuffer ()
	{
		frameTime += 0.016f;
		vertexBuffer.Update (MemoryMarshal.AsBytes (vertices.AsSpan ()), 0);
		reusableBuilder.Uniforms["uTime"] = frameTime;
		reusableBuilder.Uniforms["uTint"] = new SKColorF (1f, 0.9f, 0.8f, 1f);
		using var mesh = reusableBuilder.Build ();
		return mesh.IsValid;
	}
}
