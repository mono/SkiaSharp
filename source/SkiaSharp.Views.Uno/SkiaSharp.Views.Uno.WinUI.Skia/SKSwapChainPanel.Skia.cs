using System;
using System.Threading;
using Uno.Foundation;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	public partial class SKSwapChainPanel
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKSwapChainPanel" /> class.</summary>
		/// <remarks />
		public SKSwapChainPanel()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported on Uno Platform Skia rendering. Use Uno's SKCanvasElement for GPU-accelerated drawing.");
			}
		}

		private SKSize GetCanvasSize()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported on Uno Platform Skia rendering. Use Uno's SKCanvasElement for GPU-accelerated drawing.");
			}

			return new SKSize();
		}

		private GRContext GetGRContext()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported on Uno Platform Skia rendering. Use Uno's SKCanvasElement for GPU-accelerated drawing.");
			}

			return null;
		}

		private void DoInvalidate() { }
	}
}
