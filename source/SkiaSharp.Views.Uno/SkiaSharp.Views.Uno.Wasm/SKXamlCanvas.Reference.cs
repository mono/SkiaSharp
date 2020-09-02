#if UNO_REFERENCE_API && !__WASM__
using System;
using System.Runtime.InteropServices;
using Uno.Foundation;
using Windows.UI.Xaml;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		public SKXamlCanvas()
		{
		}

		partial void DoUnloaded() { }

		private SKSize GetCanvasSize() => throw new NotImplementedException();

		private void DoInvalidate() { }
	}
}
#endif
