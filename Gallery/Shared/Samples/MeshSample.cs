using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

/// <summary>
/// Demonstrates the SKMesh API with an automated 3D winding tunnel/maze and glowing collectible orbs.
/// Inspired by DrawnUi.Dungeon.Net: every visible tunnel slice and orb sprite is projected into a single
/// custom vertex mesh with perspective-correct attributes (UVs, depth, surface normal, material/glow)
/// and shaded by an SkSL fragment program in one DrawMesh call.
/// </summary>
public class MeshSample : CanvasSampleBase
{
	public override DateOnly? DateAdded => new DateOnly(2026, 11, 16);
	private const string VS = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			v.uvDepth = attrs.uvDepth;
			v.normalKind = attrs.normalKind;
			return v;
		}";

	// Fragment shader:
	// - Varyings carry perspective-divided (u/z, v/z, 1/z, z) in uvDepth and (nx, ny, kind, seed) in normalKind
	// - Samples the procedural brick/stone child shader for walls/floor/ceiling, or renders glowing plasma orbs
	// - Applies torch point-light attenuation, orb proximity lighting, and distance fog
	private const string FS = @"
		uniform float uTime;
		uniform float3 uTorchColor;
		uniform float3 uOrbColor;
		uniform shader uWallTexture;

		float2 main(const Varyings v, out half4 color) {
			float invZ = max(v.uvDepth.z, 0.0001);
			float2 uv = v.uvDepth.xy / invZ;
			float depth = v.uvDepth.w;
			float kind = v.normalKind.z;

			// Distance fog factor
			float fog = clamp(exp(-depth * 0.055), 0.0, 1.0);
			float3 fogColor = float3(0.04, 0.05, 0.09);

			if (kind > 0.5) {
				// Glowing collectible orb billboard
				float2 p = uv * 2.0 - 1.0;
				float r2 = dot(p, p);
				if (r2 > 1.0) {
					color = half4(0.0);
					return v.position;
				}
				float core = exp(-r2 * 3.5);
				float pulse = 0.85 + 0.15 * sin(uTime * 6.0 + v.normalKind.w * 6.28318);
				float3 orbCol = mix(uOrbColor, float3(1.0, 0.98, 0.9), core) * (1.4 * pulse * (1.0 - r2));
				float alpha = clamp((1.0 - r2) * fog, 0.0, 1.0);
				color = half4(half3(orbCol * alpha), half(alpha));
				return v.position;
			}

			// Sample procedural stone/brick texture at perspective-correct UVs
			half4 tex = uWallTexture.eval(uv * 128.0);

			// Torch flicker + normal lighting + nearest-orb glow
			float flicker = 0.92 + 0.08 * sin(uTime * 11.0 + depth * 0.7);
			float2 n = v.normalKind.xy;
			float ndotl = 0.55 + 0.45 * abs(n.x * 0.6 + n.y * 0.8);
			float torchAtten = 1.35 / (1.0 + depth * 0.08 + depth * depth * 0.006);
			float orbGlow = v.normalKind.w * (1.0 / (1.0 + depth * 0.05));

			float3 lit = float3(tex.rgb) * (uTorchColor * torchAtten * flicker * ndotl + float3(0.08, 0.10, 0.16))
			           + uOrbColor * (orbGlow * 0.45);

			float3 finalRgb = mix(fogColor, lit, fog);
			color = half4(half3(clamp(finalRgb, 0.0, 1.0)), 1.0);
			return v.position;
		}";

	private const int FloatsPerVertex = 10; // pos(2) + uvDepth(4) + normalKind(4)
	private const int Rings = 28;
	private const int Sides = 8; // octagonal winding tunnel
	private const int MaxQuads = Rings * Sides + Rings; // tunnel walls + orbs
	private const int MaxVertices = MaxQuads * 6; // 2 non-indexed triangles (6 vertices) per quad

	private static readonly string[] Palettes = { "Dungeon Torch", "Cyber Neon", "Emerald Cavern" };

	private readonly float[] vertices = new float[MaxVertices * FloatsPerVertex];

	private SKMeshSpecification? spec;
	private SKMeshBuilder? builder;
	private SKMeshVertexBuffer? vertexBuffer;
	private SKShader? wallShader;
	private SKPaint? meshPaint;
	private SKPaint? wirePaint;
	private SKPaint? hudPaint;
	private SKFont? hudFont;

	private int paletteIndex;
	private float speed = 1.0f;
	private float curvature = 1.0f;
	private bool showWireframe;
	private DateTime startTime;
	private DateTime lastFrameTime;
	private float cameraDistance;
	private int orbsCollected;
	private int lastOrbRing = -1;

	public override string Title => "Custom Mesh (3D Tunnel)";

	public override string Description =>
		"An automated 3D winding tunnel with collectible glowing orbs rendered in a single SKCanvas.DrawMesh call. " +
		"Demonstrates SKMeshSpecificationBuilder, SKMeshBuilder, perspective-correct custom varyings, dynamic buffer updates, and child shaders.";

	public override string Category => SampleManager.Shaders;

	public override bool IsAnimated => true;

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKMesh", "SKMeshSpecification", "SKMeshSpecificationBuilder",
		"SKMeshBuilder", "SKMeshVertexBuffer",
		"SKCanvas.DrawMesh",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("palette", "Theme", Palettes, paletteIndex),
		new SliderControl("speed", "Flight Speed", 0.2f, 2.5f, speed, 0.1f),
		new SliderControl("curvature", "Maze Winding", 0.0f, 2.0f, curvature, 0.1f),
		new ToggleControl("wireframe", "Show Wireframe", showWireframe),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "palette":
				paletteIndex = (int)value;
				break;
			case "speed":
				speed = (float)value;
				break;
			case "curvature":
				curvature = (float)value;
				break;
			case "wireframe":
				showWireframe = (bool)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		var now = DateTime.UtcNow;
		startTime = now;
		lastFrameTime = now;
		cameraDistance = 0f;
		orbsCollected = 0;
		lastOrbRing = -1;
		return base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(16, token);
	}

	private void EnsureResources()
	{
		if (builder != null)
			return;

		wallShader = CreateProceduralStoneShader();

		var specBuilder = SKMeshSpecification.CreateBuilder(VS, FS);
		specBuilder.ColorSpace = SKColorSpace.CreateSrgb();
		specBuilder.AlphaType = SKAlphaType.Premul;
		specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
		specBuilder.AddAttribute("uvDepth", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddAttribute("normalKind", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddVarying("uvDepth", SKMeshSpecificationVaryingType.Float4);
		specBuilder.AddVarying("normalKind", SKMeshSpecificationVaryingType.Float4);

		spec = specBuilder.Build(out var errors);
		if (spec == null)
		{
			Console.WriteLine($"Failed to build mesh specification: {errors}");
			return;
		}

		vertexBuffer = SKMeshVertexBuffer.Create(MaxVertices * FloatsPerVertex * sizeof(float));

		builder = spec.CreateMeshBuilder();
		if (wallShader != null)
			builder.Children["uWallTexture"] = wallShader;

		meshPaint = new SKPaint { Color = SKColors.White };
		wirePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(120, 220, 255, 110),
			StrokeWidth = 1f,
		};
		hudPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(230, 240, 255, 220),
		};
		hudFont = new SKFont(SampleMedia.Fonts.Default, 16f);
	}

	private static SKShader CreateProceduralStoneShader()
	{
		using var surface = SKSurface.Create(new SKImageInfo(128, 128));
		var c = surface.Canvas;
		c.Clear(new SKColor(58, 62, 74));

		using var blockPaint = new SKPaint { IsAntialias = true };
		using var mortarPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(28, 30, 38),
			StrokeWidth = 3f,
			Style = SKPaintStyle.Stroke,
		};

		var rows = 4;
		var cols = 2;
		var rowH = 128f / rows;
		var colW = 128f / cols;

		for (var r = 0; r < rows; r++)
		{
			var offset = (r % 2) * (colW * 0.5f);
			for (var col = -1; col <= cols; col++)
			{
				var x = col * colW + offset;
				var y = r * rowH;
				var shade = (byte)(68 + ((r * 3 + col * 7) & 3) * 10);
				blockPaint.Color = new SKColor(shade, (byte)(shade + 4), (byte)(shade + 12));
				var rect = new SKRect(x + 2, y + 2, x + colW - 2, y + rowH - 2);
				c.DrawRoundRect(rect, 4, 4, blockPaint);
				c.DrawRoundRect(rect, 4, 4, mortarPaint);
			}
		}

		using var img = surface.Snapshot();
		return img.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, new SKSamplingOptions(SKFilterMode.Linear));
	}

	// Smooth 3D winding path (left/right/up/down maze turns) as a function of world Z
	private void GetTunnelCenter(float z, out float cx, out float cy)
	{
		var k = curvature;
		cx = k * (MathF.Sin(z * 0.17f) * 2.2f + MathF.Cos(z * 0.07f - 1.1f) * 1.6f);
		cy = k * (MathF.Cos(z * 0.13f + 0.4f) * 1.5f + MathF.Sin(z * 0.05f + 2.0f) * 1.1f);
	}

	private static bool HasOrbAtRing(int ringIndex, out float angleOffset)
	{
		// Deterministic pseudo-random orb placement every few rings
		var hash = (uint)(ringIndex * 2654435761u);
		angleOffset = ((hash >> 8) & 0xFF) / 255f * MathF.PI * 2f;
		return (ringIndex % 3) == 0;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(new SKColor(10, 13, 23));
		EnsureResources();

		if (builder == null || vertexBuffer == null || meshPaint == null)
			return;

		var now = DateTime.UtcNow;
		var dt = Math.Clamp((float)(now - lastFrameTime).TotalSeconds, 0f, 0.1f);
		lastFrameTime = now;

		var t = (float)(now - startTime).TotalSeconds;
		cameraDistance += dt * 6.0f * speed;
		var camZ = cameraDistance;
		var baseRing = (int)MathF.Floor(camZ);

		// Count orbs as the camera flies through each ring
		if (baseRing > lastOrbRing)
		{
			if (lastOrbRing >= 0 && HasOrbAtRing(baseRing, out _))
				orbsCollected++;
			lastOrbRing = baseRing;
		}

		GetTunnelCenter(camZ, out var camX, out var camY);
		GetTunnelCenter(camZ + 1.5f, out var lookX, out var lookY);
		var dirX = (lookX - camX) / 1.5f;
		var dirY = (lookY - camY) / 1.5f;
		var bankAngle = -dirX * 0.25f + MathF.Sin(t * 0.8f) * 0.12f;
		var cosBank = MathF.Cos(bankAngle);
		var sinBank = MathF.Sin(bankAngle);

		var halfW = width * 0.5f;
		var halfH = height * 0.5f;
		var focal = MathF.Min(width, height) * 0.65f;
		const float tunnelRadius = 2.2f;

		var quadCount = 0;

		// Build back-to-front from furthest ring to nearest ring so translucent orbs blend properly
		for (var r = Rings - 1; r >= 0; r--)
		{
			var ringIdx0 = baseRing + r;
			var ringIdx1 = ringIdx0 + 1;

			var z0 = ringIdx0 - camZ;
			var z1 = ringIdx1 - camZ;
			if (z1 <= 0.25f)
				continue;
			if (z0 < 0.25f)
				z0 = 0.25f;

			GetTunnelCenter(ringIdx0, out var c0x, out var c0y);
			GetTunnelCenter(ringIdx1, out var c1x, out var c1y);

			var hasOrb0 = HasOrbAtRing(ringIdx0, out _);
			var hasOrb1 = HasOrbAtRing(ringIdx1, out var orbAngle);
			var orbGlow0 = hasOrb0 ? 1.0f : (hasOrb1 ? 0.5f : 0.0f);
			var orbGlow1 = hasOrb1 ? 1.0f : (hasOrb0 ? 0.5f : 0.0f);

			var twist0 = ringIdx0 * 0.08f;
			var twist1 = ringIdx1 * 0.08f;

			// 8 octagonal tunnel wall segments per ring
			for (var s = 0; s < Sides; s++)
			{
				var a0 = s * (MathF.PI * 2f / Sides);
				var a1 = (s + 1) * (MathF.PI * 2f / Sides);

				ProjectTunnelPoint(c0x + MathF.Cos(a0 + twist0) * tunnelRadius, c0y + MathF.Sin(a0 + twist0) * tunnelRadius, z0,
					camX, camY, dirX, dirY, cosBank, sinBank, halfW, halfH, focal, out var x00, out var y00, out var nx0, out var ny0);
				ProjectTunnelPoint(c0x + MathF.Cos(a1 + twist0) * tunnelRadius, c0y + MathF.Sin(a1 + twist0) * tunnelRadius, z0,
					camX, camY, dirX, dirY, cosBank, sinBank, halfW, halfH, focal, out var x10, out var y10, out var nx1, out var ny1);
				ProjectTunnelPoint(c1x + MathF.Cos(a1 + twist1) * tunnelRadius, c1y + MathF.Sin(a1 + twist1) * tunnelRadius, z1,
					camX, camY, dirX, dirY, cosBank, sinBank, halfW, halfH, focal, out var x11, out var y11, out _, out _);
				ProjectTunnelPoint(c1x + MathF.Cos(a0 + twist1) * tunnelRadius, c1y + MathF.Sin(a0 + twist1) * tunnelRadius, z1,
					camX, camY, dirX, dirY, cosBank, sinBank, halfW, halfH, focal, out var x01, out var y01, out _, out _);

				var u0 = (float)s / Sides * 2f;
				var u1 = (float)(s + 1) / Sides * 2f;
				var v0 = (ringIdx0 & 7) * 0.5f;
				var v1 = v0 + 0.5f;

				EmitQuad(ref quadCount,
					x00, y00, u0, v0, z0, nx0, ny0, 0f, orbGlow0,
					x10, y10, u1, v0, z0, nx1, ny1, 0f, orbGlow0,
					x11, y11, u1, v1, z1, nx1, ny1, 0f, orbGlow1,
					x01, y01, u0, v1, z1, nx0, ny0, 0f, orbGlow1);
			}

			// Emit floating collectible orb inside the tunnel ring
			if (hasOrb1 && z1 > 0.4f)
			{
				var bob = MathF.Sin(t * 3.5f + ringIdx1) * 0.35f;
				var ox = c1x + MathF.Cos(orbAngle) * 0.65f;
				var oy = c1y + MathF.Sin(orbAngle) * 0.65f + bob;

				ProjectTunnelPoint(ox, oy, z1, camX, camY, dirX, dirY, cosBank, sinBank, halfW, halfH, focal,
					out var sx, out var sy, out _, out _);

				var orbRadius = (0.42f * focal) / z1;
				var seed = (ringIdx1 % 17) / 17f;

				EmitQuad(ref quadCount,
					sx - orbRadius, sy - orbRadius, 0f, 0f, z1, 0f, 1f, 1f, seed,
					sx + orbRadius, sy - orbRadius, 1f, 0f, z1, 0f, 1f, 1f, seed,
					sx + orbRadius, sy + orbRadius, 1f, 1f, z1, 0f, 1f, 1f, seed,
					sx - orbRadius, sy + orbRadius, 0f, 1f, z1, 0f, 1f, 1f, seed);
			}
		}

		if (quadCount == 0)
			return;

		var activeVerts = quadCount * 6;
		var byteSlice = MemoryMarshal.AsBytes(vertices.AsSpan(0, activeVerts * FloatsPerVertex));
		vertexBuffer.Update(byteSlice, 0);

		GetPaletteColors(paletteIndex, out var torchColor, out var orbColor);
		builder.Uniforms["uTime"] = t;
		builder.Uniforms["uTorchColor"] = torchColor;
		builder.Uniforms["uOrbColor"] = orbColor;
		builder.Bounds = new SKRect(0, 0, width, height);
		builder.SetVertexBuffer(vertexBuffer, activeVerts);

		using var mesh = builder.Build(out var errors);
		if (mesh != null)
			canvas.DrawMesh(mesh, meshPaint);
		else
			Console.WriteLine($"Failed to build mesh: {errors}");

		if (showWireframe && wirePaint != null)
			DrawWireframe(canvas, quadCount, wirePaint);

		if (hudPaint != null && hudFont != null)
		{
			if (canvas.Context == null)
			{
				canvas.DrawText("SKMesh requires GPU rendering — switch from CPU to GPU in the top bar.", 16, height * 0.5f, SKTextAlign.Left, hudFont, hudPaint);
			}
			else
			{
				canvas.DrawText($"Orbs: {orbsCollected}   Tris: {quadCount * 2}", 16, 28, SKTextAlign.Left, hudFont, hudPaint);
			}
		}
	}

	private static void GetPaletteColors(int index, out float[] torch, out float[] orb)
	{
		switch (index)
		{
			case 1: // Cyber Neon
				torch = new[] { 0.35f, 0.75f, 1.35f };
				orb = new[] { 1.0f, 0.2f, 0.85f };
				break;
			case 2: // Emerald Cavern
				torch = new[] { 0.3f, 1.15f, 0.65f };
				orb = new[] { 0.4f, 1.0f, 0.9f };
				break;
			default: // Dungeon Torch
				torch = new[] { 1.35f, 0.82f, 0.42f };
				orb = new[] { 0.35f, 0.85f, 1.0f };
				break;
		}
	}

	private static void ProjectTunnelPoint(
		float wx, float wy, float z,
		float camX, float camY, float dirX, float dirY,
		float cosBank, float sinBank,
		float halfW, float halfH, float focal,
		out float sx, out float sy, out float nx, out float ny)
	{
		var rx = (wx - camX) - dirX * z * 0.35f;
		var ry = (wy - camY) - dirY * z * 0.35f;

		var bx = rx * cosBank - ry * sinBank;
		var by = rx * sinBank + ry * cosBank;

		var invZ = focal / z;
		sx = halfW + bx * invZ;
		sy = halfH - by * invZ;

		var len = MathF.Sqrt(bx * bx + by * by);
		if (len > 0.0001f)
		{
			nx = -bx / len;
			ny = -by / len;
		}
		else
		{
			nx = 0f;
			ny = 1f;
		}
	}

	private void EmitQuad(
		ref int quadCount,
		float x0, float y0, float u0, float v0, float z0, float nx0, float ny0, float kind0, float extra0,
		float x1, float y1, float u1, float v1, float z1, float nx1, float ny1, float kind1, float extra1,
		float x2, float y2, float u2, float v2, float z2, float nx2, float ny2, float kind2, float extra2,
		float x3, float y3, float u3, float v3, float z3, float nx3, float ny3, float kind3, float extra3)
	{
		if (quadCount >= MaxQuads)
			return;

		// Emit 2 triangles (v0, v1, v2) and (v0, v2, v3) directly for non-indexed SKMeshBuilder
		var baseIdx = quadCount * 6 * FloatsPerVertex;
		WriteVertex(baseIdx + 0 * FloatsPerVertex, x0, y0, u0, v0, z0, nx0, ny0, kind0, extra0);
		WriteVertex(baseIdx + 1 * FloatsPerVertex, x1, y1, u1, v1, z1, nx1, ny1, kind1, extra1);
		WriteVertex(baseIdx + 2 * FloatsPerVertex, x2, y2, u2, v2, z2, nx2, ny2, kind2, extra2);
		WriteVertex(baseIdx + 3 * FloatsPerVertex, x0, y0, u0, v0, z0, nx0, ny0, kind0, extra0);
		WriteVertex(baseIdx + 4 * FloatsPerVertex, x2, y2, u2, v2, z2, nx2, ny2, kind2, extra2);
		WriteVertex(baseIdx + 5 * FloatsPerVertex, x3, y3, u3, v3, z3, nx3, ny3, kind3, extra3);
		quadCount++;
	}

	private void WriteVertex(int idx, float x, float y, float u, float v, float z, float nx, float ny, float kind, float extra)
	{
		var invZ = 1.0f / z;
		vertices[idx + 0] = x;
		vertices[idx + 1] = y;
		vertices[idx + 2] = u * invZ;
		vertices[idx + 3] = v * invZ;
		vertices[idx + 4] = invZ;
		vertices[idx + 5] = z;
		vertices[idx + 6] = nx;
		vertices[idx + 7] = ny;
		vertices[idx + 8] = kind;
		vertices[idx + 9] = extra;
	}

	private void DrawWireframe(SKCanvas canvas, int quadCount, SKPaint paint)
	{
		using var pathBuilder = new SKPathBuilder();
		var triCount = quadCount * 2;
		for (var t = 0; t < triCount; t++)
		{
			var b = t * 3 * FloatsPerVertex;
			var x0 = vertices[b + 0];
			var y0 = vertices[b + 1];
			var x1 = vertices[b + FloatsPerVertex + 0];
			var y1 = vertices[b + FloatsPerVertex + 1];
			var x2 = vertices[b + 2 * FloatsPerVertex + 0];
			var y2 = vertices[b + 2 * FloatsPerVertex + 1];

			pathBuilder.MoveTo(x0, y0);
			pathBuilder.LineTo(x1, y1);
			pathBuilder.LineTo(x2, y2);
			pathBuilder.Close();
		}
		using var path = pathBuilder.Detach();
		canvas.DrawPath(path, paint);
	}

	protected override void OnDestroy()
	{
		hudFont?.Dispose(); hudFont = null;
		hudPaint?.Dispose(); hudPaint = null;
		wirePaint?.Dispose(); wirePaint = null;
		meshPaint?.Dispose(); meshPaint = null;
		builder?.Dispose(); builder = null;
		vertexBuffer?.Dispose(); vertexBuffer = null;
		spec?.Dispose(); spec = null;
		wallShader?.Dispose(); wallShader = null;
		base.OnDestroy();
	}
}
