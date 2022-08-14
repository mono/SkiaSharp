#nullable enable

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
				view.GenericMotion -= OnGenericMotion;
				if (enableTouchEvents)
				{
					view.Touch += OnTouch;
					view.GenericMotion += OnGenericMotion;
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

			var count = evt.PointerCount;

			switch (evt.ActionMasked)
			{
				case MotionEventActions.Down:
				case MotionEventActions.PointerDown:
					{
						for (var pointer = 0; pointer < count; pointer++)
						{
							var args = CreateTouchEventArgs(SKTouchAction.Pressed, true, evt, pointer);

							onTouchAction(args);
							e.Handled |= args.Handled;
						}
						break;
					}

				case MotionEventActions.Move:
					{
						for (var pointer = 0; pointer < count; pointer++)
						{
							var args = CreateTouchEventArgs(SKTouchAction.Moved, true, evt, pointer);

							onTouchAction(args);
							e.Handled |= args.Handled;
						}
						break;
					}

				case MotionEventActions.Up:
				case MotionEventActions.PointerUp:
					{
						for (var pointer = 0; pointer < count; pointer++)
						{
							var args = CreateTouchEventArgs(SKTouchAction.Released, false, evt, pointer);

							onTouchAction(args);
							e.Handled |= args.Handled;
						}
						break;
					}

				case MotionEventActions.Cancel:
					{
						for (var pointer = 0; pointer < count; pointer++)
						{
							var args = CreateTouchEventArgs(SKTouchAction.Cancelled, false, evt, pointer);

							onTouchAction(args);
							e.Handled |= args.Handled;
						}
						break;
					}
			}
		}

		private void OnGenericMotion(object sender, View.GenericMotionEventArgs e)
		{
			if (onTouchAction == null || scalePixels == null)
				return;

			var evt = e.Event;
			if (evt == null)
				return;

			if (!evt.IsFromSource(InputSourceType.Mouse))
				return;

			var pointer = evt.ActionIndex;

			var id = evt.GetPointerId(pointer);
			var coords = scalePixels?.Invoke(evt.GetX(pointer), evt.GetY(pointer)) ??
			             new SKPoint(evt.GetX(pointer), evt.GetY(pointer));

			var toolType = evt.GetToolType(pointer);
			var deviceType = GetDeviceType(toolType);
			var button = GetButton(evt.ActionButton, toolType);
			var inContact = button != SKMouseButton.Unknown;

			switch (evt.Action)
			{
				case MotionEventActions.ButtonPress:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Pressed, button, deviceType, coords, inContact);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
				case MotionEventActions.ButtonRelease:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Released, button, deviceType, coords, inContact);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
				case MotionEventActions.HoverEnter:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Entered, button, deviceType, coords, inContact);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
				case MotionEventActions.HoverExit:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Exited, button, deviceType, coords, inContact);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
				case MotionEventActions.HoverMove:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Moved, button, deviceType, coords, inContact);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
				case MotionEventActions.Scroll:
					{
						var axisValue = evt.GetAxisValue(Axis.Vscroll, pointer);
						var wheelDelta = axisValue.Equals(0) ? 0 : axisValue > 0 ? 1 : -1;
						var args = new SKTouchEventArgs(id, SKTouchAction.WheelChanged, button, deviceType, coords, inContact, wheelDelta);

						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
			}
		}

		private SKTouchEventArgs CreateTouchEventArgs(SKTouchAction actionType, bool inContact, MotionEvent evt, int pointerIndex)
		{
			var id = evt.GetPointerId(pointerIndex);
			var toolType = evt.GetToolType(pointerIndex);
			var button = GetButton(evt.ButtonState, toolType);
			var coords = scalePixels?.Invoke(evt.GetX(pointerIndex), evt.GetY(pointerIndex)) ??
			             new SKPoint(evt.GetX(pointerIndex), evt.GetY(pointerIndex));
			var deviceType = GetDeviceType(toolType);
			var pressure = evt.GetPressure(pointerIndex);

			return new SKTouchEventArgs(id, actionType, button, deviceType, coords, inContact, 0, pressure);
		}

		private static SKMouseButton GetButton(MotionEventButtonState buttonState, MotionEventToolType toolType)
		{
			var button = SKMouseButton.Left;

			if (buttonState.HasFlag(MotionEventButtonState.StylusPrimary))
			{
				button = SKMouseButton.Left;
			}
			else if (buttonState.HasFlag(MotionEventButtonState.Primary))
			{
				button = SKMouseButton.Left;
			}
			else if (buttonState.HasFlag(MotionEventButtonState.StylusSecondary))
			{
				button = SKMouseButton.Right;
			}
			else if (buttonState.HasFlag(MotionEventButtonState.Secondary))
			{
				button = SKMouseButton.Right;
			}
			else if (buttonState.HasFlag(MotionEventButtonState.Tertiary))
			{
				button = SKMouseButton.Middle;
			}
			else if (toolType == MotionEventToolType.Mouse)
			{
				button = SKMouseButton.Unknown;
			}

			return button;
		}

		private static SKTouchDeviceType GetDeviceType(MotionEventToolType toolType) =>
			toolType switch
			{
				MotionEventToolType.Unknown => SKTouchDeviceType.Touch,
				MotionEventToolType.Finger => SKTouchDeviceType.Touch,
				MotionEventToolType.Stylus => SKTouchDeviceType.Pen,
				MotionEventToolType.Eraser => SKTouchDeviceType.Eraser,
				MotionEventToolType.Mouse => SKTouchDeviceType.Mouse,
				_ => SKTouchDeviceType.Touch,
			};
	}
}
