#if UNO_REFERENCE_API
using System;

#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas
	{
		public SKXamlCanvas()
		{
			throw new NotImplementedException();
		}

		partial void DoUnloaded() => throw new NotImplementedException();

		private void DoInvalidate() => throw new NotImplementedException();
	}
}
#endif
