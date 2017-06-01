using System;
using System.Linq;
using Foundation;
using AppKit;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler : NSGestureRecognizer
	{
		private Action<SKTouchActionEventArgs> onTouchAction;
		private Func<nfloat, nfloat> scalePixels;

		public SKTouchHandler(Action<SKTouchActionEventArgs> onTouchAction, Func<nfloat, nfloat> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void Attach(NSView view)
		{
			view.AddGestureRecognizer(this);
		}

		public void Detach(NSView view)
		{
			// clean the view
			if (view != null)
			{
				view.RemoveGestureRecognizer(this);
			}

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		public override void MouseDown(NSEvent mouseEvent)
		{
			base.MouseDown(mouseEvent);

			FireEvent(SKTouchActionType.Pressed, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void MouseUp(NSEvent mouseEvent)
		{
			base.MouseUp(mouseEvent);

			FireEvent(SKTouchActionType.Released, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void MouseDragged(NSEvent mouseEvent)
		{
			base.MouseDragged(mouseEvent);

			FireEvent(SKTouchActionType.Moved, SKMouseButton.Left, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void OtherMouseDown(NSEvent mouseEvent)
		{
			base.OtherMouseDown(mouseEvent);

			FireEvent(SKTouchActionType.Pressed, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void OtherMouseUp(NSEvent mouseEvent)
		{
			base.OtherMouseUp(mouseEvent);

			FireEvent(SKTouchActionType.Released, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void OtherMouseDragged(NSEvent mouseEvent)
		{
			base.OtherMouseDragged(mouseEvent);

			FireEvent(SKTouchActionType.Moved, SKMouseButton.Middle, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void RightMouseDown(NSEvent mouseEvent)
		{
			base.RightMouseDown(mouseEvent);

			FireEvent(SKTouchActionType.Pressed, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		public override void RightMouseUp(NSEvent mouseEvent)
		{
			base.RightMouseUp(mouseEvent);

			FireEvent(SKTouchActionType.Released, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, false);
		}

		public override void RightMouseDragged(NSEvent mouseEvent)
		{
			base.RightMouseDragged(mouseEvent);

			FireEvent(SKTouchActionType.Moved, SKMouseButton.Right, SKTouchDeviceType.Mouse, mouseEvent, true);
		}

		private bool FireEvent(SKTouchActionType actionType, SKMouseButton mouse, SKTouchDeviceType device, NSEvent mouseEvent, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = mouseEvent.ButtonNumber;

			var cgPoint = LocationInView(View);
			// flip the Y coordinate for macOS
			cgPoint.Y = View.Bounds.Height - cgPoint.Y;

			var point = new SKPoint((float)scalePixels(cgPoint.X), (float)scalePixels(cgPoint.Y));

			var args = new SKTouchActionEventArgs(id, actionType, mouse, device, point, inContact);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
