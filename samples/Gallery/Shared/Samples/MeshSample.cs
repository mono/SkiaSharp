using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class MeshSample : CanvasSampleBase
{
	// A simple mesh shader: position-only vertices, solid animated color.
	// Fragment shader outputs `out half4 color` so a color space is required.
	private const string VS = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			return v;
		}";

	private const string FS = @"
		uniform float3 uColor;
		float2 main(const Varyings varyings, out half4 color) {
			color = half4(half3(uColor), 1.0);
			return varyings.position;
		}";

	private static readonly SKMeshSpecificationAttribute[] Attrs =
		{ new(SKMeshSpecificationAttributeType.Float2, 0, "position") };

	private const int Stride = sizeof(float) * 2;

	// Quad geometry: 4 vertices (positions only), 6 indices (2 triangles)
	private static readonly ushort[] Indices = { 0, 1, 2, 0, 2, 3 };

	// Builder is created once and reused each frame — expensive SkSL
	// compilation happens in Build(), cheap mesh creation happens per frame.
	private SKMeshBuilder? builder;
	private SKMeshIndexBuffer? ib;

	public override string Title => "Custom Mesh";

	public override string Description =>
		"An animated colored quad drawn with the SkMesh API. Vertex positions and colors " +
		"are driven by custom SkSL vertex and fragment shaders with uniform data.";

	public override string Category => SampleManager.Shaders;

	public override bool IsAnimated => true;

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKMesh", "SKMeshSpecification", "SKMeshBuilder",
		"SKMeshVertexBuffer", "SKMeshIndexBuffer",
		"SKCanvas.DrawMesh",
	];

	protected override Task OnInit ()
	{
		using var cs = SKColorSpace.CreateSrgb ();

		builder = SKMeshSpecification.Build (
			Attrs, Stride,
			Array.Empty<SKMeshSpecificationVarying> (),
			VS, FS,
			cs, SKAlphaType.Premul);

		// Index buffer is static — allocate once and reuse across frames
		var iData = MemoryMarshal.AsBytes (Indices.AsSpan ());
		ib = SKMeshIndexBuffer.Make (iData);

		return base.OnInit ();
	}

	protected override void OnDrawSample (SKCanvas canvas, int width, int height)
	{
		canvas.Clear (SKColors.Black);

		if (builder == null || ib == null)
			return;

		// Animate a smooth color cycle through hue using offset sine waves
		var t = (float)(DateTime.Now.TimeOfDay.TotalSeconds * 2.0);
		var r = (float)(Math.Sin (t) * 0.5 + 0.5);
		var g = (float)(Math.Sin (t + 2.094) * 0.5 + 0.5);   // offset +2π/3
		var b = (float)(Math.Sin (t + 4.189) * 0.5 + 0.5);   // offset +4π/3

		// Quad with margin, filling the canvas
		var margin = Math.Min (width, height) * 0.1f;
		var l = margin; var tp = margin;
		var ri = width - margin; var bt = height - margin;

		var verts = new float[]
		{
			l,  tp,   // top-left
			ri, tp,   // top-right
			ri, bt,   // bottom-right
			l,  bt,   // bottom-left
		};
		using var vb = SKMeshVertexBuffer.Make (MemoryMarshal.AsBytes (verts.AsSpan ()));

		builder.Uniforms["uColor"] = (ReadOnlySpan<float>)new[] { r, g, b };
		builder.Bounds = new SKRect (0, 0, width, height);

		using var mesh = builder.BuildIndexed (vb, 4, 0, ib, 6, 0);

		if (mesh == null || !mesh.IsValid)
			return;

		using var paint = new SKPaint ();
		canvas.DrawMesh (mesh, paint);
	}

	protected override void OnDestroy ()
	{
		ib?.Dispose (); ib = null;
		builder?.Dispose (); builder = null;
		base.OnDestroy ();
	}
}
