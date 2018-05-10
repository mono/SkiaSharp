using System;
using System.IO;
using System.Linq;
#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
#elif __MACOS__
using AppKit;
using Foundation;
#elif __IOS__ || __TVOS__
using Foundation;
using UIKit;
#elif __ANDROID__
using Android.App;
using Android.Content;
#elif __DESKTOP__
using System.Diagnostics;
using System.Reflection;
using System.Windows;
#elif TIZEN4_0
using Tizen.Applications;
using Xamarin.Forms.Platform.Tizen;
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
#elif __IOS__ || __TVOS__ || __MACOS__
			var path = NSBundle.MainBundle.PathForResource(Path.GetFileNameWithoutExtension(fontName), Path.GetExtension(fontName));
#elif __ANDROID__
			var path = Path.Combine(Application.Context.CacheDir.AbsolutePath, fontName);
			using (var asset = Application.Context.Assets.Open(Path.Combine("Media", fontName)))
			using (var dest = File.Open(path, FileMode.Create))
			{
				asset.CopyTo(dest);
			}
#elif __DESKTOP__
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(root, "Media", fontName);
#elif TIZEN4_0
			var path = ResourcePath.GetPath(fontName);
#endif

#if WINDOWS_UWP
			var localStorage = ApplicationData.Current.LocalFolder.Path;
#elif __IOS__ || __TVOS__
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var localStorage = Path.Combine(documents, "..", "Library");
#elif __MACOS__ || __ANDROID__
			var localStorage = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
#elif __DESKTOP__
			var localStorage = System.Windows.Forms.Application.LocalUserAppDataPath;
#elif TIZEN4_0
			var localStorage = Application.Current.DirectoryInfo.Data;
#endif

			SamplesManager.ContentFontPath = path;
			SamplesManager.OpenFile += OnOpenSampleFile;
			SamplesManager.TempDataPath = Path.Combine(localStorage, "SkiaSharpSample", "TemporaryData");
			if (!Directory.Exists(SamplesManager.TempDataPath))
			{
				Directory.CreateDirectory(SamplesManager.TempDataPath);
			}
		}

		private static async void OnOpenSampleFile(string path)
		{
#if WINDOWS_UWP
			var file = await StorageFile.GetFileFromPathAsync(path);
			await Launcher.LaunchFileAsync(file);
#elif __MACOS__
			if (!NSWorkspace.SharedWorkspace.OpenFile(path))
			{
				var alert = new NSAlert();
				alert.AddButton("OK");
				alert.MessageText = "SkiaSharp";
				alert.InformativeText = "Unable to open file.";
				alert.RunSheetModal(NSApplication.SharedApplication.MainWindow);
			}
#elif __TVOS__
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
			var vc = Xamarin.Forms.Platform.iOS.Platform.GetRenderer(Xamarin.Forms.Application.Current.MainPage) as UIViewController;
			var resourceToOpen = NSUrl.FromFilename(newPath);
			var controller = UIDocumentInteractionController.FromUrl(resourceToOpen);
			if (!controller.PresentOpenInMenu(vc.View.Bounds, vc.View, true))
			{
				new UIAlertView("SkiaSharp", "Unable to open file.", (IUIAlertViewDelegate)null, "OK").Show();
			}
#elif __ANDROID__
			// the external / shared location
			var external = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "SkiaSharpSample");
			if (!Directory.Exists(external))
			{
				Directory.CreateDirectory(external);
			}
			// copy file to external
			var newPath = Path.Combine(external, Path.GetFileName(path));
			File.Copy(path, newPath);
			// open the file
			var uri = Android.Net.Uri.FromFile(new Java.IO.File(newPath));
			var intent = new Intent(Intent.ActionView, uri);
			intent.AddFlags(ActivityFlags.NewTask);
			Application.Context.StartActivity(intent);
#elif __DESKTOP__
			Process.Start(path);
#elif TIZEN4_0
			var appControl = new AppControl
			{
				Operation = AppControlOperations.View,
				Uri = "file://" + path
			};
			var matchedApplications = AppControl.GetMatchedApplicationIds(appControl);
			if (matchedApplications.Any())
			{
				AppControl.SendLaunchRequest(appControl);
			}
#endif
		}
	}
}
