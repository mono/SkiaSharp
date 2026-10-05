using System;
using System.Linq;
using System.Reflection;
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
		public void ApiMatchesNativePaintPattern()
		{
			var type = typeof(SKGLView);
			var paint = type.GetMethod("OnPaintSurface", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(paint);
			Assert.True(paint.IsFamily);
			Assert.True(paint.IsVirtual);
			Assert.Equal(typeof(void), paint.ReturnType);
			Assert.Equal(typeof(Desktop.SKPaintGLSurfaceEventArgs), Assert.Single(paint.GetParameters()).ParameterType);
			Assert.Null(type.GetEvent("ContextChanged"));
			Assert.Null(type.GetMethod("ReleaseGlResources"));
			Assert.Equal(new[] { "CanvasSize", "EnableRenderLoop", "GRContext", "IgnorePixelScaling" },
				type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Select(property => property.Name).OrderBy(name => name));
			Assert.Equal(new[] { "linux", "macos", "windows" },
				type.GetCustomAttributes<SupportedOSPlatformAttribute>().Select(attribute => attribute.PlatformName).OrderBy(name => name));
		}

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

			using var view = new SKGLView();
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableRenderLoop);

			view.EnableRenderLoop = true;
			Assert.True(view.EnableRenderLoop);
			view.EnableRenderLoop = false;
			Assert.Null(view.GRContext);
		}
	}
}
