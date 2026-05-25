using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample.Samples;

public class ThreeDSample : CanvasSampleBase
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

	public override string Description => "Visualize 3D transformations with per-axis rotation, perspective projection, and translate controls.";

	public override IReadOnlyList<SampleControl> Controls =>
	[
		new PickerControl("projection", "Projection", Projections, projectionIndex),
		new SliderControl("perspDepth", "Camera Distance", 100, 2000, perspDepth, Description: "Distance from virtual camera. Lower values create stronger perspective."),
		new SliderControl("rotateX", "Rotate X", -180, 180, rotateX),
		new SliderControl("rotateY", "Rotate Y", -180, 180, rotateY),
		new SliderControl("rotateZ", "Rotate Z", -180, 180, rotateZ),
		new SliderControl("scale", "Scale", 0.2f, 3, scale, 0.1f),
		new SliderControl("translateX", "Translate X", -300, 300, translateX),
		new SliderControl("translateY", "Translate Y", -300, 300, translateY),
		new SliderControl("translateZ", "Translate Z", -500, 500, translateZ, Description: "Moves the object toward or away from the camera."),
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
		var size = Math.Min(width, height) * 0.25f * scale;

		// Draw grid background
		DrawGrid(canvas, width, height, cx, cy);

		// Build 4x4 rotation — intrinsic (object-local) axes
		// Each slider rotates around the object's own axis, not world space.
		// Intrinsic X-Y-Z = extrinsic Z-Y-X = multiply as rx * ry * rz
		var rx = SKMatrix44.CreateRotationDegrees(1, 0, 0, rotateX);
		var ry = SKMatrix44.CreateRotationDegrees(0, 1, 0, rotateY);
		var rz = SKMatrix44.CreateRotationDegrees(0, 0, 1, rotateZ);
		var rotation = rx * ry * rz;

		canvas.Save();

		// 1. Move to canvas center + user translation
		canvas.Translate(cx + translateX, cy + translateY);

		// 2. Apply perspective via native 4x4 canvas.Concat(SKMatrix44)
		if (projectionIndex == 1 && perspDepth > 0)
		{
			var m4 = System.Numerics.Matrix4x4.Identity;
			m4.M34 = -1f / perspDepth;
			canvas.Concat((SKMatrix44)m4);
		}

		// 3. Apply Z translation via 4x4
		if (translateZ != 0)
		{
			var tz = System.Numerics.Matrix4x4.CreateTranslation(0, 0, translateZ);
			canvas.Concat((SKMatrix44)tz);
		}

		// 4. Apply rotation via 4x4
		canvas.Concat(rotation);

		// Draw 3D axes at origin (they'll go through the full 4x4 pipeline)
		if (showAxes)
			DrawAxes(canvas, size * 1.5f);

		// Draw the shape at Z=0
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
		using var dashEffect = SKPathEffect.CreateDash(new float[] { 6, 4 }, 0);
		gridPaint.PathEffect = dashEffect;
		canvas.DrawLine(cx, 0, cx, height, gridPaint);
		canvas.DrawLine(0, cy, width, cy, gridPaint);
	}

	private static void DrawAxes(SKCanvas canvas, float axisLength)
	{
		// Draw axes at origin — they'll be transformed by the 4x4 pipeline
		var origin = SKPoint.Empty;

		using var axisPaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = 2,
			IsAntialias = true,
		};
		using var font = new SKFont { Size = 14 };
		using var textPaint = new SKPaint { IsAntialias = true };

		// X axis (red) — draw along +X
		axisPaint.Color = new SKColor(239, 68, 68);
		canvas.DrawLine(0, 0, axisLength, 0, axisPaint);
		textPaint.Color = axisPaint.Color;
		canvas.DrawText("X", axisLength + 5, 5, font, textPaint);

		// Y axis (green) — draw along +Y
		axisPaint.Color = new SKColor(34, 197, 94);
		canvas.DrawLine(0, 0, 0, axisLength, axisPaint);
		textPaint.Color = axisPaint.Color;
		canvas.DrawText("Y", 5, axisLength + 15, font, textPaint);
	}

	private void DrawMatrixInfo(SKCanvas canvas, SKMatrix44 rotation)
	{
		var boxW = 320f;
		var boxH = 130f;

		using var bgPaint = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(0, 0, 0, 200),
		};
		canvas.DrawRoundRect(new SKRoundRect(new SKRect(8, 8, 8 + boxW, 8 + boxH), 8, 8), bgPaint);

		using var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
		using var font = new SKFont(SKTypeface.Default, 11);
		using var headerFont = new SKFont(SKTypeface.Default, 11);

		canvas.DrawText("4×4 Transform Matrix:", 14, 22, headerFont, textPaint);

		textPaint.Color = new SKColor(180, 180, 180);
		var startY = 38f;
		var lineH = 16f;
		for (var row = 0; row < 4; row++)
		{
			var line = $"│ {rotation[row, 0],7:F3} {rotation[row, 1],7:F3} {rotation[row, 2],7:F3} {rotation[row, 3],7:F3} │";
			canvas.DrawText(line, 14, startY + row * lineH, font, textPaint);
		}

		textPaint.Color = new SKColor(120, 120, 120);
		font.Size = 10;
		var info = $"X:{rotateX:F1}°  Y:{rotateY:F1}°  Z:{rotateZ:F1}°  S:{scale:F1}  Z:{translateZ:F0}  {Projections[projectionIndex]}";
		if (projectionIndex == 1)
			info += $"  d:{perspDepth:F0}";
		canvas.DrawText(info, 14, startY + 4 * lineH + 4, font, textPaint);
	}
}
