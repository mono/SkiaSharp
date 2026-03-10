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
		}
	}
}
