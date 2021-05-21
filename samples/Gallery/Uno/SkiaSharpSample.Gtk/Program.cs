using System;
using GLib;
using Uno.UI.Runtime.Skia;

namespace SkiaSharpSample.Gtk
{
	class Program
	{
		static void Main(string[] args)
		{
			SkiaSharp.Views.UWP.SKSwapChainPanel.RaiseOnUnsupported = false;

			ExceptionManager.UnhandledException += delegate (UnhandledExceptionArgs expArgs)
			{
				Console.WriteLine("GLIB UNHANDLED EXCEPTION" + expArgs.ExceptionObject.ToString());
				expArgs.ExitApplication = true;
			};

			var host = new GtkHost(() => new App(), args);

			host.Run();
		}
	}
}
