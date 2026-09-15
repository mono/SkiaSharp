#if UNO_REFERENCE_API
using System;

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
	public partial class SKXamlCanvas
	{
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Windows.SKXamlCanvas" /> class.</summary>
		/// <remarks />
		public SKXamlCanvas()
		{
			throw new NotImplementedException();
		}

		partial void DoUnloaded() => throw new NotImplementedException();

		private void DoInvalidate() => throw new NotImplementedException();
	}
}
#endif
