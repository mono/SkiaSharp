#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SkiaSharp.Views.Maui.Platform
{
	internal sealed class SKTouchHandler
	{
		private Action<SKTouchEventArgs>? onTouchAction;
		private Func<double, double, SKPoint>? scalePixels;
		private readonly TouchGestureRecognizer touchGestureRecognizer;
		private readonly UIGestureRecognizer[] gestureRecognizers;
		private readonly ScrollGestureRecognizerDelegate? scrollGestureRecognizerDelegate;
		private readonly LegacyWheelDeltaProjector wheelDeltaProjector = new();
		private CGPoint? lastPointerLocation;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;

			var recognizers = new List<UIGestureRecognizer>();

			touchGestureRecognizer = new TouchGestureRecognizer(FireTouchEvent);
			recognizers.Add(touchGestureRecognizer);

			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
			{
				recognizers.Add(new UIHoverGestureRecognizer(OnHover)
				{
					CancelsTouchesInView = false,
				});
			}

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 4))
			{
				scrollGestureRecognizerDelegate = new ScrollGestureRecognizerDelegate();
				recognizers.Add(new UIPanGestureRecognizer(OnScroll)
				{
					AllowedScrollTypesMask = UIScrollTypeMask.All,
					AllowedTouchTypes = Array.Empty<NSNumber>(),
					MaximumNumberOfTouches = 0,
					CancelsTouchesInView = false,
					Delegate = scrollGestureRecognizerDelegate,
				});
			}

			gestureRecognizers = recognizers.ToArray();
			DisablesUserInteraction = false;
		}

		public bool DisablesUserInteraction { get; set; }

		public UIView? View =>
			touchGestureRecognizer.View;

		public void SetEnabled(UIView view, bool enableTouchEvents)
		{
			if (!view.UserInteractionEnabled || DisablesUserInteraction)
				view.UserInteractionEnabled = enableTouchEvents;

			if (enableTouchEvents)
			{
				foreach (var recognizer in gestureRecognizers)
				{
					if (recognizer.View != view)
						view.AddGestureRecognizer(recognizer);
				}
			}
			else
			{
				foreach (var recognizer in gestureRecognizers)
				{
					if (recognizer.View == view)
						view.RemoveGestureRecognizer(recognizer);
				}

				wheelDeltaProjector.Reset();
				lastPointerLocation = null;
			}
		}

		public void Detach(UIView view)
		{
			// clean the view
			SetEnabled(view, false);

			foreach (var recognizer in gestureRecognizers)
				recognizer.Dispose();
			scrollGestureRecognizerDelegate?.Dispose();

			// remove references
			onTouchAction = null;
			scalePixels = null;
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
			if (recognizer.View is not { } view)
				return;

			var state = recognizer.State;
			if (state is UIGestureRecognizerState.Cancelled or UIGestureRecognizerState.Failed)
			{
				recognizer.SetTranslation(CGPoint.Empty, view);
				wheelDeltaProjector.Reset();
				return;
			}
			if (state is not UIGestureRecognizerState.Began and
				not UIGestureRecognizerState.Changed and
				not UIGestureRecognizerState.Ended)
				return;

			var translation = recognizer.TranslationInView(view);
			recognizer.SetTranslation(CGPoint.Empty, view);

			var wheelDelta = wheelDeltaProjector.Project(translation.Y, state);
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

		internal sealed class LegacyWheelDeltaProjector
		{
			// Browsers use 40 logical pixels per Cocoa tick as a content-scroll
			// compatibility policy. UIKit only provides point translation, so
			// this is sensitivity for legacy v120 events, not hardware calibration.
			private const double LegacyWheelDeltaDistance = 40.0;
			private const double WheelDeltaPerNominalNotch = 120.0;
			private const double WholeDeltaTolerance = 1e-10;

			private double remainder;

			public int Project(double translationY, UIGestureRecognizerState state)
			{
				if (state == UIGestureRecognizerState.Began)
					remainder = 0;

				remainder -= translationY * WheelDeltaPerNominalNotch / LegacyWheelDeltaDistance;
				var nearestWholeDelta = Math.Round(remainder);
				if (Math.Abs(remainder - nearestWholeDelta) < WholeDeltaTolerance)
					remainder = nearestWholeDelta;
				var delta = (int)Math.Truncate(remainder);
				remainder -= delta;

				if (state == UIGestureRecognizerState.Ended)
					remainder = 0;

				return delta;
			}

			public void Reset() =>
				remainder = 0;
		}

		private sealed class ScrollGestureRecognizerDelegate : UIGestureRecognizerDelegate
		{
			public override bool ShouldRecognizeSimultaneously(
				UIGestureRecognizer gestureRecognizer,
				UIGestureRecognizer otherGestureRecognizer) =>
				otherGestureRecognizer is UIPinchGestureRecognizer;
		}

		private bool FireTouchEvent(SKTouchAction actionType, UITouch touch, bool inContact)
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

		private sealed class TouchGestureRecognizer : UIGestureRecognizer
		{
			private readonly Func<SKTouchAction, UITouch, bool, bool> fireEvent;

			public TouchGestureRecognizer(Func<SKTouchAction, UITouch, bool, bool> fireEvent)
			{
				this.fireEvent = fireEvent;
			}

			public override void TouchesBegan(NSSet touches, UIEvent evt)
			{
				base.TouchesBegan(touches, evt);

				foreach (UITouch touch in touches.Cast<UITouch>())
				{
					if (!fireEvent(SKTouchAction.Pressed, touch, true))
						IgnoreTouch(touch, evt);
				}
			}

			public override void TouchesMoved(NSSet touches, UIEvent evt)
			{
				base.TouchesMoved(touches, evt);

				foreach (UITouch touch in touches.Cast<UITouch>())
					fireEvent(SKTouchAction.Moved, touch, true);
			}

			public override void TouchesEnded(NSSet touches, UIEvent evt)
			{
				base.TouchesEnded(touches, evt);

				foreach (UITouch touch in touches.Cast<UITouch>())
					fireEvent(SKTouchAction.Released, touch, false);
			}

			public override void TouchesCancelled(NSSet touches, UIEvent evt)
			{
				base.TouchesCancelled(touches, evt);

				foreach (UITouch touch in touches.Cast<UITouch>())
					fireEvent(SKTouchAction.Cancelled, touch, false);
			}
		}
	}
}
