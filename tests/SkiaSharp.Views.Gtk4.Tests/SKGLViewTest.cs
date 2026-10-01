using System;
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
				Assert.Skip("GTK4 GPU rendering requires a Linux OpenGL display.");

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
	}
}
