using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace SkiaSharpSample;

public partial class GraphitePage : ContentPage
{
	private readonly FpsCounter fpsCounter = new ();
	private readonly object graphiteLock = new ();

	private GraphiteRecordingWorker[]? workers;
	private SKGraphiteContext? workerContext;
	private SKGraphiteRecording? backgroundRecording;
	private SKSizeI backgroundSize;
	private ulong frameNumber;
	private int particleCount = 480;
	private int parallelRecording = 1;
	private long lastStatsUpdate;
	private bool pageActive;
	private volatile float touchX;
	private volatile float touchY;
	private volatile bool touchActive;

	public GraphitePage ()
	{
		InitializeComponent ();
	}

	protected override void OnAppearing ()
	{
		base.OnAppearing ();
		lock (graphiteLock)
			pageActive = true;
		fpsCounter.Start ();
		graphiteView.HasRenderLoop = true;
	}

	protected override void OnDisappearing ()
	{
		graphiteView.HasRenderLoop = false;
		lock (graphiteLock) {
			pageActive = false;
			DisposeGraphiteWorkers ();
		}
		fpsCounter.Stop ();
		base.OnDisappearing ();
	}

	private void OnPaintSurface (object? sender, SKPaintGraphiteSurfaceEventArgs e)
	{
		lock (graphiteLock) {
			if (!pageActive) {
				e.Surface.Canvas.Clear (new SKColor (5, 8, 20));
				return;
			}

			EnsureWorkers (e.Context);

			var rawInfo = e.RawInfo;
			var size = rawInfo.Size;
			using var textureInfo = e.BackendTexture.TextureInfo
				?? throw new InvalidOperationException (
					"Unable to read the Graphite target texture information.");

			if (backgroundRecording is null || backgroundSize != size) {
				backgroundRecording?.Dispose ();
				backgroundRecording = workers![0]
					.RecordAsync (
						rawInfo,
						textureInfo,
						canvas => DrawBackground (canvas, rawInfo))
					.GetAwaiter ()
					.GetResult ();
				backgroundSize = size;
			}

			InsertRecording (e.Context, backgroundRecording, e.Surface);

			var time = (float)fpsCounter.ElapsedSeconds;
			var particles = Volatile.Read (ref particleCount);
			var parallel = Volatile.Read (ref parallelRecording) != 0;
			var firstWorker = workers![1];
			var secondWorker = parallel ? workers[2] : workers[1];

			var first = firstWorker.RecordAsync (
				rawInfo,
				textureInfo,
				canvas => DrawParticles (
					canvas, rawInfo, time, particles, 0));
			var second = secondWorker.RecordAsync (
				rawInfo,
				textureInfo,
				canvas => DrawParticles (
					canvas, rawInfo, time, particles, 1));
			Task.WaitAll (first, second);

			using var firstRecording = first.Result;
			using var secondRecording = second.Result;
			InsertRecording (e.Context, firstRecording, e.Surface);
			InsertRecording (e.Context, secondRecording, e.Surface);

			DrawGraphiteBadge (e.Surface.Canvas, rawInfo, parallel);

			frameNumber++;
			if (fpsCounter.Tick () is double fps)
				UpdateStats (e.Context, parallel, particles, fps);
		}
	}

	private void EnsureWorkers (SKGraphiteContext context)
	{
		if (workerContext == context && workers is not null)
			return;

		DisposeGraphiteWorkers ();
		workerContext = context;
		var recorders = new SKGraphiteRecorder[3];
		try {
			for (var i = 0; i < recorders.Length; i++) {
				recorders[i] = context.CreateRecorder ()
					?? throw new InvalidOperationException (
						"Unable to create a Graphite worker recorder.");
			}
			workers = new[] {
				new GraphiteRecordingWorker (recorders[0], "Graphite background"),
				new GraphiteRecordingWorker (recorders[1], "Graphite particles A"),
				new GraphiteRecordingWorker (recorders[2], "Graphite particles B"),
			};
		} catch {
			foreach (var recorder in recorders)
				recorder?.Dispose ();
			throw;
		}
	}

	private static void InsertRecording (
		SKGraphiteContext context,
		SKGraphiteRecording recording,
		SKSurface target)
	{
		var options = new SKGraphiteInsertRecordingOptions {
			TargetSurface = target,
		};
		var status = context.InsertRecording (recording, options);
		if (status != SKGraphiteInsertStatus.Success)
			throw new InvalidOperationException (
				$"Graphite recording insertion failed: {status}.");
	}

	private static void DrawBackground (SKCanvas canvas, SKImageInfo info)
	{
		canvas.Clear (new SKColor (5, 8, 20));

		using var glow = SKShader.CreateRadialGradient (
			new SKPoint (info.Width * 0.52f, info.Height * 0.42f),
			Math.Max (info.Width, info.Height) * 0.72f,
			new[] {
				new SKColor (45, 94, 180),
				new SKColor (31, 35, 90),
				new SKColor (5, 8, 20),
			},
			new[] { 0f, 0.48f, 1f },
			SKShaderTileMode.Clamp);
		using var glowPaint = new SKPaint {
			Shader = glow,
			IsAntialias = true,
		};
		canvas.DrawRect (info.Rect, glowPaint);

		using var gridPaint = new SKPaint {
			Color = new SKColor (135, 175, 255, 24),
			StrokeWidth = 1,
			IsAntialias = true,
		};
		var spacing = Math.Max (32, Math.Min (info.Width, info.Height) / 12);
		for (var x = 0; x < info.Width; x += spacing)
			canvas.DrawLine (x, 0, x, info.Height, gridPaint);
		for (var y = 0; y < info.Height; y += spacing)
			canvas.DrawLine (0, y, info.Width, y, gridPaint);
	}

	private void DrawParticles (
		SKCanvas canvas,
		SKImageInfo info,
		float time,
		int totalParticles,
		int lane)
	{
		using var paint = new SKPaint {
			IsAntialias = true,
			BlendMode = SKBlendMode.SrcOver,
		};

		var count = totalParticles / 2;
		var centerX = touchActive ? touchX : info.Width * 0.5f;
		var centerY = touchActive ? touchY : info.Height * 0.5f;
		var minDimension = Math.Min (info.Width, info.Height);

		for (var i = 0; i < count; i++) {
			var id = i * 2 + lane;
			var seed = id * 0.61803398875f;
			var orbit = 0.08f + (seed - MathF.Floor (seed)) * 0.46f;
			var angle = seed * 27.3f + time * (0.18f + (id % 11) * 0.012f) *
				(lane == 0 ? 1 : -1);
			var wobble = MathF.Sin (time * 0.7f + seed * 19f) * minDimension * 0.025f;
			var radius = minDimension * orbit + wobble;
			var x = centerX + MathF.Cos (angle) * radius;
			var y = centerY + MathF.Sin (angle * 1.17f) * radius * 0.72f;
			var size = 0.8f + (id % 9) * 0.35f;
			var alpha = (byte)(70 + (id % 6) * 24);

			paint.Color = lane == 0
				? new SKColor (100, 184, 255, alpha)
				: new SKColor (230, 115, 255, alpha);
			canvas.DrawCircle (x, y, size, paint);

			if ((id & 15) == 0) {
				paint.StrokeWidth = Math.Max (1, size * 0.5f);
				canvas.DrawLine (
					x,
					y,
					x - MathF.Sin (angle) * size * 8,
					y + MathF.Cos (angle) * size * 8,
					paint);
			}
		}
	}

	private static void DrawGraphiteBadge (
		SKCanvas canvas,
		SKImageInfo info,
		bool parallel)
	{
		using var paint = new SKPaint {
			Color = new SKColor (225, 238, 255, 185),
			IsAntialias = true,
		};
		using var font = new SKFont {
			Size = Math.Clamp (info.Width / 42f, 12, 22),
		};
		canvas.DrawText (
			parallel ? "3 recordings / 2 worker threads" : "3 recordings / 1 worker thread",
			16,
			info.Height - 18,
			SKTextAlign.Left,
			font,
			paint);
	}

	private void UpdateStats (
		SKGraphiteContext context,
		bool parallel,
		int particles,
		double fps)
	{
		var now = Environment.TickCount64;
		if (now - Interlocked.Read (ref lastStatsUpdate) < 250)
			return;
		Interlocked.Exchange (ref lastStatsUpdate, now);

		var budget = context.CurrentBudgetedBytes / (1024d * 1024d);
		var maxBudget = context.MaxBudgetedBytes / (1024d * 1024d);
		var backend = context.Backend;
		Dispatcher.Dispatch (() =>
		{
			backendLabel.Text =
				$"{backend} • {fps:F0} FPS • frame {frameNumber:N0}";
			statsLabel.Text =
				$"{particles:N0} particles • {(parallel ? "2 parallel workers" : "1 serial worker")} • background replayed";
			budgetLabel.Text =
				$"GPU budget: {budget:F1} / {maxBudget:F0} MB";
		});
	}

	private void OnRenderFailed (object? sender, SKGraphiteRenderFailedEventArgs e)
	{
		Dispatcher.Dispatch (() =>
		{
			backendLabel.Text = "Graphite rendering stopped";
			statsLabel.Text = e.Exception.Message;
		});
	}

	private void OnTouch (object? sender, SKTouchEventArgs e)
	{
		switch (e.ActionType) {
			case SKTouchAction.Pressed:
			case SKTouchAction.Moved:
				touchX = e.Location.X;
				touchY = e.Location.Y;
				touchActive = true;
				break;
			case SKTouchAction.Released:
			case SKTouchAction.Cancelled:
				touchActive = false;
				break;
		}
		e.Handled = true;
	}

	private void OnParallelToggled (object? sender, ToggledEventArgs e) =>
		Volatile.Write (ref parallelRecording, e.Value ? 1 : 0);

	private void OnParticleCountChanged (object? sender, ValueChangedEventArgs e) =>
		Volatile.Write (ref particleCount, (int)e.NewValue);

	private void DisposeGraphiteWorkers ()
	{
		backgroundRecording?.Dispose ();
		backgroundRecording = null;
		backgroundSize = default;

		if (workers is not null) {
			foreach (var worker in workers)
				worker.Dispose ();
			workers = null;
		}
		workerContext = null;
	}

	private sealed class GraphiteRecordingWorker : IDisposable
	{
		private readonly SKGraphiteRecorder recorder;
		private readonly BlockingCollection<WorkItem> work = new ();
		private readonly Thread thread;

		public GraphiteRecordingWorker (SKGraphiteRecorder recorder, string name)
		{
			this.recorder = recorder;
			thread = new Thread (Run) {
				IsBackground = true,
				Name = name,
			};
			thread.Start ();
		}

		public Task<SKGraphiteRecording> RecordAsync (
			SKImageInfo info,
			SKGraphiteTextureInfo textureInfo,
			Action<SKCanvas> draw)
		{
			var completion = new TaskCompletionSource<SKGraphiteRecording> (
				TaskCreationOptions.RunContinuationsAsynchronously);
			work.Add (new WorkItem (info, textureInfo, draw, completion));
			return completion.Task;
		}

		public void Dispose ()
		{
			work.CompleteAdding ();
			if (Thread.CurrentThread != thread)
				thread.Join ();
			work.Dispose ();
		}

		private void Run ()
		{
			using (recorder) {
				foreach (var item in work.GetConsumingEnumerable ()) {
					try {
						var canvas = recorder.CreateDeferredCanvas (
							item.Info,
							item.TextureInfo)
							?? throw new InvalidOperationException (
								"Unable to create a deferred Graphite canvas.");
						item.Draw (canvas);
						var recording = recorder.Snap ()
							?? throw new InvalidOperationException (
								"Unable to snap a worker recording.");
						item.Completion.SetResult (recording);
					} catch (Exception exception) {
						recorder.Snap ()?.Dispose ();
						item.Completion.SetException (exception);
					}
				}
			}
		}

		private sealed record WorkItem (
			SKImageInfo Info,
			SKGraphiteTextureInfo TextureInfo,
			Action<SKCanvas> Draw,
			TaskCompletionSource<SKGraphiteRecording> Completion);
	}
}
