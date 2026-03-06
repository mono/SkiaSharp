using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;

using SkiaSharp;
using SkiaSharp.Views.Android;

namespace SkiaSharpSample
{
	public class DrawingFragment : Fragment
	{
		private static readonly (string Name, SKColor SkColor, Color ViewColor)[] ColorOptions = new[]
		{
			("Black", SKColors.Black, Color.Black),
			("Red", new SKColor(0xE5, 0x39, 0x35), Color.Rgb(0xE5, 0x39, 0x35)),
			("Blue", new SKColor(0x1E, 0x88, 0xE5), Color.Rgb(0x1E, 0x88, 0xE5)),
			("Green", new SKColor(0x43, 0xA0, 0x47), Color.Rgb(0x43, 0xA0, 0x47)),
			("Orange", new SKColor(0xFB, 0x8C, 0x00), Color.Rgb(0xFB, 0x8C, 0x00)),
			("Purple", new SKColor(0x8E, 0x24, 0xAA), Color.Rgb(0x8E, 0x24, 0xAA)),
		};

		private SKCanvasView skiaView;
		private readonly List<(SKPath Path, SKColor Color, float StrokeWidth)> strokes = new();
		private SKPath currentPath;
		private SKColor currentColor = SKColors.Black;
		private float brushSize = 6f;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var root = new LinearLayout(Context)
			{
				Orientation = Orientation.Vertical,
				LayoutParameters = new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent),
			};

			var inflated = inflater.Inflate(Resource.Layout.fragment_drawing, root, false);
			skiaView = inflated.FindViewById<SKCanvasView>(Resource.Id.skiaView);
			skiaView.LayoutParameters = new LinearLayout.LayoutParams(
				ViewGroup.LayoutParams.MatchParent, 0, 1f);
			skiaView.PaintSurface += OnPaintSurface;
			skiaView.Touch += OnTouch;
			root.AddView(skiaView);

			var toolbar = CreateToolbar();
			root.AddView(toolbar);

			return root;
		}

		private View CreateToolbar()
		{
			var scroll = new HorizontalScrollView(Context)
			{
				LayoutParameters = new LinearLayout.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.WrapContent),
			};
			scroll.SetBackgroundColor(Color.Rgb(245, 245, 245));
			scroll.SetPadding(8, 8, 8, 8);

			var row = new LinearLayout(Context)
			{
				Orientation = Orientation.Horizontal,
				LayoutParameters = new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.WrapContent,
					ViewGroup.LayoutParams.WrapContent),
			};

			var dp8 = (int)(8 * Resources.DisplayMetrics.Density);
			var btnHeight = (int)(40 * Resources.DisplayMetrics.Density);
			var btnWidth = (int)(56 * Resources.DisplayMetrics.Density);

			foreach (var (name, skColor, viewColor) in ColorOptions)
			{
				var btn = new Button(Context)
				{
					Text = "",
					LayoutParameters = new LinearLayout.LayoutParams(btnWidth, btnHeight)
					{
						RightMargin = dp8 / 2,
					},
				};
				btn.SetBackgroundColor(viewColor);
				var captured = skColor;
				btn.Click += (s, e) => currentColor = captured;
				row.AddView(btn);
			}

			var sizeSmall = CreateTextButton("S", () => brushSize = 3f);
			var sizeMedium = CreateTextButton("M", () => brushSize = 6f);
			var sizeLarge = CreateTextButton("L", () => brushSize = 12f);
			row.AddView(sizeSmall);
			row.AddView(sizeMedium);
			row.AddView(sizeLarge);

			var clearBtn = CreateTextButton("Clear", () =>
			{
				foreach (var (path, _, _) in strokes)
					path.Dispose();
				strokes.Clear();
				currentPath?.Dispose();
				currentPath = null;
				skiaView.Invalidate();
			});
			row.AddView(clearBtn);

			scroll.AddView(row);
			return scroll;
		}

		private Button CreateTextButton(string text, Action onClick)
		{
			var dp8 = (int)(8 * Resources.DisplayMetrics.Density);
			var btnHeight = (int)(40 * Resources.DisplayMetrics.Density);

			var btn = new Button(Context)
			{
				Text = text,
				LayoutParameters = new LinearLayout.LayoutParams(
					ViewGroup.LayoutParams.WrapContent, btnHeight)
				{
					RightMargin = dp8 / 2,
				},
			};
			btn.SetBackgroundColor(Color.Rgb(120, 120, 120));
			btn.SetTextColor(Color.White);
			btn.Click += (s, e) => onClick();
			return btn;
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
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
				paint.StrokeWidth = brushSize;
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
						strokes.Add((currentPath, currentColor, brushSize));
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
