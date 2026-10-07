using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SkiaSharp.Views.Maui.Controls.WPF.Platform;

internal sealed class WPFTouchHandler
{
	private const long MouseId = -1;
	private readonly FrameworkElement view;
	private readonly Func<bool> ignorePixelScaling;
	private readonly Action<SKTouchEventArgs> onTouch;
	private readonly Dictionary<int, (TouchDevice Device, SKPoint Location)> activeTouches = new();
	private SKPoint mouseLocation;
	private SKMouseButton mouseButton = SKMouseButton.Unknown;
	private long mousePointerId = MouseId;
	private SKTouchDeviceType mouseDevice = SKTouchDeviceType.Mouse;
	private bool mousePressed;
	private bool enabled;

	public WPFTouchHandler(FrameworkElement view, Func<bool> ignorePixelScaling, Action<SKTouchEventArgs> onTouch)
	{
		this.view = view;
		this.ignorePixelScaling = ignorePixelScaling;
		this.onTouch = onTouch;
	}

	public void SetEnabled(bool value)
	{
		if (enabled == value)
			return;

		enabled = value;
		if (value)
		{
			view.TouchDown += OnTouchDown;
			view.TouchMove += OnTouchMove;
			view.TouchUp += OnTouchUp;
			view.LostTouchCapture += OnLostTouchCapture;
			view.MouseEnter += OnMouseEnter;
			view.MouseLeave += OnMouseLeave;
			view.MouseDown += OnMouseDown;
			view.MouseMove += OnMouseMove;
			view.MouseUp += OnMouseUp;
			view.MouseWheel += OnMouseWheel;
			view.LostMouseCapture += OnLostMouseCapture;
			view.Unloaded += OnUnloaded;
		}
		else
		{
			view.TouchDown -= OnTouchDown;
			view.TouchMove -= OnTouchMove;
			view.TouchUp -= OnTouchUp;
			view.LostTouchCapture -= OnLostTouchCapture;
			view.MouseEnter -= OnMouseEnter;
			view.MouseLeave -= OnMouseLeave;
			view.MouseDown -= OnMouseDown;
			view.MouseMove -= OnMouseMove;
			view.MouseUp -= OnMouseUp;
			view.MouseWheel -= OnMouseWheel;
			view.LostMouseCapture -= OnLostMouseCapture;
			view.Unloaded -= OnUnloaded;
			CancelActive();
		}
	}

	public void Detach() => SetEnabled(false);

	internal static SKPoint GetTouchLocation(double x, double y, double scaleX, double scaleY, bool ignorePixelScaling) =>
		new((float)(x * (ignorePixelScaling ? 1 : scaleX)), (float)(y * (ignorePixelScaling ? 1 : scaleY)));

	private SKPoint GetLocation(Point point)
	{
		var transform = PresentationSource.FromVisual(view)?.CompositionTarget.TransformToDevice ?? Matrix.Identity;
		return GetTouchLocation(point.X, point.Y, transform.M11, transform.M22, ignorePixelScaling());
	}

	private void OnTouchDown(object? sender, TouchEventArgs e)
	{
		var location = GetLocation(e.GetTouchPoint(view).Position);
		activeTouches[e.TouchDevice.Id] = (e.TouchDevice, location);
		var args = new SKTouchEventArgs(e.TouchDevice.Id, SKTouchAction.Pressed, SKMouseButton.Left, SKTouchDeviceType.Touch, location, true);
		onTouch(args);
		e.Handled = args.Handled;
		if (enabled && activeTouches.ContainsKey(e.TouchDevice.Id) && !e.TouchDevice.Capture(view))
			OnLostTouchCapture(sender, e);
	}

	private void OnTouchMove(object? sender, TouchEventArgs e)
	{
		if (!activeTouches.ContainsKey(e.TouchDevice.Id))
			return;

		var location = GetLocation(e.GetTouchPoint(view).Position);
		activeTouches[e.TouchDevice.Id] = (e.TouchDevice, location);
		var args = new SKTouchEventArgs(e.TouchDevice.Id, SKTouchAction.Moved, SKMouseButton.Left, SKTouchDeviceType.Touch, location, true);
		onTouch(args);
		e.Handled = args.Handled;
	}

	private void OnTouchUp(object? sender, TouchEventArgs e)
	{
		if (!activeTouches.Remove(e.TouchDevice.Id))
			return;

		var location = GetLocation(e.GetTouchPoint(view).Position);
		var args = new SKTouchEventArgs(e.TouchDevice.Id, SKTouchAction.Released, SKMouseButton.Left, SKTouchDeviceType.Touch, location, false);
		onTouch(args);
		e.Handled = args.Handled;
		if (e.TouchDevice.Captured == view)
			e.TouchDevice.Capture(null);
	}

	private void OnLostTouchCapture(object? sender, TouchEventArgs e)
	{
		if (activeTouches.Remove(e.TouchDevice.Id, out var touch))
			onTouch(new SKTouchEventArgs(e.TouchDevice.Id, SKTouchAction.Cancelled, SKMouseButton.Left, SKTouchDeviceType.Touch, touch.Location, false));
	}

	private static SKTouchDeviceType GetDevice(MouseEventArgs e) =>
		e.StylusDevice is not null ? SKTouchDeviceType.Pen : SKTouchDeviceType.Mouse;

	private static bool IsPromotedTouch(MouseEventArgs e) =>
		e.StylusDevice?.TabletDevice.Type == TabletDeviceType.Touch;

	private void SendMouse(SKTouchAction action, MouseEventArgs e, SKMouseButton button, int wheelDelta = 0)
	{
		if (IsPromotedTouch(e))
			return;

		mouseLocation = GetLocation(e.GetPosition(view));
		var id = mousePressed || action == SKTouchAction.Released ? mousePointerId : e.StylusDevice?.Id ?? MouseId;
		var device = mousePressed || action == SKTouchAction.Released ? mouseDevice : GetDevice(e);
		var args = new SKTouchEventArgs(id, action, button, device, mouseLocation, mousePressed && action != SKTouchAction.Released, wheelDelta);
		onTouch(args);
		e.Handled = args.Handled;
	}

	private void OnMouseEnter(object sender, MouseEventArgs e) =>
		SendMouse(SKTouchAction.Entered, e, SKMouseButton.Unknown);

	private void OnMouseLeave(object sender, MouseEventArgs e) =>
		SendMouse(SKTouchAction.Exited, e, SKMouseButton.Unknown);

	private void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (IsPromotedTouch(e) || mousePressed)
			return;

		mousePressed = true;
		mousePointerId = e.StylusDevice?.Id ?? MouseId;
		mouseDevice = GetDevice(e);
		mouseButton = e.ChangedButton switch
		{
			MouseButton.Left => SKMouseButton.Left,
			MouseButton.Middle => SKMouseButton.Middle,
			MouseButton.Right => SKMouseButton.Right,
			_ => SKMouseButton.Unknown,
		};
		SendMouse(SKTouchAction.Pressed, e, mouseButton);
		if (enabled && mousePressed && !view.CaptureMouse())
			OnLostMouseCapture(sender, e);
	}

	private void OnMouseMove(object sender, MouseEventArgs e) =>
		SendMouse(SKTouchAction.Moved, e, mouseButton);

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (IsPromotedTouch(e) || !mousePressed)
			return;

		var pressedButton = mouseButton switch
		{
			SKMouseButton.Left => MouseButton.Left,
			SKMouseButton.Middle => MouseButton.Middle,
			SKMouseButton.Right => MouseButton.Right,
			_ => e.ChangedButton,
		};
		if (e.ChangedButton != pressedButton)
			return;

		mousePressed = false;
		SendMouse(SKTouchAction.Released, e, mouseButton);
		mouseButton = SKMouseButton.Unknown;
		if (view.IsMouseCaptured)
			view.ReleaseMouseCapture();
	}

	private void OnMouseWheel(object sender, MouseWheelEventArgs e) =>
		SendMouse(SKTouchAction.WheelChanged, e, SKMouseButton.Unknown, e.Delta);

	private void OnLostMouseCapture(object sender, MouseEventArgs e)
	{
		if (!mousePressed)
			return;

		mousePressed = false;
		onTouch(new SKTouchEventArgs(mousePointerId, SKTouchAction.Cancelled, mouseButton, mouseDevice, mouseLocation, false));
		mouseButton = SKMouseButton.Unknown;
	}

	private void OnUnloaded(object sender, RoutedEventArgs e) => CancelActive();

	private void CancelActive()
	{
		var cancelledTouches = activeTouches.ToArray();
		activeTouches.Clear();
		foreach (var (id, touch) in cancelledTouches)
		{
			onTouch(new SKTouchEventArgs(id, SKTouchAction.Cancelled, SKMouseButton.Left, SKTouchDeviceType.Touch, touch.Location, false));
			if (touch.Device.Captured == view)
				touch.Device.Capture(null);
		}

		if (mousePressed)
		{
			mousePressed = false;
			onTouch(new SKTouchEventArgs(mousePointerId, SKTouchAction.Cancelled, mouseButton, mouseDevice, mouseLocation, false));
		}
		mouseButton = SKMouseButton.Unknown;
		if (view.IsMouseCaptured)
			view.ReleaseMouseCapture();
	}
}
