#if UNO_REFERENCE_API
using System;

#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		public SKSwapChainPanel()
		{
			throw new NotImplementedException();
		}

		private SKSize GetCanvasSize() => throw new NotImplementedException();

		private GRContext GetGRContext() => throw new NotImplementedException();

		partial void DoLoaded() => throw new NotImplementedException();

		partial void DoEnableRenderLoop(bool enable) => throw new NotImplementedException();

		private void DoInvalidate() => throw new NotImplementedException();

		internal void RenderFrame() => throw new NotImplementedException();
	}
}
#endif
