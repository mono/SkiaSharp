using System.Runtime.InteropServices;
using SkiaSharpSample;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class Gtk4RuntimeTests
{
	[Theory]
	[InlineData("libgtk-4.so.1", "gtk-4-1.dll", "libgtk-4-1.dll")]
	[InlineData("libcairo.so.2", "cairo-2.dll", "libcairo-2.dll")]
	[InlineData("libfontconfig.so.1", "fontconfig-1.dll", "libfontconfig-1.dll")]
	[InlineData("libpangocairo-1.0.so.0", "pangocairo-1.0-0.dll", "libpangocairo-1.0-0.dll")]
	[InlineData("libpangoft2-1.0.so.0", "pangoft2-1.0-0.dll", "libpangoft2-1.0-0.dll")]
	public void WindowsSupportsMsvcAndMingwLibraryNames(string soname, string msvc, string mingw) =>
		Assert.Equal(new[] { msvc, mingw }, Gtk4Runtime.GetLibraryNames(soname, OSPlatform.Windows));

	[Theory]
	[InlineData("libgtk-4.so.1", "libgtk-4.1.dylib")]
	[InlineData("libfontconfig.so.1", "libfontconfig.1.dylib")]
	[InlineData("libpangocairo-1.0.so.0", "libpangocairo-1.0.0.dylib")]
	public void MacUsesDylibNames(string soname, string dylib) =>
		Assert.Equal(new[] { dylib }, Gtk4Runtime.GetLibraryNames(soname, OSPlatform.OSX));

	[Fact]
	public void LinuxAndUnrelatedImportsRemainUnchanged()
	{
		Assert.Equal(new[] { "libgtk-4.so.1" }, Gtk4Runtime.GetLibraryNames("libgtk-4.so.1", OSPlatform.Linux));
		Assert.Equal(new[] { "libc" }, Gtk4Runtime.GetLibraryNames("libc", OSPlatform.Windows));
	}
}
