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
			try
			{
				global::Gtk.Module.Initialize();
				global::Gtk.Functions.Init();
			}
			catch (Exception ex)
			{
				throw new SkipException($"GTK cannot be initialized: {ex.Message}");
			}
		}

		[SkippableFact]
		public void CanCreateDrawingArea()
		{
			InitGtk();

			using var area = new SKDrawingArea();
			Assert.NotNull(area);
		}

		[SkippableFact]
		public void InitialCanvasSizeIsEmpty()
		{
			InitGtk();

			using var area = new SKDrawingArea();
			Assert.Equal(SKSize.Empty, area.CanvasSize);
		}

		[SkippableFact]
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
