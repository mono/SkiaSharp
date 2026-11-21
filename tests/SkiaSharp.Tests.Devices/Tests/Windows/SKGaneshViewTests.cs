#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp.Tests;
using SkiaSharp.Views.Maui.Controls.Tests;
using Xunit;

namespace SkiaSharp.Views.Windows.Tests
{
	public class SKGaneshViewTests : SKUITests
	{
		[UIFact]
		public void DefaultPropertyValues()
		{
			var view = new SKGaneshView();

			Assert.False(view.HasRenderLoop);
			Assert.False(view.IgnorePixelScaling);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
			Assert.Null(view.GRContext);

			view.InvalidateSurface();

			Assert.Null(view.GRContext);
			Assert.Equal(SKSize.Empty, view.CanvasSize);
		}

		[UIFact]
		public async Task RendersWithPaintSurfaceAndCleansUpOnDetach()
		{
			var hostPanel = Assert.IsAssignableFrom<Panel>(CurrentPage.Handler!.PlatformView);
			var view = new SKGaneshView
			{
				Width = 128,
				Height = 128,
			};

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
					Assert.Equal(GRSurfaceOrigin.BottomLeft, e.Origin);
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

					Interlocked.Increment(ref frameCount);
					Volatile.Read(ref frameTcs).TrySetResult(e.Context!);
				}
				catch (Exception ex)
				{
					Volatile.Read(ref frameTcs).TrySetException(ex);
				}
			};

			try
			{
				hostPanel.Children.Add(view);
				view.InvalidateSurface();

				var firstContext = await Volatile.Read(ref frameTcs).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(firstContext);
				Assert.Same(firstContext, view.GRContext);
				Assert.True(Volatile.Read(ref frameCount) >= 1);

				// Request a second frame and verify the canvas matrix was restored.
				Interlocked.Exchange(ref frameTcs, new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously));
				view.InvalidateSurface();

				var secondFrameContext = await Volatile.Read(ref frameTcs).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Same(firstContext, secondFrameContext);

				// Detach from visual tree and verify context and canvas size are cleared.
				hostPanel.Children.Remove(view);
				await AssertEx.Eventually(() => view.GRContext is null && view.CanvasSize == SKSize.Empty, timeout: 5000);

				// Reattach and verify a new context is created.
				Interlocked.Exchange(ref frameTcs, new TaskCompletionSource<GRContext>(TaskCreationOptions.RunContinuationsAsynchronously));
				hostPanel.Children.Add(view);
				view.InvalidateSurface();

				var reattachedContext = await Volatile.Read(ref frameTcs).Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.NotNull(reattachedContext);
				Assert.NotSame(firstContext, reattachedContext);
			}
			finally
			{
				hostPanel.Children.Remove(view);
			}

			await AssertEx.Eventually(() => view.GRContext is null && view.CanvasSize == SKSize.Empty, timeout: 5000);
		}

		[UIFact]
		public async Task OnPaintSurfaceOverrideIsInvokedAndRaisesPaintSurfaceEvent()
		{
			var hostPanel = Assert.IsAssignableFrom<Panel>(CurrentPage.Handler!.PlatformView);
			var view = new OverrideGaneshView
			{
				Width = 64,
				Height = 64,
			};

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
				hostPanel.Children.Add(view);
				view.InvalidateSurface();

				var pixel = await eventTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
				Assert.Equal(SKColors.Green, pixel);
			}
			finally
			{
				hostPanel.Children.Remove(view);
			}
		}

		[UIFact]
		public async Task IgnorePixelScalingUsesLogicalSizeAndScalesCanvas()
		{
			var hostPanel = Assert.IsAssignableFrom<Panel>(CurrentPage.Handler!.PlatformView);
			var view = new SKGaneshView
			{
				Width = 120,
				Height = 90,
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
				hostPanel.Children.Add(view);
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
				hostPanel.Children.Remove(view);
			}
		}

		private sealed class OverrideGaneshView : SKGaneshView
		{
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
