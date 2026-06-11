using System;
using Xunit;
using SkiaSharp;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class SKDrawingAreaTest
	{
		private static void InitGtk()
		{
			var initialized = false;
			try
			{
				global::Gtk.Module.Initialize();

				// Use gtk_init_check() rather than gtk_init(): the latter calls exit() (aborting the
				// whole test host) when no display is available, which cannot be caught as a managed
				// exception. InitCheck() returns false instead, so headless agents skip gracefully.
				initialized = global::Gtk.Functions.InitCheck();
			}
			catch (Exception ex)
			{
				Assert.Skip($"GTK cannot be initialized: {ex.Message}");
			}

			if (!initialized)
				Assert.Skip("GTK cannot be initialized: no display available");
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
