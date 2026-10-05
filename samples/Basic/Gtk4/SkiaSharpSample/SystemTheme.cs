using System;
using System.Runtime.InteropServices;
using Gtk;
using Microsoft.Win32;

namespace SkiaSharpSample;

internal static class SystemTheme
{
	public static bool IsDarkMode
	{
		get
		{
			// GTK 4.20 does not report the macOS system appearance through Gtk.Settings.
			if (OperatingSystem.IsMacOS())
			{
				var app = MacAppearance.Send(MacAppearance.GetClass("NSApplication"), MacAppearance.GetSelector("sharedApplication"));
				var appearance = MacAppearance.Send(app, MacAppearance.GetSelector("effectiveAppearance"));
				var name = MacAppearance.Send(appearance, MacAppearance.GetSelector("name"));
				var utf8 = MacAppearance.Send(name, MacAppearance.GetSelector("UTF8String"));
				return (Marshal.PtrToStringUTF8(utf8)
					?? throw new InvalidOperationException("Unable to read the macOS application appearance."))
					.Contains("Dark", StringComparison.OrdinalIgnoreCase);
			}

			if (OperatingSystem.IsWindows())
				return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int value && value == 0;

			var settings = Settings.GetDefault();
			return settings != null && (settings.GtkApplicationPreferDarkTheme ||
				(settings.GtkThemeName?.Contains("dark", StringComparison.OrdinalIgnoreCase) ?? false));
		}
	}

	public static void Apply()
	{
		if ((OperatingSystem.IsMacOS() || OperatingSystem.IsWindows()) && Settings.GetDefault() is { } settings)
		{
			var dark = IsDarkMode;
			if (settings.GtkApplicationPreferDarkTheme != dark)
				settings.GtkApplicationPreferDarkTheme = dark;
		}
	}

	private static class MacAppearance
	{
		private const string ObjC = "/usr/lib/libobjc.A.dylib";

		[DllImport(ObjC, EntryPoint = "objc_getClass")]
		public static extern nint GetClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

		[DllImport(ObjC, EntryPoint = "sel_registerName")]
		public static extern nint GetSelector([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

		[DllImport(ObjC, EntryPoint = "objc_msgSend")]
		public static extern nint Send(nint receiver, nint selector);
	}
}
