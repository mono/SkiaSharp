#if UNO_REFERENCE_API && !__WASM__
using System;
using System.Threading;
using Uno.Foundation;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		public SKSwapChainPanel()
		{
		}

		private SKSize GetCanvasSize() => throw new NotImplementedException();

		private GRContext GetGRContext() => throw new NotImplementedException();

		partial void DoLoaded()
		{
		}

		partial void DoEnableRenderLoop(bool enable) { }

		private void DoInvalidate()
		{
			
		}

		internal void RenderFrame()
		{
			
		}
	}
}
#endif
