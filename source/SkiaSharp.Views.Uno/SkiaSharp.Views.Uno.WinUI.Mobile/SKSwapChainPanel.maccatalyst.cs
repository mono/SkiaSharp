#if __MACCATALYST__
using System;
using System.Threading;
using Uno.Foundation;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace SkiaSharp.Views.Windows
{
	public partial class SKSwapChainPanel
	{
		public SKSwapChainPanel()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");
			}
		}

		private SKSize GetCanvasSize()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");
			}

			return new SKSize();
		}

		private GRContext GetGRContext()
		{
			if (RaiseOnUnsupported)
			{
				throw new NotSupportedException($"SKSwapChainPanel is not supported for Skia based platforms");
			}

			return null;
		}

		private void DoInvalidate() { }
	}
}
#endif
