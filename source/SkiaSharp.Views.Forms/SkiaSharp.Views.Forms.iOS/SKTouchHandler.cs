using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler : UIGestureRecognizer
	{
		private readonly HashSet<UITouch> trackedTouches = new HashSet<UITouch>();
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;

			DisablesUserInteraction = false;
		}

		public bool DisablesUserInteraction { get; set; }

		public void SetEnabled(UIView view, bool enableTouchEvents)
		{
			if (view != null)
			{
				if (!view.UserInteractionEnabled || DisablesUserInteraction)
				{
					view.UserInteractionEnabled = enableTouchEvents;
				}
				if (enableTouchEvents && view.GestureRecognizers?.Contains(this) != true)
				{
					view.AddGestureRecognizer(this);
				}
				else if (!enableTouchEvents && view.GestureRecognizers?.Contains(this) == true)
				{
					view.RemoveGestureRecognizer(this);
				}
			}
		}

		public void Detach(UIView view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				if (FireEvent(SKTouchAction.Pressed, touch, true))
				{
					trackedTouches.Add(touch);
				}
				else
				{
					IgnoreTouch(touch, evt);
				}
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchAction.Moved, touch, true);
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchAction.Released, touch, false);
				trackedTouches.Remove(touch);
			}
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchAction.Cancelled, touch, false);
				trackedTouches.Remove(touch);
			}
		}

		public override void Reset()
		{
			base.Reset();

			foreach (UITouch touch in trackedTouches)
			{
				FireEvent(SKTouchAction.Cancelled, touch, false);
			}

			trackedTouches.Clear();
		}

		private bool FireEvent(SKTouchAction actionType, UITouch touch, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = touch.Handle.ToInt64();

			var cgPoint = touch.LocationInView(View);
			var point = scalePixels(cgPoint.X, cgPoint.Y);

			var args = new SKTouchEventArgs(id, actionType, point, inContact);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
