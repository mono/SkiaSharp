using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SkiaSharp.Views.Windows
{
	/// <summary>A XAML canvas that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public class SKXamlCanvas : Canvas
	{
		private const float DpiBase = 96.0f;

		private static readonly DependencyProperty ProxyVisibilityProperty =
			DependencyProperty.Register(
				"ProxyVisibility",
				typeof(Visibility),
				typeof(SKXamlCanvas),
				new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

		private static bool designMode = DesignMode.DesignModeEnabled;

		private bool ignorePixelScaling;
		private bool isVisible = true;

		// workaround for https://github.com/mono/SkiaSharp/issues/1118
		private int loadUnloadCounter = 0;

		private byte[] pixels;
		private GCHandle pixelsHandle;
		private int pixelWidth;
		private int pixelHeight;
		private WriteableBitmap bitmap;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKXamlCanvas" /> class.</summary>
		/// <remarks />
		public SKXamlCanvas()
		{
			Initialize();
		}

		private void Initialize()
		{
			if (designMode)
				return;

			var display = DisplayInformation.GetForCurrentView();
			OnDpiChanged(display);

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			SizeChanged += OnSizeChanged;

			var binding = new Binding
			{
				Path = new PropertyPath(nameof(Visibility)),
				Source = this
			};
			SetBinding(ProxyVisibilityProperty, binding);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize { get; private set; }

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value><see langword="true" /> if the canvas should ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks>By default, when <see langword="false" />, the canvas is resized to 1 canvas pixel per display pixel. When <see langword="true" />, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				Invalidate();
			}
		}

		/// <summary>Gets the current DPI for the canvas.</summary>
		/// <value>The current DPI value.</value>
		/// <remarks />
		public double Dpi { get; private set; } = 1;

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Windows.SKXamlCanvas.OnPaintSurface(SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Windows.SKXamlCanvas.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// };
		/// ```
		/// ]]></format></remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Windows.SKXamlCanvas.OnPaintSurface(SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Windows.SKXamlCanvas.PaintSurface>
		/// event.
		///
		/// > [!IMPORTANT]
		/// > If this method is overridden, then the base must be called, otherwise the
		/// > event will not be fired.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface (e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// }
		/// ```
		/// ]]></format></remarks>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SKXamlCanvas canvas && e.NewValue is Visibility visibility)
			{
				canvas.isVisible = visibility == Visibility.Visible;
				canvas.Invalidate();
			}
		}

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / DpiBase;
			Invalidate();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter++;
			if (loadUnloadCounter != 1)
				return;

			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged += OnDpiChanged;

			OnDpiChanged(display);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			loadUnloadCounter--;
			if (loadUnloadCounter != 0)
				return;

			FreeBitmap();

			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;
		}

		/// <summary>Invalidates the entire surface of the control and causes the control to be redrawn.</summary>
		/// <remarks />
		public void Invalidate()
		{
			if (Dispatcher.HasThreadAccess)
				DoInvalidate();
			else
				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
		}

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if (ActualWidth <= 0 || ActualHeight <= 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var info = CreateBitmap(out var unscaledSize, out var dpi);

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;
				CanvasSize = userVisibleSize;

				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale(dpi);
					canvas.Save();
				}

				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			// This implementation is not fast enough, and providing the original pixel buffer
			// is needed, yet the internal `IBufferByteAccess` interface is not yet available in Uno.
			// Once it is, we can replace this implementation and provide the pinned array directly
			// to skia.
			using (var data = bitmap.PixelBuffer.AsStream())
			{
				data.Write(pixels, 0, pixels.Length);
				data.Flush();
			}

			bitmap.Invalidate();
		}

		private SKSizeI CreateSize(out SKSizeI unscaledSize, out float dpi)
		{
			unscaledSize = SKSizeI.Empty;
			dpi = (float)Dpi;

			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			unscaledSize = new SKSizeI((int)w, (int)h);
			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		private SKImageInfo CreateBitmap(out SKSizeI unscaledSize, out float dpi)
		{
			var size = CreateSize(out unscaledSize, out dpi);
			var info = new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);

			if (bitmap?.PixelWidth != info.Width || bitmap?.PixelHeight != info.Height)
				FreeBitmap();

			if (bitmap == null && info.Width > 0 && info.Height > 0)
			{
				bitmap = new WriteableBitmap(info.Width, info.Height);

				var brush = new ImageBrush
				{
					ImageSource = bitmap,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
					Stretch = Stretch.Fill
				};

				Background = brush;
			}

			if (pixels == null || pixelWidth != info.Width || pixelHeight != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

				pixelWidth = info.Width;
				pixelHeight = info.Height;
			}

			return info;
		}

		private void FreeBitmap()
		{
			if (pixels != null)
			{
				pixelsHandle.Free();
				pixels = null;
				bitmap = null;
			}
		}
	}
}
