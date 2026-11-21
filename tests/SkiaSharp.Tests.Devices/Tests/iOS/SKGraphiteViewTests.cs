#nullable enable

using System;
using System.Threading.Tasks;
using CoreGraphics;
using SkiaSharp.Tests;
using SkiaSharp.Views.Maui.Controls.Tests;
using UIKit;
using Xunit;

namespace SkiaSharp.Views.iOS.Tests
{
	public class SKGraphiteViewTests : SKUITests
	{
		[UIFact]
		public void DefaultPropertyValues()
		{
			using var view = new SKGraphiteView();

			Assert.False(view.HasRenderLoop);
			Assert.False(view.IgnorePixelScaling);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GraphiteContext);

			view.InvalidateSurface();

			Assert.Null(view.GraphiteContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task RendersWithPaintSurfaceAndCleansUpOnDetachAndDispose()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GraphiteMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new SKGraphiteView(new CGRect(0, 0, 128, 128));

			var frameTcs = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);
			var frameCount = 0;

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
					Assert.NotNull(e.Surface);
					Assert.NotNull(e.Context);
					Assert.Same(view.GraphiteContext, e.Context);
					Assert.NotNull(e.Recorder);
					Assert.NotNull(e.BackendTexture);
					Assert.True(e.BackendTexture!.IsValid);
					Assert.Equal(SKGraphiteBackend.Metal, e.BackendTexture.Backend);
					Assert.Equal((SKSize)e.Info.Size, view.CanvasSize);
					Assert.True(e.RawInfo.Width > 0 && e.RawInfo.Height > 0);
					Assert.Equal(SKMatrix.Identity, e.Surface.Canvas.TotalMatrix);

					e.Surface.Canvas.Clear(SKColors.Red);
					Assert.Equal(SKColors.Red, ReadPixel(e, 0, 0));

					// Leave an unbalanced save/transform to verify automatic canvas restore between frames.
					e.Surface.Canvas.Save();
					e.Surface.Canvas.Translate(50, 50);

					frameCount++;
					frameTcs.TrySetResult(e.Context!);
				}
				catch (Exception ex)
				{
					frameTcs.TrySetException(ex);
				}
			};

			try
			{
				hostView.AddSubview(view);
				view.InvalidateSurface();

				var firstContext = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(firstContext);
				Assert.Same(firstContext, view.GraphiteContext);
				Assert.True(frameCount >= 1);

				// Request a second frame and verify the canvas matrix was restored.
				frameTcs = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);
				view.InvalidateSurface();

				var secondFrameContext = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(firstContext, secondFrameContext);

				// Detach from window and verify context and canvas size are cleared.
				view.RemoveFromSuperview();
				Assert.Null(view.GraphiteContext);
				Assert.Equal(SKSize.Empty, view.CanvasSize);

				// Reattach and verify a new context is created.
				frameTcs = new TaskCompletionSource<SKGraphiteContext>(TaskCreationOptions.RunContinuationsAsynchronously);
				hostView.AddSubview(view);
				view.InvalidateSurface();

				var reattachedContext = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(reattachedContext);
				Assert.NotSame(firstContext, reattachedContext);
			}
			finally
			{
				view.RemoveFromSuperview();
			}

			view.Dispose();
			Assert.Null(view.GraphiteContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task OnPaintSurfaceOverrideIsInvokedAndRaisesPaintSurfaceEvent()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GraphiteMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new OverrideGraphiteView(new CGRect(0, 0, 64, 64));

			var eventTcs = new TaskCompletionSource<SKColor>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.True(view.OnPaintSurfaceCalled);
					eventTcs.TrySetResult(ReadPixel(e, 0, 0));
				}
				catch (Exception ex)
				{
					eventTcs.TrySetException(ex);
				}
			};

			try
			{
				hostView.AddSubview(view);
				view.InvalidateSurface();

				var pixel = await eventTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Equal(SKColors.Green, pixel);
			}
			finally
			{
				view.RemoveFromSuperview();
			}
		}

		[UIFact]
		public async Task IgnorePixelScalingUsesLogicalSizeAndScalesCanvas()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GraphiteMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new SKGraphiteView(new CGRect(0, 0, 120, 90))
			{
				IgnorePixelScaling = true,
			};

			var frameTcs = new TaskCompletionSource<(SKImageInfo Info, SKImageInfo RawInfo, SKColor CornerPixel)>(
				TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					using var paint = new SKPaint
					{
						Color = SKColors.Blue,
						Style = SKPaintStyle.Fill,
					};

					e.Surface.Canvas.Clear(SKColors.Black);
					e.Surface.Canvas.DrawRect(0, 0, e.Info.Width, e.Info.Height, paint);

					var cornerPixel = ReadPixel(
						e,
						Math.Max(0, e.RawInfo.Width - 2),
						Math.Max(0, e.RawInfo.Height - 2));

					frameTcs.TrySetResult((e.Info, e.RawInfo, cornerPixel));
				}
				catch (Exception ex)
				{
					frameTcs.TrySetException(ex);
				}
			};

			try
			{
				hostView.AddSubview(view);
				view.InvalidateSurface();

				var (info, rawInfo, cornerPixel) = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

				Assert.Equal(new SKSizeI(120, 90), info.Size);
				Assert.Equal(new SKSize(120, 90), view.CanvasSize);
				Assert.True(rawInfo.Width >= info.Width);
				Assert.True(rawInfo.Height >= info.Height);
				Assert.Equal(SKColors.Blue, cornerPixel);
			}
			finally
			{
				view.RemoveFromSuperview();
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

		private sealed class OverrideGraphiteView : SKGraphiteView
		{
			public OverrideGraphiteView(CGRect frame)
				: base(frame)
			{
			}

			public bool OnPaintSurfaceCalled { get; private set; }

			protected override void OnPaintSurface(SKPaintGraphiteSurfaceEventArgs e)
			{
				OnPaintSurfaceCalled = true;
				e.Surface.Canvas.Clear(SKColors.Green);
				base.OnPaintSurface(e);
			}
		}
	}
}
