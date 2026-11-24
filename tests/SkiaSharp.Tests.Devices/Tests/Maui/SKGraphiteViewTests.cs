#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests
{
	public class SKGraphiteViewTests : SKUITests
	{
		[UIFact]
		public void GraphiteDefaultsAndTouchForwarding()
		{
			var view = new SKGraphiteView();

			Assert.False(view.HasRenderLoop);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableTouchEvents);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GraphiteContext);

			var touch = new SKTouchEventArgs(1, SKTouchAction.Pressed, new SKPoint(12, 34), true);
			SKTouchEventArgs? received = null;

			view.Touch += (_, e) =>
			{
				received = e;
				e.Handled = true;
			};

			((ISKGraphiteView)view).OnTouch(touch);

			Assert.Same(touch, received);
			Assert.True(touch.Handled);

			view.InvalidateSurface();
		}

#if WINDOWS
		[UIFact]
		public void WindowsGraphitePresentationIsExplicitlyUnsupported()
		{
			var view = new SKGraphiteView();

			Assert.Throws<PlatformNotSupportedException>(() =>
			{
				_ = view.ToPlatform(CurrentPage.Handler!.MauiContext!);
			});
		}
#endif

#if IOS || MACCATALYST
		[UIFact]
		public async Task GraphiteRendersRestoresAndClearsContextOnDetach()
		{
			RequireGraphiteBackend();

			var view = new SKGraphiteView
			{
				EnableTouchEvents = true,
			};

			Assert.Null(view.GraphiteContext);

			var frame = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
					Assert.NotNull(e.Context);
					Assert.Same(view.GraphiteContext, e.Context);
					Assert.NotNull(e.Recorder);
					Assert.NotNull(e.BackendTexture);
					Assert.True(e.BackendTexture!.IsValid);
					Assert.Equal((SKSize)e.Info.Size, view.CanvasSize);
					Assert.True(e.RawInfo.Width > 0 && e.RawInfo.Height > 0);
					Assert.Equal(SKMatrix.Identity, e.Surface.Canvas.TotalMatrix);

					e.Surface.Canvas.Clear(SKColors.Red);
					Assert.Equal(SKColors.Red, ReadPixel(e, 0, 0));

					// Deliberately leave an unbalanced save/transform for the next frame.
					e.Surface.Canvas.Save();
					e.Surface.Canvas.Translate(100, 100);

					Volatile.Read(ref frame).TrySetResult(e.Context!);
				}
				catch (Exception ex)
				{
					Volatile.Read(ref frame).TrySetException(ex);
				}
			};

			try
			{
				CurrentPage.Content = view;
				await view.WaitForLoaded();
				await view.WaitForLayout();
				view.InvalidateSurface();

				var original = await Volatile.Read(ref frame).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(original);
				Assert.Same(original, view.GraphiteContext);

				Interlocked.Exchange(ref frame, new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously));
				view.InvalidateSurface();

				var secondFrameContext = await Volatile.Read(ref frame).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(original, secondFrameContext);
			}
			finally
			{
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
			}

			Assert.Null(view.GraphiteContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task GraphiteReattachmentCreatesANewContextGeneration()
		{
			RequireGraphiteBackend();

			var view = new SKGraphiteView();
			var frame = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.NotNull(e.Context);
					e.Surface.Canvas.Clear(SKColors.Green);
					Volatile.Read(ref frame).TrySetResult(e.Context!);
				}
				catch (Exception ex)
				{
					Volatile.Read(ref frame).TrySetException(ex);
				}
			};

			try
			{
				CurrentPage.Content = view;
				await view.WaitForLoaded();
				await view.WaitForLayout();

				var first = await Volatile.Read(ref frame).Task.WaitAsync(TimeSpan.FromSeconds(10));

				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
				view.Handler = null;

				Assert.Null(view.GraphiteContext);

				Interlocked.Exchange(ref frame, new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously));
				CurrentPage.Content = view;
				await view.WaitForLoaded();
				await view.WaitForLayout();
				view.InvalidateSurface();

				var second = await Volatile.Read(ref frame).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotSame(first, second);
			}
			finally
			{
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
			}
		}

		[UIFact]
		public async Task GraphiteInvalidationAndLogicalPixelSize()
		{
			RequireGraphiteBackend();

			var view = new SKGraphiteView();
			var frames = 0;
			var frame = new TaskCompletionSource<SKImageInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
			TaskCompletionSource<SKImageInfo>? logicalFrame = null;
			var logicalWidth = 0;
			var logicalHeight = 0;

			view.PaintSurface += (_, e) =>
			{
				e.Surface.Canvas.Clear(SKColors.Blue);
				Interlocked.Increment(ref frames);
				frame.TrySetResult(e.Info);

				if (Volatile.Read(ref logicalFrame) is { } pending &&
					e.Info.Width >= logicalWidth - 1 && e.Info.Width <= logicalWidth + 1 &&
					e.Info.Height >= logicalHeight - 1 && e.Info.Height <= logicalHeight + 1)
				{
					pending.TrySetResult(e.Info);
				}
			};

			try
			{
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

				logicalWidth = (int)view.Width;
				logicalHeight = (int)view.Height;

				var logicalCompletion = new TaskCompletionSource<SKImageInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
				Volatile.Write(ref logicalFrame, logicalCompletion);

				view.IgnorePixelScaling = true;
				view.InvalidateSurface();

				var logical = await logicalCompletion.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.InRange(logical.Width, logicalWidth - 1, logicalWidth + 1);
				Assert.InRange(logical.Height, logicalHeight - 1, logicalHeight + 1);
				Assert.Equal((SKSize)logical.Size, view.CanvasSize);

				var beforeLoop = Volatile.Read(ref frames);
				view.HasRenderLoop = true;
				await AssertEx.Eventually(() => Volatile.Read(ref frames) >= beforeLoop + 3, timeout: 5000);
			}
			finally
			{
				view.HasRenderLoop = false;
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
			}
		}

		private static SKColor ReadPixel(SKPaintGraphiteSurfaceEventArgs e, int x, int y)
		{
			using var recording = e.Recorder!.Snap();
			Assert.NotNull(recording);
			Assert.Equal(SKGraphiteInsertStatus.Success, e.Context!.InsertRecording(recording));

			var dstInfo = new SKImageInfo(1, 1, SKColorType.Rgba8888, SKAlphaType.Premul);
			byte[]? bytes = null;
			var done = false;

			e.Context.RequestReadPixels(
				e.Surface,
				dstInfo,
				new SKRectI(x, y, x + 1, y + 1),
				result =>
				{
					done = true;
					bytes = result?.ToArray();
				});

			Assert.True(e.Context.Submit(new SKGraphiteSubmitInfo { Sync = true }));
			for (var i = 0; i < 10_000 && !done; i++)
			{
				e.Context.CheckAsyncWorkCompletion();
			}

			Assert.True(done);
			Assert.NotNull(bytes);
			Assert.Equal(4, bytes!.Length);

			return new SKColor(bytes[0], bytes[1], bytes[2], bytes[3]);
		}

		private static void RequireGraphiteBackend()
		{
#if IOS || MACCATALYST
			GpuPolicy.RequireOrSkip(GpuBackends.GraphiteMetal);
#endif
		}
#endif
	}
}
