using System;
using System.Collections.Generic;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

using SkiaSharp;
using SkiaSharp.Views.Tizen;
using SKCanvasView = SkiaSharp.Views.Tizen.NUI.SKCanvasView;

namespace SkiaSharpSample;

public class DrawingPage : View
{
	record struct Stroke(SKPath Path, SKColor Color, float Width);

	static readonly SKColor[] palette =
	{
		SKColors.Black,
		new SKColor(0xE5, 0x39, 0x35), // red
		new SKColor(0x1E, 0x88, 0xE5), // blue
		new SKColor(0x43, 0xA0, 0x47), // green
		new SKColor(0xFB, 0x8C, 0x00), // orange
		new SKColor(0x8E, 0x24, 0xAA), // purple
	};

	readonly List<Stroke> strokes = new();
	readonly SKCanvasView skiaView;
	readonly FpsCounter fpsCounter = new();
	readonly TextLabel fpsLabel;

	SKPath? currentPath;
	SKColor currentColor = SKColors.Black;
	float brushSize = 6f;
	bool isDrawing;
	View? selectedSwatch;

	public DrawingPage()
	{
		WidthSpecification = LayoutParamPolicies.MatchParent;
		HeightSpecification = LayoutParamPolicies.MatchParent;
		Layout = new LinearLayout
		{
			LinearOrientation = LinearLayout.Orientation.Vertical,
		};

		// Canvas area (fills remaining space)
		var canvasContainer = new View
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = 0,
			Weight = 1f,
		};

		skiaView = new SKCanvasView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.TouchEvent += OnDrawingTouch;

		fpsLabel = new TextLabel
		{
			Text = "FPS: --",
			TextColor = new Tizen.NUI.Color(0.3f, 0.3f, 0.3f, 1f),
			PointSize = 10,
			Position = new Position(10, 10),
		};

		canvasContainer.Add(skiaView);
		canvasContainer.Add(fpsLabel);

		Add(canvasContainer);
		Add(CreateToolbar());

		fpsCounter.Start();
	}

	View CreateToolbar()
	{
		var toolbar = new View
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = 60,
			BackgroundColor = new Tizen.NUI.Color(0.95f, 0.95f, 0.95f, 1f),
			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Horizontal,
				CellPadding = new Size2D(8, 0),
				LinearAlignment = LinearLayout.Alignment.CenterVertical,
				Padding = new Extents(12, 12, 0, 0),
			},
		};

		bool first = true;
		foreach (var color in palette)
		{
			var swatch = new View
			{
				Size = new Size(36, 36),
				BackgroundColor = new Tizen.NUI.Color(
					color.Red / 255f, color.Green / 255f, color.Blue / 255f, 1f),
				CornerRadius = 18f,
				BorderlineWidth = first ? 3f : 0f,
				BorderlineColor = new Tizen.NUI.Color(0.2f, 0.5f, 1f, 1f),
			};

			if (first)
			{
				selectedSwatch = swatch;
				first = false;
			}

			var capturedColor = color;
			swatch.TouchEvent += (s, e) =>
			{
				if (e.Touch.GetState(0) != PointStateType.Down)
					return true;

				currentColor = capturedColor;

				if (selectedSwatch != null)
					selectedSwatch.BorderlineWidth = 0f;
				if (s is View v)
				{
					v.BorderlineWidth = 3f;
					v.BorderlineColor = new Tizen.NUI.Color(0.2f, 0.5f, 1f, 1f);
					selectedSwatch = v;
				}
				return true;
			};

			toolbar.Add(swatch);
		}

		// Spacer
		toolbar.Add(new View { WidthSpecification = 0, Weight = 1f, HeightSpecification = 1 });

		// Clear button
		var clearBtn = new Button { Text = "Clear", Size = new Size(80, 40) };
		clearBtn.Clicked += (s, e) =>
		{
			foreach (var stroke in strokes)
				stroke.Path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			isDrawing = false;
			skiaView.Invalidate();
		};
		toolbar.Add(clearBtn);

		return toolbar;
	}

	void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(SKColors.White);

		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		foreach (var stroke in strokes)
		{
			paint.Color = stroke.Color;
			paint.StrokeWidth = stroke.Width;
			canvas.DrawPath(stroke.Path, paint);
		}

		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}

		if (fpsCounter.Tick() is double fps)
			fpsLabel.Text = $"FPS: {fps:F0}";
	}

	bool OnDrawingTouch(object sender, View.TouchEventArgs e)
	{
		var touch = e.Touch;
		if (touch.GetPointCount() < 1)
			return true;

		var state = touch.GetState(0);
		var pos = touch.GetLocalPosition(0);

		switch (state)
		{
			case PointStateType.Down:
				isDrawing = true;
				currentPath = new SKPath();
				currentPath.MoveTo(pos.X, pos.Y);
				break;

			case PointStateType.Motion:
				if (isDrawing && currentPath != null)
					currentPath.LineTo(pos.X, pos.Y);
				break;

			case PointStateType.Up:
			case PointStateType.Leave:
				if (isDrawing && currentPath != null)
				{
					strokes.Add(new Stroke(currentPath, currentColor, brushSize));
					currentPath = null;
					isDrawing = false;
				}
				break;
		}

		skiaView.Invalidate();
		return true;
	}
}
