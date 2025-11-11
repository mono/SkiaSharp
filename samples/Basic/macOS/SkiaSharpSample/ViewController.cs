using System;
using System.Diagnostics;
using AppKit;
using CoreGraphics;
using Foundation;

using SkiaSharp;
using SkiaSharp.Views.Mac;

namespace SkiaSharpSample
{
	public partial class ViewController : NSViewController
	{
		private readonly MotionMarkScene _scene = new();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private double _lastTime;
		private int _frameCount;
		private double _accumulatedTime;
		private NSTimer? _timer;

		public ViewController(IntPtr handle)
			: base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Set initial complexity (matching C++ default)
			_scene.SetComplexity(8);

			skiaView.PaintSurface += OnPaintSurface;

			// Start render loop
			_timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1.0 / 240.0), _ =>
			{
				if (skiaView != null)
					skiaView.NeedsDisplay = true;
			});

			_lastTime = _stopwatch.Elapsed.TotalSeconds;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer?.Invalidate();
				_timer = null;
				_scene?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void OnPaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
		{
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
				BeginInvokeOnMainThread(() =>
				{
					var window = View.Window;
					if (window != null)
					{
						window.Title = $"MotionMark SkiaSharp (OpenGL) | {fps:F1} FPS | Complexity {complexity} | Elements {elementCount}";
					}
				});

				_accumulatedTime = 0.0;
				_frameCount = 0;
			}
		}

		partial void OnComplexityChanged(NSSlider sender)
		{
			_scene.SetComplexity((int)sender.IntValue);
		}
	}
}
