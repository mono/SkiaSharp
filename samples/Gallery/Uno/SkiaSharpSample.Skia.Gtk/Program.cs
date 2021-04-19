using System;
using System.Reflection;
using System.Runtime.InteropServices;
using GLib;
using Uno.UI.Runtime.Skia;

namespace SkiaSharpSample.Skia.Gtk
{
	class Program
	{
		static void Main(string[] args)
		{
			NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, ImportResolver);

			SkiaSharp.Views.UWP.SKSwapChainPanel.RaiseOnUnsupported = false;

			ExceptionManager.UnhandledException += delegate (UnhandledExceptionArgs expArgs)
			{
				Console.WriteLine("GLIB UNHANDLED EXCEPTION" + expArgs.ExceptionObject.ToString());
				expArgs.ExitApplication = true;
			};

			var host = new GtkHost(() => new App(), args);

			host.Run();
		}

		private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
		{
			IntPtr libHandle = IntPtr.Zero;

			var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				NativeLibrary.TryLoad("./runtimes/win-{arch}/native/libSkiaSharp.dll", out libHandle);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				NativeLibrary.TryLoad("./runtimes/linux-{arch}/native/libSkiaSharp.so", out libHandle);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				NativeLibrary.TryLoad("./runtimes/osx/native/libSkiaSharp.dylib", out libHandle);
			}

			return libHandle;
		}
	}
}
