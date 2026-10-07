using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp.Views.Gtk4.Tests
{
	internal static class ModuleInit
	{
		[ModuleInitializer]
		internal static void Initialize()
		{
			GLib.Module.Initialize();
			Gdk.Module.Initialize();
			Cairo.Module.Initialize();
			Graphene.Module.Initialize();
			if (!OperatingSystem.IsLinux() ||
				!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
				!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
				global::Gtk.Module.Initialize();
		}
	}
}
