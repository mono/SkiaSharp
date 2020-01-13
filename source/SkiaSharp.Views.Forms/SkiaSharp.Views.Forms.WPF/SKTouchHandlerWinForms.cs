using System;
using System.Windows.Forms;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandlerWinForms
	{
		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

		public SKTouchHandlerWinForms(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(Control view, bool enableTouchEvents)
		{
			if (view != null)
			{
				view.MouseEnter -= OnMouseEntered;
				view.MouseLeave -= OnMouseExited;
				view.MouseDown -= OnMousePressed;
				view.MouseMove -= OnMouseMoved;
				view.MouseUp -= OnMouseReleased;
				view.MouseWheel -= OnMouseWheel;

				if (enableTouchEvents)
				{
					view.MouseEnter += OnMouseEntered;
					view.MouseLeave += OnMouseExited;
					view.MouseDown += OnMousePressed;
					view.MouseMove += OnMouseMoved;
					view.MouseUp += OnMouseReleased;
					view.MouseWheel += OnMouseWheel;
				}
			}
		}

		public void Detach(Control view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		// mouse events

		private void OnMouseEntered(object sender, EventArgs e)
		{
			var view = sender as Control;
			var loc = view.PointToClient(Cursor.Position);

			var handled = CommonHandler(sender, SKTouchAction.Entered, new MouseEventArgs(0, 0, loc.X, loc.Y, 0));
		}

		private void OnMouseExited(object sender, EventArgs e)
		{
			var view = sender as Control;
			var loc = view.PointToClient(Cursor.Position);

			var handled = CommonHandler(sender, SKTouchAction.Exited, new MouseEventArgs(0, 0, loc.X, loc.Y, 0));
		}

		private void OnMousePressed(object sender, MouseEventArgs e)
		{
			var handled = CommonHandler(sender, SKTouchAction.Pressed, e);

			if (handled)
			{
				var view = sender as Control;
				view.Capture = true;
			}
		}

		private void OnMouseMoved(object sender, MouseEventArgs e)
		{
			var handled = CommonHandler(sender, SKTouchAction.Moved, e);
		}

		private void OnMouseReleased(object sender, MouseEventArgs e)
		{
			var handled = CommonHandler(sender, SKTouchAction.Released, e);

			var view = sender as Control;
			view.Capture = false;
		}

		private void OnMouseWheel(object sender, MouseEventArgs e)
		{
			var handled = CommonHandler(sender, SKTouchAction.WheelChanged, e);
		}

		// processing

		private bool CommonHandler(object sender, SKTouchAction touchActionType, MouseEventArgs evt)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var view = sender as Control;

			var id = 1;
			var action = touchActionType;
			var mouse = GetMouseButton(evt);
			var device = SKTouchDeviceType.Mouse;
			var windowsPoint = evt.Location;
			var skPoint = scalePixels(windowsPoint.X, windowsPoint.Y);
			var inContact = GetContact(evt);
			var wheelDelta = evt.Delta;

			var args = new SKTouchEventArgs(id, action, mouse, device, skPoint, inContact, wheelDelta);
			onTouchAction(args);
			return args.Handled;
		}

		private static bool GetContact(MouseEventArgs evt)
		{
			var inContact =
				evt.Button.HasFlag(MouseButtons.Left) ||
				evt.Button.HasFlag(MouseButtons.Middle) ||
				evt.Button.HasFlag(MouseButtons.Right);

			return inContact;
		}

		private static SKMouseButton GetMouseButton(MouseEventArgs evt)
		{
			var mouse = SKMouseButton.Unknown;

			if (evt.Button.HasFlag(MouseButtons.Left))
				mouse = SKMouseButton.Left;
			if (evt.Button.HasFlag(MouseButtons.Middle))
				mouse = SKMouseButton.Middle;
			if (evt.Button.HasFlag(MouseButtons.Right))
				mouse = SKMouseButton.Right;

			return mouse;
		}
	}
}
