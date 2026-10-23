using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Maui.Platforms.Linux.Gtk4.Platform;

namespace SkiaSharpSample;

[UnconditionalSuppressMessage("Interoperability", "CA1416",
	Justification = "The GTK4 host resolves the pinned backend's Linux library names on Windows and macOS.")]
internal static class Gtk4Runtime
{
	public static void Initialize()
	{
		if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
			NativeLibrary.SetDllImportResolver(typeof(GtkMauiApplication).Assembly, ResolveLibrary);

		GLib.Module.Initialize();
		// The MAUI font registrar uses Pango's fontconfig API, not its Win32/Quartz font map.
		if (!GLib.Functions.Setenv("PANGOCAIRO_BACKEND", "fc", true))
			throw new InvalidOperationException("Unable to select the fontconfig Pango backend required by MAUI GTK4.");
		Gdk.Module.Initialize();
		Cairo.Module.Initialize();
		Graphene.Module.Initialize();

		// The pinned MAUI host uses Run rather than GirCore's RunWithSynchronizationContext.
		SynchronizationContext.SetSynchronizationContext(new GLib.Internal.MainLoopSynchronizationContext());
	}

	private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		var platform = OperatingSystem.IsWindows() ? OSPlatform.Windows : OSPlatform.OSX;
		var names = GetLibraryNames(libraryName, platform);
		if (names.Length == 1 && names[0] == libraryName)
			return nint.Zero;

		foreach (var name in names)
		{
			if (NativeLibrary.TryLoad(name, assembly, searchPath, out var handle))
				return handle;
		}

		throw new DllNotFoundException($"Unable to load GTK4 library ({string.Join(", ", names)}). Include the GTK4 runtime directory in the native library search path.");
	}

	internal static string[] GetLibraryNames(string libraryName, OSPlatform platform)
	{
		var separator = libraryName.IndexOf(".so.", StringComparison.Ordinal);
		if (!libraryName.StartsWith("lib", StringComparison.Ordinal) || separator < 3)
			return [libraryName];

		var name = libraryName[..separator];
		var version = libraryName[(separator + 4)..];
		if (platform == OSPlatform.Windows)
			return [$"{name[3..]}-{version}.dll", $"{name}-{version}.dll"];
		if (platform == OSPlatform.OSX)
			return [$"{name}.{version}.dylib"];
		return [libraryName];
	}
}
