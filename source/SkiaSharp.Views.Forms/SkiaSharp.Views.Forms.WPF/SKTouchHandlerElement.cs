using System;
using System.Windows;
using System.Windows.Input;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandlerElement
	{
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

		public SKTouchHandlerElement(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(FrameworkElement view, bool enableTouchEvents)
		{
			if (view != null)
			{
				// mouse
				view.MouseEnter -= OnMouseEntered;
				view.MouseLeave -= OnMouseExited;
				view.MouseDown -= OnMousePressed;
				view.MouseMove -= OnMouseMoved;
				view.MouseUp -= OnMouseReleased;
				view.MouseWheel -= OnMouseWheel;

				// touch
				view.TouchEnter -= OnTouchEntered;
				view.TouchLeave -= OnTouchExited;
				view.TouchDown -= OnTouchPressed;
				view.TouchMove -= OnTouchMoved;
				view.TouchUp -= OnTouchReleased;

				// stylus
				view.StylusEnter -= OnStylusEntered;
				view.StylusLeave -= OnStylusExited;
				view.StylusDown -= OnStylusPressed;
				view.StylusMove -= OnStylusMoved;
				view.StylusInAirMove -= OnStylusInAirMoved;
				view.StylusUp -= OnStylusReleased;
				view.StylusButtonDown -= OnStylusButtonPressed;
				view.StylusButtonUp -= OnStylusButtonReleased;

				if (enableTouchEvents)
				{
					// mouse
					view.MouseEnter += OnMouseEntered;
					view.MouseLeave += OnMouseExited;
					view.MouseDown += OnMousePressed;
					view.MouseMove += OnMouseMoved;
					view.MouseUp += OnMouseReleased;
					view.MouseWheel += OnMouseWheel;

					// touch
					view.TouchEnter += OnTouchEntered;
					view.TouchLeave += OnTouchExited;
					view.TouchDown += OnTouchPressed;
					view.TouchMove += OnTouchMoved;
					view.TouchUp += OnTouchReleased;

					// stylus
					view.StylusEnter += OnStylusEntered;
					view.StylusLeave += OnStylusExited;
					view.StylusDown += OnStylusPressed;
					view.StylusMove += OnStylusMoved;
					view.StylusInAirMove += OnStylusInAirMoved;
					view.StylusUp += OnStylusReleased;
					view.StylusButtonDown += OnStylusButtonPressed;
					view.StylusButtonUp += OnStylusButtonReleased;
				}
			}
		}

		public void Detach(FrameworkElement view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		private void OnMouseEntered(object sender, MouseEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Entered, e);
		}

		private void OnTouchEntered(object sender, TouchEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Entered, e);
		}

		private void OnStylusEntered(object sender, StylusEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Entered, e);
		}

		private void OnMouseExited(object sender, MouseEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Exited, e);
		}

		private void OnTouchExited(object sender, TouchEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Exited, e);
		}

		private void OnStylusExited(object sender, StylusEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Exited, e);
		}

		private void OnMousePressed(object sender, MouseEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Pressed, e);

			if (e.Handled)
			{
				var view = sender as FrameworkElement;
				view.CaptureMouse();
			}
		}

		private void OnTouchPressed(object sender, TouchEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Pressed, e);
		}

		private void OnStylusPressed(object sender, StylusDownEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Pressed, e);
		}

		private void OnStylusButtonPressed(object sender, StylusButtonEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Pressed, e);
		}

		private void OnMouseMoved(object sender, MouseEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Moved, e);
		}

		private void OnTouchMoved(object sender, TouchEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Moved, e);
		}

		private void OnStylusMoved(object sender, StylusEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Moved, e);
		}

		private void OnStylusInAirMoved(object sender, StylusEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Moved, e);
		}

		private void OnMouseReleased(object sender, MouseEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Released, e);

			var view = sender as FrameworkElement;
			view.ReleaseMouseCapture();
		}

		private void OnTouchReleased(object sender, TouchEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Released, e);
		}

		private void OnStylusReleased(object sender, StylusEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Released, e);
		}

		private void OnStylusButtonReleased(object sender, StylusButtonEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.Released, e);
		}

		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			e.Handled = CommonHandler(sender, SKTouchAction.WheelChanged, e);
		}

		// processing

		private bool CommonHandler(object sender, SKTouchAction touchActionType, InputEventArgs evt)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var view = sender as FrameworkElement;

			// bail out if this view is not part the view hierarchy anymore
			if (PresentationSource.FromVisual(view) == null)
				return false;

			var id = GetId(evt);
			var action = touchActionType;
			var mouse = GetMouseButton(evt);
			var device = GetTouchDevice(evt);
			var windowsPoint = GetPosition(evt, view);
			var skPoint = scalePixels(windowsPoint.X, windowsPoint.Y);
			var inContact = GetContact(evt);
			var wheelDelta = evt is MouseWheelEventArgs wheelEvt ? wheelEvt.Delta : 0;

			var args = new SKTouchEventArgs(id, action, mouse, device, skPoint, inContact, wheelDelta);
			onTouchAction(args);
			return args.Handled;
		}

		private static bool GetContact(InputEventArgs evt)
		{
			var inContact = false;

			switch (evt)
			{
				case MouseEventArgs mouseEvent:
					inContact =
						mouseEvent.LeftButton == MouseButtonState.Pressed ||
						mouseEvent.MiddleButton == MouseButtonState.Pressed ||
						mouseEvent.RightButton == MouseButtonState.Pressed;
					break;
				case TouchEventArgs touchEvent:
					inContact = true;
					break;
				case StylusEventArgs stylusEvent:
					inContact = !stylusEvent.InAir;
					break;
			}

			return inContact;
		}

		private Point GetPosition(InputEventArgs evt, FrameworkElement view)
		{
			var point = new Point();

			switch (evt)
			{
				case MouseEventArgs mouseEvent:
					point = mouseEvent.GetPosition(view);
					break;
				case TouchEventArgs touchEvent:
					point = touchEvent.GetTouchPoint(view).Position;
					break;
				case StylusEventArgs stylusEvent:
					point = stylusEvent.GetPosition(view);
					break;
			}

			return point;
		}

		private long GetId(InputEventArgs evt)
		{
			long id = -1;

			switch (evt)
			{
				case MouseEventArgs mouseEvent:
					id = 1;
					break;
				case TouchEventArgs touchEvent:
					id = touchEvent.TouchDevice.Id;
					break;
				case StylusEventArgs stylusEvent:
					id = stylusEvent.StylusDevice.Id;
					break;
			}

			return id;
		}

		private static SKTouchDeviceType GetTouchDevice(InputEventArgs evt)
		{
			var device = SKTouchDeviceType.Mouse;
			switch (evt)
			{
				case MouseEventArgs _:
					device = SKTouchDeviceType.Mouse;
					break;
				case TouchEventArgs touch:
					device = SKTouchDeviceType.Touch;
					break;
				case StylusEventArgs stylus:
					device = stylus.Inverted ? SKTouchDeviceType.Eraser : SKTouchDeviceType.Pen;
					break;
			}
			return device;
		}

		private static SKMouseButton GetMouseButton(InputEventArgs evt)
		{
			var mouse = SKMouseButton.Unknown;

			switch (evt)
			{
				case MouseEventArgs mouseEvent:
					if (mouseEvent is MouseButtonEventArgs mouseButtonEvent)
					{
						switch (mouseButtonEvent.ChangedButton)
						{
							case MouseButton.Left:
								mouse = SKMouseButton.Left;
								break;
							case MouseButton.Right:
								mouse = SKMouseButton.Right;
								break;
							case MouseButton.Middle:
								mouse = SKMouseButton.Middle;
								break;
						}
					}
					else
					{
						if (mouseEvent.LeftButton == MouseButtonState.Pressed)
							mouse = SKMouseButton.Left;
						else if (mouseEvent.RightButton == MouseButtonState.Pressed)
							mouse = SKMouseButton.Right;
						else if (mouseEvent.MiddleButton == MouseButtonState.Pressed)
							mouse = SKMouseButton.Middle;
					}
					break;

				case TouchEventArgs touchEvent:
					{
						mouse = SKMouseButton.Left;
					}
					break;

				case StylusEventArgs stylus:
					{
						var buttonsCount = stylus.StylusDevice.StylusButtons.Count;
						var isPrimaryPressed = false;
						var isSecondaryPressed = false;
						var isTertiaryPressed = false;
						if (buttonsCount > 0)
							isPrimaryPressed = stylus.StylusDevice.StylusButtons[0].StylusButtonState == StylusButtonState.Down;
						if (buttonsCount > 1)
							isSecondaryPressed = stylus.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Down;
						if (buttonsCount > 2)
							isTertiaryPressed = stylus.StylusDevice.StylusButtons[2].StylusButtonState == StylusButtonState.Down;

						if (isPrimaryPressed)
							mouse = SKMouseButton.Left;
						else if (isSecondaryPressed)
							mouse = SKMouseButton.Right;
						else if (isTertiaryPressed)
							mouse = SKMouseButton.Middle;
					}
					break;
			}

			return mouse;
		}
	}
}
