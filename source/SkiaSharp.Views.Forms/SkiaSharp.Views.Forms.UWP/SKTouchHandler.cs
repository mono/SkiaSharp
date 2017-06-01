using System;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler
	{
		private Action<SKTouchActionEventArgs> onTouchAction;
		private Func<float, float> scalePixels;

		public SKTouchHandler(Action<SKTouchActionEventArgs> onTouchAction, Func<float, float> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void Attach(FrameworkElement view)
		{
			view.PointerEntered += OnPointerEntered;
			view.PointerExited += OnPointerExited;
			view.PointerPressed += OnPointerPressed;
			view.PointerMoved += OnPointerMoved;
			view.PointerReleased += OnPointerReleased;
			view.PointerCanceled += OnPointerCancelled;
		}

		public void Detach(FrameworkElement view)
		{
			// clean the view
			if (view != null)
			{
				view.PointerEntered -= OnPointerEntered;
				view.PointerExited -= OnPointerExited;
				view.PointerPressed -= OnPointerPressed;
				view.PointerMoved -= OnPointerMoved;
				view.PointerReleased -= OnPointerReleased;
				view.PointerCanceled -= OnPointerCancelled;
			}

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		private void OnPointerEntered(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Entered, args);
		}

		private void OnPointerExited(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Exited, args);
		}

		private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Pressed, args);

			var view = sender as FrameworkElement;
			view.CapturePointer(args.Pointer);
		}

		private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Moved, args);
		}

		private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Released, args);
		}

		private void OnPointerCancelled(object sender, PointerRoutedEventArgs args)
		{
			CommonHandler(sender, SKTouchActionType.Cancelled, args);
		}

		private bool CommonHandler(object sender, SKTouchActionType touchActionType, PointerRoutedEventArgs evt)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var view = sender as FrameworkElement;

			var id = evt.Pointer.PointerId;

			var pointerPoint = evt.GetCurrentPoint(view);
			var windowsPoint = pointerPoint.Position;
			var skPoint = new SKPoint(scalePixels((float)windowsPoint.X), scalePixels((float)windowsPoint.Y));

			var mouse = GetMouseButton(pointerPoint);
			var device = GetTouchDevice(evt);

			var args = new SKTouchActionEventArgs(id, touchActionType, mouse, device, skPoint, evt.Pointer.IsInContact);
			onTouchAction(args);
			return args.Handled;
		}

		private static SKTouchDeviceType GetTouchDevice(PointerRoutedEventArgs evt)
		{
			var device = SKTouchDeviceType.Touch;
			switch (evt.Pointer.PointerDeviceType)
			{
				case PointerDeviceType.Pen:
					device = SKTouchDeviceType.Pen;
					break;
				case PointerDeviceType.Mouse:
					device = SKTouchDeviceType.Mouse;
					break;
				case PointerDeviceType.Touch:
					device = SKTouchDeviceType.Touch;
					break;
			}

			return device;
		}

		private static SKMouseButton GetMouseButton(PointerPoint pointerPoint)
		{
			var properties = pointerPoint.Properties;

			var mouse = SKMouseButton.Unknown;

			// this is mainly for touch
			if (properties.IsLeftButtonPressed)
			{
				mouse = SKMouseButton.Left;
			}
			else if (properties.IsMiddleButtonPressed)
			{
				mouse = SKMouseButton.Middle;
			}
			else if (properties.IsRightButtonPressed)
			{
				mouse = SKMouseButton.Right;
			}

			// this is mainly for mouse
			switch (properties.PointerUpdateKind)
			{
				case PointerUpdateKind.LeftButtonPressed:
				case PointerUpdateKind.LeftButtonReleased:
					mouse = SKMouseButton.Left;
					break;
				case PointerUpdateKind.RightButtonPressed:
				case PointerUpdateKind.RightButtonReleased:
					mouse = SKMouseButton.Right;
					break;
				case PointerUpdateKind.MiddleButtonPressed:
				case PointerUpdateKind.MiddleButtonReleased:
					mouse = SKMouseButton.Middle;
					break;
				case PointerUpdateKind.XButton1Pressed:
				case PointerUpdateKind.XButton1Released:
				case PointerUpdateKind.XButton2Pressed:
				case PointerUpdateKind.XButton2Released:
				case PointerUpdateKind.Other:
				default:
					break;
			}

			return mouse;
		}
	}
}
