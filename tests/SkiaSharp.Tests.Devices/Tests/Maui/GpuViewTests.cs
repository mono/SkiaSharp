#nullable enable

using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

public partial class GpuViewTests : SKUITests
{
	[UIFact]
	public void GaneshDefaultsAndTouchForwarding() => VerifyDefaults(new SKGaneshView());

	[UIFact]
	public void GraphiteDefaultsAndTouchForwarding()
	{
		var view = new SKGraphiteView();
		VerifyDefaults(view);
		Assert.Null(view.GraphiteContext);
		Assert.Throws<InvalidOperationException>(() => view.CreateRecorder());
	}

	private static void VerifyDefaults(SKGpuView view)
	{
		Assert.False(view.HasRenderLoop);
		Assert.False(view.IgnorePixelScaling);
		Assert.False(view.EnableTouchEvents);
		Assert.Equal(SKSize.Empty, view.CanvasSize);
		var touch = new SKTouchEventArgs(1, SKTouchAction.Pressed, new SKPoint(12, 34), true);
		SKTouchEventArgs? received = null;
		view.Touch += (_, e) => { received = e; e.Handled = true; };
		((ISKGpuView)view).OnTouch(touch);
		Assert.Same(touch, received);
		Assert.True(touch.Handled);
		view.InvalidateSurface();
	}

	[UIFact]
	public async Task GaneshRendersRestoresAndClearsContextOnDetach()
	{
		var view = new SKGaneshView { EnableTouchEvents = true };
		Assert.Null(view.GRContext);
		var frame = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		view.RenderFailed += (_, e) => frame.TrySetException(e.Exception);
		GRContext? original = null;
		var created = 0;
		var destroyed = 0;
		view.ContextCreated += (_, _) => { original = view.GRContext; created++; };
		view.ContextDestroyed += (_, _) => { Assert.Null(view.GRContext); destroyed++; };
		view.PaintSurface += (_, e) =>
		{
			try
			{
				Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
				Assert.Same(view.GRContext, e.Context);
				Assert.Equal((SKSize)e.Info.Size, view.CanvasSize);
				Assert.True(e.RawInfo.Width > 0 && e.RawInfo.Height > 0);
				Assert.Equal(SKMatrix.Identity, e.Surface.Canvas.TotalMatrix);
				e.Surface.Canvas.Clear(SKColors.Red);
				using var pixels = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Rgba8888));
				Assert.True(e.Surface.ReadPixels(pixels.Info, pixels.GetPixels(), pixels.RowBytes, 0, 0));
				Assert.Equal(SKColors.Red, pixels.GetPixel(0, 0));
				// Deliberately leave an unbalanced save/transform for the next frame.
				e.Surface.Canvas.Save();
				e.Surface.Canvas.Translate(100, 100);
				frame.TrySetResult();
			}
			catch (Exception ex) { frame.TrySetException(ex); }
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		view.InvalidateSurface();
		await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.NotNull(original);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		Assert.Null(view.GRContext);
		Assert.Equal(SKSize.Empty, view.CanvasSize);
		Assert.Equal(1, created);
		Assert.Equal(1, destroyed);
	}

	[UIFact]
	public async Task GaneshReattachmentCreatesANewContextGeneration()
	{
		var view = new SKGaneshView();
		var frame = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);
		view.RenderFailed += (_, e) => frame.TrySetException(e.Exception);
		view.PaintSurface += (_, e) => { e.Surface.Canvas.Clear(SKColors.Green); frame.TrySetResult(e.Context); };
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		var first = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		view.Handler = null;
		Assert.Null(view.GRContext);
		frame = new(TaskCreationOptions.RunContinuationsAsynchronously);
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		view.InvalidateSurface();
		var second = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.NotSame(first, second);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}

#if ANDROID
	[UIFact]
	public async Task GaneshNativeTouchUsesLogicalCoordinatesWhenScalingIsIgnored()
	{
		var view = new SKGaneshView { IgnorePixelScaling = true, EnableTouchEvents = true };
		var touched = new TaskCompletionSource<SKPoint>(TaskCreationOptions.RunContinuationsAsynchronously);
		view.PaintSurface += (_, e) => e.Surface.Canvas.Clear(SKColors.Red);
		view.Touch += (_, e) => { touched.TrySetResult(e.Location); e.Handled = true; };
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		var native = (global::Android.Views.View)view.Handler!.PlatformView!;
		var density = native.Resources!.DisplayMetrics!.Density;
		using var input = global::Android.Views.MotionEvent.Obtain(0, 0,
			global::Android.Views.MotionEventActions.Down, 12 * density, 34 * density, 0)!;
		native.DispatchTouchEvent(input);
		var point = await touched.Task.WaitAsync(TimeSpan.FromSeconds(5));
		Assert.InRange(point.X, 11.9f, 12.1f);
		Assert.InRange(point.Y, 33.9f, 34.1f);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}
#endif

	[UIFact]
	public async Task GaneshInvalidationAndLogicalPixelSize()
	{
		var view = new SKGaneshView();
		var frames = 0;
		var frame = new TaskCompletionSource<SKImageInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
		view.RenderFailed += (_, e) => frame.TrySetException(e.Exception);
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Blue);
			Interlocked.Increment(ref frames);
			frame.TrySetResult(e.Info);
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		await Task.Delay(150);
		var idleFrames = Volatile.Read(ref frames);
		await Task.Delay(150);
		Assert.Equal(idleFrames, Volatile.Read(ref frames));
		view.InvalidateSurface();
		await AssertEx.Eventually(() => Volatile.Read(ref frames) > idleFrames, timeout: 5000);
		view.IgnorePixelScaling = true;
		frame = new(TaskCreationOptions.RunContinuationsAsynchronously);
		view.InvalidateSurface();
		var logical = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.InRange(logical.Width, (int)view.Width - 1, (int)view.Width + 1);
		Assert.InRange(logical.Height, (int)view.Height - 1, (int)view.Height + 1);
		var beforeLoop = Volatile.Read(ref frames);
		view.HasRenderLoop = true;
		await AssertEx.Eventually(() => Volatile.Read(ref frames) >= beforeLoop + 3, timeout: 5000);
		view.HasRenderLoop = false;
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}

#if IOS || MACCATALYST || ANDROID
	[UIFact]
	public async Task FailedGraphiteFrameCancelsAllReadbacksEvenWhenCallbackThrows()
	{
		var view = new SKGraphiteView();
		var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var callbacks = 0;
		view.RenderFailed += (_, _) => { };
		view.PaintSurface += (_, e) =>
		{
			e.RequestReadPixels(new SKImageInfo(1, 1, SKColorType.Rgba8888), new SKRectI(0, 0, 1, 1), pixels =>
			{
				Assert.Null(pixels);
				Interlocked.Increment(ref callbacks);
				throw new InvalidOperationException("Deliberate cancellation callback failure.");
			});
			e.RequestReadPixels(new SKImageInfo(1, 1, SKColorType.Rgba8888), new SKRectI(0, 0, 1, 1), pixels =>
			{
				Assert.Null(pixels);
				Interlocked.Increment(ref callbacks);
				cancelled.TrySetResult();
			});
			throw new InvalidOperationException("Deliberate frame failure.");
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.Equal(2, Volatile.Read(ref callbacks));
		await AssertEx.Eventually(() => view.GraphiteContext is null, timeout: 5000);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}

	[UIFact]
	public async Task GraphiteExplicitRetryCreatesANewContextAfterPaintFailure()
	{
		var view = new SKGraphiteView();
		var failed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var recovered = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);
		SKGraphiteContext? first = null;
		var shouldFail = true;
		view.RenderFailed += (_, _) => failed.TrySetResult();
		view.PaintSurface += (_, e) =>
		{
			if (shouldFail)
			{
				first = e.Context;
				throw new InvalidOperationException("Deliberate first-frame failure.");
			}
			e.Surface.Canvas.Clear(SKColors.Blue);
			recovered.TrySetResult(e.Context);
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		await failed.Task.WaitAsync(TimeSpan.FromSeconds(10));
		await AssertEx.Eventually(() => view.GraphiteContext is null, timeout: 5000);
		shouldFail = false;
		for (var i = 0; i < 100 && !recovered.Task.IsCompleted; i++)
		{
			view.InvalidateSurface();
			await Task.Delay(50);
		}
		Assert.NotSame(first, await recovered.Task.WaitAsync(TimeSpan.FromSeconds(5)));
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}

	[UIFact]
	public async Task GraphiteReattachmentCanRetryAfterRetirementCompletes()
	{
		var view = new SKGraphiteView();
		var frame = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Green);
			frame.TrySetResult(e.Context);
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		var first = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		view.Handler = null;
		frame = new(TaskCreationOptions.RunContinuationsAsynchronously);
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		for (var i = 0; i < 100 && !frame.Task.IsCompleted; i++)
		{
			view.InvalidateSurface();
			await Task.Delay(50);
		}
		Assert.NotSame(first, await frame.Task.WaitAsync(TimeSpan.FromSeconds(5)));
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}

	[UIFact]
	public async Task GraphiteResizePreservesContextAndCallerRecorder()
	{
		var view = new SKGraphiteView { WidthRequest = 120, HeightRequest = 120,
			HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
		SKGraphiteRecorder? recorder = null;
		var frame = new TaskCompletionSource<SKImageInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
		var minimumWidth = 0;
		view.ContextCreated += (_, _) => recorder = view.CreateRecorder();
		view.ContextDestroying += (_, _) => recorder?.Dispose();
		view.RenderFailed += (_, e) => frame.TrySetException(e.Exception);
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Blue);
			if (e.Info.Width > Volatile.Read(ref minimumWidth)) frame.TrySetResult(e.Info);
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		var before = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		var context = view.GraphiteContext;
		Assert.NotNull(context);
		Assert.NotNull(recorder);
		var handle = recorder.Handle;
		Volatile.Write(ref minimumWidth, before.Width);
		frame = new(TaskCreationOptions.RunContinuationsAsynchronously);
		view.WidthRequest = 200;
		view.HeightRequest = 160;
		await AssertEx.Eventually(() => view.Width > 120, timeout: 5000);
		view.InvalidateSurface();
		var after = await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.True(after.Width > before.Width);
		Assert.Same(context, view.GraphiteContext);
		Assert.Equal(handle, recorder.Handle);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		Assert.Null(view.GraphiteContext);
	}

	[UIFact]
	public async Task GraphiteRejectsRecordingsFromAnotherViewBeforeNativeInsertion()
	{
		var first = new SKGraphiteView();
		var second = new SKGraphiteView();
		SKGraphiteRecorder? producer = null;
		SKGraphiteRecording? recording = null;
		var verified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var attempted = false;
		first.RenderFailed += (_, e) => verified.TrySetException(e.Exception);
		second.RenderFailed += (_, e) => verified.TrySetException(e.Exception);
		first.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Green);
			if (recording is not null) return;
			producer = first.CreateRecorder();
			using var target = SKSurface.Create(producer, new SKImageInfo(8, 8))!;
			target.Canvas.Clear(SKColors.Blue);
			recording = first.SnapRecording(producer);
			first.Dispatcher.Dispatch(() => second.InvalidateSurface());
		};
		second.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Red);
			if (recording is null || attempted) return;
			attempted = true;
			try
			{
				Assert.Throws<InvalidOperationException>(() => second.InsertRecording(recording));
				verified.TrySetResult();
			}
			catch (Exception ex) { verified.TrySetException(ex); }
		};
		first.ContextDestroying += (_, _) => { recording?.Dispose(); producer?.Dispose(); };
		var host = new Grid { RowDefinitions = { new RowDefinition(), new RowDefinition() } };
		host.Children.Add(first);
		host.Children.Add(second);
		Grid.SetRow(second, 1);
		CurrentPage.Content = host;
		await host.WaitForLoaded();
		await host.WaitForLayout();
		await verified.Task.WaitAsync(TimeSpan.FromSeconds(10));
		CurrentPage.Content = null;
		first.Handler?.DisconnectHandler();
		second.Handler?.DisconnectHandler();
		Assert.Null(first.GraphiteContext);
		Assert.Null(second.GraphiteContext);
	}

	[UIFact]
	public async Task GraphiteProducerRecordingIsInsertedBeforeFrame()
	{
		var view = new SKGraphiteView();
		SKGraphiteRecorder? recorder = null;
		Task<(SKGraphiteRecording Recording, SKImage Image)>? producer = null;
		var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var inserted = false;
		view.ContextCreated += (_, _) =>
		{
			recorder = view.CreateRecorder();
			producer = Task.Run(() =>
			{
				using var surface = SKSurface.Create(recorder, new SKImageInfo(16, 16))
					?? throw new InvalidOperationException("Producer surface creation failed.");
				surface.Canvas.Clear(SKColors.Blue);
				var image = surface.Snapshot();
				var recording = view.SnapRecording(recorder);
				view.Dispatcher.Dispatch(() => view.InvalidateSurface());
				return (recording, image);
			});
		};
		view.ContextDestroying += (_, _) =>
		{
			if (producer is not null)
			{
				var result = producer.GetAwaiter().GetResult();
				result.Image.Dispose();
				result.Recording.Dispose();
			}
			recorder?.Dispose();
		};
		view.RenderFailed += (_, e) => finished.TrySetException(e.Exception);
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Black);
			if (inserted || producer?.IsCompleted != true) return;
			var result = producer.GetAwaiter().GetResult();
			Assert.Equal(SKGraphiteInsertStatus.Success, view.InsertRecording(result.Recording));
			e.Surface.Canvas.DrawImage(result.Image, 0, 0, SKSamplingOptions.Default);
			inserted = true;
			e.RequestReadPixels(new SKImageInfo(1, 1, SKColorType.Rgba8888), new SKRectI(0, 0, 1, 1),
				pixels =>
				{
					try
					{
						Assert.NotNull(pixels);
						Assert.Equal(new byte[] { 0, 0, 255, 255 }, pixels.ToArray());
						finished.TrySetResult();
					}
					catch (Exception ex) { finished.TrySetException(ex); }
				});
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		await finished.Task.WaitAsync(TimeSpan.FromSeconds(10));
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		Assert.Null(view.GraphiteContext);
	}

	[UIFact]
	public async Task GraphiteReadbackCompletesWhileIdleAndDependentRecordersShutdown()
	{
		var view = new SKGraphiteView();
		var pixel = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		var recorders = new List<SKGraphiteRecorder>();
		SKGraphiteContext? original = null;
		var requested = false;
		view.ContextCreated += (_, _) =>
		{
			original = view.GraphiteContext;
			for (var i = 0; i < 8; i++) recorders.Add(view.CreateRecorder());
			Assert.Throws<InvalidOperationException>(() => view.CreateRecorder());
		};
		view.ContextDestroying += (_, _) =>
		{
			foreach (var recorder in recorders) recorder.Dispose();
			recorders.Clear();
		};
		view.RenderFailed += (_, e) => pixel.TrySetException(e.Exception);
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Red);
			if (requested) return;
			requested = true;
			Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
			Assert.Same(view.GraphiteContext, e.Context);
			e.RequestReadPixels(new SKImageInfo(1, 1, SKColorType.Rgba8888),
				new SKRectI(0, 0, 1, 1), result =>
				{
					if (result is null) pixel.TrySetException(new InvalidOperationException("Graphite readback failed."));
					else pixel.TrySetResult(BinaryPrimitives.ReadInt32LittleEndian(result.GetPlaneData(0)));
				});
		};
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		view.InvalidateSurface();
		var rgba = await pixel.Task.WaitAsync(TimeSpan.FromSeconds(10));
		Assert.Equal(unchecked((int)0xff0000ff), rgba);
		Assert.NotNull(original);
		Assert.False(view.HasRenderLoop);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
		Assert.Empty(recorders);
		Assert.Null(view.GraphiteContext);
		Assert.Equal(SKSize.Empty, view.CanvasSize);
		Assert.Equal(IntPtr.Zero, original.Handle);
	}

	[UIFact]
	public async Task GraphitePaintFailureIsVisibleAndClearsContext()
	{
		var view = new SKGraphiteView();
		var failure = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
		var expected = new InvalidOperationException("Deliberate paint failure.");
		view.PaintSurface += (_, _) => throw expected;
		view.RenderFailed += (_, e) => failure.TrySetResult(e.Exception);
		CurrentPage.Content = view;
		await view.WaitForLoaded();
		await view.WaitForLayout();
		view.InvalidateSurface();
		Assert.Same(expected, await failure.Task.WaitAsync(TimeSpan.FromSeconds(10)));
		await AssertEx.Eventually(() => view.GraphiteContext is null, timeout: 5000);
		CurrentPage.Content = null;
		view.Handler?.DisconnectHandler();
	}
#elif WINDOWS
	[UIFact]
	public void WindowsGraphitePresentationIsExplicitlyUnsupported()
	{
		var handler = new Handlers.SKGraphiteViewHandler();
		Assert.Throws<PlatformNotSupportedException>(() => handler.CreateRecorder());
	}
#endif
}
