using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SkiaSharpSample
{
	public sealed partial class MainPage : Page
	{
		private SKImage image;

		public MainPage()
		{
			InitializeComponent();

			Loaded += OnLoaded;
		}

		public class CorsBypassHandler : DelegatingHandler
		{
			public CorsBypassHandler()
			{
				InnerHandler = new HttpClientHandler();
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var builder = new UriBuilder(request.RequestUri);

#if __WASM__
				builder.Host = "cors-anywhere.herokuapp.com";
				builder.Path = request.RequestUri.Host + builder.Path;
#endif

				return base.SendAsync(new HttpRequestMessage(request.Method, builder.Uri), cancellationToken);
			}
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			using var client = new HttpClient(new CorsBypassHandler());
			using var stream = await client.GetStreamAsync("https://github.com/mono/SkiaSharp/raw/master/images/nuget.png");

			image = SKImage.FromEncodedData(stream);

			using var ms = new MemoryStream();
			using var i = image.ToRasterImage(true);
			using var pixmap = i.PeekPixels();
			Console.WriteLine("SUCCESS? " + pixmap.Encode(ms, SKPngEncoderOptions.Default));

			using var img = SKImage.Create(new SKImageInfo(100, 100));
			Console.WriteLine("img: " + img.ColorType);

			using var data = SKData.Create(img.Handle, 10, (ptr, ctx) => Console.WriteLine("RELEASE: " + ptr + "  " + ctx));
			Console.WriteLine("data: " + data.Size);

			var glinterface = GRGlInterface.AssembleInterface((ctx, name) =>
			{
				Console.WriteLine("name: " + name);
				return IntPtr.Zero;
			});

			skiaView.Invalidate();
		}

		class Drawable : SKDrawable
		{
			protected override void OnDraw(SKCanvas canvas)
			{
				canvas.Clear(SKColors.Red);
			}

			protected override SKRect OnGetBounds()
			{
				return SKRect.Create(100, 100);
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var display = DisplayInformation.GetForCurrentView();
			var scale = display.LogicalDpi / 96.0f;
			var scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			if (image != null)
			{
				canvas.DrawImage(image, (scaledSize.Width - image.Width) / 2, scaledSize.Height / 4);
			}

			var drw = new Drawable();

			using (new SKAutoCanvasRestore(canvas))
			{
				canvas.ClipRect(drw.Bounds);
				drw.Draw(canvas, 0, 0);
			}

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint(scaledSize.Width / 2, (scaledSize.Height / 5) + paint.TextSize);
			canvas.DrawText("SkiaSharp", coord, paint);
		}
	}
}
