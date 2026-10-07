using System;
using Xunit;
using SkiaSharp;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Gtk4.Tests
{
	[CollectionDefinition("GTK widgets", DisableParallelization = true)]
	public class GtkWidgetCollection { }

	[Collection("GTK widgets")]
	public class SKDrawingAreaTest
	{
		private static bool initialized;

		internal static void InitGtk()
		{
			if (initialized)
				return;
			// gtk_init()/gtk_init_check() call native exit() when no display can be opened, which
			// cannot be caught as a managed exception and aborts the whole test host. Skip up-front on
			// headless environments (no X11/Wayland display) before touching any GTK display API.
			var hasDisplay =
				!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
				!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
			if (OperatingSystem.IsLinux() && !hasDisplay)
				Assert.Skip("GTK cannot be initialized: no display available");

			global::Gtk.Module.Initialize();

			if (!global::Gtk.Functions.InitCheck())
				Assert.Skip("GTK cannot be initialized: no display available");
			initialized = true;
		}

		[Fact]
		public void CanCreateDrawingArea()
		{
			InitGtk();

			using var area = new SKDrawingArea();
			Assert.NotNull(area);
		}

		[Fact]
		public void InitialCanvasSizeIsEmpty()
		{
			InitGtk();

			using var area = new SKDrawingArea();
			Assert.Equal(SKSize.Empty, area.CanvasSize);
			Assert.False(area.IgnorePixelScaling);
		}

		[Fact]
		public void PaintSurfaceEventCanBeSubscribed()
		{
			InitGtk();

			var eventRaised = false;
			using var area = new SKDrawingArea();
			area.PaintSurface += (sender, e) => eventRaised = true;

			// Event won't be raised without a display, but we verify subscription works
			Assert.False(eventRaised);
		}
	}
}
