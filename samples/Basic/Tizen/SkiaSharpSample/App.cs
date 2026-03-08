using System;
using System.Collections.Generic;
using ElmSharp;
using Tizen.Applications;

using SkiaSharp;
using SkiaSharp.Views.Tizen;

namespace SkiaSharpSample
{
	public class App : CoreUIApplication
	{
		public static SamplePage DefaultPage { get; set; } = SamplePage.Cpu;

		// Static data shared across pages
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

		private static readonly float[] blobColors =
		{
			1.0f, 0.3f, 0.4f,    // hot pink
			0.3f, 0.7f, 1.0f,    // sky blue
			1.0f, 0.6f, 0.1f,    // orange
			0.4f, 1.0f, 0.7f,    // mint
			0.7f, 0.3f, 1.0f,    // purple
			1.0f, 0.9f, 0.2f,    // yellow
		};

		private static readonly (SKColor Light, SKColor Dark)[] StrokeColorPalette =
		{
			(SKColors.Black, SKColors.White),
			(new SKColor(0xE5, 0x39, 0x35), new SKColor(0xEF, 0x53, 0x50)),
			(new SKColor(0x1E, 0x88, 0xE5), new SKColor(0x42, 0xA5, 0xF5)),
			(new SKColor(0x43, 0xA0, 0x47), new SKColor(0x66, 0xBB, 0x6A)),
			(new SKColor(0xFB, 0x8C, 0x00), new SKColor(0xFF, 0xA7, 0x26)),
			(new SKColor(0x8E, 0x24, 0xAA), new SKColor(0xAB, 0x47, 0xBC)),
		};

		private static readonly string[] StrokeColorNames =
			{ "Black", "Red", "Blue", "Green", "Orange", "Purple" };

		private static bool IsDarkMode
		{
			get
			{
				try
				{
					var profile = Elementary.GetProfile();
					return profile?.Contains("dark", StringComparison.OrdinalIgnoreCase) ?? false;
				}
				catch { return false; }
			}
		}

		private static SKColor CanvasBackground => IsDarkMode ? new SKColor(0x11, 0x13, 0x18) : SKColors.White;

		// Readonly instance fields
		private readonly FpsCounter fpsCounter = new();
		private readonly List<(SKPath Path, SKColor Color, float Width)> drawingStrokes = new();

		// Instance fields
		private Window window;
		private Naviframe naviframe;
		private NaviItem mainNaviItem;

		// GPU page state
		private Lazy<SKRuntimeShaderBuilder> shaderBuilder;

		// Drawing page state
		private SKPath currentDrawingPath;
		private int drawingColorIndex;
		private SKCanvasView drawingCanvas;

		private SKColor GetStrokeColor(int index) =>
			IsDarkMode ? StrokeColorPalette[index].Dark : StrokeColorPalette[index].Light;

		public static void Main(string[] args)
		{
			Elementary.Initialize();
			Elementary.ThemeOverlay();

			var app = new App();
			app.Run(args);
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Initialize();
		}

		private void Initialize()
		{
			window = new Window("SkiaSharp");
			window.BackButtonPressed += OnBackButtonPressed;
			window.AvailableRotations =
				DisplayRotation.Degree_0 | DisplayRotation.Degree_180 |
				DisplayRotation.Degree_270 | DisplayRotation.Degree_90;
			window.Show();

			var conformant = new Conformant(window);
			conformant.Show();

			naviframe = new Naviframe(window);
			naviframe.Show();
			conformant.SetContent(naviframe);

			ShowMainMenu();

			if (DefaultPage == SamplePage.Gpu)
				ShowGpuPage();
		}

		private void ShowMainMenu()
		{
			var box = new Box(window)
			{
				IsHorizontal = false,
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			box.Show();

			var title = new Label(window)
			{
				Text = "<font_size=28><b>SkiaSharp Demos</b></font_size>",
				AlignmentX = -1,
				WeightX = 1,
			};
			title.Show();
			box.PackEnd(title);

			var btn1 = new Button(window)
			{
				Text = "CPU Canvas — Gradient & Circles",
				AlignmentX = -1,
				WeightX = 1,
			};
			btn1.Clicked += (s, e) => ShowCpuCanvasPage();
			btn1.Show();
			box.PackEnd(btn1);

			var btn2 = new Button(window)
			{
				Text = "GPU (GL) — SkSL Metaball Shader",
				AlignmentX = -1,
				WeightX = 1,
			};
			btn2.Clicked += (s, e) => ShowGpuPage();
			btn2.Show();
			box.PackEnd(btn2);

			var btn3 = new Button(window)
			{
				Text = "Drawing — Multi-Stroke Touch",
				AlignmentX = -1,
				WeightX = 1,
			};
			btn3.Clicked += (s, e) => ShowDrawingPage();
			btn3.Show();
			box.PackEnd(btn3);

			mainNaviItem = naviframe.Push(box, "SkiaSharp");
		}

		// ===== Page 1: CPU Canvas =====

		private void ShowCpuCanvasPage()
		{
			var skiaView = new SKCanvasView(window);
			skiaView.IgnorePixelScaling = true;
			skiaView.PaintSurface += OnPaintCpuCanvas;
			skiaView.Show();

			naviframe.Push(skiaView, "CPU Canvas");
		}

		private void OnPaintCpuCanvas(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.Info.Width;
			var height = e.Info.Height;
			var center = new SKPoint(width / 2f, height / 2f);
			var radius = Math.Max(width, height) / 2f;

			canvas.Clear(SKColors.White);

			// Background gradient
			using var shader = SKShader.CreateRadialGradient(center, radius, gradientColors, SKShaderTileMode.Clamp);
			using var bgPaint = new SKPaint
			{
				IsAntialias = true,
				Shader = shader,
			};
			canvas.DrawRect(0, 0, width, height, bgPaint);

			// Circles
			using var circlePaint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
			};
			foreach (var (x, y, r, color) in circles)
			{
				circlePaint.Color = color;
				canvas.DrawCircle(x * width, y * height, r * Math.Min(width, height), circlePaint);
			}

			// Centered text
			using var textPaint = new SKPaint
			{
				Color = SKColors.White,
				IsAntialias = true,
			};
			using var font = new SKFont { Size = width * 0.12f };
			canvas.DrawText("SkiaSharp", center.X, center.Y + font.Size / 3f, SKTextAlign.Center, font, textPaint);
		}

		// ===== Page 2: GPU (GL) =====

		private const string MetaballShaderSource = @"
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
            0.5 + 0.4 * cos(t * speed * 0.8 + phase * 1.3) * sin(t * speed * 0.4 + fi * 0.7));
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
}";

		private void ShowGpuPage()
		{
			SKGLSurfaceView glView;
			try
			{
				glView = new SKGLSurfaceView(window);
			}
			catch (Exception ex)
			{
				ShowFallbackPage("GPU (GL)", $"SKGLSurfaceView not available:\n{ex.Message}");
				return;
			}

			glView.RenderingMode = RenderingMode.Continuously;
			glView.PaintSurface += OnPaintGpuSurface;
			glView.Show();

			shaderBuilder = new Lazy<SKRuntimeShaderBuilder>(() => SKRuntimeEffect.BuildShader(MetaballShaderSource));
			fpsCounter.Start();
			naviframe.Push(glView, "GPU (GL)");
		}

		private void OnPaintGpuSurface(object sender, SKPaintGLSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			var width = e.BackendRenderTarget.Width;
			var height = e.BackendRenderTarget.Height;

			var builder = shaderBuilder.Value;
			builder.Uniforms["iTime"] = fpsCounter.ElapsedSeconds;
			builder.Uniforms["iResolution"] = new float[] { (float)width, (float)height };
			builder.Uniforms["iTouchPos"] = new float[] { 0f, 0f };
			builder.Uniforms["iTouchActive"] = 0f;
			builder.Uniforms["iColors"] = blobColors;

			using var shader = builder.Build();
			using var paint = new SKPaint { Shader = shader };
			canvas.DrawRect(0, 0, width, height, paint);

			fpsCounter.Tick();
		}

		private void ShowFallbackPage(string title, string message)
		{
			var skiaView = new SKCanvasView(window);
			skiaView.IgnorePixelScaling = true;
			skiaView.PaintSurface += (s, e) =>
			{
				var canvas = e.Surface.Canvas;
				canvas.Clear(new SKColor(0x33, 0x33, 0x33));

				using var textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
				using var font = new SKFont { Size = 24 };
				canvas.DrawText(message, e.Info.Width / 2f, e.Info.Height / 2f,
					SKTextAlign.Center, font, textPaint);
			};
			skiaView.Show();
			naviframe.Push(skiaView, title);
		}

		// ===== Page 3: Drawing =====

		private void ShowDrawingPage()
		{
			ClearDrawing();

			var box = new Box(window)
			{
				IsHorizontal = false,
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			box.Show();

			// Toolbar
			var toolbar = new Box(window)
			{
				IsHorizontal = true,
				AlignmentX = -1,
				WeightX = 1,
			};
			toolbar.Show();

			var clearBtn = new Button(window) { Text = "Clear" };
			clearBtn.Clicked += (s, e) => ClearDrawing();
			clearBtn.Show();
			toolbar.PackEnd(clearBtn);

			var colorBtn = new Button(window) { Text = "Color: Black" };
			colorBtn.Clicked += (s, e) =>
			{
				drawingColorIndex = (drawingColorIndex + 1) % StrokeColorPalette.Length;
				colorBtn.Text = $"Color: {StrokeColorNames[drawingColorIndex]}";
			};
			colorBtn.Show();
			toolbar.PackEnd(colorBtn);

			box.PackEnd(toolbar);

			// Canvas
			drawingCanvas = new SKCanvasView(window)
			{
				IgnorePixelScaling = true,
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			drawingCanvas.PaintSurface += OnPaintDrawing;
			drawingCanvas.Show();

			// Touch handling via GestureLayer (the standard ElmSharp touch mechanism)
			var gestureLayer = new GestureLayer(drawingCanvas);
			gestureLayer.Attach(drawingCanvas);

			gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Start, data =>
			{
				currentDrawingPath = new SKPath();
				currentDrawingPath.MoveTo(data.X2, data.Y2);
				drawingStrokes.Add((currentDrawingPath, GetStrokeColor(drawingColorIndex), 4f));
			});

			gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, data =>
			{
				currentDrawingPath?.LineTo(data.X2, data.Y2);
				drawingCanvas.Invalidate();
			});

			gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, data =>
			{
				currentDrawingPath = null;
				drawingCanvas.Invalidate();
			});

			gestureLayer.SetTapCallback(GestureLayer.GestureType.Tap, GestureLayer.GestureState.End, data =>
			{
				var dotPath = new SKPath();
				dotPath.AddCircle(data.X, data.Y, 4);
				drawingStrokes.Add((dotPath, GetStrokeColor(drawingColorIndex), 4f));
				drawingCanvas.Invalidate();
			});

			box.PackEnd(drawingCanvas);
			naviframe.Push(box, "Drawing");
		}

		private void OnPaintDrawing(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(CanvasBackground);

			using var paint = new SKPaint
			{
				IsAntialias = true,
				Style = SKPaintStyle.Stroke,
				StrokeCap = SKStrokeCap.Round,
				StrokeJoin = SKStrokeJoin.Round,
			};

			foreach (var (path, color, strokeWidth) in drawingStrokes)
			{
				paint.Color = color;
				paint.StrokeWidth = strokeWidth;
				canvas.DrawPath(path, paint);
			}
		}

		private void ClearDrawing()
		{
			foreach (var (path, _, _) in drawingStrokes)
				path.Dispose();
			drawingStrokes.Clear();
			currentDrawingPath = null;
			drawingCanvas?.Invalidate();
		}

		// ===== Navigation =====

		private void OnBackButtonPressed(object sender, EventArgs e)
		{
			if (naviframe.TopItem != mainNaviItem)
			{
				ClearDrawing();
				if (shaderBuilder?.IsValueCreated == true)
					shaderBuilder.Value.Dispose();
				shaderBuilder = null;
				fpsCounter.Stop();
				naviframe.Pop();
			}
			else
			{
				Exit();
			}
		}
	}
}
