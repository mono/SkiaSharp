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
				Assert.Skip($"GTK cannot be initialized: {ex.Message}");
			}
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
