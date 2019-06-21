#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Android;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private Bitmap bitmap;
		private SKImageInfo info;

		private bool designMode;

		public SKXamlCanvas()
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			SizeChanged += OnSizeChanged;

			RegisterPropertyChangedCallback(VisibilityProperty, (s, e) => OnVisibilityChanged(s));
			OnVisibilityChanged(this);
		}

		private void Initialize()
		{
			designMode = !Extensions.IsValidEnvironment;

			if (designMode)
				return;

			// create the initial info
			info = new SKImageInfo(0, 0, SKColorType.Rgba8888, SKAlphaType.Premul);
		}


		private static bool GetIsInitialized() => true;

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / 96.0f;
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// var display = DisplayInformation.GetForCurrentView();
			// display.DpiChanged += OnDpiChanged;

			//OnDpiChanged(display);
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// var display = DisplayInformation.GetForCurrentView();
			// display.DpiChanged -= OnDpiChanged;
		}

		private void DoInvalidate()
		{

		}

		protected override void OnDraw(global::Android.Graphics.Canvas canvas)
		{
			base.OnDraw(canvas);

			if (designMode)
				return;

			if (info.Width == 0 || info.Height == 0 || Visibility != Visibility.Visible)
				return;

			// create the bitmap data if we need it
			if (bitmap == null || bitmap.Handle == IntPtr.Zero || bitmap.Width != info.Width || bitmap.Height != info.Height)
			{
				FreeBitmap();
				bitmap = Bitmap.CreateBitmap(info.Width, info.Height, Bitmap.Config.Argb8888);
			}

			// create a surface
			using (var surface = SKSurface.Create(info, bitmap.LockPixels(), info.RowBytes))
			{
				// draw using SkiaSharp
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

				surface.Canvas.Flush();
			}
			bitmap.UnlockPixels();

			// draw bitmap to canvas
			if (IgnorePixelScaling)
				canvas.DrawBitmap(bitmap, info.Rect.ToRect(), new RectF(0, 0, (float)Width, (float)Height), null);
			else
				canvas.DrawBitmap(bitmap, 0, 0, null);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			// update the info with the new sizes
			UpdateCanvasSize(w, h);
		}

		private void UpdateCanvasSize(int w, int h)
		{
			if (designMode)
				return;

			if (IgnorePixelScaling)
			{
				var scale = global::Uno.UI.ContextHelper.Current.Resources.DisplayMetrics.Density;
				info.Width = (int)(w / scale);
				info.Height = (int)(h / scale);
			}
			else
			{
				info.Width = w;
				info.Height = h;
			}
		}

		private void FreeBitmap()
		{
			if (bitmap != null)
			{
				// free and recycle the bitmap data
				if (bitmap.Handle != IntPtr.Zero && !bitmap.IsRecycled)
				{
					bitmap.Recycle();
				}
				bitmap.Dispose();
				bitmap = null;
			}
		}
	}
}
#endif
