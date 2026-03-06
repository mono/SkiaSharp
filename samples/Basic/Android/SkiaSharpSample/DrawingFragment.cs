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

namespace SkiaSharpSample
{
	public class DrawingFragment : Fragment
	{
		private static readonly (int ViewId, SKColor SkColor)[] ColorOptions = new[]
		{
			(Resource.Id.colorBlack, SKColors.Black),
			(Resource.Id.colorRed, new SKColor(0xE5, 0x39, 0x35)),
			(Resource.Id.colorBlue, new SKColor(0x1E, 0x88, 0xE5)),
			(Resource.Id.colorGreen, new SKColor(0x43, 0xA0, 0x47)),
			(Resource.Id.colorOrange, new SKColor(0xFB, 0x8C, 0x00)),
			(Resource.Id.colorPurple, new SKColor(0x8E, 0x24, 0xAA)),
		};

		private SKCanvasView skiaView;
		private View selectedSwatch;
		private Android.Widget.SeekBar brushSlider;
		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor = SKColors.Black;

		private float BrushSize => brushSlider?.Progress + 1 ?? 6f;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate(Resource.Layout.fragment_drawing, container, false);

			skiaView = view.FindViewById<SKCanvasView>(Resource.Id.skiaView);
			skiaView.PaintSurface += OnPaintSurface;
			skiaView.Touch += OnTouch;

			// Wire up color swatches
			foreach (var (viewId, skColor) in ColorOptions)
			{
				var swatch = view.FindViewById<View>(viewId);
				var captured = skColor;
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
			brushSlider = view.FindViewById<Android.Widget.SeekBar>(Resource.Id.brushSlider);

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
			// Remove border from previous selection
			if (selectedSwatch != null)
				selectedSwatch.Background = selectedSwatch.Background is LayerDrawable
					? ((LayerDrawable)selectedSwatch.Background).GetDrawable(0)
					: selectedSwatch.Background;

			// Add selection ring
			var bg = swatch.Background;
			var ring = new GradientDrawable();
			ring.SetShape(ShapeType.Rectangle);
			ring.SetStroke(4, Color.White);
			ring.SetCornerRadius(2);
			var outer = new GradientDrawable();
			outer.SetShape(ShapeType.Rectangle);
			outer.SetStroke(3, Color.DarkGray);
			outer.SetCornerRadius(2);
			swatch.Background = new LayerDrawable(new Drawable[] { bg, ring, outer });
			selectedSwatch = swatch;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			var canvas = e.Surface.Canvas;
			canvas.Clear(SKColors.White);

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
}
