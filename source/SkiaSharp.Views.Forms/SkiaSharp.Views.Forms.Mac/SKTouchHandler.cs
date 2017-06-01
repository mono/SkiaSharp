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

			FireEvent(SKTouchActionType.Pressed, mouseEvent, true);
		}

		public override void MouseUp(NSEvent mouseEvent)
		{
			base.MouseUp(mouseEvent);

			FireEvent(SKTouchActionType.Released, mouseEvent, false);
		}

		public override void MouseDragged(NSEvent mouseEvent)
		{
			base.MouseDragged(mouseEvent);

			FireEvent(SKTouchActionType.Moved, mouseEvent, true);
		}

		private bool FireEvent(SKTouchActionType actionType, NSEvent mouseEvent, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = mouseEvent.ButtonNumber;

			var cgPoint = LocationInView(View);
			// flip the Y coordinate for macOS
			cgPoint.Y = View.Bounds.Height - cgPoint.Y;

			var point = new SKPoint((float)scalePixels(cgPoint.X), (float)scalePixels(cgPoint.Y));

			var args = new SKTouchActionEventArgs(id, actionType, point, inContact);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
