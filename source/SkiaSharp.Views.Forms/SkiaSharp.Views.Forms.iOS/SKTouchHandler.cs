using System;
using System.Linq;
using Foundation;
using UIKit;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler : UIGestureRecognizer
	{
		private Action<SKTouchActionEventArgs> onTouchAction;
		private Func<nfloat, nfloat> scalePixels;

		public SKTouchHandler(Action<SKTouchActionEventArgs> onTouchAction, Func<nfloat, nfloat> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void Attach(UIView view)
		{
			view.AddGestureRecognizer(this);
		}

		public void Detach(UIView view)
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

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchActionType.Pressed, touch, true);
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchActionType.Moved, touch, true);
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchActionType.Released, touch, false);
			}
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchActionType.Cancelled, touch, false);
			}
		}

		private bool FireEvent(SKTouchActionType actionType, UITouch touch, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = touch.Handle.ToInt64();

			var cgPoint = touch.LocationInView(View);
			var point = new SKPoint((float)scalePixels(cgPoint.X), (float)scalePixels(cgPoint.Y));

			var args = new SKTouchActionEventArgs(id, actionType, point, inContact);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
