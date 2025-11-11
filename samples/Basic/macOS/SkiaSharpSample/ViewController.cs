using System;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;

using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	public class ViewController : NSViewController
	{
		private SKGLView? _skiaView;
		private readonly MotionMarkScene _scene = new();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private double _lastTime;
		private int _frameCount;
		private double _accumulatedTime;

		public ViewController()
		{
		}

		public override void LoadView()
		{
			// Create the view programmatically (no storyboard)
			View = new NSView(new CGRect(0, 0, 1280, 720));
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Create SKGLView programmatically
			_skiaView = new SKGLView(View.Bounds)
			{
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
			};

			View.AddSubview(_skiaView);

			// Set initial complexity (matching C++ default)
			_scene.SetComplexity(8);

			_skiaView.PaintSurface += OnPaintSurface;

			_lastTime = _stopwatch.Elapsed.TotalSeconds;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_scene?.Dispose();
			}
			base.Dispose(disposing);
		}

		public void RenderFrame()
		{
			if (_skiaView == null)
				return;

			// Direct render call (like C++ Window::onPaint)
			// This forces SKGLView to render immediately without display queue
			_skiaView.NeedsDisplay = true;
			_skiaView.DisplayIfNeeded();
		}

		private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
		{
			if (e.Surface == null || e.Surface.Canvas == null)
				return;

			var canvas = e.Surface.Canvas;
			var width = e.BackendRenderTarget.Width;
			var height = e.BackendRenderTarget.Height;

			_scene.Render(canvas, width, height);

			// FPS tracking
			double currentTime = _stopwatch.Elapsed.TotalSeconds;
			double dt = Math.Clamp(currentTime - _lastTime, 1.0 / 240.0, 0.25);
			_lastTime = currentTime;

			_accumulatedTime += dt;
			_frameCount++;

			if (_accumulatedTime >= 0.5)
			{
				double fps = _frameCount / _accumulatedTime;
				var complexity = _scene.Complexity;
				var elementCount = _scene.ElementCount;
				
				// Update window title with FPS
				var window = View?.Window;
				if (window != null)
				{
					window.Title = $"MotionMark SkiaSharp (OpenGL) | {fps:F1} FPS | Complexity {complexity} | Elements {elementCount}";
				}

				_accumulatedTime = 0.0;
				_frameCount = 0;
			}
		}
	}
}
