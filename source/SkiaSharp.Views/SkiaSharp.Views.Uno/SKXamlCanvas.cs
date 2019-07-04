using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
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
		private bool ignorePixelScaling;
		private bool isVisible;

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				Invalidate();
			}
		}

		public SKSize CanvasSize => GetCanvasSize();

		public double Dpi { get; private set; } = 1;

		public static bool IsInitialized => GetIsInitialized();

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private static void OnVisibilityChanged(DependencyObject d)
		{
			if (d is SKXamlCanvas canvas)
			{
				canvas.isVisible = canvas.Visibility == Visibility.Visible;
				canvas.Invalidate();
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		public void Invalidate()
		{
			if (Dispatcher.HasThreadAccess)
			{
				DoInvalidate();
			}
			else
			{
				Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
			}
		}
	}
}
