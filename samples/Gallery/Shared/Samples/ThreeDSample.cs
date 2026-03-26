using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ThreeDSample : InteractiveSampleBase
{
	private float rotateX;
	private float rotateY = 30f;
	private float rotateZ;
	private float translateX;
	private float translateY;
	private float scale = 1f;
	private int projectionIndex;
	private float perspDepth = 500f;
	private bool showAxes = true;
	private bool showShadow = true;

	private static readonly string[] Projections = { "Orthographic", "Perspective" };

	public override string Title => "3D Transforms";

	public override string Category => SampleCategories.General;

	public override string Description => "Visualize 3D transformations with rotation around X, Y, and Z axes.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new SliderControl("rotateX", "Rotate X", -180, 180, rotateX),
		new SliderControl("rotateY", "Rotate Y", -180, 180, rotateY),
		new SliderControl("rotateZ", "Rotate Z", -180, 180, rotateZ),
		new SliderControl("translateX", "Translate X", -300, 300, translateX),
		new SliderControl("translateY", "Translate Y", -300, 300, translateY),
		new SliderControl("scale", "Scale", 0.2f, 3, scale, 0.1f),
		new PickerControl("projection", "Projection", Projections, projectionIndex),
		new SliderControl("perspDepth", "Perspective Depth", 100, 2000, perspDepth, 50),
		new ToggleControl("showAxes", "Show Axes", showAxes),
		new ToggleControl("shadow", "Show Shadow", showShadow),
	];

	protected override void OnControlChanged(string id, object value)
	{
		switch (id)
		{
			case "rotateX": rotateX = (float)value; break;
			case "rotateY": rotateY = (float)value; break;
			case "rotateZ": rotateZ = (float)value; break;
			case "translateX": translateX = (float)value; break;
			case "translateY": translateY = (float)value; break;
			case "scale": scale = (float)value; break;
			case "projection": projectionIndex = (int)value; break;
			case "perspDepth": perspDepth = (float)value; break;
			case "showAxes": showAxes = (bool)value; break;
			case "shadow": showShadow = (bool)value; break;
		}
	}

	protected override void OnDrawSample(SKCanvas canvas, int width, int height)
	{
		canvas.Clear(new SKColor(30, 30, 50));

		var cx = width / 2f;
		var cy = height / 2f;
		var size = Math.Min(width, height) * 0.2f * scale;

		// Draw grid background
		DrawGrid(canvas, width, height, cx, cy);

		// Build rotation matrix from current slider values
		var rx = SKMatrix44.CreateRotationDegrees(1, 0, 0, rotateX);
		var ry = SKMatrix44.CreateRotationDegrees(0, 1, 0, rotateY);
		var rz = SKMatrix44.CreateRotationDegrees(0, 0, 1, rotateZ);

		var rotation = SKMatrix44.CreateIdentity();
		rotation.PostConcat(rx);
		rotation.PostConcat(ry);
		rotation.PostConcat(rz);

		canvas.Save();
		// Translate to center, then apply user translation before rotation
		canvas.Translate(cx + translateX, cy + translateY);

		// Apply perspective if selected
		if (projectionIndex == 1)
		{
			var persp = SKMatrix.Identity;
			persp.Persp2 = 1f / perspDepth;
			canvas.Concat(ref persp);
		}

		// Draw 3D axes before rotation (world axes)
		if (showAxes)
			DrawAxes(canvas, rotation, size * 1.8f);

		// Apply rotation
		var m = rotation.Matrix;
		canvas.Concat(ref m);

		// Draw the shape
		var side = rotation.MapPoint(new SKPoint3(0, 0, 1)).Z > 0;
		var rect = new SKRect(-size, -size, size, size);

		using var fillPaint = new SKPaint
		{
			Color = side ? new SKColor(147, 51, 234) : new SKColor(34, 197, 94),
			Style = SKPaintStyle.Fill,
			IsAntialias = true,
		};
		canvas.DrawRoundRect(rect, 20, 20, fillPaint);

		using var strokePaint = new SKPaint
		{
			Color = SKColors.White.WithAlpha(80),
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2,
			IsAntialias = true,
		};
		canvas.DrawRoundRect(rect, 20, 20, strokePaint);

		// Face label
		using var labelFont = new SKFont { Size = 20 };
		using var labelPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		canvas.DrawText(side ? "FRONT" : "BACK", 0, 7, SKTextAlign.Center, labelFont, labelPaint);

		canvas.Restore();

		// Shadow (drawn in 2D space below the object)
		if (showShadow)
		{
			canvas.Save();
			canvas.Translate(cx, cy + size * 2 + 10);

			var shadowWidth = size * (0.5f + 0.5f * MathF.Abs(MathF.Cos(rotateY * MathF.PI / 180f)));
			var shadowRect = new SKRect(-shadowWidth, -8, shadowWidth, 8);

			using var shadowPaint = new SKPaint
			{
				IsAntialias = true,
				MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6),
				Color = new SKColor(0, 0, 0, 60),
			};
			canvas.DrawOval(shadowRect, shadowPaint);
			canvas.Restore();
		}

		// Matrix info overlay
		DrawMatrixInfo(canvas, rotation);
	}

	private static void DrawGrid(SKCanvas canvas, int width, int height, float cx, float cy)
	{
		using var gridPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(255, 255, 255, 15),
			StrokeWidth = 1,
		};

		var spacing = 40f;
		for (var x = cx % spacing; x < width; x += spacing)
			canvas.DrawLine(x, 0, x, height, gridPaint);
		for (var y = cy % spacing; y < height; y += spacing)
			canvas.DrawLine(0, y, width, y, gridPaint);

		// Center crosshair
		gridPaint.Color = new SKColor(255, 255, 255, 40);
		gridPaint.PathEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0);
		canvas.DrawLine(cx, 0, cx, height, gridPaint);
		canvas.DrawLine(0, cy, width, cy, gridPaint);
	}

	private static void DrawAxes(SKCanvas canvas, SKMatrix44 rotation, float axisLength)
	{
		// Transform unit axis vectors through the rotation
		var xEnd = rotation.MapPoint(new SKPoint(axisLength, 0));
		var yEnd = rotation.MapPoint(new SKPoint(0, axisLength));
		var zEnd = rotation.MapPoint(new SKPoint(0, 0));
		// Z axis needs special handling — map a point along Z
		var zPt3 = rotation.MapPoint(new SKPoint3(0, 0, axisLength));
		var zEnd2d = new SKPoint(zPt3.X, zPt3.Y);

		var origin = SKPoint.Empty;

		// X axis (red)
		using var xPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(239, 68, 68),
			StrokeWidth = 2,
			IsAntialias = true,
		};
		canvas.DrawLine(origin, xEnd, xPaint);

		using var xFont = new SKFont { Size = 14 };
		using var xTextPaint = new SKPaint { Color = xPaint.Color, IsAntialias = true };
		canvas.DrawText("X", xEnd.X + 5, xEnd.Y + 5, xFont, xTextPaint);

		// Y axis (green)
		using var yPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(34, 197, 94),
			StrokeWidth = 2,
			IsAntialias = true,
		};
		canvas.DrawLine(origin, yEnd, yPaint);
		canvas.DrawText("Y", yEnd.X + 5, yEnd.Y + 5, xFont, new SKPaint { Color = yPaint.Color, IsAntialias = true });

		// Z axis (blue)
		using var zPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = new SKColor(59, 130, 246),
			StrokeWidth = 2,
			IsAntialias = true,
		};
		canvas.DrawLine(origin, zEnd2d, zPaint);
		canvas.DrawText("Z", zEnd2d.X + 5, zEnd2d.Y + 5, xFont, new SKPaint { Color = zPaint.Color, IsAntialias = true });
	}

	private void DrawMatrixInfo(SKCanvas canvas, SKMatrix44 rotation)
	{
		var m = rotation.Matrix;
		var boxW = 280f;
		var boxH = 120f;

		using var bgPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(0, 0, 0, 200),
		};
		canvas.DrawRoundRect(new SKRoundRect(new SKRect(8, 8, 8 + boxW, 8 + boxH), 8, 8), bgPaint);

		using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var font = new SKFont(SKTypeface.Default, 13);
		using var headerFont = new SKFont(SKTypeface.Default, 12);

		canvas.DrawText("3D Rotation Matrix:", 14, 24, headerFont, textPaint);

		var vals = new float[]
		{
			m.ScaleX, m.SkewX, m.Persp0,
			m.SkewY, m.ScaleY, m.Persp1,
			m.TransX, m.TransY, m.Persp2,
		};

		textPaint.Color = new SKColor(180, 180, 180);
		var startY = 44f;
		var lineH = 18f;
		for (var row = 0; row < 3; row++)
		{
			var line = $"│ {vals[row * 3],7:F3}  {vals[row * 3 + 1],7:F3}  {vals[row * 3 + 2],7:F3} │";
			canvas.DrawText(line, 14, startY + row * lineH, font, textPaint);
		}

		textPaint.Color = new SKColor(120, 120, 120);
		font.Size = 11;
		canvas.DrawText($"X:{rotateX:F0}°  Y:{rotateY:F0}°  Z:{rotateZ:F0}°  S:{scale:F1}  {Projections[projectionIndex]}", 14, startY + 3 * lineH + 4, font, textPaint);
	}
}
