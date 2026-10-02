using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SkiaSharp.Views.Gtk;
using Xunit;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class SKGLViewTest
	{
		[Fact]
		public void IsNativeGtkGlArea() =>
			Assert.True(typeof(global::Gtk.GLArea).IsAssignableFrom(typeof(SKGLView)));

		[Fact]
		[SupportedOSPlatform("linux")]
		public void InitialStateDoesNotRequireGlContext()
		{
			if (!OperatingSystem.IsLinux())
				Assert.Skip("GTK4 widget tests require a Linux GTK display; macOS GUI must run on the application main thread.");

			SKDrawingAreaTest.InitGtk();

			using var view = new SKGLView();
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableRenderLoop);

			view.EnableRenderLoop = true;
			Assert.True(view.EnableRenderLoop);
			view.EnableRenderLoop = false;
			view.ReleaseGlResources();
			view.ReleaseGlResources();
			Assert.Null(view.GRContext);
		}

		[Fact]
		[SupportedOSPlatform("macos")]
		public void MacOpenGlFrameworkExportsFramebufferQuery()
		{
			if (!OperatingSystem.IsMacOS())
				Assert.Skip("The macOS OpenGL framework is only present on macOS.");

			var library = NativeLibrary.Load("/System/Library/Frameworks/OpenGL.framework/OpenGL");
			try
			{
				Assert.NotEqual(IntPtr.Zero, NativeLibrary.GetExport(library, "glGetIntegerv"));
			}
			finally
			{
				NativeLibrary.Free(library);
			}
		}
	}
}
