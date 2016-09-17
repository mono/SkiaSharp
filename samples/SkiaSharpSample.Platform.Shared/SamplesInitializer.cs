#if WINDOWS_UWP
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
#elif __IOS__
using System.IO;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
#elif __ANDROID__
using System.IO;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
#endif

namespace SkiaSharpSample
{
	public static class SamplesInitializer
	{
		public static void Init()
		{
			var fontName = "content-font.ttf";

#if WINDOWS_UWP
			var pkg = Package.Current.InstalledLocation.Path;
			var path = Path.Combine(pkg, "Assets", "Media", fontName);
#elif __IOS__
			var path = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(fontName), Path.GetExtension(fontName));
#elif __ANDROID__
			var path = Path.Combine(Application.Context.CacheDir.AbsolutePath, fontName);
			using (var asset = Application.Context.Assets.Open(Path.Combine("Media", fontName)))
			using (var dest = File.Open(path, FileMode.Create))
			{
				asset.CopyTo(dest);
			}
#endif
			SamplesManager.ContentFontPath = path;
			SamplesManager.OpenFile += OnOpenSampleFile;
		}

		private static async void OnOpenSampleFile(string path)
		{
#if WINDOWS_UWP
			var file = await StorageFile.GetFileFromPathAsync(path);
			await Launcher.LaunchFileAsync(file);
#elif __IOS__
			// the external / shared location
			var external = Path.Combine(Path.GetTempPath(), "SkiaSharpSample");
			if (!Directory.Exists(external))
			{
				Directory.CreateDirectory(external);
			}
			// copy file to external
			var newPath = Path.Combine(external, Path.GetFileName(path));
			File.Copy(path, newPath);
			// open the file
			var vc = Platform.GetRenderer(Application.Current.MainPage) as UIViewController;
			var resourceToOpen = NSUrl.FromFilename(external);
			var controller = UIDocumentInteractionController.FromUrl(resourceToOpen);
			if (!controller.PresentOpenInMenu(vc.View.Bounds, vc.View, true))
			{
				new UIAlertView("SkiaSharp", "Unable to open file.", null, "OK").Show();
			}
#elif __ANDROID__
			// the external / shared location
			var external = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath, "SkiaSharpSample");
			if (!Directory.Exists(external))
			{
				Directory.CreateDirectory(external);
			}
			// copy file to external
			var newPath = Path.Combine(external, Path.GetFileName(path));
			File.Copy(path, newPath);
			// open the file
			var uri = Uri.FromFile(new Java.IO.File(newPath));
			var intent = new Intent(Intent.ActionView, uri);
			intent.AddFlags(ActivityFlags.NewTask);
			Application.Context.StartActivity(intent);
#endif
		}
	}
}
