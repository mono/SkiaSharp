using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// A per-frame animated-shader workload against the public runtime-effect API: one compiled
// SKRuntimeEffect is reused, and each op rebuilds the uniforms (a float time + an SKColor
// tint), turns them into a shader, and paints a full-surface rect. This is the common path
// an app with an animated SkSL shader pays every frame, and it stitches together several of
// the areas the recent PRs touched:
//   * SKRuntimeEffectUniforms build + ToData lifecycle — where #4442 fixed an SKData leak;
//   * the managed SKColor -> SKColorF convert (#4370) is paid when setting the colour uniform;
//   * shader creation + a real draw.
// Per-frame allocations are the headline signal (a leak or extra managed alloc here is paid
// at frame rate), alongside the wall-clock time.
public class RuntimeEffectShaderBenchmark
{
	private const int Size = 256;

	// Deterministic, self-contained SkSL (no external content). Uses only the two uniforms
	// the benchmark sets each frame so the compile is stable across every SkiaSharp version.
	private const string Sksl =
		"uniform float time;" +
		"uniform float4 tint;" +
		"half4 main(float2 p) {" +
		"  float2 uv = p / float2(256.0, 256.0);" +
		"  float w = 0.5 + 0.5 * sin(time + uv.x * 6.2831853);" +
		"  return half4(half3(tint.rgb) * half(w), half(tint.a));" +
		"}";

	private SKSurface _surface = null!;
	private SKCanvas _canvas = null!;
	private SKRuntimeEffect _effect = null!;
	private SKPaint _paint = null!;
	private SKRect _rect;

	[GlobalSetup]
	public void Setup ()
	{
		_surface = SKSurface.Create (new SKImageInfo (Size, Size));
		_canvas = _surface.Canvas;
		_effect = SKRuntimeEffect.CreateShader (Sksl, out var errors)
			?? throw new InvalidOperationException ("SkSL failed to compile: " + errors);
		_paint = new SKPaint ();
		_rect = new SKRect (0, 0, Size, Size);
	}

	[GlobalCleanup]
	public void Cleanup ()
	{
		_paint.Dispose ();
		_effect.Dispose ();
		_surface.Dispose ();
	}

	// One animated frame: fresh uniforms (time + an SKColor tint that hits the #4370 convert),
	// build the shader, paint it. The shader is disposed after the draw (the native paint holds
	// its own ref for the duration of DrawRect).
	[Benchmark]
	public int DrawFrame ()
	{
		using var uniforms = new SKRuntimeEffectUniforms (_effect) {
			{ "time", 1.5f },
			{ "tint", new SKColor (90, 160, 220, 255) }, // SKColor -> SKColorF convert (#4370)
		};
		using var shader = _effect.ToShader (uniforms);
		_paint.Shader = shader;
		_canvas.DrawRect (_rect, _paint);
		_canvas.Flush ();
		return Size;
	}
}
