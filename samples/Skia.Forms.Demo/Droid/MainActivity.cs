using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;

namespace Skia.Forms.Demo.Droid
{
	[Activity (Label = "Skia.Forms.Demo.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			// set up resource paths
			string fontName = "content-font.ttf";
			string fontPath = Path.Combine (CacheDir.AbsolutePath, fontName);
			using (var asset = Assets.Open (fontName))
			using (var dest = File.Open (fontPath, FileMode.Create)) {
				asset.CopyTo (dest);
			}
			SkiaSharp.Demos.CustomFontPath = fontPath;


			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

			LoadApplication (new App ());
		}
	}
}

