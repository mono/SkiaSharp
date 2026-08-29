using System.Threading;
using SkiaSharp;
#if __IOS__
using SkiaSharp.Views.iOS;
#elif __MACOS__
using SkiaSharp.Views.Mac;
#elif __TVOS__
using SkiaSharp.Views.tvOS;
#endif

namespace SkiaSharpSample;

internal sealed class GraphiteSceneRenderer : IDisposable
{
	const int WorkerCount = 4;
	const long RecorderBudgetBytes = 32 * 1024 * 1024;

	readonly GraphiteSceneWorker[] workers;
	readonly SKPaint touchPaint = new()
	{
		Color = SKColors.White,
		IsAntialias = true,
		Style = SKPaintStyle.Stroke,
	};

	SKImage[] displayImages = Array.Empty<SKImage>();
	bool workersActive;

	public GraphiteSceneRenderer(SKPaintGraphiteSurfaceEventArgs e)
	{
		workers = new GraphiteSceneWorker[WorkerCount];
		try
		{
			for (var i = 0; i < workers.Length; i++)
			{
				var recorder = e.CreateRecorder(RecorderBudgetBytes)
					?? throw new InvalidOperationException($"Unable to create Graphite Recorder {i}.");
				workers[i] = new GraphiteSceneWorker(recorder, i);
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public void Draw(
		SKPaintGraphiteSurfaceEventArgs e,
		double elapsedSeconds,
		SKPoint? touchPoint)
	{
		// Keep presenting the last complete update while workers prepare the next one.
		if (workersActive && workers.All(worker => worker.IsCompleted))
			CompleteWorkerBatch(e);

		var grid = GraphiteTileGrid.Create(e.Info.Width, e.Info.Height, workers.Length);
		if (!workersActive)
			StartWorkerBatch(grid, elapsedSeconds);

		var canvas = e.Surface.Canvas;
		canvas.Clear(new SKColor(0x05, 0x07, 0x0E));
		for (var i = 0; i < displayImages.Length; i++)
		{
			canvas.DrawImage(
				displayImages[i],
				grid.Destination(i),
				SKSamplingOptions.Default);
		}

		if (touchPoint is { } touch)
		{
			touchPaint.StrokeWidth = Math.Max(3f, Math.Min(e.Info.Width, e.Info.Height) * 0.008f);
			canvas.DrawCircle(
				touch.X * e.Info.Width,
				touch.Y * e.Info.Height,
				Math.Min(e.Info.Width, e.Info.Height) * 0.09f,
				touchPaint);
		}
	}

	public void Dispose()
	{
		for (var i = workers.Length - 1; i >= 0; i--)
			workers[i]?.Dispose();
		foreach (var image in displayImages)
			image.Dispose();
		displayImages = Array.Empty<SKImage>();
		touchPaint.Dispose();
	}

	void StartWorkerBatch(GraphiteTileGrid grid, double elapsedSeconds)
	{
		for (var i = 0; i < workers.Length; i++)
			workers[i].Start(grid.TileWidth, grid.TileHeight, elapsedSeconds);
		workersActive = true;
	}

	void CompleteWorkerBatch(SKPaintGraphiteSurfaceEventArgs e)
	{
		var results = new GraphiteSceneResult?[workers.Length];
		Exception? workerFailure = null;
		for (var i = 0; i < workers.Length; i++)
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

		var nextImages = new SKImage[results.Length];
		try
		{
			for (var i = 0; i < results.Length; i++)
			{
				var result = results[i]
					?? throw new InvalidOperationException($"Graphite worker {i} returned no result.");
				// Producer recordings must precede the presentation recording that samples their images.
				var status = e.InsertRecording(result.Recording);
				if (status != SKGraphiteInsertStatus.Success)
					throw new InvalidOperationException($"Unable to insert Graphite recording {i}: {status}.");
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

		foreach (var image in displayImages)
			image.Dispose();
		displayImages = nextImages;
	}

	static void DisposeResults(GraphiteSceneResult?[] results)
	{
		foreach (var result in results)
			result?.Dispose();
	}
}

internal sealed class GraphiteSceneWorker : IDisposable
{
	readonly int index;
	readonly SKGraphiteRecorder recorder;
	readonly AutoResetEvent start = new(false);
	readonly ManualResetEventSlim done = new(false);
	readonly Thread thread;

	volatile bool stopping;
	GraphiteSceneRequest request;
	GraphiteSceneResult? result;
	Exception? failure;

	public GraphiteSceneWorker(SKGraphiteRecorder recorder, int index)
	{
		this.recorder = recorder;
		this.index = index;
		thread = new Thread(Run)
		{
			IsBackground = true,
			Name = $"Graphite scene Recorder {index}",
		};
		thread.Start();
	}

	public bool IsCompleted => done.IsSet;

	public void Start(int width, int height, double elapsedSeconds)
	{
		ThrowIfFailed();
		if (result is not null)
			throw new InvalidOperationException($"Graphite worker {index} still owns its previous result.");

		request = new GraphiteSceneRequest(width, height, elapsedSeconds);
		done.Reset();
		start.Set();
	}

	public GraphiteSceneResult Finish()
	{
		done.Wait();
		ThrowIfFailed();
		var completed = result
			?? throw new InvalidOperationException($"Graphite worker {index} completed without a result.");
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
	}

	void Run()
	{
		SKSurface? surface = null;
		GraphiteScenePainter? painter = null;
		var width = 0;
		var height = 0;

		try
		{
			painter = new GraphiteScenePainter(index);
			while (true)
			{
				start.WaitOne();
				if (stopping)
					break;

				var current = request;
				if (surface is null || width != current.Width || height != current.Height)
				{
					surface?.Dispose();
					width = current.Width;
					height = current.Height;
					surface = SKSurface.Create(
						recorder,
						new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul))
						?? throw new InvalidOperationException($"Unable to create Graphite surface {index}.");
				}

				painter.Draw(surface.Canvas, width, height, current.ElapsedSeconds);
				var image = surface.Snapshot()
					?? throw new InvalidOperationException($"Unable to snapshot Graphite surface {index}.");
				var recording = recorder.Snap();
				if (recording is null)
				{
					image.Dispose();
					throw new InvalidOperationException($"Graphite Recorder {index}.Snap() returned null.");
				}

				result = new GraphiteSceneResult(image, recording);
				done.Set();
			}
		}
		catch (Exception exception)
		{
			failure = exception;
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

	void ThrowIfFailed()
	{
		if (failure is not null)
			throw new InvalidOperationException($"Graphite worker {index} failed.", failure);
	}
}

internal sealed class GraphiteScenePainter : IDisposable
{
	static readonly SKColor[] Palette =
	{
		new(0x35, 0x8C, 0xFF),
		new(0x7C, 0x4D, 0xFF),
		new(0x00, 0xC9, 0xA7),
		new(0xFF, 0xB0, 0x2E),
		new(0xF4, 0x5B, 0x8A),
	};

	readonly int index;
	readonly SKPaint fill = new() { IsAntialias = true };

	public GraphiteScenePainter(int index) =>
		this.index = index;

	public void Draw(SKCanvas canvas, int width, int height, double elapsedSeconds)
	{
		canvas.Clear(new SKColor(
			(byte)(8 + index * 7),
			(byte)(11 + index * 5),
			(byte)(26 + index * 9)));

		var time = (float)elapsedSeconds;
		var count = Math.Clamp(width * height / 3_000, 180, 480);
		for (var i = 0; i < count; i++)
		{
			var hash = Mix((uint)(i + index * 65_537));
			var phase = time * (0.3f + i % 7 * 0.04f) + i * 0.13f;
			var x = Unit(hash) * width + MathF.Sin(phase) * 8f;
			var y = Unit(Mix(hash + 1)) * height + MathF.Cos(phase * 1.2f) * 8f;
			var size = 3f + Unit(Mix(hash + 2)) * Math.Min(width, height) * 0.08f;
			fill.Color = Palette[(i + index) % Palette.Length].WithAlpha((byte)(80 + hash % 150));

			if ((i & 1) == 0)
				canvas.DrawCircle(x, y, size * 0.5f, fill);
			else
				canvas.DrawRoundRect(new SKRect(x, y, x + size, y + size), size * 0.2f, size * 0.2f, fill);
		}
	}

	public void Dispose() =>
		fill.Dispose();

	static uint Mix(uint value)
	{
		value ^= value >> 16;
		value *= 0x7FEB352D;
		value ^= value >> 15;
		value *= 0x846CA68B;
		return value ^ (value >> 16);
	}

	static float Unit(uint value) =>
		(value & 0x00FF_FFFF) / 16_777_215f;
}

internal sealed class GraphiteSceneResult : IDisposable
{
	SKImage? image;
	SKGraphiteRecording? recording;

	public GraphiteSceneResult(SKImage image, SKGraphiteRecording recording)
	{
		this.image = image;
		this.recording = recording;
	}

	public SKGraphiteRecording Recording =>
		recording ?? throw new ObjectDisposedException(nameof(GraphiteSceneResult));

	public SKImage TakeImage()
	{
		var value = image ?? throw new ObjectDisposedException(nameof(GraphiteSceneResult));
		image = null;
		return value;
	}

	public void Dispose()
	{
		recording?.Dispose();
		recording = null;
		image?.Dispose();
		image = null;
	}
}

internal readonly record struct GraphiteSceneRequest(
	int Width,
	int Height,
	double ElapsedSeconds);

internal readonly record struct GraphiteTileGrid(
	int Columns,
	int TileWidth,
	int TileHeight,
	int Gap)
{
	public static GraphiteTileGrid Create(int width, int height, int tileCount)
	{
		const int gap = 4;
		var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(tileCount)));
		var rows = Math.Max(1, (int)Math.Ceiling(tileCount / (double)columns));
		var tileWidth = Math.Max(1, (width - gap * (columns + 1)) / columns);
		var tileHeight = Math.Max(1, (height - gap * (rows + 1)) / rows);
		return new GraphiteTileGrid(columns, tileWidth, tileHeight, gap);
	}

	public SKRect Destination(int index)
	{
		var column = index % Columns;
		var row = index / Columns;
		var left = Gap + column * (TileWidth + Gap);
		var top = Gap + row * (TileHeight + Gap);
		return new SKRect(left, top, left + TileWidth, top + TileHeight);
	}
}
