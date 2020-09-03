#if UNO_REFERENCE_API
using System;
using Windows.UI.Xaml;

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
