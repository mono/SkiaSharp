#if UNO_REFERENCE_API && !__WASM__
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Uno.Extensions;
using Uno.Foundation;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		public SKXamlCanvas()
		{
		}

		private SKSize GetCanvasSize() => throw new NotImplementedException();

		private static bool GetIsInitialized() => true;

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
		}

		private void DoInvalidate()
		{

		}
	}
}
#endif
