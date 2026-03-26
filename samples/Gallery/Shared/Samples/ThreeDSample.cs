using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ThreeDSample : InteractiveSampleBase
{
	private float rotateX;
	private float rotateY = 45f;
	private float rotateZ;
	private float translateX;
	private float translateY;
	private float translateZ;
	private float scale = 1f;
	private int projectionIndex;
	private float perspDepth = 400f;
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
		new SliderControl("translateZ", "Translate Z", -500, 500, translateZ),
		new SliderControl("scale", "Scale", 0.2f, 3, scale, 0.1f),
		new PickerControl("projection", "Projection", Projections, projectionIndex),
		new SliderControl("perspDepth", "Camera Distance", 100, 2000, perspDepth),
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
			case "translateZ": translateZ = (float)value; break;
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
		var size = Math.Min(width, height) * 0.35f * scale;

		// Draw grid background
		DrawGrid(canvas, width, height, cx, cy);

		// Build rotation matrix from current slider values
		var rx = SKMatrix44.CreateRotationDegrees(1, 0, 0, rotateX);
		var ry = SKMatrix44.CreateRotationDegrees(0, 1, 0, rotateY);
		var rz = SKMatrix44.CreateRotationDegrees(0, 0, 1, rotateZ);

		var rotation = SKMatrix44.Concat(SKMatrix44.Concat(rx, ry), rz);

		canvas.Save();
		canvas.Translate(cx + translateX, cy + translateY);

		// Extract the 3x3 rotation matrix
		var m = rotation.Matrix;

		// Apply perspective by setting Persp0/Persp1 on the 3x3 matrix.
		// After Y-rotation, a point at (x,0) in 2D corresponds to 3D point
		// (x*cosY, 0, -x*sinY). The perspective divide needs w = 1 + z/d.
		// Since z = -x*sinY in the rotated space, we set Persp0 = sinY/d
		// so that w = Persp0*x + Persp2 = (sinY/d)*x + 1.
		// Similarly for X-rotation affecting y: Persp1 = sinX/d.
		if (projectionIndex == 1 && perspDepth > 0)
		{
			var radY = rotateY * MathF.PI / 180f;
			var radX = rotateX * MathF.PI / 180f;
			m.Persp0 = MathF.Sin(radY) / perspDepth;
			m.Persp1 = -MathF.Sin(radX) * MathF.Cos(radY) / perspDepth;
		}

		// Apply Z translation as uniform scale (perspective-correct)
		if (translateZ != 0 && projectionIndex == 1 && perspDepth > 0)
		{
			var zScale = perspDepth / (perspDepth + translateZ);
			if (zScale > 0.01f)
			{
				m.ScaleX *= zScale;
				m.SkewX *= zScale;
				m.SkewY *= zScale;
				m.ScaleY *= zScale;
			}
		}

		// Draw 3D axes in screen space (before applying the full transform)
		if (showAxes)
		{
			// Map axis endpoints through the same perspective matrix
			var axisM = rotation.Matrix;
			if (projectionIndex == 1 && perspDepth > 0)
			{
				var radY = rotateY * MathF.PI / 180f;
				var radX = rotateX * MathF.PI / 180f;
				axisM.Persp0 = MathF.Sin(radY) / perspDepth;
				axisM.Persp1 = -MathF.Sin(radX) * MathF.Cos(radY) / perspDepth;
			}
			DrawAxes(canvas, axisM, size * 1.5f);
		}

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

	private static void DrawAxes(SKCanvas canvas, SKMatrix m, float axisLength)
	{
		// Map axis endpoints through the perspective-aware 3x3 matrix
		var xEnd = m.MapPoint(axisLength, 0);
		var yEnd = m.MapPoint(0, axisLength);
		// Z axis: in the 3x3, Z maps through Persp0/Persp1. 
		// We approximate by mapping a small offset in the "depth" direction
		var zEnd = new SKPoint(
			-m.SkewX * axisLength,   // Z component mapped through the rotation
			-m.TransY * 0);          // approximate
		// Better: use the 4x4 rotation info embedded in the matrix
		// For a Y-rotated Z axis, it points in the (-sinY, 0) direction in 2D
		// For general rotation, derive from matrix columns
		// Col 3 of the 4x4 = (m02, m12, m22, m32) but we don't have those.
		// Just draw a point at the origin for Z.
		// Actually we can reconstruct: if the matrix is rotation*perspective,
		// then the Z axis direction is approximately (-SkewX, -SkewY) normalized
		// But that's not quite right either. Skip Z axis for now.

		var origin = SKPoint.Empty;

		// X axis (red)
		using var axisPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2,
			IsAntialias = true,
		};
		axisPaint.Color = new SKColor(239, 68, 68);
		canvas.DrawLine(origin, xEnd, axisPaint);

		using var font = new SKFont { Size = 14 };
		using var textPaint = new SKPaint { IsAntialias = true };
		textPaint.Color = axisPaint.Color;
		canvas.DrawText("X", xEnd.X + 5, xEnd.Y + 5, font, textPaint);

		// Y axis (green)
		axisPaint.Color = new SKColor(34, 197, 94);
		canvas.DrawLine(origin, yEnd, axisPaint);
		textPaint.Color = axisPaint.Color;
		canvas.DrawText("Y", yEnd.X + 5, yEnd.Y + 5, font, textPaint);
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
