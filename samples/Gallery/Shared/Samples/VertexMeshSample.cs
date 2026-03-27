using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class VertexMeshSample : CanvasSampleBase
{
	private float gridSize = 3f;
	private int colorModeIndex;
	private bool wireframe;

	private static readonly string[] ColorModes = { "Rainbow", "Warm", "Cool", "Mono" };

	public override string Title => "Vertex Mesh";

	public override string Category => SampleCategories.General;

	public override string Description => "Render colored triangle meshes with adjustable grid density, color modes, and wireframe overlay.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("colorMode", "Color Mode", ColorModes, colorModeIndex),
		new SliderControl("gridSize", "Grid Size", 2, 8, gridSize, 1),
		new ToggleControl("wireframe", "Show Wireframe", wireframe),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "gridSize":
				gridSize = (float)value;
				break;
			case "colorMode":
				colorModeIndex = (int)value;
				break;
			case "wireframe":
				wireframe = (bool)value;
				break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(SKColors.White);

		var n = (int)gridSize;
		var margin = 40f;
		var cellW = (width - margin * 2) / n;
		var cellH = (height - margin * 2) / n;

		// Build vertex grid: (n+1) x (n+1) shared vertices, indexed triangles
		var rows = n + 1;
		var cols = n + 1;
		var vertexCount = rows * cols;
		var vertices = new SKPoint[vertexCount];
		var colors = new SKColor[vertexCount];

		for (var row = 0; row < rows; row++)
		{
			for (var col = 0; col < cols; col++)
			{
				var idx = row * cols + col;
				var px = (float)col / n;
				var py = (float)row / n;
				vertices[idx] = new SKPoint(margin + col * cellW, margin + row * cellH);
				colors[idx] = GetVertexColor(px, py, 0);
			}
		}

		// Two triangles per cell
		var indexCount = n * n * 6;
		var indices = new ushort[indexCount];
		var ii = 0;
		for (var row = 0; row < n; row++)
		{
			for (var col = 0; col < n; col++)
			{
				var tl = (ushort)(row * cols + col);
				var tr = (ushort)(tl + 1);
				var bl = (ushort)((row + 1) * cols + col);
				var br = (ushort)(bl + 1);

				indices[ii++] = tl;
				indices[ii++] = tr;
				indices[ii++] = bl;

				indices[ii++] = tr;
				indices[ii++] = br;
				indices[ii++] = bl;
			}
		}

		using var paint = new SKPaint { IsAntialias = true };
		using var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, vertices, null, colors, indices);
		canvas.DrawVertices(verts, SKBlendMode.Dst, paint);

		if (wireframe)
		{
			using var wirePaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = new SKColor(0, 0, 0, 80),
				StrokeWidth = 1,
			};

			for (var i = 0; i < indices.Length; i += 3)
			{
				using var triPath = new SKPath();
				triPath.MoveTo(vertices[indices[i]]);
				triPath.LineTo(vertices[indices[i + 1]]);
				triPath.LineTo(vertices[indices[i + 2]]);
				triPath.Close();
				canvas.DrawPath(triPath, wirePaint);
			}
		}
	}

	private SKColor GetVertexColor(float px, float py, float t)
	{
		return colorModeIndex switch
		{
			1 => WarmColor(px, py), // Warm
			2 => CoolColor(px, py), // Cool
			3 => MonoColor(px, py), // Mono
			_ => RainbowColor(px, py), // Rainbow
		};
	}

	private static SKColor RainbowColor(float px, float py)
	{
		var hue = (px + py) / 2f * 360f;
		return SKColor.FromHsl(hue % 360, 80, 60);
	}

	private static SKColor WarmColor(float px, float py)
	{
		var t = (px + py) / 2f;
		var r = (byte)(200 + t * 55);
		var g = (byte)(60 + t * 120);
		var b = (byte)(20 + t * 40);
		return new SKColor(r, g, b);
	}

	private static SKColor CoolColor(float px, float py)
	{
		var t = (px + py) / 2f;
		var r = (byte)(20 + t * 60);
		var g = (byte)(80 + t * 120);
		var b = (byte)(160 + t * 95);
		return new SKColor(r, g, b);
	}

	private static SKColor MonoColor(float px, float py)
	{
		var t = (px + py) / 2f;
		var v = (byte)(40 + t * 180);
		return new SKColor(v, v, v);
	}
}
