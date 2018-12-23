using System.Windows.Forms;

using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace SkiaSharpSample
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void skiaView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			// the the canvas and properties
			var canvas = e.Surface.Canvas;

			// get the screen density for scaling
			var scale = 1f;
			var scaledSize = new SKSize(e.Info.Width / scale, e.Info.Height / scale);

			// handle the device screen density
			canvas.Scale(scale);

			// make sure the canvas is blank
			canvas.Clear(SKColors.White);

			// draw some text
			var paint = new SKPaint
			{
				Color = SKColors.Black,
				IsAntialias = true,
				Style = SKPaintStyle.Fill,
				TextAlign = SKTextAlign.Center,
				TextSize = 24
			};
			var coord = new SKPoint(scaledSize.Width / 2, (scaledSize.Height + paint.TextSize) / 2);
			canvas.DrawText("SkiaSharp", coord, paint);
			var drawable = new Class1();
			var bounds = drawable.Bounds;
			canvas.DrawDrawable(drawable, 0, 0);
			drawable.Snapshot();
			var id = drawable.GenerationId;
			drawable.NotifyDrawingChanged();

			var recorder = new SKPictureRecorder();
			var recorderCanvas = recorder.BeginRecording(new SKRect(0,0,100,100));
			recorderCanvas.DrawRect(new SKRect(20,20,80,80), paint);
			var d = recorder.EndRecordingAsDrawable();
			bounds = d.Bounds;
		}
	}
}
