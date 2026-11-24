#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp.Tests;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests
{
	public class SKGaneshViewTests : SKUITests
	{
		[UIFact]
		public void GaneshDefaultsAndTouchForwarding()
		{
			var view = new SKGaneshView();

			Assert.False(view.HasRenderLoop);
			Assert.False(view.IgnorePixelScaling);
			Assert.False(view.EnableTouchEvents);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);

			var touch = new SKTouchEventArgs(1, SKTouchAction.Pressed, new SKPoint(12, 34), true);
			SKTouchEventArgs? received = null;

			view.Touch += (_, e) =>
			{
				received = e;
				e.Handled = true;
			};

			((ISKGaneshView)view).OnTouch(touch);

			Assert.Same(touch, received);
			Assert.True(touch.Handled);

			view.InvalidateSurface();
		}

#if ANDROID || IOS || MACCATALYST
		[UIFact]
		public async Task GaneshRendersRestoresAndClearsContextOnDetach()
		{
			RequireGaneshBackend();

			var view = new SKGaneshView
			{
				EnableTouchEvents = true,
			};

			Assert.Null(view.GRContext);

			var frame = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
					Assert.NotNull(e.Context);
					Assert.Same(view.GRContext, e.Context);
					Assert.NotNull(e.BackendRenderTarget);
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
				Assert.Same(original, view.GRContext);

				Interlocked.Exchange(ref frame, new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously));
				view.InvalidateSurface();

				var secondFrameContext = await Volatile.Read(ref frame).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(original, secondFrameContext);
			}
			finally
			{
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
			}

			Assert.Null(view.GRContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task GaneshReattachmentCreatesANewContextGeneration()
		{
			RequireGaneshBackend();

			var view = new SKGaneshView();
			var frame = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);

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

				Assert.Null(view.GRContext);

				Interlocked.Exchange(ref frame, new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously));
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

#if ANDROID
		[UIFact]
		public async Task GaneshNativeTouchUsesLogicalCoordinatesWhenScalingIsIgnored()
		{
			var view = new SKGaneshView
			{
				IgnorePixelScaling = true,
				EnableTouchEvents = true,
			};

			var touched = new TaskCompletionSource<SKPoint>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				e.Surface.Canvas.Clear(SKColors.Red);
			};

			view.Touch += (_, e) =>
			{
				touched.TrySetResult(e.Location);
				e.Handled = true;
			};

			try
			{
				CurrentPage.Content = view;
				await view.WaitForLoaded();
				await view.WaitForLayout();

				var native = (global::Android.Views.View)view.Handler!.PlatformView!;
				var density = native.Resources!.DisplayMetrics!.Density;

				using var input = global::Android.Views.MotionEvent.Obtain(
					0,
					0,
					global::Android.Views.MotionEventActions.Down,
					12 * density,
					34 * density,
					0)!;

				native.DispatchTouchEvent(input);

				var point = await touched.Task.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.InRange(point.X, 11.9f, 12.1f);
				Assert.InRange(point.Y, 33.9f, 34.1f);
			}
			finally
			{
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
			}
		}
#endif

		[UIFact]
		public async Task GaneshInvalidationAndLogicalPixelSize()
		{
			RequireGaneshBackend();

			var view = new SKGaneshView();
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

				// A pre-scaling Android frame may still finish after the UI changes the property.
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

		private static void RequireGaneshBackend()
		{
#if IOS || MACCATALYST
			GpuPolicy.RequireOrSkip(GpuBackends.GaneshMetal);
#endif
		}
#endif
	}
}
