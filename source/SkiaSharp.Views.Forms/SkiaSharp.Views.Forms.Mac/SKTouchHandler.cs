using System;
using System.Linq;
using Foundation;
using AppKit;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler : NSGestureRecognizer
	{
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(NSView view, bool enableTouchEvents)
		{
			if (view != null)
			{
				if (enableTouchEvents && !view.GestureRecognizers.Contains(this))
				{
					view.AddGestureRecognizer(this);
				}
				else if (!enableTouchEvents && view.GestureRecognizers.Contains(this))
				{
					view.RemoveGestureRecognizer(this);
				}
			}
		}

		public void Detach(NSView view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		public override void MouseDown(NSEvent mouseEvent)
		{
			base.MouseDown(mouseEvent);

			FireEvent(SKTouchAction.Pressed, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void MouseUp(NSEvent mouseEvent)
		{
			base.MouseUp(mouseEvent);

			FireEvent(SKTouchAction.Released, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void MouseDragged(NSEvent mouseEvent)
		{
			base.MouseDragged(mouseEvent);

			FireEvent(SKTouchAction.Moved, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void OtherMouseDown(NSEvent mouseEvent)
		{
			base.OtherMouseDown(mouseEvent);

			FireEvent(SKTouchAction.Pressed, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void OtherMouseUp(NSEvent mouseEvent)
		{
			base.OtherMouseUp(mouseEvent);

			FireEvent(SKTouchAction.Released, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void OtherMouseDragged(NSEvent mouseEvent)
		{
			base.OtherMouseDragged(mouseEvent);

			FireEvent(SKTouchAction.Moved, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void RightMouseDown(NSEvent mouseEvent)
		{
			base.RightMouseDown(mouseEvent);

			FireEvent(SKTouchAction.Pressed, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void RightMouseUp(NSEvent mouseEvent)
		{
			base.RightMouseUp(mouseEvent);

			FireEvent(SKTouchAction.Released, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void RightMouseDragged(NSEvent mouseEvent)
		{
			base.RightMouseDragged(mouseEvent);

			FireEvent(SKTouchAction.Moved, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		private bool FireEvent(SKTouchAction actionType, SKMouseButton mouse, SKTouchDeviceType device, NSEvent mouseEvent, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = mouseEvent.ButtonNumber;

			var cgPoint = LocationInView(View);
			// flip the Y coordinate for macOS
			cgPoint.Y = View.Bounds.Height - cgPoint.Y;

			var point = scalePixels(cgPoint.X, cgPoint.Y);
			var wheelDelta = (int)mouseEvent.ScrollingDeltaY;

			var args = new SKTouchEventArgs(id, actionType, mouse, device, point, inContact, wheelDelta, mouseEvent.Pressure);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
