using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// A mesh rebuilt every frame (animation, a game scene): one reused SKMeshBuilder, its uniforms set
// by name each frame, then Build(). The vertex buffer is created once; only the uniforms change.
//
//   dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*SKMeshUniformsBenchmark*'
//
// Run it on the commit before and after a change to SKRuntimeEffectUniforms: the uniforms class keeps
// its state in private fields, so the old body cannot be copied inline as an Old baseline.
[MemoryDiagnoser]
public class SKMeshUniformsBenchmark
{
	private const string Uniforms = "uniform float4 u0; uniform float4 u1; uniform float4 u2; uniform float4 u3; uniform float4 u4; uniform float4 u5; uniform float4 u6; uniform float4 u7;";

	// How many float4 uniforms the scene sets per frame.
	[Params(1, 5, 8)]
	public int Count { get; set; }

	private SKMeshSpecification spec;
	private SKMeshVertexBuffer vertices;
	private SKMeshBuilder builder;
	private string[] names;
	private float[] value;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var vs = Uniforms + " Varyings main(const Attributes a) { Varyings v; v.position = a.position; return v; }";
		var fs = Uniforms + " float2 main(const Varyings v, out half4 color) { color = half4(u0 + u1 + u2 + u3 + u4 + u5 + u6 + u7); return v.position; }";
		spec = SKMeshSpecification.Create(
			[new SKMeshSpecificationAttribute(SKMeshSpecificationAttributeType.Float2, 0, "position")], 8,
			[], vs, fs, SKColorSpace.CreateSrgb(), SKAlphaType.Premul, out var errors)
			?? throw new InvalidOperationException(errors);
		ReadOnlySpan<float> triangle = [10, 10, 90, 10, 50, 90];
		vertices = SKMeshVertexBuffer.Create(System.Runtime.InteropServices.MemoryMarshal.AsBytes(triangle));
		builder = spec.CreateMeshBuilder();
		builder.SetVertexBuffer(vertices, 3);
		builder.Bounds = new SKRect(0, 0, 100, 100);
		names = ["u0", "u1", "u2", "u3", "u4", "u5", "u6", "u7"];
		value = [0.5f, 0.25f, 0.125f, 1f];
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		builder.Dispose();
		vertices.Dispose();
		spec.Dispose();
	}

	// One frame: set the uniforms by name, build the mesh.
	[Benchmark(Baseline = true)]
	public int SetAndBuild()
	{
		var uniforms = builder.Uniforms;
		for (var i = 0; i < Count; i++)
			uniforms[names[i]] = value.AsSpan();
		using var mesh = builder.Build();
		return mesh.VertexCount;
	}

	// One frame that clears the previous frame's values first.
	[Benchmark]
	public int ResetSetAndBuild()
	{
		var uniforms = builder.Uniforms;
		uniforms.Reset();
		for (var i = 0; i < Count; i++)
			uniforms[names[i]] = value.AsSpan();
		using var mesh = builder.Build();
		return mesh.VertexCount;
	}
}
