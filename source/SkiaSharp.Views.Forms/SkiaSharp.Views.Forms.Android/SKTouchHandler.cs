using System;
using Android.Views;

#if __MAUI__
namespace SkiaSharp.Views.Maui.Platform
#else
namespace SkiaSharp.Views.Forms
#endif
{
	internal class SKTouchHandler
	{
		private Action<SKTouchEventArgs>? onTouchAction;
		private Func<double, double, SKPoint>? scalePixels;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(View view, bool enableTouchEvents)
		{
			if (view != null)
			{
				view.Touch -= OnTouch;
				if (enableTouchEvents)
				{
					view.Touch += OnTouch;
				}
			}
		}

		public void Detach(View view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		private void OnTouch(object? sender, View.TouchEventArgs e)
		{
			if (onTouchAction == null || scalePixels == null)
				return;

			var evt = e.Event;
			if (evt == null)
				return;

			var pointer = evt.ActionIndex;

			var id = evt.GetPointerId(pointer);
			var coords = scalePixels(evt.GetX(pointer), evt.GetY(pointer));

			var toolType = evt.GetToolType(pointer);

			var deviceType = GetDeviceType(toolType);

			var pressure = evt.GetPressure(pointer);

			var button = GetButton(evt, toolType);

			switch (evt.ActionMasked)
			{
				case MotionEventActions.Down:
				case MotionEventActions.PointerDown:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Pressed, button, deviceType, coords, true, 0, pressure);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}

				case MotionEventActions.Move:
					{
						var count = evt.PointerCount;
						for (pointer = 0; pointer < count; pointer++)
						{
							id = evt.GetPointerId(pointer);
							coords = scalePixels(evt.GetX(pointer), evt.GetY(pointer));

							var args = new SKTouchEventArgs(id, SKTouchAction.Moved, button, deviceType, coords, true, 0, pressure);

							onTouchAction(args);
							e.Handled = e.Handled || args.Handled;
						}
						break;
					}

				case MotionEventActions.Up:
				case MotionEventActions.PointerUp:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Released, button, deviceType, coords, false, 0, pressure);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}

				case MotionEventActions.Cancel:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Cancelled, button, deviceType, coords, false, 0, pressure);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
			}
		}

		private static SKMouseButton GetButton(MotionEvent evt, MotionEventToolType toolType)
		{
			var button = SKMouseButton.Left;

			if (toolType == MotionEventToolType.Eraser)
			{
				button = SKMouseButton.Middle;
			}
			else if (evt.ButtonState.HasFlag(MotionEventButtonState.StylusSecondary))
			{
				button = SKMouseButton.Right;
			}

			return button;
		}

		private static SKTouchDeviceType GetDeviceType(MotionEventToolType toolType) =>
			toolType switch
			{
				MotionEventToolType.Unknown => SKTouchDeviceType.Touch,
				MotionEventToolType.Finger => SKTouchDeviceType.Touch,
				MotionEventToolType.Stylus => SKTouchDeviceType.Pen,
				MotionEventToolType.Eraser => SKTouchDeviceType.Pen,
				MotionEventToolType.Mouse => SKTouchDeviceType.Mouse,
				_ => SKTouchDeviceType.Touch,
			};
	}
}
