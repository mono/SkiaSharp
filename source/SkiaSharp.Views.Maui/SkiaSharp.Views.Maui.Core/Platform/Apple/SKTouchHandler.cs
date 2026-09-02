#nullable enable

using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SkiaSharp.Views.Maui.Platform
{
	internal class SKTouchHandler : UIGestureRecognizer
	{
		private const double ScrollPointsPerNotch = 40.0;

		private Action<SKTouchEventArgs>? onTouchAction;
		private Func<double, double, SKPoint>? scalePixels;
		private UIHoverGestureRecognizer? hoverGestureRecognizer;
		private UIPanGestureRecognizer? scrollGestureRecognizer;
		private ScrollGestureRecognizerDelegate? scrollGestureRecognizerDelegate;
		private CGPoint? lastPointerLocation;

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
					AddPointerRecognizers(view);
				}
				else if (!enableTouchEvents && view.GestureRecognizers?.Contains(this) == true)
				{
					RemovePointerRecognizers(view);
					view.RemoveGestureRecognizer(this);
				}
			}
		}

		public void Detach(UIView view)
		{
			// clean the view
			SetEnabled(view, false);

			hoverGestureRecognizer?.Dispose();
			hoverGestureRecognizer = null;
			scrollGestureRecognizer?.Dispose();
			scrollGestureRecognizer = null;
			scrollGestureRecognizerDelegate?.Dispose();
			scrollGestureRecognizerDelegate = null;
			lastPointerLocation = null;

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				if (!FireEvent(SKTouchAction.Pressed, touch, true))
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
			}
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);

			foreach (UITouch touch in touches.Cast<UITouch>())
			{
				FireEvent(SKTouchAction.Cancelled, touch, false);
			}
		}

		private void AddPointerRecognizers(UIView view)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				hoverGestureRecognizer ??= new UIHoverGestureRecognizer(OnHover)
				{
					CancelsTouchesInView = false,
				};
				view.AddGestureRecognizer(hoverGestureRecognizer);
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 4))
			{
				scrollGestureRecognizerDelegate ??= new ScrollGestureRecognizerDelegate();
				scrollGestureRecognizer ??= new UIPanGestureRecognizer(OnScroll)
				{
					AllowedScrollTypesMask = UIScrollTypeMask.All,
					MaximumNumberOfTouches = 0,
					CancelsTouchesInView = false,
					Delegate = scrollGestureRecognizerDelegate,
				};
				view.AddGestureRecognizer(scrollGestureRecognizer);
			}
		}

		private void RemovePointerRecognizers(UIView view)
		{
			if (hoverGestureRecognizer != null && view.GestureRecognizers?.Contains(hoverGestureRecognizer) == true)
				view.RemoveGestureRecognizer(hoverGestureRecognizer);

			if (scrollGestureRecognizer != null && view.GestureRecognizers?.Contains(scrollGestureRecognizer) == true)
				view.RemoveGestureRecognizer(scrollGestureRecognizer);

			lastPointerLocation = null;
		}

		private void OnHover(UIHoverGestureRecognizer recognizer)
		{
			var action = recognizer.State switch
			{
				UIGestureRecognizerState.Began => SKTouchAction.Entered,
				UIGestureRecognizerState.Changed => SKTouchAction.Moved,
				UIGestureRecognizerState.Ended or UIGestureRecognizerState.Cancelled => SKTouchAction.Exited,
				_ => (SKTouchAction?)null,
			};
			if (action == null || recognizer.View is not { } view)
				return;

			var location = recognizer.LocationInView(view);
			lastPointerLocation = location;
			FirePointerEvent(action.Value, recognizer, location);

			if (action == SKTouchAction.Exited)
				lastPointerLocation = null;
		}

		private void OnScroll(UIPanGestureRecognizer recognizer)
		{
			if ((recognizer.State != UIGestureRecognizerState.Began &&
				recognizer.State != UIGestureRecognizerState.Changed) ||
				recognizer.View is not { } view)
				return;

			var translation = recognizer.TranslationInView(view);
			recognizer.SetTranslation(CGPoint.Empty, view);

			var wheelDelta = NormalizeWheelDelta(translation.Y);
			if (wheelDelta == 0)
				return;

			var location = lastPointerLocation ?? recognizer.LocationInView(view);
			FirePointerEvent(SKTouchAction.WheelChanged, recognizer, location, wheelDelta);
		}

		private bool FirePointerEvent(SKTouchAction actionType, UIGestureRecognizer recognizer, CGPoint location, int wheelDelta = 0)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = ((IntPtr)recognizer.Handle).ToInt64();
			var point = scalePixels(location.X, location.Y);
			var args = new SKTouchEventArgs(
				id,
				actionType,
				SKMouseButton.Unknown,
				SKTouchDeviceType.Mouse,
				point,
				false,
				wheelDelta,
				0);
			onTouchAction(args);
			return args.Handled;
		}

		internal static int NormalizeWheelDelta(double translationY)
		{
			if (translationY == 0)
				return 0;

			var delta = (int)Math.Round(
				-translationY * 120.0 / ScrollPointsPerNotch,
				MidpointRounding.AwayFromZero);
			return delta == 0 ? -Math.Sign(translationY) : delta;
		}

		private sealed class ScrollGestureRecognizerDelegate : UIGestureRecognizerDelegate
		{
			public override bool ShouldRecognizeSimultaneously(
				UIGestureRecognizer gestureRecognizer,
				UIGestureRecognizer otherGestureRecognizer) =>
				otherGestureRecognizer is UIPinchGestureRecognizer;
		}

		private bool FireEvent(SKTouchAction actionType, UITouch touch, bool inContact)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var id = ((IntPtr)touch.Handle).ToInt64();

			var cgPoint = touch.LocationInView(View);
			var point = scalePixels(cgPoint.X, cgPoint.Y);

			var args = new SKTouchEventArgs(id, actionType, point, inContact);
			onTouchAction(args);
			return args.Handled;
		}
	}
}
