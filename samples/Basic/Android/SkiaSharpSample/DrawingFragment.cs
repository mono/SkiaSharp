using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;

using SkiaSharp;
using SkiaSharp.Views.Android;

namespace SkiaSharpSample;

public class DrawingFragment : Fragment
{
	private static readonly int[] SwatchIds = new[]
	{
		Resource.Id.colorBlack,
		Resource.Id.colorRed,
		Resource.Id.colorBlue,
		Resource.Id.colorGreen,
		Resource.Id.colorOrange,
		Resource.Id.colorPurple,
	};

	private SKCanvasView skiaView;
	private View selectedSwatch;
	private Google.Android.Material.Slider.Slider brushSlider;
	private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
	private SKPath currentPath;
	private SKColor currentColor = SKColors.Black;
	private SKColor canvasBackground = SKColors.White;

	private float BrushSize => brushSlider?.Value ?? 4f;

	public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
	{
		var view = inflater.Inflate(Resource.Layout.fragment_drawing, container, false);

		// Resolve theme surface color for canvas background
		var typedValue = new Android.Util.TypedValue();
		if (Context.Theme.ResolveAttribute(Resource.Attribute.colorSurface, typedValue, true))
		{
			var c = new Color(typedValue.Data);
			canvasBackground = new SKColor((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
		}

		// Resolve theme onSurface color as default drawing color
		if (Context.Theme.ResolveAttribute(Resource.Attribute.colorOnSurface, typedValue, true))
		{
			var c = new Color(typedValue.Data);
			currentColor = new SKColor((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
		}

		skiaView = view.FindViewById<SKCanvasView>(Resource.Id.skiaView);
		skiaView.PaintSurface += OnPaintSurface;
		skiaView.Touch += OnTouch;

		// Wire up color swatches — resolve SKColor from each swatch's background
		foreach (var viewId in SwatchIds)
		{
			var swatch = view.FindViewById<View>(viewId);
			SKColor bgColor = SKColors.Black;
			if (OperatingSystem.IsAndroidVersionAtLeast(24)
				&& swatch.Background is GradientDrawable gd && gd.Color != null)
			{
				var c = new Color(gd.Color.DefaultColor);
				bgColor = new SKColor((byte)c.R, (byte)c.G, (byte)c.B);
			}
			else if (swatch.Background is ColorDrawable cd)
			{
				bgColor = new SKColor((byte)cd.Color.R, (byte)cd.Color.G, (byte)cd.Color.B);
			}
			var captured = bgColor;
			swatch.Click += (s, e) =>
			{
				currentColor = captured;
				SetSelectedSwatch(swatch);
			};
		}

		// Select first swatch by default
		selectedSwatch = view.FindViewById<View>(Resource.Id.colorBlack);
		SetSelectedSwatch(selectedSwatch);

		// Brush size slider
		brushSlider = view.FindViewById<Google.Android.Material.Slider.Slider>(Resource.Id.brushSlider);

		// Clear button
		var clearBtn = view.FindViewById<MaterialButton>(Resource.Id.btnClear);
		clearBtn.Click += (s, e) =>
		{
			foreach (var (path, _, _) in strokes)
				path.Dispose();
			strokes.Clear();
			currentPath?.Dispose();
			currentPath = null;
			skiaView?.Invalidate();
		};

		return view;
	}

	private void SetSelectedSwatch(View swatch)
	{
		// Reset previous selection
		if (selectedSwatch != null)
		{
			selectedSwatch.ScaleX = 1.0f;
			selectedSwatch.ScaleY = 1.0f;
			selectedSwatch.Elevation = 0;
		}

		// Highlight selected with scale and elevation
		swatch.ScaleX = 1.3f;
		swatch.ScaleY = 1.3f;
		swatch.Elevation = 8;
		selectedSwatch = swatch;
	}

	private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
	{
		var canvas = e.Surface.Canvas;
		canvas.Clear(canvasBackground);

		if (skiaView.Width <= 0 || skiaView.Height <= 0)
			return;

		using var paint = new SKPaint
		{
			IsAntialias = true,
			Style = SKPaintStyle.Stroke,
			StrokeCap = SKStrokeCap.Round,
			StrokeJoin = SKStrokeJoin.Round,
		};

		float sx = (float)e.Info.Width / skiaView.Width;
		float sy = (float)e.Info.Height / skiaView.Height;
		canvas.Scale(sx, sy);

		foreach (var (path, color, strokeWidth) in strokes)
		{
			paint.Color = color;
			paint.StrokeWidth = strokeWidth;
			canvas.DrawPath(path, paint);
		}

		if (currentPath != null)
		{
			paint.Color = currentColor;
			paint.StrokeWidth = BrushSize;
			canvas.DrawPath(currentPath, paint);
		}
	}

	private void OnTouch(object sender, View.TouchEventArgs e)
	{
		var x = e.Event.GetX();
		var y = e.Event.GetY();

		switch (e.Event.Action)
		{
			case MotionEventActions.Down:
				currentPath = new SKPath();
				currentPath.MoveTo(x, y);
				skiaView.Invalidate();
				break;
			case MotionEventActions.Move:
				currentPath?.LineTo(x, y);
				skiaView.Invalidate();
				break;
			case MotionEventActions.Up:
			case MotionEventActions.Cancel:
				if (currentPath != null)
				{
					strokes.Add((currentPath, currentColor, BrushSize));
					currentPath = null;
					skiaView.Invalidate();
				}
				break;
		}
		e.Handled = true;
	}

	public override void OnDestroyView()
	{
		if (skiaView != null)
		{
			skiaView.PaintSurface -= OnPaintSurface;
			skiaView.Touch -= OnTouch;
			skiaView = null;
		}
		foreach (var (path, _, _) in strokes)
			path.Dispose();
		strokes.Clear();
		currentPath?.Dispose();
		currentPath = null;
		base.OnDestroyView();
	}
}
