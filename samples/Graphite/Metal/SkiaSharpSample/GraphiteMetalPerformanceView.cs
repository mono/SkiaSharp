using System.Diagnostics;
using System.Runtime.InteropServices;
using CoreGraphics;
using Metal;
using SkiaSharp;

namespace SkiaSharpSample;

internal sealed class GraphiteMetalPerformanceView : PerformanceMetalView
{
	private readonly Stopwatch lifetime = Stopwatch.StartNew();
	private readonly List<GraphiteTileWorker> workers = new();
	private SKImage[] displayImages = Array.Empty<SKImage>();
	private IMTLCommandQueue? commandQueue;
	private SKGraphiteMtlBackendContext? backendContext;
	private SKGraphiteContext? context;
	private SKGraphiteRecorder? presentationRecorder;
	private ulong frameId;
	private bool contextUnrecoverable;
	private bool workersActive;
	private double lastWorkerWallMs;

	public GraphiteMetalPerformanceView(
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
			context is null || presentationRecorder is null ||
			commandQueue is null)
		{
			return null;
		}

		var width = (int)DrawableSize.Width;
		var height = (int)DrawableSize.Height;
		if (width <= 0 || height <= 0)
			return null;

		var contentUpdated = false;
		if (workersActive && workers.All(worker => worker.IsCompleted))
		{
			CompleteWorkerBatch();
			contentUpdated = true;
		}

		if (!workersActive && workers.Count != settings.WorkerCount)
			EnsureWorkers(settings.WorkerCount);

		if (!workersActive)
			StartWorkerBatch(width, height, settings);

		var displayGrid = TileGrid.Create(
			width,
			height,
			Math.Max(1, displayImages.Length));
		using var backendTexture = SKGraphiteBackendTexture.CreateMetal(
			width,
			height,
			texture.Handle)
			?? throw new InvalidOperationException(
				"Unable to wrap the current Metal drawable.");
		using var surface = SKSurface.Create(
			presentationRecorder,
			backendTexture,
			SKColorType.Bgra8888)
			?? throw new InvalidOperationException(
				"Unable to create the Graphite drawable surface.");

		var canvas = surface.Canvas;
		canvas.Clear(new SKColor(0x05, 0x07, 0x0E));
		for (var i = 0; i < displayImages.Length; i++)
		{
			canvas.DrawImage(
				displayImages[i],
				displayGrid.Destination(i),
				SKSamplingOptions.Default);
		}

		using var presentation = presentationRecorder.Snap()
			?? throw new InvalidOperationException(
				"Unable to snap the Graphite presentation recording.");
		ThrowIfInsertFailed(
			context.InsertRecording(presentation),
			"presentation");

		if (!context.Submit(new SKGraphiteSubmitInfo
		{
			Sync = false,
			MarkBoundary = true,
			FrameID = ++frameId,
		}))
		{
			contextUnrecoverable = true;
			throw new InvalidOperationException(
				"Unable to submit the Graphite frame.");
		}

		using var commandBuffer = commandQueue.CommandBuffer()
			?? throw new InvalidOperationException(
				"Unable to create a Graphite presentation command buffer.");
		commandBuffer.PresentDrawable(drawable);
		commandBuffer.Commit();

		frame.Stop();
		return new FrameMeasurement(
			CpuFrameMs: frame.Elapsed.TotalMilliseconds,
			TileWallMs: lastWorkerWallMs,
			UiThreadBusyMs: frame.Elapsed.TotalMilliseconds,
			ContentUpdated: contentUpdated);
	}

	protected override void QuiesceRenderer()
	{
		var contextIsHealthy = context is not null &&
			!context.IsDeviceLost &&
			!contextUnrecoverable;
		if (workersActive)
		{
			if (contextIsHealthy)
				CompleteWorkerBatch();
			else if (DiscardWorkerBatch() is { } workerFailure)
				throw workerFailure;
		}

		if (!contextIsHealthy)
			return;

		if (!context!.Submit(new SKGraphiteSubmitInfo { Sync = true }))
		{
			contextUnrecoverable = true;
			throw new InvalidOperationException(
				"Unable to quiesce the Graphite context.");
		}
	}

	protected override void DisposeRenderer()
	{
		try
		{
			QuiesceRenderer();
		}
		finally
		{
			DisposeWorkers();
			DisposeDisplayImages();
			presentationRecorder?.Dispose();
			presentationRecorder = null;
			context?.Dispose();
			context = null;
			backendContext?.Dispose();
			backendContext = null;
			commandQueue?.Dispose();
			commandQueue = null;
		}
	}

	private void EnsureRenderer()
	{
		if (context is not null)
			return;

		if (!SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Metal))
			throw new PlatformNotSupportedException(
				"The loaded libSkiaSharp does not include Graphite Metal.");

		var device = Device
			?? throw new PlatformNotSupportedException("No Metal device is available.");
		if (!MetalCanDriveGraphite(device.Handle))
			throw new PlatformNotSupportedException(
				"The Metal device does not support Apple7+ or Mac2, required by Graphite.");

		commandQueue = device.CreateCommandQueue()
			?? throw new PlatformNotSupportedException(
				"Unable to create a Graphite Metal command queue.");
		backendContext = new SKGraphiteMtlBackendContext
		{
			MtlDevice = device.Handle,
			MtlQueue = commandQueue.Handle,
		};
		context = SKGraphiteContext.CreateMetal(backendContext)
			?? throw new PlatformNotSupportedException(
				"Unable to create a Graphite Metal context.");

		var imageCache = new SKGraphiteImageCache();
		presentationRecorder = context.CreateRecorder(
			-1,
			imageCache.FindOrCreate,
			imageCache.Dispose);
		if (presentationRecorder is null)
		{
			imageCache.Dispose();
			throw new InvalidOperationException(
				"Unable to create the Graphite presentation Recorder.");
		}
	}

	private void EnsureWorkers(int count)
	{
		if (workers.Count == count)
			return;

		if (workers.Count > 0 &&
			context is not null &&
			!context.IsDeviceLost &&
			!contextUnrecoverable)
		{
			context.Submit(new SKGraphiteSubmitInfo { Sync = true });
		}

		DisposeWorkers();
		DisposeDisplayImages();
		try
		{
			for (var i = 0; i < count; i++)
			{
				workers.Add(new GraphiteTileWorker(
					context ?? throw new InvalidOperationException(
						"Graphite context is unavailable."),
					i));
			}
		}
		catch
		{
			DisposeWorkers();
			throw;
		}
	}

	private void DisposeWorkers()
	{
		var workerFailure = DiscardWorkerBatch();
		for (var i = workers.Count - 1; i >= 0; i--)
			workers[i].Dispose();
		workers.Clear();
		workersActive = false;
		if (workerFailure is not null)
			throw workerFailure;
	}

	private Exception? DiscardWorkerBatch()
	{
		if (!workersActive)
			return null;

		Exception? failure = null;
		for (var i = 0; i < workers.Count; i++)
		{
			try
			{
				workers[i].Finish().Dispose();
			}
			catch (Exception exception)
			{
				failure ??= exception;
			}
		}
		workersActive = false;
		return failure;
	}

	private void StartWorkerBatch(
		int width,
		int height,
		RenderSettings settings)
	{
		var grid = TileGrid.Create(width, height, workers.Count);
		for (var i = 0; i < workers.Count; i++)
		{
			workers[i].Start(
				grid.TileWidth,
				grid.TileHeight,
				settings,
				lifetime.Elapsed.TotalSeconds);
		}
		workersActive = true;
	}

	private void CompleteWorkerBatch()
	{
		if (context is null)
			throw new InvalidOperationException("Graphite context is unavailable.");

		var results = new GraphiteTileResult?[workers.Count];
		Exception? workerFailure = null;
		for (var i = 0; i < workers.Count; i++)
		{
			try
			{
				results[i] = workers[i].Finish();
			}
			catch (Exception exception)
			{
				workerFailure ??= exception;
			}
		}
		workersActive = false;

		if (workerFailure is not null)
		{
			DisposeResults(results);
			throw workerFailure;
		}

		lastWorkerWallMs = results.Max(result => result?.RecordMs ?? 0);
		var nextImages = new SKImage[results.Length];
		try
		{
			for (var i = 0; i < results.Length; i++)
			{
				var result = results[i]
					?? throw new InvalidOperationException(
						$"Graphite worker {i} returned no result.");
				ThrowIfInsertFailed(
					context.InsertRecording(result.Recording),
					$"tile {i}");
				nextImages[i] = result.TakeImage();
			}
		}
		catch
		{
			foreach (var image in nextImages)
				image?.Dispose();
			throw;
		}
		finally
		{
			DisposeResults(results);
		}

		DisposeDisplayImages();
		displayImages = nextImages;
	}

	private void DisposeDisplayImages()
	{
		foreach (var image in displayImages)
			image.Dispose();
		displayImages = Array.Empty<SKImage>();
	}

	private void ThrowIfInsertFailed(
		SKGraphiteInsertStatus status,
		string operation)
	{
		if (status == SKGraphiteInsertStatus.Success)
			return;

		if (status is
			SKGraphiteInsertStatus.AddCommandsFailed or
			SKGraphiteInsertStatus.AsyncShaderCompilesFailed or
			SKGraphiteInsertStatus.OutOfOrderRecording)
		{
			contextUnrecoverable = true;
		}
		throw new InvalidOperationException(
			$"Unable to insert the Graphite {operation} recording: {status}.");
	}

	private static void DisposeResults(GraphiteTileResult?[] results)
	{
		foreach (var result in results)
			result?.Dispose();
	}

	private static bool MetalCanDriveGraphite(IntPtr device)
	{
		var selector = sel_registerName("supportsFamily:");
		foreach (var family in new ulong[] { 1009, 1008, 1007, 2002 })
		{
			if (objc_msgSend_supportsFamily(device, selector, family) != 0)
				return true;
		}
		return false;
	}

	[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
	private static extern byte objc_msgSend_supportsFamily(
		IntPtr receiver,
		IntPtr selector,
		ulong family);

	[DllImport("/usr/lib/libobjc.dylib", CharSet = CharSet.Ansi)]
	private static extern IntPtr sel_registerName(string name);
}
