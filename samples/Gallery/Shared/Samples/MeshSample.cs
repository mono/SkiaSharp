using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

/// <summary>
/// Demonstrates the SKMesh API with an NxN deformable grid textured with an image.
/// Ported from the Skia mesh2d demo: https://github.com/google/skia/blob/main/demos.skia.org/demos/mesh2d/index.html
/// Features: configurable grid LOD, animated deformations (squircle, twirl, wiggle, cylinder), wireframe overlay.
/// </summary>
public class MeshSample : CanvasSampleBase
{
	// Vertex shader: passes position and UV through to the fragment shader.
	private const string VS = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			v.uv = attrs.uv;
			return v;
		}";

	// Fragment shader: returns local coordinates for the paint's shader to sample.
	// No `out half4 color` means the paint's shader (our image) is used directly.
	private const string FS = @"
		uniform float2 uImageSize;
		float2 main(const Varyings varyings) {
			return varyings.uv * uImageSize;
		}";

	// Vertex layout: position (float2) + uv (float2)
	private static readonly SKMeshSpecificationAttribute[] Attrs =
	{
		new(SKMeshSpecificationAttributeType.Float2, 0, "position"),
		new(SKMeshSpecificationAttributeType.Float2, 8, "uv"),
	};

	private static readonly SKMeshSpecificationVarying[] Varyings =
	{
		new(SKMeshSpecificationVaryingType.Float2, "uv"),
	};

	private const int Stride = sizeof(float) * 4; // position (2) + uv (2)

	private static readonly string[] Animators = { "Squircle", "Twirl", "Wiggle", "Cylinder" };

	private SKMeshBuilder? builder;
	private SKImage? image;
	private SKShader? imageShader;
	private int lod = 8;
	private int animatorIndex = 3; // Cylinder
	private bool showMesh;
	private DateTime timeBase;

	public override string Title => "Custom Mesh";

	public override string Description =>
		"A deformable textured grid drawn with the SKMesh API. " +
		"An NxN vertex grid is animated by vertex deformation functions (squircle, twirl, wiggle, cylinder) " +
		"while UV coordinates map an image texture through SkSL shaders.";

	public override string Category => SampleManager.Shaders;

	public override bool IsAnimated => true;

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKMesh", "SKMeshSpecification", "SKMeshBuilder",
		"SKMeshVertexBuffer", "SKMeshIndexBuffer",
		"SKCanvas.DrawMesh",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("animator", "Animation", Animators, animatorIndex),
		new SliderControl("lod", "Grid Detail", 4, 64, lod, 4),
		new ToggleControl("showMesh", "Show Wireframe", showMesh),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "animator":
				animatorIndex = (int)value;
				break;
			case "lod":
				lod = Math.Max(4, (int)(float)value);
				break;
			case "showMesh":
				showMesh = (bool)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		timeBase = DateTime.UtcNow;

		// Load the baboon image and create an image shader
		using var stream = SampleMedia.Images.Baboon;
		using var data = SKData.Create(stream);
		image = SKImage.FromEncodedData(data);

		if (image != null)
		{
			imageShader = image.ToShader(
				SKShaderTileMode.Decal, SKShaderTileMode.Decal,
				SKSamplingOptions.Default);
		}

		// Build the mesh specification with SkSL shaders.
		// No color space needed since FS doesn't output color.
		builder = SKMeshSpecification.Build(
			Attrs, Stride, Varyings, VS, FS,
			null, SKAlphaType.Premul);

		return base.OnInit();
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(new SKColor(0xFF333333));

		if (builder == null || image == null || imageShader == null)
			return;

		var size = Math.Min(width, height) * 0.85f;
		var ox = (width - size) * 0.5f;
		var oy = (height - size) * 0.5f;

		// Compute animation parameter t ∈ [0, 1] (ping-pong)
		var elapsed = (DateTime.UtcNow - timeBase).TotalSeconds;
		var t = (float)Math.Abs((elapsed / 2.0) % 2.0 - 1.0);

		// Generate grid vertices with animation applied
		var n = lod;
		var vertexCount = n * n;
		var verts = new float[vertexCount * 4]; // position.xy + uv.xy per vertex

		for (var row = 0; row < n; row++)
		{
			for (var col = 0; col < n; col++)
			{
				var u = (float)col / (n - 1);
				var v = (float)row / (n - 1);

				// Apply the selected animator to get deformed position
				AnimateVertex(u, v, t, out var px, out var py);

				var idx = (row * n + col) * 4;
				verts[idx + 0] = ox + px * size; // position.x
				verts[idx + 1] = oy + py * size; // position.y
				verts[idx + 2] = u;               // uv.x
				verts[idx + 3] = v;               // uv.y
			}
		}

		// Generate index buffer: 2 triangles per grid cell
		var indexCount = (n - 1) * (n - 1) * 6;
		var indices = new ushort[indexCount];
		var ii = 0;
		for (var row = 0; row < n - 1; row++)
		{
			for (var col = 0; col < n - 1; col++)
			{
				var v0 = (ushort)(row * n + col);
				var v1 = (ushort)(v0 + n);

				indices[ii++] = v0;
				indices[ii++] = (ushort)(v0 + 1);
				indices[ii++] = (ushort)(v1 + 1);

				indices[ii++] = v0;
				indices[ii++] = v1;
				indices[ii++] = (ushort)(v1 + 1);
			}
		}

		// Create GPU buffers
		using var vb = SKMeshVertexBuffer.Make(MemoryMarshal.AsBytes(verts.AsSpan()));
		using var ib = SKMeshIndexBuffer.Make(MemoryMarshal.AsBytes(indices.AsSpan()));

		if (vb == null || ib == null)
			return;

		// Set uniforms — the paint shader handles the image texture
		builder.Uniforms["uImageSize"] = (ReadOnlySpan<float>)new[] { (float)image.Width, (float)image.Height };
		builder.Bounds = new SKRect(0, 0, width, height);

		// Build the mesh and draw
		using var mesh = builder.BuildIndexed(vb, vertexCount, 0, ib, indexCount, 0);

		if (mesh == null || !mesh.IsValid)
			return;

		// The paint's shader is sampled at the local coords returned by the FS
		using var paint = new SKPaint { Shader = imageShader };
		canvas.DrawMesh(mesh, paint);

		// Optional wireframe overlay
		if (showMesh)
			DrawWireframe(canvas, verts, indices, n);
	}

	private void AnimateVertex(float u, float v, float t, out float px, out float py)
	{
		switch (animatorIndex)
		{
			case 0: // Squircle
				SquircleAnimator(u, v, t, out px, out py);
				break;
			case 1: // Twirl
				TwirlAnimator(u, v, t, out px, out py);
				break;
			case 2: // Wiggle
				WiggleAnimator(u, v, t, out px, out py);
				break;
			case 3: // Cylinder
				CylinderAnimator(u, v, t, out px, out py);
				break;
			default:
				px = u;
				py = v;
				break;
		}
	}

	private static void SquircleAnimator(float u, float v, float t, out float px, out float py)
	{
		var cx = u - 0.5f;
		var cy = v - 0.5f;
		var maxAbs = MathF.Max(MathF.Abs(cx), MathF.Abs(cy));
		if (maxAbs < 0.0001f)
		{
			px = u;
			py = v;
			return;
		}
		var d = MathF.Sqrt(cx * cx + cy * cy) * 0.5f / maxAbs;
		var s = Lerp(1f, 0.5f / d, t);
		px = cx * s + 0.5f;
		py = cy * s + 0.5f;
	}

	private static void TwirlAnimator(float u, float v, float t, out float px, out float py)
	{
		const float maxRotate = MathF.PI * 4f;
		var cx = u - 0.5f;
		var cy = v - 0.5f;
		var r = MathF.Sqrt(cx * cx + cy * cy);
		var a = maxRotate * r * t;
		px = cx * MathF.Cos(a) - cy * MathF.Sin(a) + 0.5f;
		py = cy * MathF.Cos(a) + cx * MathF.Sin(a) + 0.5f;
	}

	private void WiggleAnimator(float u, float v, float t, out float px, out float py)
	{
		var radius = t * 0.2f / (MathF.Sqrt(lod) - 1f);
		var vertIdx = (int)((v * lod + u) * 100f); // pseudo-index for phase
		var phase = vertIdx * MathF.PI * 0.1505f;
		var angle = phase + t * MathF.PI * 2f;
		px = u + radius * MathF.Cos(angle);
		py = v + radius * MathF.Sin(angle);
	}

	private static void CylinderAnimator(float u, float v, float t, out float px, out float py)
	{
		const float cylRadius = 0.2f;
		var cylPos = t;

		py = v;
		if (u <= cylPos)
		{
			px = u;
			return;
		}

		var arcLen = u - cylPos;
		var arcAng = arcLen / cylRadius;
		px = cylPos + MathF.Sin(arcAng) * cylRadius;
	}

	private static float Lerp(float a, float b, float t) => a + t * (b - a);

	private static void DrawWireframe(SKCanvas canvas, float[] verts, ushort[] indices, int n)
	{
		using var wirePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(80, 180, 255, 160),
			StrokeWidth = 0.8f,
		};

		for (var i = 0; i < indices.Length; i += 3)
		{
			var i0 = indices[i] * 4;
			var i1 = indices[i + 1] * 4;
			var i2 = indices[i + 2] * 4;

			using var pathBuilder = new SKPathBuilder();
			pathBuilder.MoveTo(verts[i0], verts[i0 + 1]);
			pathBuilder.LineTo(verts[i1], verts[i1 + 1]);
			pathBuilder.LineTo(verts[i2], verts[i2 + 1]);
			pathBuilder.Close();
			using var path = pathBuilder.Detach();
			canvas.DrawPath(path, wirePaint);
		}
	}

	protected override void OnDestroy()
	{
		imageShader?.Dispose(); imageShader = null;
		image?.Dispose(); image = null;
		builder?.Dispose(); builder = null;
		base.OnDestroy();
	}
}
