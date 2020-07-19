using System;
using Android.App;
using Android.Runtime;
using Com.Nostra13.Universalimageloader.Core;
using Windows.UI.Xaml.Media;

namespace SkiaSharpSample.Droid
{
	[Application]
	public class Application : Windows.UI.Xaml.NativeApplication
	{
		public Application(IntPtr javaReference, JniHandleOwnership transfer)
			: base(() => new App(), javaReference, transfer)
		{
			ConfigureUniversalImageLoader();
		}

		private void ConfigureUniversalImageLoader()
		{
			// Create global configuration and initialize ImageLoader with this config
			var config = new ImageLoaderConfiguration
				.Builder(Context)
				.Build();

			ImageLoader.Instance.Init(config);

			ImageSource.DefaultImageLoader = ImageLoader.Instance.LoadImageAsync;
		}
	}
}
