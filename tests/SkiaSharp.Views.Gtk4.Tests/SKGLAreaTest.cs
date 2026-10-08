using System;
using System.Runtime.Versioning;
using SkiaSharp.Views.Gtk;
using Xunit;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class SKGLAreaTest
	{
		[Fact]
		public void EpoxyExportsGlDispatchPointers()
		{
			Assert.NotEqual(nint.Zero, GtkGl.GetProcedureAddress("glGetIntegerv"));
			Assert.NotEqual(nint.Zero, GtkGl.GetProcedureAddress("glGetString"));
			Assert.NotEqual(nint.Zero, GtkGl.GetProcedureAddress("glBindFramebuffer"));
			Assert.NotEqual(nint.Zero, GtkGl.GetProcedureAddress("glGetFramebufferAttachmentParameteriv"));
			Assert.Equal(nint.Zero, GtkGl.GetProcedureAddress("glMissingSkiaSharpTestFunction"));
		}

		[Fact]
		[SupportedOSPlatform("linux")]
		public void InitialStateDoesNotRequireGlContext()
		{
			if (!OperatingSystem.IsLinux())
				Assert.Skip("GTK4 widget tests require a Linux GTK display; macOS GUI must run on the application main thread.");

			SKDrawingAreaTest.InitGtk();

			using var view = new SKGLArea();
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableRenderLoop);
			Assert.False(view.GetAutoRender());
			Assert.True(view.GetHasStencilBuffer());

			view.EnableRenderLoop = true;
			Assert.True(view.EnableRenderLoop);
			view.EnableRenderLoop = false;
			Assert.Null(view.GRContext);
		}
	}
}
