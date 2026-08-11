using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

using SkiaSharp;
using SkiaSharp.Views.Android;

namespace SkiaSharpSample;

public class GraphiteFragment : Fragment
{
	static readonly SKColor[] colors =
	{
		new(0x35, 0x8C, 0xFF),
		new(0x7C, 0x4D, 0xFF),
		new(0x00, 0xC9, 0xA7),
		new(0xFF, 0xB0, 0x2E),
		new(0xF4, 0x5B, 0x8A),
	};

	readonly object touchSync = new();

	SKGraphiteVulkanView skiaView;
	TextView statusLabel;
	FpsCounter fpsCounter = new();
	SKPoint touchPoint = new(0.5f, 0.5f);
	bool touchActive;

	public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
	{
		if (Build.VERSION.SdkInt < BuildVersionCodes.N)
		{
			return new TextView(Context)
			{
				Gravity = GravityFlags.Center,
				Text = "Graphite Vulkan requires Android 7.0 (API 24) or later.",
			};
		}

		var view = inflater.Inflate(Resource.Layout.fragment_graphite, container, false);
		skiaView = view.FindViewById<SKGraphiteVulkanView>(Resource.Id.skiaGraphiteView);
		statusLabel = view.FindViewById<TextView>(Resource.Id.graphiteStatus);
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.RenderFailed += OnRenderFailed;
		skiaView.Touch += OnTouch;
		skiaView.RenderMode = Android.Opengl.Rendermode.Continuously;
		return view;
	}

	public override void OnResume()
	{
		base.OnResume();
		fpsCounter = new FpsCounter();
		fpsCounter.Start();
		skiaView?.OnResume();
	}

	public override void OnPause()
	{
		skiaView?.OnPause();
		fpsCounter.Stop();
		base.OnPause();
	}

	public override void OnDestroyView()
	{
		if (skiaView != null)
		{
			skiaView.OnPause();
			skiaView.PaintSurface -= OnPaintSurface;
			skiaView.RenderFailed -= OnRenderFailed;
			skiaView.Touch -= OnTouch;
			skiaView = null;
		}
		statusLabel = null;
		base.OnDestroyView();
	}

	void OnPaintSurface(object sender, SKPaintGraphiteSurfaceEventArgs e)
	{
		SKPoint point;
		bool active;
		lock (touchSync)
		{
			point = touchPoint;
			active = touchActive;
		}

		DrawScene(e.Surface.Canvas, e.Info, fpsCounter.ElapsedSeconds, point, active);

		if (fpsCounter.Tick() is double fps)
		{
			Activity?.RunOnUiThread(() =>
			{
				if (statusLabel != null)
					statusLabel.Text = $"GRAPHITE / VULKAN  |  FPS: {fps:F0}";
			});
		}
	}

	void OnRenderFailed(object sender, SKGraphiteRenderFailedEventArgs e)
	{
		if (statusLabel != null)
			statusLabel.Text = "GRAPHITE / VULKAN  |  RENDER FAILED";
		Toast.MakeText(Context, e.Exception.Message, ToastLength.Long)?.Show();
	}

	void OnTouch(object sender, View.TouchEventArgs e)
	{
		lock (touchSync)
		{
			switch (e.Event.ActionMasked)
			{
				case MotionEventActions.Down:
				case MotionEventActions.Move:
					touchActive = true;
					if (skiaView.Width > 0 && skiaView.Height > 0)
					{
						touchPoint = new SKPoint(
							e.Event.GetX() / skiaView.Width,
							e.Event.GetY() / skiaView.Height);
					}
					break;
				case MotionEventActions.Up:
				case MotionEventActions.Cancel:
					touchActive = false;
					break;
			}
		}
		e.Handled = true;
	}

	static void DrawScene(SKCanvas canvas, SKImageInfo info, float elapsed, SKPoint touch, bool active)
	{
		var width = info.Width;
		var height = info.Height;
		var scale = Math.Min(width, height);
		canvas.Clear(new SKColor(0x08, 0x0B, 0x1A));

		using var paint = new SKPaint { IsAntialias = true };
		for (var i = 0; i < colors.Length; i++)
		{
			var angle = elapsed * (0.35f + i * 0.07f) + i * 1.25f;
			var x = width * 0.5f + MathF.Cos(angle) * width * (0.18f + i * 0.015f);
			var y = height * 0.5f + MathF.Sin(angle * 1.3f) * height * (0.16f + i * 0.012f);
			paint.Color = colors[i];
			paint.Style = SKPaintStyle.Fill;
			canvas.DrawCircle(x, y, scale * (0.055f + i * 0.008f), paint);
		}

		if (active)
		{
			paint.Color = SKColors.White;
			paint.Style = SKPaintStyle.Stroke;
			paint.StrokeWidth = Math.Max(3f, scale * 0.008f);
			canvas.DrawCircle(touch.X * width, touch.Y * height, scale * 0.09f, paint);
		}

		using var titleFont = new SKFont { Size = scale * 0.11f };
		using var subtitleFont = new SKFont { Size = scale * 0.038f };
		paint.Color = SKColors.White;
		paint.Style = SKPaintStyle.Fill;
		canvas.DrawText("GRAPHITE", width * 0.5f, height * 0.48f, SKTextAlign.Center, titleFont, paint);
		paint.Color = new SKColor(0x8E, 0xC5, 0xFF);
		canvas.DrawText("VULKAN", width * 0.5f, height * 0.55f, SKTextAlign.Center, subtitleFont, paint);
	}
}
