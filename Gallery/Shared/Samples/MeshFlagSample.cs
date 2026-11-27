using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;
using Svg.Skia;

namespace SkiaSharpSample.Samples;

/// <summary>
/// Demonstrates SKMesh with a waving 3D SkiaSharp logo silk flag/tapestry.
/// A spring-mass Verlet cloth simulation streams per-vertex 2D positions, perspective-correct UVs,
/// 3D surface normals, and fabric fold tension into a dynamic SKMeshVertexBuffer every frame.
/// The SkSL fragment shader samples two child shaders (the SkiaSharp SVG banner and a micro-weave
/// pattern) and computes directional fold lighting, anisotropic silk sheen, and gold-thread border highlights.
/// </summary>
public class MeshFlagSample : CanvasSampleBase
{
	private const string VS = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			v.uvDepth = attrs.uvDepth;
			v.normalFold = attrs.normalFold;
			return v;
		}";

	private const string FS = @"
		uniform float uTime;
		uniform float uSheen;
		uniform float uStyle;
		uniform shader uBannerTex;
		uniform shader uWeaveTex;

		float2 main(const Varyings v, out half4 color) {
			float invZ = max(v.uvDepth.z, 0.0001);
			float2 uv = clamp(v.uvDepth.xy / invZ, float2(0.0), float2(1.0));
			float3 n = normalize(v.normalFold.xyz);
			float fold = v.normalFold.w;

			// Sample banner artwork (512x320) and fine silk weave pattern
			half4 banner = uBannerTex.eval(uv * float2(512.0, 320.0));
			half4 weave = uWeaveTex.eval(uv * float2(640.0, 400.0));

			// Directional sunlight + sky ambient + deep fold occlusion
			float3 lightDir = normalize(float3(-0.42, -0.52, 0.74));
			float3 viewDir  = float3(0.0, 0.0, 1.0);
			float3 halfVec  = normalize(lightDir + viewDir);

			float ndotl = max(dot(n, lightDir), 0.0);
			float wrapDiff = 0.35 + 0.65 * clamp((dot(n, lightDir) + 0.35) / 1.35, 0.0, 1.0);

			// Anisotropic silk sheen along horizontal & vertical threads
			float specBase = max(dot(n, halfVec), 0.0);
			float silkSpec = pow(specBase, 20.0) * 0.55 + pow(1.0 - abs(n.x), 8.0) * 0.18;

			// Gold embroidered border frame around the tapestry
			float2 edgeDist = min(uv, 1.0 - uv);
			float minEdge = min(edgeDist.x * 1.6, edgeDist.y);
			float outerGold = smoothstep(0.042, 0.032, minEdge);
			float innerFrame = smoothstep(0.068, 0.060, minEdge) * (1.0 - smoothstep(0.052, 0.046, minEdge));
			float goldMask = clamp(outerGold + innerFrame * 0.85, 0.0, 1.0);

			float3 baseCol = float3(banner.rgb);
			float3 sheenTint = float3(0.95, 0.97, 1.0);

			if (uStyle > 1.5) {
				// Holographic Foil style
				float irid = 0.5 + 0.5 * sin(dot(n.xy, float2(9.0, 7.0)) + uTime * 2.5 + uv.x * 5.0);
				float3 holo = mix(float3(0.25, 0.85, 1.0), float3(1.0, 0.35, 0.85), irid);
				sheenTint = holo * 1.35;
				baseCol = mix(baseCol, baseCol * (0.75 + 0.45 * holo), 0.35);
			} else if (uStyle > 0.5) {
				// Gold Brocade style
				sheenTint = float3(1.0, 0.84, 0.42);
				baseCol = mix(baseCol, baseCol * float3(1.12, 0.98, 0.78), 0.25);
			}

			float3 goldThread = float3(0.98, 0.78, 0.26) * (0.65 + 0.85 * pow(specBase, 10.0));
			baseCol = mix(baseCol, goldThread, goldMask * 0.75);

			// Combine weave micro-detail, fold self-shadowing, and specular sheen
			float weaveMod = 0.88 + 0.22 * float(weave.r);
			float foldOcclusion = clamp(1.0 - fold * 0.42, 0.45, 1.15);

			float3 lit = baseCol * weaveMod * wrapDiff * foldOcclusion
			           + sheenTint * (silkSpec * uSheen * (0.55 + 0.65 * ndotl));

			color = half4(half3(clamp(lit, 0.0, 1.0)), 1.0);
			return v.position;
		}";

	private const int Cols = 38;
	private const int Rows = 24;
	private const int NodeCount = Cols * Rows;
	private const int QuadCount = (Cols - 1) * (Rows - 1);
	private const int FloatsPerVertex = 10; // pos(2) + uvDepth(4) + normalFold(4)
	private const int BannerW = 512;
	private const int BannerH = 320;

	private static readonly string[] FabricStyles = { "Royal Silk", "Gold Brocade", "Holographic Foil" };

	// Verlet cloth simulation state in normalized flag space (width = 1.6, height = 1.0)
	private readonly float[] posX = new float[NodeCount];
	private readonly float[] posY = new float[NodeCount];
	private readonly float[] posZ = new float[NodeCount];
	private readonly float[] prevX = new float[NodeCount];
	private readonly float[] prevY = new float[NodeCount];
	private readonly float[] prevZ = new float[NodeCount];
	private readonly float[] normX = new float[NodeCount];
	private readonly float[] normY = new float[NodeCount];
	private readonly float[] normZ = new float[NodeCount];
	private readonly float[] tension = new float[NodeCount];

	private readonly float[] vertices = new float[NodeCount * FloatsPerVertex];
	private readonly ushort[] indices = new ushort[QuadCount * 6];

	private SKMeshSpecification? spec;
	private SKIndexedMeshBuilder? builder;
	private SKMeshVertexBuffer? vertexBuffer;
	private SKMeshIndexBuffer? indexBuffer;
	private SKShader? bannerShader;
	private SKShader? weaveShader;
	private SKPaint? meshPaint;
	private SKPaint? wirePaint;
	private SKPaint? polePaint;
	private SKPaint? shadowPaint;
	private SKPaint? hudPaint;
	private SKFont? hudFont;

	private int styleIndex;
	private float windStrength = 1.2f;
	private float turbulence = 1.0f;
	private float sheen = 1.0f;
	private bool showWireframe;
	private DateTime startTime;
	private float simTime;

	public override string Title => "SkiaSharp Flag (Cloth Mesh)";

	public override DateOnly? DateAdded => new DateOnly(2026, 11, 16);

	public override string Description =>
		"A waving 3D SkiaSharp silk flag simulated with Verlet cloth physics and rendered with SKMesh. " +
		"Streams per-vertex 3D normals and fold tension into a custom SkSL shader with silk specular sheen and two child textures.";

	public override string Category => SampleManager.Shaders;

	public override bool IsAnimated => true;

	public override IReadOnlyList<string> ApiTags =>
	[
		"SKMesh", "SKMeshSpecification", "SKMeshSpecificationBuilder",
		"SKMeshBuilder", "SKIndexedMeshBuilder", "SKMeshVertexBuffer", "SKMeshIndexBuffer",
		"SKCanvas.DrawMesh",
	];

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("style", "Fabric Style", FabricStyles, styleIndex),
		new SliderControl("wind", "Wind Strength", 0.2f, 2.5f, windStrength, 0.1f),
		new SliderControl("turbulence", "Gust Turbulence", 0.0f, 2.0f, turbulence, 0.1f),
		new SliderControl("sheen", "Silk Sheen", 0.0f, 2.0f, sheen, 0.1f),
		new ToggleControl("wireframe", "Show Wireframe", showWireframe),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "style":
				styleIndex = (int)value;
				break;
			case "wind":
				windStrength = (float)value;
				break;
			case "turbulence":
				turbulence = (float)value;
				break;
			case "sheen":
				sheen = (float)value;
				break;
			case "wireframe":
				showWireframe = (bool)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		startTime = DateTime.UtcNow;
		simTime = 0f;
		ResetCloth();
		return base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(16, token);
	}

	private void ResetCloth()
	{
		const float flagW = 1.6f;
		const float flagH = 1.0f;

		for (var r = 0; r < Rows; r++)
		{
			var v = (float)r / (Rows - 1);
			for (var c = 0; c < Cols; c++)
			{
				var u = (float)c / (Cols - 1);
				var idx = r * Cols + c;
				var wave = MathF.Sin(u * 6.5f + v * 1.8f) * 0.14f * u;
				var x = u * flagW * 0.96f;
				var y = v * flagH + (u * (1f - u)) * 0.04f;
				var z = wave;

				posX[idx] = prevX[idx] = x;
				posY[idx] = prevY[idx] = y;
				posZ[idx] = prevZ[idx] = z;
				normX[idx] = 0f;
				normY[idx] = 0f;
				normZ[idx] = 1f;
				tension[idx] = 0f;
			}
		}

		// Pre-warm the cloth simulation so the first frame already has natural folds
		for (var i = 0; i < 20; i++)
			StepClothPhysics();
	}

	private void EnsureResources()
	{
		if (builder != null)
			return;

		bannerShader = CreateBannerShader();
		weaveShader = CreateWeaveShader();

		var specBuilder = SKMeshSpecification.CreateBuilder(VS, FS);
		specBuilder.ColorSpace = SKColorSpace.CreateSrgb();
		specBuilder.AlphaType = SKAlphaType.Premul;
		specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
		specBuilder.AddAttribute("uvDepth", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddAttribute("normalFold", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddVarying("uvDepth", SKMeshSpecificationVaryingType.Float4);
		specBuilder.AddVarying("normalFold", SKMeshSpecificationVaryingType.Float4);

		spec = specBuilder.Build(out var errors);
		if (spec == null)
		{
			Console.WriteLine($"Failed to build mesh specification: {errors}");
			return;
		}

		var i = 0;
		for (var r = 0; r < Rows - 1; r++)
		{
			for (var c = 0; c < Cols - 1; c++)
			{
				var i00 = (ushort)(r * Cols + c);
				var i10 = (ushort)(i00 + 1);
				var i01 = (ushort)((r + 1) * Cols + c);
				var i11 = (ushort)(i01 + 1);

				indices[i++] = i00;
				indices[i++] = i10;
				indices[i++] = i11;
				indices[i++] = i00;
				indices[i++] = i11;
				indices[i++] = i01;
			}
		}

		vertexBuffer = SKMeshVertexBuffer.Create(NodeCount * FloatsPerVertex * sizeof(float));
		indexBuffer = SKMeshIndexBuffer.Create(MemoryMarshal.AsBytes(indices.AsSpan()));

		builder = spec.CreateIndexedMeshBuilder();
		if (bannerShader != null)
			builder.Children["uBannerTex"] = bannerShader;
		if (weaveShader != null)
			builder.Children["uWeaveTex"] = weaveShader;
		if (indexBuffer != null)
			builder.SetIndexBuffer(indexBuffer);

		meshPaint = new SKPaint { Color = SKColors.White };
		wirePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(160, 230, 255, 110),
			StrokeWidth = 1f,
		};
		polePaint = new SKPaint { IsAntialias = true };
		shadowPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(0, 0, 0, 90),
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 18f),
		};
		hudPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(225, 235, 255, 210),
		};
		hudFont = new SKFont(SampleMedia.Fonts.Default, 14f);
	}

	private static SKShader CreateBannerShader()
	{
		using var surface = SKSurface.Create(new SKImageInfo(BannerW, BannerH));
		var c = surface.Canvas;

		// Deep royal gradient backdrop
		using (var bgShader = SKShader.CreateLinearGradient(
			new SKPoint(0, 0),
			new SKPoint(BannerW, BannerH),
			new[] { new SKColor(18, 28, 68), new SKColor(32, 54, 118), new SKColor(15, 24, 58) },
			new[] { 0f, 0.55f, 1f },
			SKShaderTileMode.Clamp))
		using (var bgPaint = new SKPaint { Shader = bgShader })
		{
			c.DrawRect(0, 0, BannerW, BannerH, bgPaint);
		}

		// Decorative diagonal accent ribbons in SkiaSharp brand colors
		DrawRibbon(c, new SKColor(47, 69, 157, 110), 220f, 90f);
		DrawRibbon(c, new SKColor(217, 87, 63, 130), 290f, 44f);
		DrawRibbon(c, new SKColor(123, 167, 69, 130), 336f, 40f);
		DrawRibbon(c, new SKColor(38, 92, 188, 130), 378f, 36f);

		// Soft radial glow behind the logo
		using (var glowShader = SKShader.CreateRadialGradient(
			new SKPoint(132, BannerH * 0.5f),
			120f,
			new[] { new SKColor(255, 255, 255, 65), new SKColor(255, 255, 255, 0) },
			new[] { 0f, 1f },
			SKShaderTileMode.Clamp))
		using (var glowPaint = new SKPaint { Shader = glowShader, IsAntialias = true })
		{
			c.DrawCircle(132, BannerH * 0.5f, 120f, glowPaint);
		}

		// Frosted emblem medallion on the left
		var emblemRect = new SKRect(42, 70, 222, 250);
		using (var medallionPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(248, 250, 255, 242),
		})
		using (var medallionRing = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 4f,
			Color = new SKColor(245, 198, 78),
		})
		{
			c.DrawRoundRect(emblemRect, 36, 36, medallionPaint);
			c.DrawRoundRect(emblemRect, 36, 36, medallionRing);
		}

		// Render the SkiaSharp SVG logo inside the emblem
		using (var svg = new SKSvg())
		using (var stream = SampleMedia.Images.SkiaSharpLogoSvg)
		{
			svg.Load(stream);
			if (svg.Picture != null)
			{
				var b = svg.Picture.CullRect;
				if (!b.IsEmpty)
				{
					var pad = 22f;
					var availW = emblemRect.Width - pad * 2;
					var availH = emblemRect.Height - pad * 2;
					var s = Math.Min(availW / b.Width, availH / b.Height);
					var dx = emblemRect.MidX - (b.Width * s) * 0.5f - b.Left * s;
					var dy = emblemRect.MidY - (b.Height * s) * 0.5f - b.Top * s;

					c.Save();
					c.Translate(dx, dy);
					c.Scale(s);
					c.DrawPicture(svg.Picture);
					c.Restore();
				}
			}
		}

		// Bold "SkiaSharp" title and subtitle on the right
		var baseTf = SampleMedia.Fonts.Default;
		using var boldTf = baseTf.Clone(new[]
		{
			new SKFontVariationPositionCoordinate { Axis = SKFourByteTag.Parse("wght"), Value = 800 },
		}) ?? baseTf;

		using var titleFont = new SKFont(boldTf, 46f);
		using var subFont = new SKFont(baseTf, 17f);
		using var titlePaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var subPaint = new SKPaint { IsAntialias = true, Color = new SKColor(195, 220, 255) };

		c.DrawText("SkiaSharp", 248, 152, SKTextAlign.Left, titleFont, titlePaint);
		c.DrawText("2D GRAPHICS  ·  SKMESH", 250, 184, SKTextAlign.Left, subFont, subPaint);

		// Accent bar under the text
		using (var barShader = SKShader.CreateLinearGradient(
			new SKPoint(250, 200),
			new SKPoint(465, 200),
			new[] { new SKColor(217, 87, 63), new SKColor(123, 167, 69), new SKColor(52, 152, 219) },
			new[] { 0f, 0.5f, 1f },
			SKShaderTileMode.Clamp))
		using (var barPaint = new SKPaint { Shader = barShader, IsAntialias = true })
		{
			c.DrawRoundRect(new SKRect(250, 200, 465, 208), 4, 4, barPaint);
		}

		using var img = surface.Snapshot();
		return img.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear));
	}

	private static void DrawRibbon(SKCanvas c, SKColor color, float xOffset, float thickness)
	{
		using var pathBuilder = new SKPathBuilder();
		pathBuilder.MoveTo(xOffset, 0);
		pathBuilder.LineTo(xOffset + thickness, 0);
		pathBuilder.LineTo(xOffset + thickness - 140f, BannerH);
		pathBuilder.LineTo(xOffset - 140f, BannerH);
		pathBuilder.Close();
		using var path = pathBuilder.Detach();
		using var paint = new SKPaint { Color = color, IsAntialias = true };
		c.DrawPath(path, paint);
	}

	private static SKShader CreateWeaveShader()
	{
		using var surface = SKSurface.Create(new SKImageInfo(16, 16));
		var c = surface.Canvas;
		c.Clear(new SKColor(128, 128, 128));

		using var p = new SKPaint { IsAntialias = true };
		for (var y = 0; y < 16; y += 4)
		{
			for (var x = 0; x < 16; x += 4)
			{
				var warp = ((x / 4 + y / 4) & 1) == 0;
				p.Color = warp ? new SKColor(195, 195, 195) : new SKColor(85, 85, 85);
				c.DrawRect(x, y, 3, 3, p);
			}
		}

		using var img = surface.Snapshot();
		return img.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat, new SKSamplingOptions(SKFilterMode.Linear));
	}

	private void StepClothPhysics()
	{
		const float dt = 1f / 60f;
		const float flagW = 1.6f;
		const float flagH = 1.0f;
		const float restX = flagW / (Cols - 1);
		const float restY = flagH / (Rows - 1);
		const float restDiag = 0.06055f; // sqrt(restX^2 + restY^2)
		const float damping = 0.985f;

		simTime += dt;
		var t = simTime;

		// 1. Verlet integration + wind, billow & gravity forces
		for (var r = 0; r < Rows; r++)
		{
			var v = (float)r / (Rows - 1);
			for (var c = 0; c < Cols; c++)
			{
				var idx = r * Cols + c;

				// Left hoist edge (c == 0) is pinned to the flagpole at the top, bottom, and middle grommets
				if (c == 0 && (r == 0 || r == Rows - 1 || r == Rows / 2))
				{
					posX[idx] = 0f;
					posY[idx] = v * flagH;
					posZ[idx] = 0f;
					continue;
				}

				var u = (float)c / (Cols - 1);
				var x = posX[idx];
				var y = posY[idx];
				var z = posZ[idx];

				var vx = (x - prevX[idx]) * damping;
				var vy = (y - prevY[idx]) * damping;
				var vz = (z - prevZ[idx]) * damping;

				prevX[idx] = x;
				prevY[idx] = y;
				prevZ[idx] = z;

				// Multi-octave traveling wind waves + vertical ripple + trailing edge snap
				var speedRate = 4.5f + windStrength * 2.2f;
				var wave1 = MathF.Sin(u * 7.5f - t * speedRate + v * 2.2f);
				var wave2 = MathF.Cos(u * 13.5f - t * (speedRate * 1.55f) - v * 3.8f) * turbulence;
				var wave3 = MathF.Sin(u * 21.0f - t * (speedRate * 2.2f) + v * 6.0f) * turbulence * 0.45f;

				var envelope = MathF.Pow(u, 0.85f);
				var windForce = windStrength * envelope;

				var ax = (3.2f + 1.1f * wave1) * windStrength;
				var ay = 0.65f - 0.35f * Math.Min(windStrength, 1.5f) * u + wave2 * 0.45f * windForce;
				var az = (wave1 * 2.8f + wave2 * 1.6f + wave3) * windForce;

				posX[idx] = x + vx + ax * dt * dt;
				posY[idx] = y + vy + ay * dt * dt;
				posZ[idx] = z + vz + az * dt * dt;

				// Keep hoist edge sliding close to the flagpole
				if (c == 0)
				{
					posX[idx] *= 0.15f;
					posZ[idx] *= 0.2f;
				}
			}
		}

		// 2. Distance constraint relaxation (structural + shear springs)
		for (var iter = 0; iter < 5; iter++)
		{
			// Horizontal constraints
			for (var r = 0; r < Rows; r++)
			{
				var rowBase = r * Cols;
				for (var c = 0; c < Cols - 1; c++)
				{
					SatisfyConstraint(rowBase + c, rowBase + c + 1, restX);
				}
			}

			// Vertical constraints
			for (var r = 0; r < Rows - 1; r++)
			{
				var rowBase = r * Cols;
				for (var c = 0; c < Cols; c++)
				{
					SatisfyConstraint(rowBase + c, rowBase + Cols + c, restY);
				}
			}

			// Diagonal shear constraints (every second iteration for crisp folds without sag)
			if ((iter & 1) == 0)
			{
				for (var r = 0; r < Rows - 1; r++)
				{
					var rowBase = r * Cols;
					for (var c = 0; c < Cols - 1; c++)
					{
						SatisfyConstraint(rowBase + c, rowBase + Cols + c + 1, restDiag);
						SatisfyConstraint(rowBase + c + 1, rowBase + Cols + c, restDiag);
					}
				}
			}
		}

		// 3. Compute smooth 3D vertex normals and local fold curvature/compression
		for (var r = 0; r < Rows; r++)
		{
			var rPrev = Math.Max(0, r - 1);
			var rNext = Math.Min(Rows - 1, r + 1);
			for (var c = 0; c < Cols; c++)
			{
				var cPrev = Math.Max(0, c - 1);
				var cNext = Math.Min(Cols - 1, c + 1);
				var idx = r * Cols + c;

				var dxX = posX[r * Cols + cNext] - posX[r * Cols + cPrev];
				var dxY = posY[r * Cols + cNext] - posY[r * Cols + cPrev];
				var dxZ = posZ[r * Cols + cNext] - posZ[r * Cols + cPrev];

				var dyX = posX[rNext * Cols + c] - posX[rPrev * Cols + c];
				var dyY = posY[rNext * Cols + c] - posY[rPrev * Cols + c];
				var dyZ = posZ[rNext * Cols + c] - posZ[rPrev * Cols + c];

				// Cross product dy x dx so front face points toward +Z (viewer)
				var nx = dyY * dxZ - dyZ * dxY;
				var ny = dyZ * dxX - dyX * dxZ;
				var nz = dyX * dxY - dyY * dxX;
				var len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
				if (len > 0.00001f)
				{
					nx /= len;
					ny /= len;
					nz /= len;
				}
				else
				{
					nx = 0f;
					ny = 0f;
					nz = 1f;
				}

				if (nz < 0f)
				{
					nx = -nx;
					ny = -ny;
					nz = -nz;
				}

				normX[idx] = nx;
				normY[idx] = ny;
				normZ[idx] = nz;

				// Fold metric: how much the horizontal span compressed or bent into Z
				var spanSpan = MathF.Sqrt(dxX * dxX + dxY * dxY + dxZ * dxZ);
				var expectedSpan = (cNext - cPrev) * restX;
				var compression = Math.Clamp(1.0f - spanSpan / Math.Max(expectedSpan, 0.0001f), 0f, 1f);
				tension[idx] = Math.Clamp(compression * 2.5f + Math.Max(0f, -posZ[idx] * 2.2f), 0f, 1.5f);
			}
		}
	}

	private void SatisfyConstraint(int i1, int i2, float restDist)
	{
		var dx = posX[i2] - posX[i1];
		var dy = posY[i2] - posY[i1];
		var dz = posZ[i2] - posZ[i1];
		var dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);
		if (dist < 0.00001f)
			return;

		var diff = (dist - restDist) / dist;
		var pin1 = IsPinned(i1);
		var pin2 = IsPinned(i2);

		if (pin1 && pin2)
			return;

		if (pin1)
		{
			posX[i2] -= dx * diff;
			posY[i2] -= dy * diff;
			posZ[i2] -= dz * diff;
		}
		else if (pin2)
		{
			posX[i1] += dx * diff;
			posY[i1] += dy * diff;
			posZ[i1] += dz * diff;
		}
		else
		{
			var half = diff * 0.5f;
			posX[i1] += dx * half;
			posY[i1] += dy * half;
			posZ[i1] += dz * half;
			posX[i2] -= dx * half;
			posY[i2] -= dy * half;
			posZ[i2] -= dz * half;
		}
	}

	private static bool IsPinned(int idx)
	{
		var c = idx % Cols;
		if (c != 0)
			return false;
		var r = idx / Cols;
		return r == 0 || r == Rows - 1 || r == Rows / 2;
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		DrawBackdrop(canvas, width, height);
		EnsureResources();

		if (builder == null || vertexBuffer == null || indexBuffer == null || meshPaint == null)
			return;

		StepClothPhysics();

		// Project 3D cloth vertices to 2D canvas coordinates with perspective
		var scale = MathF.Min((width * 0.62f) / 1.65f, height * 0.56f);
		var poleX = (width - scale * 1.48f) * 0.5f;
		var topY = (height - scale * 1.0f) * 0.42f;
		const float cameraDist = 2.6f;

		for (var r = 0; r < Rows; r++)
		{
			var v = (float)r / (Rows - 1);
			for (var c = 0; c < Cols; c++)
			{
				var u = (float)c / (Cols - 1);
				var idx = r * Cols + c;

				var wx = posX[idx];
				var wy = posY[idx];
				var wz = posZ[idx];

				var depth = Math.Max(0.5f, cameraDist - wz);
				var persp = cameraDist / depth;
				var invZ = 1.0f / depth;

				var sx = poleX + wx * scale * persp;
				var sy = topY + wy * scale * persp - wz * scale * 0.28f;

				var b = idx * FloatsPerVertex;
				vertices[b + 0] = sx;
				vertices[b + 1] = sy;
				vertices[b + 2] = u * invZ;
				vertices[b + 3] = v * invZ;
				vertices[b + 4] = invZ;
				vertices[b + 5] = depth;
				vertices[b + 6] = normX[idx];
				vertices[b + 7] = normY[idx];
				vertices[b + 8] = normZ[idx];
				vertices[b + 9] = tension[idx];
			}
		}

		// Soft drop shadow beneath the waving flag
		if (shadowPaint != null)
		{
			using var shadowBuilder = new SKPathBuilder();
			shadowBuilder.MoveTo(vertices[0] + 18, vertices[1] + 28);
			for (var c = 1; c < Cols; c++)
			{
				var b = c * FloatsPerVertex;
				shadowBuilder.LineTo(vertices[b] + 18, vertices[b + 1] + 28);
			}
			for (var r = 1; r < Rows; r++)
			{
				var b = (r * Cols + (Cols - 1)) * FloatsPerVertex;
				shadowBuilder.LineTo(vertices[b] + 18, vertices[b + 1] + 28);
			}
			for (var c = Cols - 2; c >= 0; c--)
			{
				var b = ((Rows - 1) * Cols + c) * FloatsPerVertex;
				shadowBuilder.LineTo(vertices[b] + 18, vertices[b + 1] + 28);
			}
			shadowBuilder.Close();
			using var shadowPath = shadowBuilder.Detach();
			canvas.DrawPath(shadowPath, shadowPaint);
		}

		DrawFlagpole(canvas, poleX, topY, scale, height);

		// Upload updated cloth vertices and draw the mesh in a single GPU call
		var byteSlice = MemoryMarshal.AsBytes(vertices.AsSpan());
		vertexBuffer.Update(byteSlice, 0);

		var elapsed = (float)(DateTime.UtcNow - startTime).TotalSeconds;
		builder.Uniforms["uTime"] = elapsed;
		builder.Uniforms["uSheen"] = sheen;
		builder.Uniforms["uStyle"] = (float)styleIndex;
		builder.Bounds = new SKRect(0, 0, width, height);
		builder.SetVertexBuffer(vertexBuffer, NodeCount);

		using var mesh = builder.Build(out var errors);
		if (mesh != null)
			canvas.DrawMesh(mesh, meshPaint);
		else
			Console.WriteLine($"Failed to build mesh: {errors}");

		if (showWireframe && wirePaint != null)
			DrawWireframe(canvas, wirePaint);

		if (hudPaint != null && hudFont != null)
		{
			if (canvas.Context == null)
			{
				canvas.DrawText("SKMesh requires GPU rendering — switch from CPU to GPU in the top bar.", 16, height * 0.5f, SKTextAlign.Left, hudFont, hudPaint);
			}
			else
			{
				canvas.DrawText($"Verlet Grid: {Cols}×{Rows} ({NodeCount} verts, {QuadCount * 2} tris)", 16, height - 16, SKTextAlign.Left, hudFont, hudPaint);
			}
		}
	}

	private static void DrawBackdrop(SKCanvas canvas, int width, int height)
	{
		using var bgShader = SKShader.CreateLinearGradient(
			new SKPoint(0, 0),
			new SKPoint(0, height),
			new[] { new SKColor(12, 18, 36), new SKColor(22, 34, 64), new SKColor(14, 20, 38) },
			new[] { 0f, 0.6f, 1f },
			SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { Shader = bgShader };
		canvas.DrawRect(0, 0, width, height, bgPaint);
	}

	private void DrawFlagpole(SKCanvas canvas, float poleX, float topY, float scale, int height)
	{
		if (polePaint == null)
			return;

		var poleRadius = Math.Max(4f, scale * 0.022f);
		var poleTop = topY - scale * 0.08f;

		// Metallic brushed flagpole
		using (var poleShader = SKShader.CreateLinearGradient(
			new SKPoint(poleX - poleRadius * 2f, 0),
			new SKPoint(poleX + poleRadius * 2f, 0),
			new[] { new SKColor(90, 98, 115), new SKColor(225, 232, 245), new SKColor(70, 78, 95) },
			new[] { 0f, 0.45f, 1f },
			SKShaderTileMode.Clamp))
		{
			polePaint.Shader = poleShader;
			canvas.DrawRoundRect(new SKRect(poleX - poleRadius * 1.8f, poleTop, poleX - poleRadius * 0.2f, height), 4, 4, polePaint);
		}

		// Golden finial sphere at the top of the flagpole
		using (var finialShader = SKShader.CreateRadialGradient(
			new SKPoint(poleX - poleRadius * 1.4f, poleTop - poleRadius * 1.4f),
			poleRadius * 3.2f,
			new[] { new SKColor(255, 245, 190), new SKColor(235, 180, 55), new SKColor(130, 85, 18) },
			new[] { 0f, 0.55f, 1f },
			SKShaderTileMode.Clamp))
		{
			polePaint.Shader = finialShader;
			canvas.DrawCircle(poleX - poleRadius, poleTop, poleRadius * 2.4f, polePaint);
		}

		// Brass grommet rings attaching the hoist edge to the pole
		polePaint.Shader = null;
		polePaint.Color = new SKColor(240, 195, 75);
		foreach (var r in new[] { 0, Rows / 2, Rows - 1 })
		{
			var b = (r * Cols) * FloatsPerVertex;
			canvas.DrawCircle(vertices[b], vertices[b + 1], poleRadius * 0.85f, polePaint);
		}
	}

	private void DrawWireframe(SKCanvas canvas, SKPaint paint)
	{
		using var pathBuilder = new SKPathBuilder();
		for (var r = 0; r < Rows - 1; r++)
		{
			for (var c = 0; c < Cols - 1; c++)
			{
				var i00 = (r * Cols + c) * FloatsPerVertex;
				var i10 = i00 + FloatsPerVertex;
				var i01 = ((r + 1) * Cols + c) * FloatsPerVertex;
				var i11 = i01 + FloatsPerVertex;

				pathBuilder.MoveTo(vertices[i00], vertices[i00 + 1]);
				pathBuilder.LineTo(vertices[i10], vertices[i10 + 1]);
				pathBuilder.LineTo(vertices[i11], vertices[i11 + 1]);
				pathBuilder.LineTo(vertices[i01], vertices[i01 + 1]);
				pathBuilder.Close();
				pathBuilder.MoveTo(vertices[i00], vertices[i00 + 1]);
				pathBuilder.LineTo(vertices[i11], vertices[i11 + 1]);
			}
		}
		using var path = pathBuilder.Detach();
		canvas.DrawPath(path, paint);
	}

	protected override void OnDestroy()
	{
		hudFont?.Dispose(); hudFont = null;
		hudPaint?.Dispose(); hudPaint = null;
		shadowPaint?.Dispose(); shadowPaint = null;
		polePaint?.Dispose(); polePaint = null;
		wirePaint?.Dispose(); wirePaint = null;
		meshPaint?.Dispose(); meshPaint = null;
		builder?.Dispose(); builder = null;
		indexBuffer?.Dispose(); indexBuffer = null;
		vertexBuffer?.Dispose(); vertexBuffer = null;
		spec?.Dispose(); spec = null;
		weaveShader?.Dispose(); weaveShader = null;
		bannerShader?.Dispose(); bannerShader = null;
		base.OnDestroy();
	}
}
