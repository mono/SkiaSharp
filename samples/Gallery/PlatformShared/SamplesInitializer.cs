using System;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
#if __WASM__
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Uno.Foundation;
#endif
#if WINDOWS_UWP
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
#if !__WASM__
using Launcher = Xamarin.Essentials.Launcher;
#endif
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
#elif __TIZEN__
using Tizen.Applications;
using Xamarin.Forms.Platform.Tizen;
#endif

namespace Xamarin.Essentials
{
	// dummy placeholder
}

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
			var path = Path.Combine(FileSystem.CacheDirectory, fontName);
			using (var asset = Application.Context.Assets.Open(Path.Combine("Media", fontName)))
			using (var dest = File.Open(path, FileMode.Create))
			{
				asset.CopyTo(dest);
			}
#elif __DESKTOP__
			var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(root, "Media", fontName);
#elif __TIZEN__
			var path = ResourcePath.GetPath(fontName);
#elif HAS_UNO
			// Uno does not have a uniform app package, so just use the embedded font
			string path;
			using (var stream = SampleMedia.Fonts.EmbeddedFont)
			{
				var appData = ApplicationData.Current.LocalFolder.Path;
				var temp = Path.Combine(appData, "SkiaSharpSample", "Assets", "Media");
				if (!Directory.Exists(temp))
					Directory.CreateDirectory(temp);

				path = Path.Combine(temp, fontName);
				using (var writer = File.OpenWrite(path))
					stream.CopyTo(writer);
			}
#endif

#if WINDOWS_UWP || __IOS__ || __TVOS__ || __ANDROID__ || __TIZEN__
			var localStorage = FileSystem.AppDataDirectory;
#elif __MACOS__
			var localStorage = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
#elif __DESKTOP__
			var localStorage = System.Windows.Forms.Application.LocalUserAppDataPath;
#elif HAS_UNO
			var localStorage = ApplicationData.Current.LocalFolder.Path;
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
#if WINDOWS_UWP || __TVOS__ || __IOS__ || __ANDROID__ || __TIZEN__
			var title = "Open " + Path.GetExtension(path).ToUpperInvariant();
			await Launcher.OpenAsync(new OpenFileRequest(title, new ReadOnlyFile(path)));
#elif __MACOS__
			if (!NSWorkspace.SharedWorkspace.OpenFile(path))
			{
				var alert = new NSAlert();
				alert.AddButton("OK");
				alert.MessageText = "SkiaSharp";
				alert.InformativeText = "Unable to open file.";
				alert.RunSheetModal(NSApplication.SharedApplication.MainWindow);
			}
#elif __DESKTOP__
			Process.Start(path);
#elif HAS_UNO
			var data = File.ReadAllBytes(path);
			var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
			var pinnedData = gch.AddrOfPinnedObject();
			try
			{
				WebAssemblyRuntime.InvokeJS($"fileSaveAs('{Path.GetFileName(path)}', {pinnedData}, {data.Length})");
			}
			finally
			{
				gch.Free();
			}
#endif
		}
	}
}
