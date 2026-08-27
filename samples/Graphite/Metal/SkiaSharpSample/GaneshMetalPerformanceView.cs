using System.Diagnostics;
using CoreGraphics;
using Metal;
using SkiaSharp;

namespace SkiaSharpSample;

internal sealed class GaneshMetalPerformanceView : PerformanceMetalView
{
	private IMTLCommandQueue? commandQueue;
	private GRMtlBackendContext? backendContext;
	private GRContext? context;
	private GaneshTileSet? tileSet;
	private readonly Stopwatch lifetime = Stopwatch.StartNew();

	public GaneshMetalPerformanceView(
		CGRect frame,
		RenderSettingsStore settings)
		: base(frame, settings)
	{
	}

	protected override FrameMeasurement? DrawFrame(RenderSettings settings)
	{
		var frame = Stopwatch.StartNew();
		EnsureRenderer();

		var drawable = CurrentDrawable;
		var texture = drawable?.Texture;
		if (drawable is null || texture is null ||
			context is null || commandQueue is null)
		{
			return null;
		}

		var width = (int)DrawableSize.Width;
		var height = (int)DrawableSize.Height;
		if (width <= 0 || height <= 0)
			return null;

		var grid = TileGrid.Create(width, height, settings.WorkerCount);
		tileSet ??= new GaneshTileSet();
		tileSet.Ensure(context, grid, settings.WorkerCount);

		var tileWatch = Stopwatch.StartNew();
		var images = tileSet.Render(settings, lifetime.Elapsed.TotalSeconds);
		tileWatch.Stop();

		try
		{
			var metalInfo = new GRMtlTextureInfo(texture.Handle);
			using var renderTarget = new GRBackendRenderTarget(width, height, metalInfo);
			using var surface = SKSurface.Create(
				context,
				renderTarget,
				GRSurfaceOrigin.TopLeft,
				SKColorType.Bgra8888)
				?? throw new InvalidOperationException(
					"Unable to create the Ganesh drawable surface.");

			var canvas = surface.Canvas;
			canvas.Clear(new SKColor(0x05, 0x07, 0x0E));
			for (var i = 0; i < images.Length; i++)
			{
				canvas.DrawImage(
					images[i],
					grid.Destination(i),
					SKSamplingOptions.Default);
			}

			context.Flush(submit: true, synchronous: false);
			using var commandBuffer = commandQueue.CommandBuffer()
				?? throw new InvalidOperationException(
					"Unable to create a Ganesh presentation command buffer.");
			commandBuffer.PresentDrawable(drawable);
			commandBuffer.Commit();
		}
		finally
		{
			foreach (var image in images)
				image.Dispose();
		}

		frame.Stop();
		return new FrameMeasurement(
			CpuFrameMs: frame.Elapsed.TotalMilliseconds,
			TileWallMs: tileWatch.Elapsed.TotalMilliseconds,
			UiThreadBusyMs: frame.Elapsed.TotalMilliseconds,
			ContentUpdated: true);
	}

	protected override void QuiesceRenderer() =>
		context?.Flush(submit: true, synchronous: true);

	protected override void DisposeRenderer()
	{
		tileSet?.Dispose();
		tileSet = null;
		context?.Flush(submit: true, synchronous: true);
		context?.Dispose();
		context = null;
		backendContext?.Dispose();
		backendContext = null;
		commandQueue?.Dispose();
		commandQueue = null;
	}

	private void EnsureRenderer()
	{
		if (context is not null)
			return;

		var device = Device
			?? throw new PlatformNotSupportedException("No Metal device is available.");
		commandQueue = device.CreateCommandQueue()
			?? throw new PlatformNotSupportedException(
				"Unable to create a Ganesh Metal command queue.");
		backendContext = new GRMtlBackendContext
		{
			DeviceHandle = device.Handle,
			QueueHandle = commandQueue.Handle,
		};
		context = GRContext.CreateMetal(backendContext)
			?? throw new PlatformNotSupportedException(
				"Unable to create a Ganesh Metal context.");
	}

	private sealed class GaneshTileSet : IDisposable
	{
		private SKSurface[] surfaces = Array.Empty<SKSurface>();
		private TileWorkloadPainter[] painters =
			Array.Empty<TileWorkloadPainter>();
		private int width;
		private int height;

		public void Ensure(GRContext context, TileGrid grid, int count)
		{
			if (surfaces.Length == count &&
				width == grid.TileWidth &&
				height == grid.TileHeight)
			{
				return;
			}

			Dispose();
			width = grid.TileWidth;
			height = grid.TileHeight;
			surfaces = new SKSurface[count];
			painters = new TileWorkloadPainter[count];
			var info = new SKImageInfo(
				width,
				height,
				SKColorType.Bgra8888,
				SKAlphaType.Premul);

			try
			{
				for (var i = 0; i < count; i++)
				{
					surfaces[i] = SKSurface.Create(context, budgeted: true, info)
						?? throw new InvalidOperationException(
							$"Unable to create Ganesh tile {i}.");
					painters[i] = new TileWorkloadPainter(i);
				}
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public SKImage[] Render(
			RenderSettings settings,
			double elapsedSeconds)
		{
			var images = new SKImage[surfaces.Length];
			var info = new SKImageInfo(
				width,
				height,
				SKColorType.Bgra8888,
				SKAlphaType.Premul);

			try
			{
				for (var i = 0; i < surfaces.Length; i++)
				{
					painters[i].Draw(
						surfaces[i].Canvas,
						info,
						settings,
						elapsedSeconds);
					images[i] = surfaces[i].Snapshot()
						?? throw new InvalidOperationException(
							$"Unable to snapshot Ganesh tile {i}.");
				}
				return images;
			}
			catch
			{
				foreach (var image in images)
					image?.Dispose();
				throw;
			}
		}

		public void Dispose()
		{
			for (var i = surfaces.Length - 1; i >= 0; i--)
			{
				painters[i]?.Dispose();
				surfaces[i]?.Dispose();
			}
			painters = Array.Empty<TileWorkloadPainter>();
			surfaces = Array.Empty<SKSurface>();
			width = 0;
			height = 0;
		}
	}
}
