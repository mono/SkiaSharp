using System;
using System.Runtime.InteropServices;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class MeshSample : CanvasSampleBase
{
	private int visualMode;
	private int gridSize = 16;
	private float intensity = 0.5f;
	private float speed = 1f;

	private static readonly string[] VisualModes = { "Ripple Pond", "Silk Fabric", "Terrain Map", "Aurora Borealis" };

	// Vertex shader deforms the grid using a time uniform — no per-frame buffer recreation
	private const string VertexShader = @"
		uniform float uTime;
		uniform float uIntensity;
		uniform int uMode;
		uniform float2 uCenter;

		Varyings main(const Attributes attrs) {
			Varyings v;
			float2 pos = attrs.position;
			float2 uv = attrs.uv;
			float amp = uIntensity * 30.0;

			if (uMode == 0) {
				// Ripple Pond
				float dx = pos.x - uCenter.x;
				float dy = pos.y - uCenter.y;
				float dist = sqrt(dx*dx + dy*dy);
				float ripple = sin(dist * 0.06 - uTime * 4.0) * amp * (1.0 - clamp(dist / max(uCenter.x, uCenter.y), 0.0, 1.0));
				pos.y += ripple;
			} else if (uMode == 1) {
				// Silk Fabric
				float drape = sin(uv.x * 3.14159) * sin(uv.y * 3.14159);
				float wave = sin(uv.x * 6.28 + uTime * 0.8) * amp * 0.6;
				float wave2 = sin(uv.y * 9.42 + uTime * 0.5) * amp * 0.4;
				pos.y += (wave + wave2) * drape;
			} else if (uMode == 2) {
				// Terrain Map
				float e = 0.0;
				e += sin(uv.x * 3.2 + uTime*0.3) * cos(uv.y * 2.7 + uTime*0.2) * 0.5;
				e += sin(uv.x * 7.1 - uTime*0.1) * cos(uv.y * 6.3) * 0.25;
				e += sin(uv.x * 13.0 + uv.y * 11.0 + uTime*0.05) * 0.125;
				pos.y -= (e * 0.5 + 0.5) * amp * 2.0;
			} else {
				// Aurora
				float wave = sin(uv.x * 9.42 + uTime * 0.7) * amp * (1.0 - uv.y * 0.5);
				float sway = sin(uTime * 0.3 + uv.x * 2.0) * amp * 0.5;
				pos.y += wave + sway;
			}

			v.position = pos;
			v.color = attrs.color;
			v.uv = uv;
			return v;
		}";

	private const string FragmentShader = @"
		float2 main(const Varyings varyings, out half4 color) {
			color = varyings.color;
			return varyings.position;
		}";

	private static readonly SKMeshSpecificationAttribute[] MeshAttributes =
	{
		new(SKMeshSpecificationAttributeType.Float2, 0, "position"),
		new(SKMeshSpecificationAttributeType.Float4, 8, "color"),
		new(SKMeshSpecificationAttributeType.Float2, 24, "uv"),
	};

	private static readonly SKMeshSpecificationVarying[] MeshVaryings =
	{
		new(SKMeshSpecificationVaryingType.Float4, "color"),
		new(SKMeshSpecificationVaryingType.Float2, "uv"),
	};

	// position(2) + color(4) + uv(2) = 8 floats = 32 bytes
	private const int Stride = 8 * sizeof(float);
	private const int FloatsPerVertex = 8;

	private SKMeshSpecification? cachedSpec;
	private SKPaint? cachedPaint;
	private SKMeshVertexBuffer? cachedVB;
	private SKMeshIndexBuffer? cachedIB;
	private int cachedVertexCount;
	private int cachedIndexCount;
	private int cachedGridForBuffers;
	private int cachedModeForBuffers = -1;
	private SKRect cachedBounds;

	public override string Title => "Custom Mesh";

	public override string Category => SampleCategories.Shaders;

	public override string Description =>
		"Draw custom vertex meshes with SkSL shaders. Four visual modes demonstrate per-vertex deformation and color interpolation.";

	public override bool IsAnimated => true;

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("visualMode", "Visual Mode", VisualModes, visualMode),
		new SliderControl("gridSize", "Detail", 4, 25, gridSize, 1),
		new SliderControl("intensity", "Intensity", 0, 1, intensity, 0.01f),
		new SliderControl("speed", "Speed", 0, 3, speed, 0.1f),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "visualMode":
				visualMode = (int)value;
				break;
			case "gridSize":
				gridSize = (int)(float)value;
				break;
			case "intensity":
				intensity = (float)value;
				break;
			case "speed":
				speed = (float)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		// Background per mode
		var bgColor = visualMode switch
		{
			0 => new SKColor(0xFF0a1628),  // deep water
			1 => new SKColor(0xFF1a0a2e),  // purple
			2 => new SKColor(0xFF0a2040),  // ocean
			3 => new SKColor(0xFF050a18),  // night sky
			_ => new SKColor(0xFF1a1a2e),
		};
		canvas.Clear(bgColor);

		if (visualMode == 3)
			DrawStars(canvas, width, height);

		cachedSpec ??= SKMeshSpecification.Create(
			MeshAttributes, Stride, MeshVaryings,
			VertexShader, FragmentShader,
			SKColorSpace.CreateSrgb(), SKAlphaType.Premul, out _);

		if (cachedSpec == null)
			return;

		cachedPaint ??= new SKPaint { IsAntialias = true, Color = SKColors.White };

		EnsureBuffers(width, height);

		if (cachedVB == null || cachedIB == null)
			return;

		// Only the uniform data changes per frame — the mesh buffers stay fixed
		var time = (float)DateTime.Now.TimeOfDay.TotalSeconds * speed;
		var uniformData = new float[] { time, intensity, 0, 0, width / 2f, height / 2f };
		// uMode is int — write as int bits
		BitConverter.TryWriteBytes(MemoryMarshal.AsBytes(uniformData.AsSpan()).Slice(8, 4), visualMode);

		using var uniforms = SKData.CreateCopy(MemoryMarshal.AsBytes(uniformData.AsSpan()));

		using var mesh = cachedSpec.ToMeshIndexed(
			SKMeshMode.Triangles,
			cachedVB, cachedVertexCount, 0,
			cachedIB, cachedIndexCount, 0,
			uniforms,
			cachedBounds);

		if (mesh == null)
			return;

		canvas.DrawMesh(mesh, cachedPaint);
	}

	private void EnsureBuffers(int width, int height)
	{
		if (cachedVB != null && cachedGridForBuffers == gridSize
			&& cachedModeForBuffers == visualMode
			&& cachedBounds.Width == width && cachedBounds.Height == height)
			return;

		cachedVB?.Dispose();
		cachedIB?.Dispose();

		cachedBounds = new SKRect(0, 0, width, height);
		cachedGridForBuffers = gridSize;
		cachedModeForBuffers = visualMode;

		var cols = gridSize;
		var rows = visualMode == 3 ? (int)(gridSize * 0.6f) : gridSize;
		if (rows < 4) rows = 4;

		var margin = visualMode == 3 ? 0f : 20f;
		var gridW = width - 2 * margin;
		var gridH = visualMode == 3 ? height * 0.55f : height - 2 * margin;
		var topY = visualMode == 3 ? height * 0.08f : margin;

		cachedVertexCount = (cols + 1) * (rows + 1);
		var vertexData = new float[cachedVertexCount * FloatsPerVertex];

		for (int row = 0; row <= rows; row++)
		{
			for (int col = 0; col <= cols; col++)
			{
				var idx = (row * (cols + 1) + col) * FloatsPerVertex;
				var u = (float)col / cols;
				var v = (float)row / rows;

				var x = margin + u * gridW;
				var y = topY + v * gridH;

				vertexData[idx] = x;
				vertexData[idx + 1] = y;

				// Per-mode vertex colors
				float r, g, b, a;
				ComputeVertexColor(u, v, out r, out g, out b, out a);

				vertexData[idx + 2] = r;
				vertexData[idx + 3] = g;
				vertexData[idx + 4] = b;
				vertexData[idx + 5] = a;
				vertexData[idx + 6] = u;
				vertexData[idx + 7] = v;
			}
		}

		cachedIndexCount = cols * rows * 6;
		var indexData = new ushort[cachedIndexCount];
		var ii = 0;

		for (int row = 0; row < rows; row++)
		{
			for (int col = 0; col < cols; col++)
			{
				var topLeft = (ushort)(row * (cols + 1) + col);
				var topRight = (ushort)(topLeft + 1);
				var bottomLeft = (ushort)(topLeft + cols + 1);
				var bottomRight = (ushort)(bottomLeft + 1);

				indexData[ii++] = topLeft;
				indexData[ii++] = topRight;
				indexData[ii++] = bottomLeft;
				indexData[ii++] = topRight;
				indexData[ii++] = bottomRight;
				indexData[ii++] = bottomLeft;
			}
		}

		cachedVB = SKMeshVertexBuffer.Make(MemoryMarshal.AsBytes(vertexData.AsSpan()));
		cachedIB = SKMeshIndexBuffer.Make(MemoryMarshal.AsBytes(indexData.AsSpan()));
	}

	private void ComputeVertexColor(float u, float v, out float r, out float g, out float b, out float a)
	{
		a = 1f;

		switch (visualMode)
		{
			case 0: // Ripple Pond — blue-cyan gradient
				r = 0.05f;
				g = 0.15f + 0.35f * (1f - v);
				b = 0.4f + 0.5f * (1f - v);
				break;

			case 1: // Silk Fabric — purple iridescence
				var hue = (280f + u * 50f) % 360f;
				var c = SKColor.FromHsl(hue, 70f, 45f + v * 20f);
				r = c.Red / 255f;
				g = c.Green / 255f;
				b = c.Blue / 255f;
				break;

			case 2: // Terrain — altitude coloring baked in as gradient
				var elev = u * 0.4f + v * 0.6f;
				if (elev < 0.3f) { r = 0.05f; g = 0.2f; b = 0.5f; }
				else if (elev < 0.45f) { r = 0.7f; g = 0.65f; b = 0.45f; }
				else if (elev < 0.65f) { r = 0.15f; g = 0.5f; b = 0.12f; }
				else if (elev < 0.82f) { r = 0.45f; g = 0.4f; b = 0.35f; }
				else { r = 0.9f; g = 0.9f; b = 0.95f; }
				break;

			case 3: // Aurora — green curtain fading
				var horizShape = MathF.Sin(u * MathF.PI);
				var vertFade = MathF.Pow(1f - v, 1.5f);
				g = 0.6f * vertFade;
				r = 0.15f * vertFade;
				b = 0.3f * vertFade;
				a = horizShape * vertFade * 0.8f;
				break;

			default:
				r = g = b = 1f;
				break;
		}
	}

	private void DrawStars(SKCanvas canvas, int width, int height)
	{
		using var starPaint = new SKPaint { IsAntialias = true };
		var rng = new Random(42);

		for (int i = 0; i < 80; i++)
		{
			var x = (float)(rng.NextDouble() * width);
			var y = (float)(rng.NextDouble() * height * 0.7f);
			var size = 0.5f + (float)rng.NextDouble() * 1.5f;
			var brightness = (byte)(100 + rng.Next(155));
			starPaint.Color = new SKColor(brightness, brightness, (byte)Math.Min(255, brightness + 30));
			canvas.DrawCircle(x, y, size, starPaint);
		}
	}

	protected override void OnDestroy()
	{
		cachedSpec?.Dispose();
		cachedSpec = null;
		cachedPaint?.Dispose();
		cachedPaint = null;
		cachedVB?.Dispose();
		cachedVB = null;
		cachedIB?.Dispose();
		cachedIB = null;
	}
}
