using System;
using System.Collections.Generic;
using System.Linq;

using CoreGraphics;
using Foundation;
using UIKit;
using ObjCRuntime;

using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace TouchTracking.iOS
{
    class TouchRecognizer : UIGestureRecognizer
    {
        Element element;        // Forms element for firing events
        UIView view;            // iOS UIView 
        TouchTracking.TouchEffect touchEffect;
        bool capture;

        static Dictionary<UIView, TouchRecognizer> viewDictionary = 
            new Dictionary<UIView, TouchRecognizer>();

        static Dictionary<long, TouchRecognizer> idToTouchDictionary = 
            new Dictionary<long, TouchRecognizer>();

        public TouchRecognizer(Element element, UIView view, TouchTracking.TouchEffect touchEffect)
        {
            this.element = element;
            this.view = view;
            this.touchEffect = touchEffect;

            viewDictionary.Add(view, this);
        }

        public void Detach()
        {
            viewDictionary.Remove(view);
        }

        static long GetTouchId(UITouch touch)
        {
            // Use the handle's hash as a stable identifier for the touch
            return (long)touch.Handle.GetHashCode();
        }

        // touches = touches of interest; evt = all touches of type UITouch
        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = GetTouchId(touch);
                FireEvent(this, id, TouchActionType.Pressed, touch, true);

                if (!idToTouchDictionary.ContainsKey(id))
                {
                    idToTouchDictionary.Add(id, this);
                }
            }

            // Save the setting of the Capture property
            capture = touchEffect.Capture;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = GetTouchId(touch);

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Moved, touch, true);
                }
                else
                {
                    CheckForBoundaryHop(touch);

                    if (idToTouchDictionary.TryGetValue(id, out var recognizer) && recognizer != null)
                    {
                        FireEvent(recognizer, id, TouchActionType.Moved, touch, true);
                    }
                }
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = GetTouchId(touch);

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Released, touch, false);
                }
                else
                {
                    CheckForBoundaryHop(touch);

                    if (idToTouchDictionary.TryGetValue(id, out var recognizer) && recognizer != null)
                    {
                        FireEvent(recognizer, id, TouchActionType.Released, touch, false);
                    }
                }
                idToTouchDictionary.Remove(id);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = GetTouchId(touch);

                if (capture)
                {
                    FireEvent(this, id, TouchActionType.Cancelled, touch, false);
                }
                else if (idToTouchDictionary.TryGetValue(id, out var recognizer) && recognizer != null)
                {
                    FireEvent(recognizer, id, TouchActionType.Cancelled, touch, false);
                }
                idToTouchDictionary.Remove(id);
            }
        }

        void CheckForBoundaryHop(UITouch touch)
        {
            long id = GetTouchId(touch);

            // TODO: Might require converting to a List for multiple hits
            TouchRecognizer recognizerHit = null;

            foreach (UIView view in viewDictionary.Keys)
            {
                CGPoint location = touch.LocationInView(view);

                if (new CGRect(new CGPoint(), view.Frame.Size).Contains(location))
                {
                    recognizerHit = viewDictionary[view];
                }
            }
            if (idToTouchDictionary.TryGetValue(id, out var currentRecognizer) && recognizerHit != currentRecognizer)
            {
                if (currentRecognizer != null)
                {
                    FireEvent(currentRecognizer, id, TouchActionType.Exited, touch, true);
                }
                if (recognizerHit != null)
                {
                    FireEvent(recognizerHit, id, TouchActionType.Entered, touch, true);
                }
                idToTouchDictionary[id] = recognizerHit;
            }
        }

        void FireEvent(TouchRecognizer recognizer, long id, TouchActionType actionType, UITouch touch, bool isInContact)
        {
            // Convert touch location to MAUI Point value
            CGPoint cgPoint = touch.LocationInView(recognizer.View);
            Point xfPoint = new Point(cgPoint.X, cgPoint.Y);

            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = recognizer.touchEffect.OnTouchAction;

            // Call that method
            onTouchAction(recognizer.element,
                new TouchActionEventArgs(id, actionType, xfPoint, isInContact));
        }
    }
}