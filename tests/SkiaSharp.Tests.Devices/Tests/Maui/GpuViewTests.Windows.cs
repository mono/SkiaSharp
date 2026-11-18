#nullable enable

#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace SkiaSharp.Views.Maui.Controls.Tests;

public partial class GpuViewTests
{
	[UIFact]
	public async Task GaneshNativePointerUsesLogicalCoordinatesWhenScalingIsIgnored()
	{
		var view = new SKGaneshView { IgnorePixelScaling = true, EnableTouchEvents = true };
		var frame = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var pressed = new TaskCompletionSource<SKTouchEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
		Exception? failure = null;
		uint injected = 0;
		view.RenderFailed += (_, e) => { failure = e.Exception; frame.TrySetException(e.Exception); };
		view.PaintSurface += (_, e) =>
		{
			e.Surface.Canvas.Clear(SKColors.Blue);
			using var paint = new SKPaint { Color = SKColors.Magenta };
			e.Surface.Canvas.DrawRect(0, 0, 64, 64, paint);
			frame.TrySetResult();
		};
		view.Touch += (_, e) =>
		{
			e.Handled = true;
			if (e.ActionType == SKTouchAction.Pressed) pressed.TrySetResult(e);
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
			await AssertEx.Eventually(() => failure is null &&
				ScreenColor(screen) == 0x00ff00ff && ScreenColor(blue) == 0x00ff0000,
				timeout: 5000, message: "The Ganesh pattern was not presented in the composited window.");
			Assert.Null(failure);
			Assert.True(SetCursorPos(screen.X, screen.Y));
			Assert.Equal(handle, GetForegroundWindow());
			var inputs = new[] {
				new MouseEvent { Mouse = new MouseData { Flags = 0x0002 } },
				new MouseEvent { Mouse = new MouseData { Flags = 0x0004 } },
			};
			injected = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<MouseEvent>());
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
				var release = new[] { new MouseEvent { Mouse = new MouseData { Flags = 0x0004 } } };
				released = SendInput(1, release, Marshal.SizeOf<MouseEvent>());
			}
			var restored = SetCursorPos(previousPosition.X, previousPosition.Y);
			CurrentPage.Content = null;
			view.Handler?.DisconnectHandler();
			Assert.Null(failure);
			Assert.Equal(1u, released);
			Assert.True(restored);
		}

		CursorPoint ScreenPoint(double x, double y)
		{
			var location = native.TransformToVisual(window.Content).TransformPoint(new global::Windows.Foundation.Point(x, y));
			var scale = native.XamlRoot.RasterizationScale;
			var screen = new CursorPoint { X = (int)Math.Round(location.X * scale), Y = (int)Math.Round(location.Y * scale) };
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
		finally { Assert.Equal(1, ReleaseDC(IntPtr.Zero, dc)); }
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct CursorPoint
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MouseData
	{
		public int X;
		public int Y;
		public uint Data;
		public uint Flags;
		public uint Time;
		public UIntPtr ExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MouseEvent
	{
		public uint Type;
		public MouseData Mouse;
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClientToScreen(IntPtr window, ref CursorPoint point);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out CursorPoint point);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint SendInput(uint count, [In] MouseEvent[] inputs, int size);

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr window);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr window, IntPtr dc);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern uint GetPixel(IntPtr dc, int x, int y);
}
#endif
