using System.Diagnostics;
using SkiaSharp;

namespace SkiaSharpSample;

internal sealed class GraphiteTileWorker : IDisposable
{
	private readonly int index;
	private readonly SKGraphiteRecorder recorder;
	private readonly AutoResetEvent start = new(false);
	private readonly ManualResetEventSlim done = new(false);
	private readonly ManualResetEventSlim ready = new(false);
	private readonly Thread thread;

	private volatile bool stopping;
	private Request request;
	private GraphiteTileResult? result;
	private Exception? failure;

	public GraphiteTileWorker(
		SKGraphiteContext context,
		int index)
	{
		this.index = index;
		recorder = CreateRecorder(context, index);

		thread = new Thread(Run)
		{
			IsBackground = true,
			Name = $"Graphite tile Recorder {index}",
		};
		var started = false;
		try
		{
			thread.Start();
			started = true;
			ready.Wait();
			ThrowIfFailed();
		}
		catch
		{
			stopping = true;
			if (started)
			{
				start.Set();
				thread.Join();
			}
			else
			{
				recorder.Dispose();
			}
			done.Dispose();
			start.Dispose();
			ready.Dispose();
			throw;
		}
	}

	public bool IsCompleted => done.IsSet;

	public void Start(
		int width,
		int height,
		RenderSettings settings,
		double elapsedSeconds)
	{
		ThrowIfFailed();
		if (result is not null)
			throw new InvalidOperationException(
				$"Graphite worker {index} still owns its previous result.");

		request = new Request(width, height, settings, elapsedSeconds);
		done.Reset();
		start.Set();
	}

	public GraphiteTileResult Finish()
	{
		done.Wait();
		ThrowIfFailed();
		var completed = result
			?? throw new InvalidOperationException(
				$"Graphite worker {index} completed without a result.");
		result = null;
		return completed;
	}

	public void Dispose()
	{
		stopping = true;
		start.Set();
		thread.Join();
		done.Dispose();
		start.Dispose();
		ready.Dispose();
	}

	private void Run()
	{
		SKSurface? surface = null;
		TileWorkloadPainter? painter = null;
		var width = 0;
		var height = 0;

		try
		{
			painter = new TileWorkloadPainter(index);
			ready.Set();

			while (true)
			{
				start.WaitOne();
				if (stopping)
					break;

				var current = request;
				if (surface is null ||
					width != current.Width ||
					height != current.Height)
				{
					surface?.Dispose();
					width = current.Width;
					height = current.Height;
					surface = SKSurface.Create(
						recorder,
						new SKImageInfo(
							width,
							height,
							SKColorType.Bgra8888,
							SKAlphaType.Premul))
						?? throw new InvalidOperationException(
							$"Unable to create Graphite tile surface {index}.");
				}

				var stopwatch = Stopwatch.StartNew();
				painter.Draw(
					surface.Canvas,
					new SKImageInfo(
						width,
						height,
						SKColorType.Bgra8888,
						SKAlphaType.Premul),
					current.Settings,
					current.ElapsedSeconds);
				var image = surface.Snapshot()
					?? throw new InvalidOperationException(
						$"Unable to snapshot Graphite tile {index}.");
				var recording = recorder.Snap();
				if (recording is null)
				{
					image.Dispose();
					throw new InvalidOperationException(
						$"Graphite worker Recorder {index}.Snap() returned null.");
				}
				stopwatch.Stop();
				result = new GraphiteTileResult(
					image,
					recording,
					stopwatch.Elapsed.TotalMilliseconds);
				done.Set();
			}
		}
		catch (Exception exception)
		{
			failure = exception;
			ready.Set();
			done.Set();
		}
		finally
		{
			result?.Dispose();
			painter?.Dispose();
			surface?.Dispose();
			recorder.Dispose();
		}
	}

	private void ThrowIfFailed()
	{
		if (failure is not null)
			throw new InvalidOperationException(
				$"Graphite worker {index} failed.",
				failure);
	}

	private static SKGraphiteRecorder CreateRecorder(
		SKGraphiteContext context,
		int index)
	{
		var imageCache = new SKGraphiteImageCache();
		var recorder = context.CreateRecorder(
			-1,
			imageCache.FindOrCreate,
			imageCache.Dispose);
		if (recorder is not null)
			return recorder;

		imageCache.Dispose();
		throw new InvalidOperationException(
			$"Unable to create Graphite worker Recorder {index}.");
	}

	private readonly record struct Request(
		int Width,
		int Height,
		RenderSettings Settings,
		double ElapsedSeconds);
}

internal sealed class GraphiteTileResult : IDisposable
{
	private SKImage? image;
	private SKGraphiteRecording? recording;

	public GraphiteTileResult(
		SKImage image,
		SKGraphiteRecording recording,
		double recordMs)
	{
		this.image = image;
		this.recording = recording;
		RecordMs = recordMs;
	}

	public SKGraphiteRecording Recording =>
		recording
		?? throw new ObjectDisposedException(nameof(GraphiteTileResult));

	public SKImage TakeImage()
	{
		var value = image
			?? throw new ObjectDisposedException(nameof(GraphiteTileResult));
		image = null;
		return value;
	}

	public double RecordMs { get; }

	public void Dispose()
	{
		recording?.Dispose();
		recording = null;
		image?.Dispose();
		image = null;
	}
}
