using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

/// <summary>
/// Demonstrates SKMesh with a 3D Earth globe inspired by DrawnUi's gallery3d "Globe with pins":
/// - A 28×56 UV sphere mesh with back-face culling and perspective-correct custom varyings.
/// - Procedural Blue Marble equirectangular Earth texture + night city lights child shaders.
/// - SkSL day/night terminator lighting, ocean specular reflection, animated clouds, and Fresnel atmosphere rim glow.
/// - Six world cities with horizon occlusion, screen-blended 4-ray starburst flares, a frosted glass callout card,
///   and smooth ease-in-out 3D camera glides when selecting a city.
/// </summary>
public class MeshGlobeSample : CanvasSampleBase
{
	private const string VS = @"
		Varyings main(const Attributes attrs) {
			Varyings v;
			v.position = attrs.position;
			v.uvDepth = attrs.uvDepth;
			v.normalSpec = attrs.normalSpec;
			return v;
		}";

	private const string FS = @"
		uniform float uTime;
		uniform float uSunAngle;
		uniform float uAtmosphere;
		uniform shader uEarthTex;

		float2 main(const Varyings v, out half4 color) {
			float invZ = max(v.uvDepth.z, 0.0001);
			float2 uv = clamp(v.uvDepth.xy / invZ, float2(0.0), float2(1.0));
			float3 n = normalize(v.normalSpec.xyz);
			float lat = (0.5 - uv.y) * 3.14159265;

			// Sample the NASA Blue Marble equirectangular Earth texture (2048x1024).
			half4 tex = uEarthTex.eval(uv * float2(2048.0, 1024.0));
			float3 baseCol = float3(tex.rgb);

			// Detect ocean vs land/ice directly from the Blue Marble pixel chromaticity (oceans are deep blue)
			float oceanMask = clamp((baseCol.b - max(baseCol.r, baseCol.g) * 0.92) * 5.0, 0.0, 1.0);
			float landMask = 1.0 - oceanMask;

			// Sun direction in view space controlled by uSunAngle
			float3 lightDir = normalize(float3(sin(uSunAngle), -0.22, cos(uSunAngle)));
			float3 viewDir  = float3(0.0, 0.0, 1.0);
			float3 halfVec  = normalize(lightDir + viewDir);

			float ndotl = dot(n, lightDir);
			float dayFactor = smoothstep(-0.18, 0.22, ndotl);
			float twilight  = smoothstep(-0.22, 0.04, ndotl) * (1.0 - smoothstep(0.04, 0.30, ndotl));
			float diffuse   = 0.14 + 0.86 * clamp((ndotl + 0.15) / 1.15, 0.0, 1.0);

			// Ocean sun glint & specular highlight
			float specAngle = max(dot(n, halfVec), 0.0);
			float oceanSpec = pow(specAngle, 32.0) * 0.55 * oceanMask * dayFactor
			                + pow(specAngle, 10.0) * 0.12 * oceanMask * dayFactor;

			float3 lit = baseCol * diffuse + float3(1.0, 0.96, 0.86) * oceanSpec;

			// Warm sunset/sunrise scattering along the day-night terminator
			lit += float3(0.80, 0.30, 0.10) * twilight * 0.35;

			// Night-side city lights glowing on continents when rotated into shadow
			float nightFactor = smoothstep(0.08, -0.24, ndotl);
			float cityCluster1 = sin(uv.x * 220.0) * cos(uv.y * 110.0);
			float cityCluster2 = sin(uv.x * 480.0 + 1.3) * sin(uv.y * 240.0 + 0.7);
			float citySpark = smoothstep(0.62, 0.88, cityCluster1 * 0.55 + cityCluster2 * 0.45)
			                * landMask * smoothstep(1.15, 0.55, abs(lat));
			lit += float3(1.0, 0.76, 0.34) * citySpark * nightFactor * 0.95;
			lit += baseCol * 0.06 * nightFactor;

			// Fresnel atmospheric limb glow around the sphere horizon
			float fresnel = pow(clamp(1.0 - max(n.z, 0.0), 0.0, 1.0), 3.0);
			float3 atmoColor = mix(float3(0.18, 0.50, 0.96), float3(0.46, 0.78, 1.0), dayFactor);
			lit = mix(lit, atmoColor, fresnel * 0.55 * clamp(uAtmosphere, 0.0, 2.0));
			lit += atmoColor * pow(fresnel, 2.0) * 0.30 * uAtmosphere * (0.35 + 0.65 * dayFactor);

			color = half4(half3(clamp(lit, 0.0, 1.0)), 1.0);
			return v.position;
		}";

	private const int LatSteps = 28;
	private const int LonSteps = 56;
	private const int GridCols = LonSteps + 1;
	private const int GridRows = LatSteps + 1;
	private const int VertexCount = GridCols * GridRows;
	private const int MaxQuadCount = LatSteps * LonSteps;
	private const int FloatsPerVertex = 10; // pos(2) + uvDepth(4) + normalSpec(4)

	private readonly record struct CityPin(string Name, string Country, float Lat, float Lon);

	// The exact 6 world cities from DrawnUi.Rust/examples/gallery3d
	private static readonly CityPin[] Cities =
	[
		new("Tokyo",          "Japan",          35.7f,  139.7f),
		new("Sydney",         "Australia",     -33.9f,  151.2f),
		new("Cape Town",      "South Africa",  -33.9f,   18.4f),
		new("London",         "United Kingdom", 51.5f,   -0.1f),
		new("New York",       "United States",  40.7f,  -74.0f),
		new("Rio de Janeiro", "Brazil",        -22.9f,  -43.2f),
	];

	private static readonly string[] CityChoices =
	[
		"Auto Tour (All Cities)",
		"Tokyo, Japan",
		"Sydney, Australia",
		"Cape Town, South Africa",
		"London, United Kingdom",
		"New York, United States",
		"Rio de Janeiro, Brazil",
	];

	// Precomputed unit-sphere coordinates and UVs for the (LatSteps + 1) x (LonSteps + 1) grid
	private readonly float[] sphereX = new float[VertexCount];
	private readonly float[] sphereY = new float[VertexCount];
	private readonly float[] sphereZ = new float[VertexCount];
	private readonly float[] sphereU = new float[VertexCount];
	private readonly float[] sphereV = new float[VertexCount];

	// Per-frame transformed view-space Z and 2D screen positions for back-face culling & wireframe
	private readonly float[] viewZ = new float[VertexCount];
	private readonly float[] screenX = new float[VertexCount];
	private readonly float[] screenY = new float[VertexCount];
	private readonly float[] vertices = new float[VertexCount * FloatsPerVertex];
	private readonly ushort[] indices = new ushort[MaxQuadCount * 6];

	private SKMeshSpecification? spec;
	private SKIndexedMeshBuilder? builder;
	private SKMeshVertexBuffer? vertexBuffer;
	private SKMeshIndexBuffer? indexBuffer;
	private SKShader? earthShader;
	private SKPaint? meshPaint;
	private SKPaint? wirePaint;
	private SKPaint? haloPaint;
	private SKPaint? flarePaint;
	private SKPaint? calloutBgPaint;
	private SKPaint? calloutBorderPaint;
	private SKPaint? starPaint;
	private SKPaint? titlePaint;
	private SKPaint? subtitlePaint;
	private SKPaint? hudPaint;
	private SKFont? titleFont;
	private SKFont? subtitleFont;
	private SKFont? hudFont;
	private SKPath? starPath;

	// Deterministic starfield positions in normalized [0..1] coordinates
	private readonly (float X, float Y, float Radius, byte Alpha)[] stars = new (float, float, float, byte)[90];

	private int cityChoiceIndex;
	private int activeCityIndex;
	private float spinSpeed = 12.0f; // degrees/sec (matches gallery3d's 10-12 deg/s spin)
	private float sunAngleDeg = -32.0f;
	private float atmosphere = 1.0f;
	private bool showWireframe;

	// Globe orientation (yaw & pitch in degrees) and smooth 0.6s city-glide state
	private float yawDeg = -20.0f;
	private float pitchDeg = 15.0f;
	private bool isGliding;
	private float glideFromYaw;
	private float glideFromPitch;
	private float glideToYaw;
	private float glideToPitch;
	private float glideProgress;
	private float autoTourTimer;

	private DateTime startTime;
	private DateTime lastFrameTime;

	public override string Title => "3D Earth Globe (City Pins)";

	public override DateOnly? DateAdded => new DateOnly(2026, 11, 16);

	public override string Description =>
		"A 3D Earth globe rendered with SKMesh, inspired by DrawnUi's gallery3d. " +
		"Features a 28×56 UV sphere with back-face culling, day/night terminator & city lights, " +
		"screen-blended starburst pins that hide behind the horizon, and smooth camera glides between world cities.";

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
		new PickerControl("city", "Focus City", CityChoices, cityChoiceIndex),
		new SliderControl("spin", "Spin Speed", 0.0f, 45.0f, spinSpeed, 1.0f),
		new SliderControl("sun", "Sun Angle", -180.0f, 180.0f, sunAngleDeg, 5.0f),
		new SliderControl("atmo", "Atmosphere", 0.0f, 1.8f, atmosphere, 0.1f),
		new ToggleControl("wireframe", "Show Wireframe", showWireframe),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "city":
				cityChoiceIndex = (int)value;
				if (cityChoiceIndex > 0)
				{
					activeCityIndex = cityChoiceIndex - 1;
					StartGlideToCity(activeCityIndex);
				}
				else
				{
					autoTourTimer = 0f;
					StartGlideToCity(activeCityIndex);
				}
				break;
			case "spin":
				spinSpeed = (float)value;
				break;
			case "sun":
				sunAngleDeg = (float)value;
				break;
			case "atmo":
				atmosphere = (float)value;
				break;
			case "wireframe":
				showWireframe = (bool)value;
				break;
		}
	}

	protected override Task OnInit()
	{
		startTime = DateTime.UtcNow;
		lastFrameTime = startTime;
		yawDeg = -20.0f;
		pitchDeg = 15.0f;
		activeCityIndex = 3; // Start focused on London / Europe-Africa view (-20, 15)
		autoTourTimer = 0f;

		BuildSphereGrid();
		BuildStars();
		StartGlideToCity(activeCityIndex);

		return base.OnInit();
	}

	protected override async Task OnUpdate(CancellationToken token)
	{
		await Task.Delay(16, token);
	}

	private void BuildSphereGrid()
	{
		// Matches DrawnUi's Mesh3D::sphere(1.0) & lathe UV convention:
		// latitude runs from -90° (south pole, v = 1) to +90° (north pole, v = 0)
		// longitude wraps around the Y axis with u = 1 - s / LonSteps
		for (var r = 0; r <= LatSteps; r++)
		{
			var vFrac = (float)r / LatSteps;
			var lat = MathF.PI * (vFrac - 0.5f); // -pi/2 .. +pi/2
			var cosLat = MathF.Cos(lat);
			var sinLat = MathF.Sin(lat);
			var v = 1.0f - vFrac; // v = 0 at north pole, 1 at south pole

			for (var s = 0; s <= LonSteps; s++)
			{
				var uFrac = (float)s / LonSteps;
				var angle = uFrac * MathF.Tau;
				var cosLon = MathF.Cos(angle);
				var sinLon = MathF.Sin(angle);

				var idx = r * GridCols + s;
				sphereX[idx] = cosLat * cosLon;
				sphereY[idx] = sinLat;
				sphereZ[idx] = cosLat * sinLon;
				sphereU[idx] = 1.0f - uFrac;
				sphereV[idx] = v;
			}
		}
	}

	private void BuildStars()
	{
		uint seed = 0x6D2B79F5u;
		for (var i = 0; i < stars.Length; i++)
		{
			seed = seed * 1664525u + 1013904223u;
			var sx = ((seed >> 8) & 0xFFFFFF) / (float)0x1000000;
			seed = seed * 1664525u + 1013904223u;
			var sy = ((seed >> 8) & 0xFFFFFF) / (float)0x1000000;
			seed = seed * 1664525u + 1013904223u;
			var sr = 0.6f + (((seed >> 8) & 0xFFFF) / 65535f) * 1.4f;
			var sa = (byte)(90 + (i % 5) * 32);
			stars[i] = (sx, sy, sr, sa);
		}
	}

	/// <summary>
	/// Converts a (latitude, longitude) pair in degrees to a 3D point on the unit sphere,
	/// matching gallery3d's <c>on_globe(lat, lon)</c>.
	/// </summary>
	private static (float X, float Y, float Z) OnGlobe(float latDeg, float lonDeg, float radius = 1.004f)
	{
		var lat = latDeg * (MathF.PI / 180f);
		var angle = (1.0f - (lonDeg + 180.0f) / 360.0f) * MathF.Tau;
		var cosLat = MathF.Cos(lat);
		return (
			radius * cosLat * MathF.Cos(angle),
			radius * MathF.Sin(lat),
			radius * cosLat * MathF.Sin(angle)
		);
	}

	/// <summary>
	/// Starts a 0.6s ease-in-out camera glide so the chosen city faces the viewer
	/// along the shortest angular path (matching gallery3d's <c>turn_to_city</c>).
	/// </summary>
	private void StartGlideToCity(int cityIndex)
	{
		var city = Cities[cityIndex];
		var (x, _, z) = OnGlobe(city.Lat, city.Lon, 1.0f);

		// Target yaw brings (x, y, z) to face +Z toward the camera; pitch tilts toward city's latitude
		var targetYaw = -MathF.Atan2(x, z) * (180f / MathF.PI);
		var targetPitch = Math.Clamp(city.Lat * 0.75f, -50f, 60f);

		// Shortest angular path around the 360° circle
		var deltaYaw = ((targetYaw - yawDeg + 540f) % 360f);
		if (deltaYaw < 0f)
			deltaYaw += 360f;
		deltaYaw -= 180f;

		glideFromYaw = yawDeg;
		glideFromPitch = pitchDeg;
		glideToYaw = yawDeg + deltaYaw;
		glideToPitch = targetPitch;
		glideProgress = 0f;
		isGliding = true;
	}

	private static float EaseInOut(float t) =>
		t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) * 0.5f;

	private void EnsureResources()
	{
		if (builder != null)
			return;

		earthShader = CreateEarthMapShader();

		var specBuilder = SKMeshSpecification.CreateBuilder(VS, FS);
		specBuilder.ColorSpace = SKColorSpace.CreateSrgb();
		specBuilder.AlphaType = SKAlphaType.Premul;
		specBuilder.AddAttribute("position", SKMeshSpecificationAttributeType.Float2);
		specBuilder.AddAttribute("uvDepth", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddAttribute("normalSpec", SKMeshSpecificationAttributeType.Float4);
		specBuilder.AddVarying("uvDepth", SKMeshSpecificationVaryingType.Float4);
		specBuilder.AddVarying("normalSpec", SKMeshSpecificationVaryingType.Float4);

		spec = specBuilder.Build(out var errors);
		if (spec == null)
		{
			Console.WriteLine($"Failed to build globe mesh specification: {errors}");
			return;
		}

		vertexBuffer = SKMeshVertexBuffer.Create(VertexCount * FloatsPerVertex * sizeof(float));
		indexBuffer = SKMeshIndexBuffer.Create(MaxQuadCount * 6 * sizeof(ushort));

		builder = spec.CreateIndexedMeshBuilder();
		if (earthShader != null)
			builder.Children["uEarthTex"] = earthShader;

		meshPaint = new SKPaint { Color = SKColors.White };
		wirePaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(130, 215, 255, 95),
			StrokeWidth = 1f,
		};
		haloPaint = new SKPaint { IsAntialias = true };
		flarePaint = new SKPaint
		{
			IsAntialias = true,
			BlendMode = SKBlendMode.Screen,
		};
		calloutBgPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(26, 30, 46, 238),
		};
		calloutBorderPaint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 1.2f,
			Color = new SKColor(255, 255, 255, 55),
		};
		starPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(246, 184, 32),
		};
		titlePaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(242, 245, 255),
		};
		subtitlePaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(165, 178, 205),
		};
		hudPaint = new SKPaint
		{
			IsAntialias = true,
			Color = new SKColor(210, 225, 250, 215),
		};

		var baseTf = SampleMedia.Fonts.Default;
		titleFont = new SKFont(baseTf, 14f) { Embolden = true };
		subtitleFont = new SKFont(baseTf, 11.5f);
		hudFont = new SKFont(baseTf, 13.5f);

		// 5-pointed star path from gallery3d's callout card
		using var sb = new SKPathBuilder();
		sb.MoveTo(7.00f, 0.40f);
		sb.LineTo(8.70f, 5.05f);
		sb.LineTo(13.66f, 5.24f);
		sb.LineTo(9.76f, 8.30f);
		sb.LineTo(11.11f, 13.06f);
		sb.LineTo(7.00f, 10.30f);
		sb.LineTo(2.89f, 13.06f);
		sb.LineTo(4.24f, 8.30f);
		sb.LineTo(0.34f, 5.24f);
		sb.LineTo(5.30f, 5.05f);
		sb.Close();
		starPath = sb.Detach();
	}

	/// <summary>
	/// Loads the embedded public-domain NASA Blue Marble equirectangular Earth texture (2048×1024).
	/// </summary>
	private static SKShader? CreateEarthMapShader()
	{
		using var stream = SampleMedia.Images.Earth;
		using var img = SKImage.FromEncodedData(stream);
		return img?.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear));
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		var now = DateTime.UtcNow;
		var dt = Math.Clamp((float)(now - lastFrameTime).TotalSeconds, 0f, 0.1f);
		lastFrameTime = now;
		var elapsed = (float)(now - startTime).TotalSeconds;

		DrawSpaceBackdrop(canvas, width, height, elapsed);
		EnsureResources();

		if (builder == null || vertexBuffer == null || indexBuffer == null || meshPaint == null)
			return;

		// Update globe rotation & city-to-city smooth camera glide
		if (isGliding)
		{
			glideProgress = Math.Min(1.0f, glideProgress + dt / 0.65f);
			var v = EaseInOut(glideProgress);
			yawDeg = glideFromYaw + (glideToYaw - glideFromYaw) * v;
			pitchDeg = glideFromPitch + (glideToPitch - glideFromPitch) * v;
			if (glideProgress >= 1.0f)
				isGliding = false;
		}
		else
		{
			yawDeg -= spinSpeed * dt;

			// In Auto Tour mode, glide to the next world city every 4.2 seconds
			if (cityChoiceIndex == 0)
			{
				autoTourTimer += dt;
				if (autoTourTimer >= 4.2f)
				{
					autoTourTimer = 0f;
					activeCityIndex = (activeCityIndex + 1) % Cities.Length;
					StartGlideToCity(activeCityIndex);
				}
			}
		}

		var yawRad = yawDeg * (MathF.PI / 180f);
		var pitchRad = pitchDeg * (MathF.PI / 180f);
		var cosYaw = MathF.Cos(yawRad);
		var sinYaw = MathF.Sin(yawRad);
		var cosPitch = MathF.Cos(pitchRad);
		var sinPitch = MathF.Sin(pitchRad);

		var centerX = width * 0.5f;
		var centerY = height * 0.52f;
		var radiusPx = MathF.Min(width, height) * 0.36f;
		const float cameraDist = 3.0f;

		// Soft outer atmospheric halo behind the globe
		if (haloPaint != null && atmosphere > 0.01f)
		{
			var haloRadius = radiusPx * (1.18f + 0.08f * atmosphere);
			using var haloShader = SKShader.CreateRadialGradient(
				new SKPoint(centerX, centerY),
				haloRadius,
				new[]
				{
					new SKColor(70, 165, 255, (byte)Math.Clamp((int)(95 * atmosphere), 0, 200)),
					new SKColor(35, 95, 210, (byte)Math.Clamp((int)(45 * atmosphere), 0, 140)),
					new SKColor(10, 20, 45, 0),
				},
				new[] { 0.72f, 0.88f, 1.0f },
				SKShaderTileMode.Clamp);
			haloPaint.Shader = haloShader;
			canvas.DrawCircle(centerX, centerY, haloRadius, haloPaint);
		}

		// Transform sphere vertices into view space and project with perspective
		for (var i = 0; i < VertexCount; i++)
		{
			RotateGlobePoint(
				sphereX[i], sphereY[i], sphereZ[i],
				cosYaw, sinYaw, cosPitch, sinPitch,
				out var vx, out var vy, out var vz);

			var depth = Math.Max(0.4f, cameraDist - vz);
			var persp = cameraDist / depth;
			var invZ = 1.0f / depth;

			// Screen Y is downward, while 3D +Y (North Pole) is upward
			var sx = centerX + vx * radiusPx * persp;
			var sy = centerY - vy * radiusPx * persp;

			viewZ[i] = vz;
			screenX[i] = sx;
			screenY[i] = sy;

			var b = i * FloatsPerVertex;
			vertices[b + 0] = sx;
			vertices[b + 1] = sy;
			vertices[b + 2] = sphereU[i] * invZ;
			vertices[b + 3] = sphereV[i] * invZ;
			vertices[b + 4] = invZ;
			vertices[b + 5] = depth;
			vertices[b + 6] = vx;
			vertices[b + 7] = vy;
			vertices[b + 8] = vz;
			vertices[b + 9] = 1.0f;
		}

		// Emit only front-facing quads (back-face culling gives exact occlusion for a convex sphere)
		var indexCount = 0;
		for (var r = 0; r < LatSteps; r++)
		{
			var row0 = r * GridCols;
			var row1 = (r + 1) * GridCols;
			for (var s = 0; s < LonSteps; s++)
			{
				var i00 = (ushort)(row0 + s);
				var i10 = (ushort)(row0 + s + 1);
				var i01 = (ushort)(row1 + s);
				var i11 = (ushort)(row1 + s + 1);

				// Check both view-space normal Z and 2D screen-space signed area
				var avgZ = viewZ[i00] + viewZ[i10] + viewZ[i01] + viewZ[i11];
				if (avgZ <= -0.15f)
					continue;

				var cross = (screenX[i10] - screenX[i00]) * (screenY[i01] - screenY[i00])
				          - (screenY[i10] - screenY[i00]) * (screenX[i01] - screenX[i00]);
				if (cross <= 0f)
					continue;

				indices[indexCount++] = i00;
				indices[indexCount++] = i10;
				indices[indexCount++] = i11;
				indices[indexCount++] = i00;
				indices[indexCount++] = i11;
				indices[indexCount++] = i01;
			}
		}

		if (indexCount > 0)
		{
			vertexBuffer.Update(MemoryMarshal.AsBytes(vertices.AsSpan()), 0);
			indexBuffer.Update(MemoryMarshal.AsBytes(indices.AsSpan(0, indexCount)), 0);

			builder.Uniforms["uTime"] = elapsed;
			builder.Uniforms["uSunAngle"] = sunAngleDeg * (MathF.PI / 180f);
			builder.Uniforms["uAtmosphere"] = atmosphere;
			builder.Bounds = new SKRect(0, 0, width, height);
			builder.SetVertexBuffer(vertexBuffer, VertexCount);
			builder.SetIndexBuffer(indexBuffer, indexCount);

			using var mesh = builder.Build(out var errors);
			if (mesh != null)
				canvas.DrawMesh(mesh, meshPaint);
			else
				Console.WriteLine($"Failed to build globe mesh: {errors}");

			if (showWireframe && wirePaint != null)
				DrawWireframe(canvas, indexCount, wirePaint);
		}

		// Project & draw the 6 world city pins and the active city's frosted glass callout
		DrawCityPinsAndCallouts(
			canvas, elapsed,
			cosYaw, sinYaw, cosPitch, sinPitch,
			centerX, centerY, radiusPx, cameraDist);

		if (hudPaint != null && hudFont != null)
		{
			if (canvas.Context == null)
			{
				canvas.DrawText("SKMesh requires GPU rendering — switch from CPU to GPU in the top bar.", 16, height * 0.5f, SKTextAlign.Left, hudFont, hudPaint);
			}
			else
			{
				var focusCity = Cities[activeCityIndex];
				canvas.DrawText(
					$"Focus: {focusCity.Name} ({focusCity.Lat:0.0}°, {focusCity.Lon:0.0}°)   Visible Tris: {indexCount / 3}",
					16, height - 16, SKTextAlign.Left, hudFont, hudPaint);
			}
		}
	}

	private static void RotateGlobePoint(
		float x, float y, float z,
		float cosYaw, float sinYaw, float cosPitch, float sinPitch,
		out float vx, out float vy, out float vz)
	{
		// 1. Yaw around Y axis
		var x1 = x * cosYaw + z * sinYaw;
		var y1 = y;
		var z1 = -x * sinYaw + z * cosYaw;

		// 2. Pitch around X axis so positive pitch tilts the northern hemisphere toward the viewer (+Z)
		vx = x1;
		vy = y1 * cosPitch - z1 * sinPitch;
		vz = y1 * sinPitch + z1 * cosPitch;
	}

	private void DrawCityPinsAndCallouts(
		SKCanvas canvas, float elapsed,
		float cosYaw, float sinYaw, float cosPitch, float sinPitch,
		float centerX, float centerY, float radiusPx, float cameraDist)
	{
		if (flarePaint == null || calloutBgPaint == null || calloutBorderPaint == null ||
			starPaint == null || titlePaint == null || subtitlePaint == null ||
			titleFont == null || subtitleFont == null || starPath == null)
			return;

		Span<(bool Visible, float X, float Y, float Z)> projected = stackalloc (bool, float, float, float)[Cities.Length];

		for (var i = 0; i < Cities.Length; i++)
		{
			var c = Cities[i];
			var (gx, gy, gz) = OnGlobe(c.Lat, c.Lon, 1.005f);
			RotateGlobePoint(gx, gy, gz, cosYaw, sinYaw, cosPitch, sinPitch, out var vx, out var vy, out var vz);

			// Horizon occlusion check: hide pins when they rotate onto the far side of the Earth
			if (vz <= 0.08f)
			{
				projected[i] = (false, 0f, 0f, vz);
				continue;
			}

			var depth = cameraDist - vz;
			var persp = cameraDist / depth;
			var sx = centerX + vx * radiusPx * persp;
			var sy = centerY - vy * radiusPx * persp;
			projected[i] = (true, sx, sy, vz);
		}

		// 1. Draw screen-blended 4-ray starburst flares for all visible cities (matching gallery3d's pin())
		for (var i = 0; i < Cities.Length; i++)
		{
			var p = projected[i];
			if (!p.Visible)
				continue;

			// Each city's flare flashes once every 2.6s, out of step by 0.37 (exact gallery3d formula)
			var phase = (elapsed / 2.6f + i * 0.37f) % 1.0f;
			var flash = MathF.Pow(MathF.Max(0f, MathF.Sin(phase * MathF.PI)), 8f);
			var isSelected = i == activeCityIndex;
			var scale = (0.55f + 0.70f * flash) * (isSelected ? 1.25f : 1.0f);
			var rotDeg = 20.0f * flash;
			var flareRadius = 22.0f * scale;

			canvas.Save();
			canvas.Translate(p.X, p.Y);
			canvas.RotateDegrees(rotDeg);

			// Screen-blended circular golden glow
			using (var glowShader = SKShader.CreateRadialGradient(
				SKPoint.Empty,
				flareRadius,
				new[]
				{
					new SKColor(255, 250, 230, 255),
					new SKColor(255, 226, 150, 210),
					new SKColor(255, 200, 70, 0),
				},
				new[] { 0.0f, 0.28f, 1.0f },
				SKShaderTileMode.Clamp))
			{
				flarePaint.Shader = glowShader;
				canvas.DrawCircle(0, 0, flareRadius, flarePaint);
			}

			// 4 starburst rays (0°, 90°, 45°, 135°)
			DrawFlareRay(canvas, flareRadius * 2.1f, 2.2f, 0f);
			DrawFlareRay(canvas, flareRadius * 2.1f, 2.2f, 90f);
			DrawFlareRay(canvas, flareRadius * 1.25f, 1.6f, 45f);
			DrawFlareRay(canvas, flareRadius * 1.25f, 1.6f, 135f);

			// White-hot center core
			flarePaint.Shader = null;
			flarePaint.Color = SKColors.White;
			canvas.DrawCircle(0, 0, 2.8f, flarePaint);

			canvas.Restore();
		}

		// 2. Draw the frosted glass callout card over the focused city if it is on the visible hemisphere
		var active = projected[activeCityIndex];
		if (active.Visible)
		{
			var city = Cities[activeCityIndex];
			const float cardW = 172f;
			const float cardH = 46f;
			const float rise = 62f;

			var cardLeft = Math.Clamp(active.X - cardW * 0.5f, 12f, canvas.DeviceClipBounds.Width - cardW - 12f);
			var cardTop = Math.Max(12f, active.Y - rise);
			var cardRect = new SKRect(cardLeft, cardTop, cardLeft + cardW, cardTop + cardH);

			// Connector stem from card bottom to city flare
			using (var stemPaint = new SKPaint
			{
				IsAntialias = true,
				Color = new SKColor(255, 215, 110, 140),
				StrokeWidth = 1.2f,
			})
			{
				canvas.DrawLine(active.X, cardRect.Bottom, active.X, active.Y - 6f, stemPaint);
			}

			canvas.DrawRoundRect(cardRect, 12f, 12f, calloutBgPaint);
			canvas.DrawRoundRect(cardRect, 12f, 12f, calloutBorderPaint);

			// Gold star icon
			canvas.Save();
			canvas.Translate(cardLeft + 12f, cardTop + (cardH - 14f) * 0.5f);
			canvas.DrawPath(starPath, starPaint);
			canvas.Restore();

			// City name & country subtitle
			canvas.DrawText(city.Name, cardLeft + 35f, cardTop + 21f, SKTextAlign.Left, titleFont, titlePaint);
			canvas.DrawText(city.Country, cardLeft + 35f, cardTop + 37f, SKTextAlign.Left, subtitleFont, subtitlePaint);
		}
	}

	private void DrawFlareRay(SKCanvas canvas, float length, float thickness, float angleDeg)
	{
		if (flarePaint == null)
			return;

		var halfLen = length * 0.5f;
		var halfThick = thickness * 0.5f;

		canvas.Save();
		canvas.RotateDegrees(angleDeg);
		using var rayShader = SKShader.CreateLinearGradient(
			new SKPoint(-halfLen, 0),
			new SKPoint(halfLen, 0),
			new[]
			{
				new SKColor(255, 255, 255, 0),
				new SKColor(255, 248, 220, 240),
				new SKColor(255, 255, 255, 0),
			},
			new[] { 0.0f, 0.5f, 1.0f },
			SKShaderTileMode.Clamp);
		flarePaint.Shader = rayShader;
		canvas.DrawRoundRect(new SKRect(-halfLen, -halfThick, halfLen, halfThick), halfThick, halfThick, flarePaint);
		canvas.Restore();
	}

	private void DrawSpaceBackdrop(SKCanvas canvas, int width, int height, float elapsed)
	{
		using var bgShader = SKShader.CreateRadialGradient(
			new SKPoint(width * 0.5f, height * 0.52f),
			MathF.Max(width, height) * 0.75f,
			new[] { new SKColor(14, 22, 46), new SKColor(8, 10, 22), new SKColor(4, 6, 14) },
			new[] { 0.0f, 0.55f, 1.0f },
			SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { Shader = bgShader };
		canvas.DrawRect(0, 0, width, height, bgPaint);

		using var starDotPaint = new SKPaint { IsAntialias = true };
		for (var i = 0; i < stars.Length; i++)
		{
			var s = stars[i];
			var twinkle = 0.65f + 0.35f * MathF.Sin(elapsed * 2.2f + i * 1.7f);
			starDotPaint.Color = new SKColor(220, 235, 255, (byte)(s.Alpha * twinkle));
			canvas.DrawCircle(s.X * width, s.Y * height, s.Radius, starDotPaint);
		}
	}

	private void DrawWireframe(SKCanvas canvas, int indexCount, SKPaint paint)
	{
		using var pathBuilder = new SKPathBuilder();
		for (var i = 0; i < indexCount; i += 3)
		{
			var i0 = indices[i + 0];
			var i1 = indices[i + 1];
			var i2 = indices[i + 2];

			pathBuilder.MoveTo(screenX[i0], screenY[i0]);
			pathBuilder.LineTo(screenX[i1], screenY[i1]);
			pathBuilder.LineTo(screenX[i2], screenY[i2]);
			pathBuilder.Close();
		}
		using var path = pathBuilder.Detach();
		canvas.DrawPath(path, paint);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		starPath?.Dispose();
		starPath = null;

		hudFont?.Dispose();
		hudFont = null;

		subtitleFont?.Dispose();
		subtitleFont = null;

		titleFont?.Dispose();
		titleFont = null;

		hudPaint?.Dispose();
		hudPaint = null;

		subtitlePaint?.Dispose();
		subtitlePaint = null;

		titlePaint?.Dispose();
		titlePaint = null;

		starPaint?.Dispose();
		starPaint = null;

		calloutBorderPaint?.Dispose();
		calloutBorderPaint = null;

		calloutBgPaint?.Dispose();
		calloutBgPaint = null;

		flarePaint?.Dispose();
		flarePaint = null;

		haloPaint?.Dispose();
		haloPaint = null;

		wirePaint?.Dispose();
		wirePaint = null;

		meshPaint?.Dispose();
		meshPaint = null;

		builder?.Dispose();
		builder = null;

		vertexBuffer?.Dispose();
		vertexBuffer = null;

		indexBuffer?.Dispose();
		indexBuffer = null;

		earthShader?.Dispose();
		earthShader = null;

		spec?.Dispose();
		spec = null;
	}
}
