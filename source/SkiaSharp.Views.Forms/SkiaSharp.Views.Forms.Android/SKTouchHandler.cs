using System;
using Android.Views;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler
	{
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<float, float> scalePixels;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<float, float> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void Attach(View view)
		{
			view.Touch += OnTouch;
		}

		public void Detach(View view)
		{
			// clean the view
			if (view != null)
			{
				view.Touch -= OnTouch;
			}

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
			var coords = new SKPoint(scalePixels(evt.GetX(pointer)), scalePixels(evt.GetY(pointer)));

			switch (evt.ActionMasked)
			{
				case MotionEventActions.Down:
				case MotionEventActions.PointerDown:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Pressed, coords, true);
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
							coords = new SKPoint(scalePixels(evt.GetX(pointer)), scalePixels(evt.GetY(pointer)));

							var args = new SKTouchEventArgs(id, SKTouchAction.Moved, coords, true);
							onTouchAction(args);
							e.Handled = e.Handled || args.Handled;
						}
						break;
					}

				case MotionEventActions.Up:
				case MotionEventActions.PointerUp:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Released, coords, false);
						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}

				case MotionEventActions.Cancel:
					{
						var args = new SKTouchEventArgs(id, SKTouchAction.Cancelled, coords, false);
						onTouchAction(args);
						e.Handled = args.Handled;
						break;
					}
			}
		}
	}
}
