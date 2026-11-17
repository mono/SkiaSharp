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
	public class SKGaneshViewTests : SKUITests
	{
		[UIFact]
		public void DefaultPropertyValues()
		{
			using var view = new SKGaneshView();

			Assert.False(view.HasRenderLoop);
			Assert.False(view.IgnorePixelScaling);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);

			view.InvalidateSurface();

			Assert.Null(view.GRContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task RendersWithPaintSurfaceAndCleansUpOnDetachAndDispose()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GaneshMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new SKGaneshView(new CGRect(0, 0, 128, 128));

			var frameTcs = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);
			var frameCount = 0;

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.IsAssignableFrom<SKPaintSurfaceEventArgs>(e);
					Assert.NotNull(e.Surface);
					Assert.NotNull(e.Context);
					Assert.Same(view.GRContext, e.Context);
					Assert.NotNull(e.BackendRenderTarget);
					Assert.Equal(GRSurfaceOrigin.TopLeft, e.Origin);
					Assert.Equal((SKSize)e.Info.Size, view.CanvasSize);
					Assert.True(e.RawInfo.Width > 0 && e.RawInfo.Height > 0);
					Assert.Equal(SKMatrix.Identity, e.Surface.Canvas.TotalMatrix);

					e.Surface.Canvas.Clear(SKColors.Red);

					using var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Rgba8888));
					Assert.True(e.Surface.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0));
					Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));

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
				Assert.Same(firstContext, view.GRContext);
				Assert.True(frameCount >= 1);

				// Request a second frame and verify the canvas matrix was restored.
				frameTcs = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);
				view.InvalidateSurface();

				var secondFrameContext = await frameTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(firstContext, secondFrameContext);

				// Detach from window and verify context and canvas size are cleared.
				view.RemoveFromSuperview();
				Assert.Null(view.GRContext);
				Assert.Equal(SKSize.Empty, view.CanvasSize);

				// Reattach and verify a new context is created.
				frameTcs = new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously);
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
			Assert.Null(view.GRContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task OnPaintSurfaceOverrideIsInvokedAndRaisesPaintSurfaceEvent()
		{
			GpuPolicy.RequireOrSkip(GpuBackends.GaneshMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new OverrideGaneshView(new CGRect(0, 0, 64, 64));

			var eventTcs = new TaskCompletionSource<SKColor>(TaskCreationOptions.RunContinuationsAsynchronously);

			view.PaintSurface += (_, e) =>
			{
				try
				{
					Assert.True(view.OnPaintSurfaceCalled);

					using var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Rgba8888));
					Assert.True(e.Surface.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0));
					eventTcs.TrySetResult(bitmap.GetPixel(0, 0));
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
			GpuPolicy.RequireOrSkip(GpuBackends.GaneshMetal);

			var hostView = Assert.IsAssignableFrom<UIView>(CurrentPage.Handler!.PlatformView);
			using var view = new SKGaneshView(new CGRect(0, 0, 120, 90))
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

					using var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Rgba8888));
					Assert.True(e.Surface.ReadPixels(
						bitmap.Info,
						bitmap.GetPixels(),
						bitmap.RowBytes,
						Math.Max(0, e.RawInfo.Width - 2),
						Math.Max(0, e.RawInfo.Height - 2)));

					frameTcs.TrySetResult((e.Info, e.RawInfo, bitmap.GetPixel(0, 0)));
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

		private sealed class OverrideGaneshView : SKGaneshView
		{
			public OverrideGaneshView(CGRect frame)
				: base(frame)
			{
			}

			public bool OnPaintSurfaceCalled { get; private set; }

			protected override void OnPaintSurface(SKPaintGaneshSurfaceEventArgs e)
			{
				OnPaintSurfaceCalled = true;
				e.Surface.Canvas.Clear(SKColors.Green);
				base.OnPaintSurface(e);
			}
		}
	}
}
