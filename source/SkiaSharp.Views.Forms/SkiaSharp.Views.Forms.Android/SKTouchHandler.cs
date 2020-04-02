using System;
using Android.Views;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler
	{
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

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

		private void OnTouch(object sender, View.TouchEventArgs e)
		{
			if (onTouchAction == null || scalePixels == null)
				return;

			var evt = e.Event;
			var pointer = evt.ActionIndex;

			var id = evt.GetPointerId(pointer);
			var coords = scalePixels(evt.GetX(pointer), evt.GetY(pointer));

			var toolType = evt.GetToolType(id);
			var deviceType = GetDeviceType(toolType);

			switch (evt.ActionMasked)
			{
				case MotionEventActions.Down:
				case MotionEventActions.PointerDown:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Pressed, SKMouseButton.Left, deviceType, coords, true);

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

							var args = new SKTouchEventArgs(id, SKTouchAction.Moved, SKMouseButton.Left, deviceType, coords, true);
							onTouchAction(args);
							e.Handled = e.Handled || args.Handled;
						}
						break;
					}

				case MotionEventActions.Up:
				case MotionEventActions.PointerUp:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Released, SKMouseButton.Left, deviceType, coords, false);
						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}

				case MotionEventActions.Cancel:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Cancelled, SKMouseButton.Left, deviceType, coords, false);
						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
			}
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
