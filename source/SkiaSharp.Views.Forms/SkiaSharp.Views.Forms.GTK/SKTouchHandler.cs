using System;
using Gdk;
using Gtk;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler
	{
		private const int MouseWheelDelta = 120;

		private Action<SKTouchEventArgs> onTouchAction;
		private Func<double, double, SKPoint> scalePixels;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(Widget widget, bool enableTouchEvents)
		{
			const EventMask events =
				EventMask.EnterNotifyMask |
				EventMask.PointerMotionMask |
				EventMask.ButtonPressMask |
				EventMask.ButtonReleaseMask |
				EventMask.ScrollMask |
				EventMask.LeaveNotifyMask;

			if (widget != null)
			{
				widget.EnterNotifyEvent -= OnEnter;
				widget.MotionNotifyEvent -= OnMotion;
				widget.ButtonPressEvent -= OnPressed;
				widget.ButtonReleaseEvent -= OnReleased;
				widget.ScrollEvent -= OnScroll;
				widget.LeaveNotifyEvent -= OnLeave;

				widget.Events &= ~events;

				if (enableTouchEvents)
				{
					widget.EnterNotifyEvent += OnEnter;
					widget.MotionNotifyEvent += OnMotion;
					widget.ButtonPressEvent += OnPressed;
					widget.ButtonReleaseEvent += OnReleased;
					widget.ScrollEvent += OnScroll;
					widget.LeaveNotifyEvent += OnLeave;

					widget.Events |= events;
				}
			}
		}

		public void Detach(Widget widget)
		{
			// clean the view
			SetEnabled(widget, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		// events

		private void OnEnter(object sender, EnterNotifyEventArgs e)
		{
		}

		private void OnPressed(object sender, ButtonPressEventArgs e)
		{
			e.RetVal = CommonHandler(sender, SKTouchAction.Pressed, e.Event);
		}

		private void OnMotion(object sender, MotionNotifyEventArgs e)
		{
			e.RetVal = CommonHandler(sender, SKTouchAction.Moved, e.Event);
		}

		private void OnReleased(object sender, ButtonReleaseEventArgs e)
		{
			e.RetVal = CommonHandler(sender, SKTouchAction.Released, e.Event);
		}

		private void OnScroll(object sender, ScrollEventArgs e)
		{
			e.RetVal = CommonHandler(sender, SKTouchAction.WheelChanged, e.Event);
		}

		private void OnLeave(object sender, LeaveNotifyEventArgs e)
		{
		}

		// processing

		private bool CommonHandler(object sender, SKTouchAction touchActionType, Event evt)
		{
			if (onTouchAction == null || scalePixels == null)
				return false;

			var view = sender as Widget;

			var id = GetId(evt);
			var action = touchActionType;
			var mouse = GetMouseButton(evt);
			var device = GetTouchDevice(evt);
			var windowsPoint = GetPosition(evt);
			var skPoint = scalePixels(windowsPoint.X, windowsPoint.Y);
			var inContact = GetContact(evt);
			var wheelDelta = GetWheelDelta(evt);

			var args = new SKTouchEventArgs(id, action, mouse, device, skPoint, inContact, wheelDelta);
			onTouchAction(args);
			return args.Handled;
		}

		private int GetWheelDelta(Event evt)
		{
			if (evt is EventScroll scroll)
			{
				var topLeftDir =
					scroll.Direction == ScrollDirection.Up ||
					scroll.Direction == ScrollDirection.Left;

				return topLeftDir ? MouseWheelDelta : -MouseWheelDelta;
			}

			return 0;
		}

		private static bool GetContact(Event evt)
		{
			var state = evt switch
			{
				EventMotion motion => motion.State,
				EventButton button when button.Type == EventType.ButtonPress => ModifierType.Button1Mask,
				EventScroll scroll => scroll.State,
				_ => ModifierType.None,
			};

			return
				state.HasFlag(ModifierType.Button1Mask) ||
				state.HasFlag(ModifierType.Button2Mask) ||
				state.HasFlag(ModifierType.Button3Mask);
		}

		private SKPoint GetPosition(Event evt) =>
			evt switch
			{
				EventMotion motion => new SKPoint((float)motion.X, (float)motion.Y),
				EventButton button => new SKPoint((float)button.X, (float)button.Y),
				EventScroll scroll => new SKPoint((float)scroll.X, (float)scroll.Y),
				_ => SKPoint.Empty,
			};

		private long GetId(Event evt) =>
			evt switch
			{
				EventMotion motion => (long)motion.Device.Handle,
				EventButton button => (long)button.Device.Handle,
				EventScroll scroll => (long)scroll.Device.Handle,
				_ => -1,
			};

		private static SKTouchDeviceType GetTouchDevice(Event evt)
		{
			var source = evt switch
			{
				EventMotion motion => motion.Device.Source,
				EventButton button => button.Device.Source,
				EventScroll scroll => scroll.Device.Source,
				_ => InputSource.Mouse,
			};

			return source switch
			{
				InputSource.Mouse => SKTouchDeviceType.Mouse,
				InputSource.Pen => SKTouchDeviceType.Pen,
				InputSource.Eraser => SKTouchDeviceType.Pen,
				InputSource.Cursor => SKTouchDeviceType.Mouse,
				_ => SKTouchDeviceType.Mouse,
			};
		}

		private static SKMouseButton GetMouseButton(Event evt)
		{
			var mouse = SKMouseButton.Unknown;

			var state = evt switch
			{
				EventMotion motion => motion.State,
				EventButton button when button.Button == 1 => ModifierType.Button1Mask,
				EventButton button when button.Button == 2 => ModifierType.Button2Mask,
				EventButton button when button.Button == 3 => ModifierType.Button3Mask,
				EventScroll scroll => scroll.State,
				_ => ModifierType.None,
			};

			if (state.HasFlag(ModifierType.Button1Mask))
				mouse = SKMouseButton.Left;
			else if (state.HasFlag(ModifierType.Button3Mask))
				mouse = SKMouseButton.Right;
			else if (state.HasFlag(ModifierType.Button2Mask))
				mouse = SKMouseButton.Middle;

			return mouse;
		}
	}
}
