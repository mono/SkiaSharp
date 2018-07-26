using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public abstract class SKTest
	{
		protected const string Category = nameof(Category);
		protected const string GpuCategory = "GPU";

		protected static bool IsLinux;
		protected static bool IsMac;
		protected static bool IsUnix;
		protected static bool IsWindows;

		protected static readonly string DefaultFontFamily;
		protected static readonly string PathToAssembly;
		protected static readonly string PathToFonts;
		protected static readonly string PathToImages;

		static SKTest()
		{
			// the the base paths
			PathToAssembly = Directory.GetCurrentDirectory();
			PathToFonts = Path.Combine(PathToAssembly, "fonts");
			PathToImages = Path.Combine(PathToAssembly, "images");

			// some platforms run the tests from a temporary location, so copy the native files
#if !NET_STANDARD
			var skiaRoot = Path.GetDirectoryName(typeof(SkiaSharp.SKImageInfo).Assembly.Location);
			var harfRoot = Path.GetDirectoryName(typeof(HarfBuzzSharp.Buffer).Assembly.Location);

			foreach (var file in Directory.GetFiles(PathToAssembly))
			{
				var fname = Path.GetFileNameWithoutExtension(file);

				var skiaDest = Path.Combine(skiaRoot, Path.GetFileName(file));
				if (fname == "libSkiaSharp" && !File.Exists(skiaDest))
				{
					File.Copy(file, skiaDest, true);
				}

				var harfDest = Path.Combine(harfRoot, Path.GetFileName(file));
				if (fname == "libHarfBuzzSharp" && !File.Exists(harfDest))
				{
					File.Copy(file, harfDest, true);
				}
			}
#endif

			// set the OS fields
#if NET_STANDARD
			IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
			IsUnix = IsLinux || IsMac;
			IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
			IsMac = MacPlatformDetector.IsMac.Value;
			IsUnix = Environment.OSVersion.Platform == PlatformID.Unix || IsMac;
			IsLinux = IsUnix && !IsMac;
			IsWindows = !IsUnix;
#endif

			// set the test fields
			DefaultFontFamily = IsLinux ? "DejaVu Sans" : "Arial";
		}

		protected static void SaveBitmap(SKBitmap bmp, string filename = "output.png")
		{
			using (var bitmap = new SKBitmap(bmp.Width, bmp.Height))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(SKColors.Transparent);
				canvas.DrawBitmap(bmp, 0, 0);
				canvas.Flush();

				using (var stream = File.OpenWrite(Path.Combine(PathToImages, filename)))
				using (var image = SKImage.FromBitmap(bitmap))
				using (var data = image.Encode())
				{
					data.SaveTo(stream);
				}
			}
		}

		protected static SKBitmap CreateTestBitmap(byte alpha = 255)
		{
			var bmp = new SKBitmap(40, 40);
			bmp.Erase(SKColors.Transparent);

			using (var canvas = new SKCanvas(bmp))
			using (var paint = new SKPaint())
			{

				var x = bmp.Width / 2;
				var y = bmp.Height / 2;

				paint.Color = SKColors.Red.WithAlpha(alpha);
				canvas.DrawRect(SKRect.Create(0, 0, x, y), paint);

				paint.Color = SKColors.Green.WithAlpha(alpha);
				canvas.DrawRect(SKRect.Create(x, 0, x, y), paint);

				paint.Color = SKColors.Blue.WithAlpha(alpha);
				canvas.DrawRect(SKRect.Create(0, y, x, y), paint);

				paint.Color = SKColors.Yellow.WithAlpha(alpha);
				canvas.DrawRect(SKRect.Create(x, y, x, y), paint);
			}

			return bmp;
		}

		private static class MacPlatformDetector
		{
			internal static readonly Lazy<bool> IsMac = new Lazy<bool> (IsRunningOnMac);

			[DllImport ("libc")]
			static extern int uname (IntPtr buf);

			static bool IsRunningOnMac ()
			{
				IntPtr buf = IntPtr.Zero;
				try {
					buf = Marshal.AllocHGlobal (8192);
					// This is a hacktastic way of getting sysname from uname ()
					if (uname (buf) == 0) {
						string os = Marshal.PtrToStringAnsi (buf);
						if (os == "Darwin")
							return true;
					}
				} catch {
				} finally {
					if (buf != IntPtr.Zero)
						Marshal.FreeHGlobal (buf);
				}
				return false;
			}
		}

		public static class MacDynamicLibraries
		{
			private const string SystemLibrary = "/usr/lib/libSystem.dylib";
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlopen(string path, int mode);
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);
			[DllImport(SystemLibrary)]
			public static extern void dlclose(IntPtr handle);
		}

		public static class LinuxDynamicLibraries
		{
			private const string SystemLibrary = "libdl.so";
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlopen(string path, int mode);
			[DllImport(SystemLibrary)]
			public static extern IntPtr dlsym(IntPtr handle, string symbol);
			[DllImport(SystemLibrary)]
			public static extern void dlclose(IntPtr handle);
		}

		public static class WindowsDynamicLibraries
		{
			private const string SystemLibrary = "Kernel32.dll";
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr LoadLibrary(string lpFileName);
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
			[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
			public static extern void FreeLibrary(IntPtr hModule);
		}

		protected GlContext CreateGlContext()
		{
			try {
				if (IsLinux) {
					return new GlxContext();
				} else if (IsMac) {
					return new CglContext();
				} else if (IsWindows) {
					return new WglContext();
				} else {
					throw new PlatformNotSupportedException();
				}
			} catch (Exception ex) {
				throw new SkipException("Unable to create GL context: " + ex.Message);
			}
		}

		//private void TestGlVersion()
		//{
		//	var minimumVersion = new Version(1, 5);
		//	string versionString = null;

		//	if (IsLinux) {
		//	} else if (IsMac) {
		//	} else if (IsWindows) {
		//		versionString = Wgl.VersionString;
		//	} else {
		//	}

		//	// OpenGL version number is 'MAJOR.MINOR***'
		//	var versionNumber = versionString?.Trim()?.Split(' ')?.FirstOrDefault();

		//	Version version;
		//	if (versionNumber != null && Version.TryParse(versionNumber, out version)) {
		//		if (version < minimumVersion) {
		//			Assert.Ignore($"Available OpenGL version ({versionString}) is below minimum ({minimumVersion}).");
		//		}
		//	}
		//}
	}
}
