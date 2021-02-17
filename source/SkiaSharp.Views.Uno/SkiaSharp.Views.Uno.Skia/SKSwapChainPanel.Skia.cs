using System;
using System.Threading;
using Uno.Foundation;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel
	{
		public SKSwapChainPanel()
			=> throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");

		private SKSize GetCanvasSize()
			=> throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");

		private GRContext GetGRContext()
			=> throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");

		private void DoInvalidate() { }
	}
}
