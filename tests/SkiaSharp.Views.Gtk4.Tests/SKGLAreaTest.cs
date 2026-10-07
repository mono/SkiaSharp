using System;
using SkiaSharp.Views.Gtk;
using Xunit;

namespace SkiaSharp.Views.Gtk4.Tests
{
	[Collection("GTK widgets")]
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
		public void InitialStateDoesNotRequireGlContext()
		{
			SKDrawingAreaTest.InitGtk();

			using var view = new SKGLArea();
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableRenderLoop);
			Assert.Equal(global::Gdk.GLAPI.Gl | global::Gdk.GLAPI.Gles, view.GetAllowedApis());

			view.EnableRenderLoop = true;
			Assert.True(view.EnableRenderLoop);
			view.EnableRenderLoop = false;
			Assert.Null(view.GRContext);
		}
	}
}
