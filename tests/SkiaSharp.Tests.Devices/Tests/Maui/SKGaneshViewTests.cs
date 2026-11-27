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

#if ANDROID || IOS || MACCATALYST || WINDOWS
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
#elif WINDOWS
		[UIFact]
		public async Task GaneshNativePointerUsesLogicalCoordinatesWhenScalingIsIgnored()
		{
			var view = new SKGaneshView
			{
				IgnorePixelScaling = true,
				EnableTouchEvents = true,
			};

			var frame = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			var pressed = new TaskCompletionSource<SKTouchEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
			uint injected = 0;

			view.PaintSurface += (_, e) =>
			{
				try
				{
					e.Surface.Canvas.Clear(SKColors.Blue);

					using var paint = new SKPaint
					{
						Color = SKColors.Magenta,
					};
					e.Surface.Canvas.DrawRect(0, 0, 64, 64, paint);

					frame.TrySetResult();
				}
				catch (Exception ex)
				{
					frame.TrySetException(ex);
				}
			};

			view.Touch += (_, e) =>
			{
				e.Handled = true;
				if (e.ActionType == SKTouchAction.Pressed)
				{
					pressed.TrySetResult(e);
				}
			};

			CurrentPage.Content = view;
			await view.WaitForLoaded();
			await view.WaitForLayout();
			await frame.Task.WaitAsync(TimeSpan.FromSeconds(10));

			var native = (Microsoft.UI.Xaml.FrameworkElement)view.Handler!.PlatformView!;
			var window = (Microsoft.UI.Xaml.Window)CurrentPage.Window.Handler!.PlatformView!;
			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
			GetWindowThreadProcessId(handle, out var processId);
			Assert.Equal((uint)Environment.ProcessId, processId);
			Assert.True(GetCursorPos(out var previousPosition));

			try
			{
				window.Activate();
				await AssertEx.Eventually(() => GetForegroundWindow() == handle, timeout: 5000);

				var screen = ScreenPoint(12, 34);
				var blue = ScreenPoint(80, 80);

				await AssertEx.Eventually(
					() => ScreenColor(screen) == 0x00ff00ff && ScreenColor(blue) == 0x00ff0000,
					timeout: 5000,
					message: "The Ganesh pattern was not presented in the composited window.");

				Assert.True(SetCursorPos(screen.X, screen.Y));
				Assert.Equal(handle, GetForegroundWindow());

				var inputs = new[]
				{
					new MouseEvent { Mouse = new MouseData { Flags = 0x0002 } },
					new MouseEvent { Mouse = new MouseData { Flags = 0x0004 } },
				};
				injected = SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf<MouseEvent>());
				Assert.Equal((uint)inputs.Length, injected);

				var touch = await pressed.Task.WaitAsync(TimeSpan.FromSeconds(5));
				Assert.Equal(SKTouchDeviceType.Mouse, touch.DeviceType);
				Assert.Equal(SKMouseButton.Left, touch.MouseButton);
				Assert.InRange(touch.Location.X, 11f, 13f);
				Assert.InRange(touch.Location.Y, 33f, 35f);
			}
			finally
			{
				uint released = 1;
				if (injected == 1)
				{
					var release = new[]
					{
						new MouseEvent { Mouse = new MouseData { Flags = 0x0004 } },
					};
					released = SendInput(1, release, System.Runtime.InteropServices.Marshal.SizeOf<MouseEvent>());
				}

				var restored = SetCursorPos(previousPosition.X, previousPosition.Y);
				CurrentPage.Content = null;
				view.Handler?.DisconnectHandler();
				Assert.Equal(1u, released);
				Assert.True(restored);
			}

			CursorPoint ScreenPoint(double x, double y)
			{
				var location = native.TransformToVisual(window.Content).TransformPoint(new global::Windows.Foundation.Point(x, y));
				var scale = native.XamlRoot.RasterizationScale;
				var screen = new CursorPoint
				{
					X = (int)Math.Round(location.X * scale),
					Y = (int)Math.Round(location.Y * scale),
				};
				Assert.True(ClientToScreen(handle, ref screen));
				return screen;
			}
		}

		private static uint ScreenColor(CursorPoint point)
		{
			var dc = GetDC(IntPtr.Zero);
			Assert.NotEqual(IntPtr.Zero, dc);
			try
			{
				var color = GetPixel(dc, point.X, point.Y);
				Assert.NotEqual(uint.MaxValue, color);
				return color;
			}
			finally
			{
				Assert.Equal(1, ReleaseDC(IntPtr.Zero, dc));
			}
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		private struct CursorPoint
		{
			public int X;
			public int Y;
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		private struct MouseData
		{
			public int X;
			public int Y;
			public uint Data;
			public uint Flags;
			public uint Time;
			public UIntPtr ExtraInfo;
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		private struct MouseEvent
		{
			public uint Type;
			public MouseData Mouse;
		}

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		private static extern bool ClientToScreen(IntPtr window, ref CursorPoint point);

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out CursorPoint point);

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		private static extern uint SendInput(uint count, [System.Runtime.InteropServices.In] MouseEvent[] inputs, int size);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr window);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr window, IntPtr dc);

		[System.Runtime.InteropServices.DllImport("gdi32.dll", SetLastError = true)]
		private static extern uint GetPixel(IntPtr dc, int x, int y);
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
