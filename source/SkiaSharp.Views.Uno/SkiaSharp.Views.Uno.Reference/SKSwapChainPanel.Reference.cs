#if UNO_REFERENCE_API
using System;

#if WINDOWS
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
