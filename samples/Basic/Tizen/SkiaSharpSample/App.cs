using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tizen.NUI;
using Tizen.NUI.BaseComponents;
using Tizen.NUI.Components;

using SkiaSharp;
using SkiaSharp.Views.Tizen;
using SKCanvasView = SkiaSharp.Views.Tizen.NUI.SKCanvasView;
using SKGLSurfaceView = SkiaSharp.Views.Tizen.NUI.SKGLSurfaceView;

namespace SkiaSharpSample;

record struct Stroke(SKPath Path, SKColor Color, float Width);

public class App : NUIApplication
{
	// ── CPU tab data ────────────────────────────────────────────────

	static readonly (float X, float Y, float R, SKColor Color)[] circles =
	{
		(0.20f, 0.30f, 0.10f, new SKColor(0xFF, 0x4D, 0x66, 0xCC)),
		(0.75f, 0.25f, 0.08f, new SKColor(0x4D, 0xB3, 0xFF, 0xCC)),
		(0.15f, 0.70f, 0.07f, new SKColor(0xFF, 0x99, 0x1A, 0xCC)),
		(0.80f, 0.70f, 0.12f, new SKColor(0x66, 0xFF, 0xB3, 0xCC)),
		(0.50f, 0.15f, 0.06f, new SKColor(0xB3, 0x4D, 0xFF, 0xCC)),
		(0.40f, 0.80f, 0.09f, new SKColor(0xFF, 0xE6, 0x33, 0xCC)),
	};

	static readonly SKColor[] gradientColors =
	{
		new SKColor(0x44, 0x88, 0xFF),
		new SKColor(0x88, 0x33, 0xCC),
	};

	// ── GPU tab data ────────────────────────────────────────────────

	const string SkslSource = @"
uniform float iTime;
uniform float2 iResolution;
uniform float2 iTouchPos;
uniform float iTouchActive;
uniform float3 iColors[6];

half4 main(float2 fragCoord) {
	float2 uv = fragCoord / iResolution;
	float aspect = iResolution.x / iResolution.y;
	float2 st = float2(uv.x * aspect, uv.y);
	float t = iTime;
	float field = 0.0;
	float3 weighted = float3(0.0);
	for (int i = 0; i < 6; i++) {
		float fi = float(i);
		float phase = fi * 1.047;
		float speed = 0.3 + fi * 0.07;
		float2 center = float2(
			aspect * 0.5 + 0.4 * sin(t * speed + phase) * cos(t * speed * 0.6 + fi),
			0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7)
		);
		float2 d = st - center;
		float r = length(d);
		float strength = 0.030 / (r * r + 0.002);
		field += strength;
		weighted += iColors[i] * strength;
	}
	if (iTouchActive > 0.5) {
		float2 touchSt = float2(iTouchPos.x * aspect, iTouchPos.y);
		float2 d = st - touchSt;
		float r = length(d);
		float strength = 0.050 / (r * r + 0.002);
		field += strength;
		weighted += float3(1.0, 0.95, 0.9) * strength;
	}
	float3 blobColor = weighted / max(field, 0.001);
	float edge = smoothstep(5.0, 8.0, field);
	float innerGlow = smoothstep(8.0, 20.0, field) * 0.3;
	float3 bg = float3(0.03, 0.02, 0.08);
	bg += float3(0.02, 0.01, 0.03) * sin(t * 0.2 + uv.y * 3.0);
	float halo = smoothstep(3.0, 5.0, field) * (1.0 - edge);
	float3 result = bg;
	result += blobColor * halo * 0.4;
	result = mix(result, blobColor * (1.0 + innerGlow), edge);
	float2 vc = uv - 0.5;
	float vignette = 1.0 - dot(vc, vc) * 0.8;
	result *= vignette;
	return half4(clamp(result, 0.0, 1.0), 1.0);
}
";

	static readonly float[] blobColors =
	{
		1.0f, 0.3f, 0.4f,   // rose
		0.3f, 0.7f, 1.0f,   // sky blue
		1.0f, 0.6f, 0.1f,   // amber
		0.4f, 1.0f, 0.7f,   // mint
		0.7f, 0.3f, 1.0f,   // violet
		1.0f, 0.9f, 0.2f,   // yellow
	};

	readonly Stopwatch gpuClock = new();
	readonly SKPaint shaderPaint = new();
	SKRuntimeShaderBuilder? shaderBuilder;
	SKPoint gpuTouchPos;
	bool gpuTouchActive;
	Timer? gpuTimer;
	SKGLSurfaceView? gpuView;
	TextLabel? fpsLabel;
	int gpuFrameCount;
	double gpuLastFpsTime;

	// ── Drawing tab data ────────────────────────────────────────────

	static readonly SKColor[] drawingColors =
	{
		SKColors.Black,
		new SKColor(0xE5, 0x39, 0x35), // red
		new SKColor(0x1E, 0x88, 0xE5), // blue
		new SKColor(0x43, 0xA0, 0x47), // green
		new SKColor(0xFB, 0x8C, 0x00), // orange
		new SKColor(0x8E, 0x24, 0xAA), // purple
	};

	readonly List<Stroke> strokes = new();
	SKPath? currentPath;
	SKColor currentColor = SKColors.Black;
	float brushSize = 6f;
	bool isDrawing;
	SKCanvasView? drawingCanvas;
	View? selectedSwatch;

	// ── App lifecycle ───────────────────────────────────────────────

	public static void Main(string[] args) => new App().Run(args);

	protected override void OnCreate()
	{
		base.OnCreate();

		GetDefaultWindow().KeyEvent += OnKeyEvent;

		var tabView = new TabView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};

		tabView.AddTab(new TabButton { Text = "CPU" }, CreateCpuContent());
		tabView.AddTab(new TabButton { Text = "GPU" }, CreateGpuContent());
		tabView.AddTab(new TabButton { Text = "Drawing" }, CreateDrawingContent());

		GetDefaultWindow().Add(tabView);

		// Build the SkSL shader (may fail on some devices)
		try
		{
			shaderBuilder = SKRuntimeEffect.BuildShader(SkslSource);
		}
		catch
		{
			// Shader compilation not supported — GPU tab will show fallback
		}

		// Start the GPU animation clock and timer
		gpuClock.Start();
		gpuTimer = new Timer(16); // ~60 fps
		gpuTimer.Tick += (s, e) =>
		{
			gpuView?.Invalidate();
			return true;
		};
		gpuTimer.Start();

		// Kick initial render for all views after layout completes
		var initTimer = new Timer(100);
		initTimer.Tick += (s, e) =>
		{
			gpuView?.Invalidate();
			drawingCanvas?.Invalidate();
			return false;
		};
		initTimer.Start();
	}

	// ── CPU tab ─────────────────────────────────────────────────────

	View CreateCpuContent()
	{
		var canvas = new SKCanvasView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};
		canvas.PaintSurface += OnCpuPaintSurface;
		return canvas;
	}

	void OnCpuPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;
		var center = new SKPoint(width / 2f, height / 2f);
		var radius = Math.Max(width, height) / 2f;

		canvas.Clear(SKColors.White);

		// Gradient background
		using var shader = SKShader.CreateRadialGradient(
			center, radius, gradientColors, SKShaderTileMode.Clamp);
		using var bgPaint = new SKPaint { IsAntialias = true, Shader = shader };
		canvas.DrawRect(0, 0, width, height, bgPaint);

		// Translucent circles
		using var circlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
		foreach (var (x, y, r, color) in circles)
		{
			circlePaint.Color = color;
			canvas.DrawCircle(x * width, y * height, r * Math.Min(width, height), circlePaint);
		}

		// Centered title text
		using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
		using var font = new SKFont { Size = width * 0.12f };
		canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f,
			SKTextAlign.Center, font, textPaint);
	}

	// ── GPU tab ─────────────────────────────────────────────────────

	View CreateGpuContent()
	{
		var container = new View
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};

		gpuView = new SKGLSurfaceView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
		};
		gpuView.PaintSurface += OnGpuPaintSurface;
		gpuView.TouchEvent += OnGpuTouch;

		fpsLabel = new TextLabel
		{
			Text = "FPS: --",
			TextColor = Tizen.NUI.Color.White,
			PointSize = 10,
			PositionUsesPivotPoint = true,
			ParentOrigin = ParentOrigin.TopLeft,
			PivotPoint = PivotPoint.TopLeft,
			Position = new Position(10, 10),
		};

		container.Add(gpuView);
		container.Add(fpsLabel);
		return container;
	}

	void OnGpuPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		var width = e.Info.Width;
		var height = e.Info.Height;

		if (shaderBuilder == null)
		{
			// Fallback when SkSL is not available
			canvas.Clear(new SKColor(0x08, 0x05, 0x14));
			using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
			using var font = new SKFont { Size = width * 0.06f };
			canvas.DrawText("SkSL shader not supported",
				width / 2f, height / 2f, SKTextAlign.Center, font, paint);
			return;
		}

		shaderBuilder.Uniforms["iTime"] = (float)gpuClock.Elapsed.TotalSeconds;
		shaderBuilder.Uniforms["iResolution"] = new float[] { width, height };
		shaderBuilder.Uniforms["iTouchPos"] = new float[] { gpuTouchPos.X, gpuTouchPos.Y };
		shaderBuilder.Uniforms["iTouchActive"] = gpuTouchActive ? 1f : 0f;
		shaderBuilder.Uniforms["iColors"] = blobColors;

		using var shader = shaderBuilder.Build();
		shaderPaint.Shader = shader;
		canvas.DrawRect(0, 0, width, height, shaderPaint);

		// Update FPS counter
		gpuFrameCount++;
		var now = gpuClock.Elapsed.TotalSeconds;
		if (now - gpuLastFpsTime >= 0.5)
		{
			var fps = gpuFrameCount / (now - gpuLastFpsTime);
			gpuFrameCount = 0;
			gpuLastFpsTime = now;
			if (fpsLabel != null)
				fpsLabel.Text = $"FPS: {fps:F0}";
		}
	}

	bool OnGpuTouch(object sender, View.TouchEventArgs e)
	{
		var touch = e.Touch;
		if (touch.GetPointCount() < 1)
			return true;

		var state = touch.GetState(0);
		var pos = touch.GetLocalPosition(0);
		var view = sender as View;
		var w = view?.Size.Width ?? 1f;
		var h = view?.Size.Height ?? 1f;

		switch (state)
		{
			case PointStateType.Down:
			case PointStateType.Motion:
				gpuTouchActive = true;
				gpuTouchPos = new SKPoint(pos.X / w, pos.Y / h);
				break;
			case PointStateType.Up:
			case PointStateType.Leave:
				gpuTouchActive = false;
				break;
		}

		return true;
	}

	// ── Drawing tab ─────────────────────────────────────────────────

	View CreateDrawingContent()
	{
		var container = new View
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = LayoutParamPolicies.MatchParent,
			Layout = new LinearLayout
			{
				LinearOrientation = LinearLayout.Orientation.Vertical,
			},
		};

		drawingCanvas = new SKCanvasView
		{
			WidthSpecification = LayoutParamPolicies.MatchParent,
			HeightSpecification = 0,
			Weight = 1f,
		};
		drawingCanvas.PaintSurface += OnDrawingPaintSurface;
		drawingCanvas.TouchEvent += OnDrawingTouch;

		container.Add(drawingCanvas);
		container.Add(CreateDrawingToolbar());
		return container;
	}

	View CreateDrawingToolbar()
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

		// Color swatches
		bool first = true;
		foreach (var color in drawingColors)
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

				// Update selection indicator
				if (selectedSwatch != null)
				{
					selectedSwatch.BorderlineWidth = 0f;
				}
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
		var clearBtn = new Button
		{
			Text = "Clear",
			Size = new Size(80, 40),
		};
		clearBtn.Clicked += (s, e) =>
		{
			foreach (var stroke in strokes)
				stroke.Path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			isDrawing = false;
			drawingCanvas?.Invalidate();
		};
		toolbar.Add(clearBtn);

		return toolbar;
	}

	void OnDrawingPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
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

		// Draw completed strokes
		foreach (var stroke in strokes)
		{
			paint.Color = stroke.Color;
			paint.StrokeWidth = stroke.Width;
			canvas.DrawPath(stroke.Path, paint);
		}

		// Draw current in-progress stroke
		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = brushSize;
			canvas.DrawPath(currentPath, paint);
		}
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

		drawingCanvas?.Invalidate();
		return true;
	}

	// ── Navigation ──────────────────────────────────────────────────

	void OnKeyEvent(object? sender, Window.KeyEventArgs e)
	{
		if (e.Key.State == Key.StateType.Down &&
			(e.Key.KeyPressedName == "XF86Back" || e.Key.KeyPressedName == "Escape"))
		{
			Exit();
		}
	}
}
