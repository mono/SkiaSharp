#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Opengl;
using Android.Views;
using Microsoft.Maui.Controls;
using SkiaSharp.Tests;
using SkiaSharp.Views.Maui.Controls.Tests;
using Xunit;
using MauiGLView = SkiaSharp.Views.Maui.Controls.SKGLView;

namespace SkiaSharp.Views.Android.Tests;

public class GLTextureViewTests : SKUITests
{
	[UIFact]
	public Task Issue_533_ContinuousRenderingIsDisplayPaced() =>
		WithTextureView(true, async (view, platformView, paints) =>
		{
			Assert.Equal(Rendermode.Continuously, platformView.RenderMode);
			await AssertDisplayPaced(paints);
			Assert.False(paints.PaintedOnUIThread);
		});

	[UIFact]
	public Task Issue_533_WhenDirtyRequestsAndRenderModeTransitionsWork() =>
		WithTextureView(false, async (view, platformView, paints) =>
		{
			Assert.Equal(Rendermode.WhenDirty, platformView.RenderMode);
			await AssertIdle(paints);

			var beforeEvent = paints.Count;
			var eventThread = await QueueOnGLThread(platformView);
			Assert.NotEqual(Environment.CurrentManagedThreadId, eventThread);
			Assert.Equal(beforeEvent, paints.Count);
			await AssertIdle(paints);

			await AssertSingleFrame(paints, platformView.RequestRender);
			await AssertSingleFrame(paints, view.InvalidateSurface);

			view.HasRenderLoop = true;
			await AssertDisplayPaced(paints);

			view.HasRenderLoop = false;
			await QueueOnGLThread(platformView);
			await AssertIdle(paints);
			await AssertSingleFrame(paints, platformView.RequestRender);

			view.HasRenderLoop = true;
			await AssertDisplayPaced(paints);
		});

	[UIFact]
	public Task Issue_533_MissedDisplayFramesAreNotQueued() =>
		WithTextureView(true, async (view, platformView, paints) =>
		{
			using var release = new ManualResetEventSlim();
			var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			platformView.QueueEvent(() =>
			{
				entered.TrySetResult();
				release.Wait(TimeSpan.FromSeconds(5));
				completed.TrySetResult();
			});

			try
			{
				await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
				await Task.Delay(300);
				await AssertDisplayPaced(paints, release.Set);
			}
			finally
			{
				release.Set();
				await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			}
		});

	[UIFact]
	public Task Issue_533_PauseResizeAndReattachmentPreserveRendering() =>
		WithTextureView(true, async (view, platformView, paints) =>
		{
			// These lifecycle methods synchronously wait on the UI thread.
			platformView.OnPause();
			platformView.RequestRender();
			await QueueOnGLThread(platformView);
			await AssertIdle(paints);

			var beforeResume = paints.Count;
			platformView.OnResume();
			await AssertEx.Eventually(() => paints.Count > beforeResume, timeout: 5000);
			await AssertDisplayPaced(paints);

			var beforeResize = paints.Count;
			Assert.NotNull(platformView.SurfaceTexture);
			platformView.OnSurfaceTextureSizeChanged(platformView.SurfaceTexture, platformView.Width, platformView.Height);
			await AssertEx.Eventually(() => paints.Count > beforeResize, timeout: 5000);

			await DetachAndReattach(platformView, paints);
			Assert.Equal(Rendermode.Continuously, platformView.RenderMode);
			await AssertDisplayPaced(paints);

			view.HasRenderLoop = false;
			await QueueOnGLThread(platformView);
			await AssertIdle(paints);
			await DetachAndReattach(platformView, paints);
			Assert.Equal(Rendermode.WhenDirty, platformView.RenderMode);
			await AssertIdle(paints);
			await AssertSingleFrame(paints, platformView.RequestRender);
		});

	private async Task WithTextureView(
		bool hasRenderLoop,
		Func<MauiGLView, SKGLTextureView, PaintCounter, Task> test)
	{
		var paints = new PaintCounter();
		var view = new MauiGLView
		{
			HasRenderLoop = hasRenderLoop,
			WidthRequest = 128,
			HeightRequest = 128,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		view.PaintSurface += (sender, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Red);
			paints.Record();
		};

		SKGLTextureView? platformView = null;
		try
		{
			CurrentPage.Content = view;
			await view.WaitForLoaded();
			await view.WaitForLayout();
			platformView = Assert.IsAssignableFrom<SKGLTextureView>(view.Handler!.PlatformView);
			await AssertEx.Eventually(() => paints.Count > 0, timeout: 5000);
			await Task.Delay(200);
			await test(view, platformView, paints);
		}
		finally
		{
			platformView ??= view.Handler?.PlatformView as SKGLTextureView;
			CurrentPage.Content = null;
			view.Handler?.DisconnectHandler();
			platformView?.Dispose();
		}
	}

	private static async Task AssertDisplayPaced(PaintCounter paints, Action? start = null)
	{
		using var frames = new DisplayFrames();
		var before = paints.Count;
		frames.Start();
		try
		{
			start?.Invoke();
			await Task.Delay(750);
		}
		finally
		{
			frames.Stop();
		}

		var rendered = paints.Count - before;
		Assert.True(frames.Count > 0, "No display callbacks were observed.");
		Assert.True(rendered > 0, "The render loop did not draw any frames.");
		Assert.True(rendered <= frames.Count + 2,
			$"Rendered {rendered} frames during {frames.Count} display callbacks (at most two boundary frames are allowed).");
	}

	private static async Task AssertIdle(PaintCounter paints)
	{
		var before = paints.Count;
		await Task.Delay(250);
		Assert.Equal(before, paints.Count);
	}

	private static async Task AssertSingleFrame(PaintCounter paints, Action request)
	{
		var before = paints.Count;
		request();
		await AssertEx.Eventually(() => paints.Count > before, timeout: 5000);
		await AssertIdle(paints);
		Assert.Equal(before + 1, paints.Count);
	}

	private static async Task<int> QueueOnGLThread(SKGLTextureView view)
	{
		var completed = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
		view.QueueEvent(() => completed.TrySetResult(Environment.CurrentManagedThreadId));
		return await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
	}

	private static async Task DetachAndReattach(SKGLTextureView view, PaintCounter paints)
	{
		var parent = Assert.IsAssignableFrom<ViewGroup>(view.Parent);
		var index = parent.IndexOfChild(view);
		var layoutParameters = view.LayoutParameters;
		parent.RemoveView(view);
		var before = paints.Count;
		try
		{
			Assert.False(view.IsAttachedToWindow);
			await AssertIdle(paints);
		}
		finally
		{
			parent.AddView(view, index, layoutParameters);
		}

		await AssertEx.Eventually(() => view.IsAvailable && paints.Count > before, timeout: 5000);
		await Task.Delay(200);
	}

	private sealed class PaintCounter
	{
		private readonly int uiThread = Environment.CurrentManagedThreadId;
		private int count;
		private int paintedOnUIThread;

		public int Count => Volatile.Read(ref count);

		public bool PaintedOnUIThread => Volatile.Read(ref paintedOnUIThread) != 0;

		public void Record()
		{
			if (Environment.CurrentManagedThreadId == uiThread)
				Interlocked.Exchange(ref paintedOnUIThread, 1);
			Interlocked.Increment(ref count);
		}
	}

	private sealed class DisplayFrames : Java.Lang.Object, Choreographer.IFrameCallback
	{
		private readonly Choreographer choreographer = Choreographer.Instance!;
		private bool running;

		public int Count { get; private set; }

		public void Start()
		{
			running = true;
			choreographer.PostFrameCallback(this);
		}

		public void Stop()
		{
			running = false;
			choreographer.RemoveFrameCallback(this);
		}

		public void DoFrame(long frameTimeNanos)
		{
			if (!running)
				return;

			Count++;
			choreographer.PostFrameCallback(this);
		}
	}
}
