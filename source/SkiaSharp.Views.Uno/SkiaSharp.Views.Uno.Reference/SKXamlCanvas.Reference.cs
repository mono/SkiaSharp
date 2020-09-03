#if UNO_REFERENCE_API
using System;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		public SKXamlCanvas()
		{
			throw new NotImplementedException();
		}

		partial void DoUnloaded() => throw new NotImplementedException();

		private SKSize GetCanvasSize() => throw new NotImplementedException();

		private void DoInvalidate() => throw new NotImplementedException();
	}
}
#endif
