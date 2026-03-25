using System;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class VertexMeshSample : InteractiveSampleBase
{
	private float gridSize = 3f;
	private int colorModeIndex;
	private bool wireframe;

	private static readonly string[] ColorModes = { "Rainbow", "Warm", "Cool", "Mono" };

	public override string Title => "Vertex Mesh";

	public override string Category => SampleCategories.General;

	public override string Description => "Render colored triangle meshes with adjustable grid and color modes.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("gridSize", "Grid Size", 2, 8, gridSize, 1),
		new PickerControl("colorMode", "Color Mode", ColorModes, colorModeIndex),
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

		// Generate vertices for each cell, triangulated
		var vertexCount = n * n * 6; // 2 triangles per cell, 3 vertices each
		var vertices = new SKPoint[vertexCount];
		var colors = new SKColor[vertexCount];

		var idx = 0;
		for (var row = 0; row < n; row++)
		{
			for (var col = 0; col < n; col++)
			{
				var x0 = margin + col * cellW;
				var y0 = margin + row * cellH;
				var x1 = x0 + cellW;
				var y1 = y0 + cellH;

				// Triangle 1: top-left, top-right, bottom-left
				vertices[idx] = new SKPoint(x0, y0);
				vertices[idx + 1] = new SKPoint(x1, y0);
				vertices[idx + 2] = new SKPoint(x0, y1);

				// Triangle 2: top-right, bottom-right, bottom-left
				vertices[idx + 3] = new SKPoint(x1, y0);
				vertices[idx + 4] = new SKPoint(x1, y1);
				vertices[idx + 5] = new SKPoint(x0, y1);

				// Assign colors
				for (var v = 0; v < 6; v++)
				{
					var t = (float)(row * n + col) / (n * n);
					var vi = idx + v;
					var px = (vertices[vi].X - margin) / (width - margin * 2);
					var py = (vertices[vi].Y - margin) / (height - margin * 2);
					colors[vi] = GetVertexColor(px, py, t);
				}

				idx += 6;
			}
		}

		using var paint = new SKPaint { IsAntialias = true };
		canvas.DrawVertices(SKVertexMode.Triangles, vertices, colors, paint);

		if (wireframe)
		{
			using var wirePaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				Color = new SKColor(0, 0, 0, 80),
				StrokeWidth = 1,
			};

			for (var i = 0; i < vertexCount; i += 3)
			{
				using var triPath = new SKPath();
				triPath.MoveTo(vertices[i]);
				triPath.LineTo(vertices[i + 1]);
				triPath.LineTo(vertices[i + 2]);
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
