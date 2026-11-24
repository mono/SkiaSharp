using System.Runtime.CompilerServices;
using Xunit;

// GTK initialization and widget access must not run concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

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
