using System;

namespace SkiaSharp.Views.Forms
{
	internal class GetCanvasSizeEventArgs : EventArgs
	{
		public SKSize CanvasSize { get; set; }
	}
}
