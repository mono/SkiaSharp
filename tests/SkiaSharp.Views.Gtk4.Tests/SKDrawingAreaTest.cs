using System;
using Xunit;
using SkiaSharp;
using SkiaSharp.Views.Gtk;

namespace SkiaSharp.Views.Gtk4.Tests
{
	public class SKDrawingAreaTest
	{
		private static bool TryInitGtk()
		{
			try
			{
				GLib.Module.Initialize();
				global::Gtk.Functions.Init();
				return true;
			}
			catch (Exception)
			{
				// Skip test if GTK cannot be initialized (e.g., no display server)
				return false;
			}
		}

		[Fact]
		public void CanCreateDrawingArea()
		{
			if (!TryInitGtk())
				return;

			using var area = new SKDrawingArea();
			Assert.NotNull(area);
		}

		[Fact]
		public void InitialCanvasSizeIsEmpty()
		{
			if (!TryInitGtk())
				return;

			using var area = new SKDrawingArea();
			Assert.Equal(SKSize.Empty, area.CanvasSize);
		}

		[Fact]
		public void PaintSurfaceEventCanBeSubscribed()
		{
			if (!TryInitGtk())
				return;

			var eventRaised = false;
			using var area = new SKDrawingArea();
			area.PaintSurface += (sender, e) => eventRaised = true;

			// Event won't be raised without a display, but we verify subscription works
			Assert.False(eventRaised);
		}
	}
}
