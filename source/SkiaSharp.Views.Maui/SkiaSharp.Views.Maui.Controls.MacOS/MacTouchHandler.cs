using System;
using AppKit;
using CoreGraphics;
using SkiaSharp.Views.Maui;

namespace SkiaSharp.Views.Maui.Handlers
{
	internal sealed class MacTouchHandler
	{
		private NSView? platformView;
		private object? virtualView;
		private NSTrackingArea? trackingArea;
		private bool enabled;
		private bool inContact;
		private SKMouseButton pressedButton;
		private SKPoint lastLocation;

		public void Connect(NSView platformView, object virtualView)
		{
			this.platformView = platformView;
			this.virtualView = virtualView;
		}

		public void SetEnabled(bool enabled)
		{
			if (this.enabled == enabled)
				return;

			if (!enabled)
				Cancel();

			this.enabled = enabled;
			if (platformView is null)
				return;

			if (enabled)
			{
				trackingArea = new NSTrackingArea(
					CGRect.Empty,
					NSTrackingAreaOptions.ActiveInKeyWindow |
					NSTrackingAreaOptions.InVisibleRect |
					NSTrackingAreaOptions.MouseEnteredAndExited |
					NSTrackingAreaOptions.MouseMoved,
					platformView,
					null);
				platformView.AddTrackingArea(trackingArea);
			}
			else if (trackingArea is not null)
			{
				platformView.RemoveTrackingArea(trackingArea);
				trackingArea.Dispose();
				trackingArea = null;
			}
		}

		public void Disconnect()
		{
			SetEnabled(false);
			virtualView = null;
			platformView = null;
		}

		public bool Handle(NSEvent e, SKTouchAction action, SKMouseButton button = SKMouseButton.Unknown)
		{
			if (!enabled || platformView is null || virtualView is null)
				return false;

			if (action == SKTouchAction.Released && (!inContact || button != pressedButton))
				return false;
			if (action == SKTouchAction.Pressed)
				Cancel();

			var p = platformView.ConvertPointFromView(e.LocationInWindow, null);
			var scale = IsIgnoringPixelScaling ? 1 : (double)(platformView.Window?.BackingScaleFactor ?? 1);
			var y = platformView.IsFlipped ? (double)p.Y : (double)(platformView.Bounds.Height - p.Y);
			lastLocation = new SKPoint((float)(p.X * scale), (float)(y * scale));

			if (action == SKTouchAction.Pressed)
			{
				inContact = true;
				pressedButton = button;
			}
			else if (action == SKTouchAction.Released || action == SKTouchAction.Cancelled)
			{
				inContact = false;
			}

			var effectiveButton = button == SKMouseButton.Unknown ? pressedButton : button;
			var args = new SKTouchEventArgs(
				effectiveButton switch
				{
					SKMouseButton.Right => 2,
					SKMouseButton.Middle => 3,
					_ => 1,
				},
				action,
				effectiveButton,
				SKTouchDeviceType.Mouse,
				lastLocation,
				inContact,
				action == SKTouchAction.WheelChanged ? (int)e.ScrollingDeltaY : 0);
			OnTouch(args);
			if (action == SKTouchAction.Released || action == SKTouchAction.Cancelled)
				pressedButton = SKMouseButton.Unknown;
			return args.Handled;
		}

		private bool IsIgnoringPixelScaling => virtualView switch
		{
			ISKCanvasView canvas => canvas.IgnorePixelScaling,
			ISKGLView gl => gl.IgnorePixelScaling,
			_ => false,
		};

		private void Cancel()
		{
			if (!inContact)
				return;

			inContact = false;
			var args = new SKTouchEventArgs(
				pressedButton switch
				{
					SKMouseButton.Right => 2,
					SKMouseButton.Middle => 3,
					_ => 1,
				},
				SKTouchAction.Cancelled, pressedButton, SKTouchDeviceType.Mouse, lastLocation, false);
			OnTouch(args);
			pressedButton = SKMouseButton.Unknown;
		}

		private void OnTouch(SKTouchEventArgs args)
		{
			if (virtualView is ISKCanvasView canvas)
				canvas.OnTouch(args);
			else if (virtualView is ISKGLView gl)
				gl.OnTouch(args);
		}
	}
}
