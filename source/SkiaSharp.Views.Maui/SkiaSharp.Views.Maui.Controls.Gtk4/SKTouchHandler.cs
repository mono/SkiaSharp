using System;
using Gtk;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

internal sealed class SKTouchHandler
{
	private readonly Widget widget;
	private readonly Action<SKTouchEventArgs> onTouch;
	private readonly Func<int> getScale;
	private readonly SKTouchContact contact = new();
	private GestureClick? click;
	private EventControllerMotion? motion;
	private EventControllerScroll? scroll;
	private double x;
	private double y;

	public SKTouchHandler(Widget widget, Action<SKTouchEventArgs> onTouch, Func<int> getScale)
	{
		this.widget = widget;
		this.onTouch = onTouch;
		this.getScale = getScale;
	}

	public void SetEnabled(bool enabled)
	{
		if (!enabled)
		{
			Detach();
			return;
		}

		if (click is not null)
			return;

		click = GestureClick.New();
		click.SetButton(0);
		click.OnPressed += OnPressed;
		click.OnReleased += OnReleased;
		click.OnCancel += OnCancel;
		widget.AddController(click);

		motion = EventControllerMotion.New();
		motion.OnEnter += OnEnter;
		motion.OnMotion += OnMotion;
		motion.OnLeave += OnLeave;
		widget.AddController(motion);

		scroll = EventControllerScroll.New(EventControllerScrollFlags.Vertical);
		scroll.OnScroll += OnScroll;
		widget.AddController(scroll);
	}

	public void Detach()
	{
		contact.Cancel(Emit);

		if (click is not null)
		{
			click.OnPressed -= OnPressed;
			click.OnReleased -= OnReleased;
			click.OnCancel -= OnCancel;
			widget.RemoveController(click);
			click = null;
		}
		if (motion is not null)
		{
			motion.OnEnter -= OnEnter;
			motion.OnMotion -= OnMotion;
			motion.OnLeave -= OnLeave;
			widget.RemoveController(motion);
			motion = null;
		}
		if (scroll is not null)
		{
			scroll.OnScroll -= OnScroll;
			widget.RemoveController(scroll);
			scroll = null;
		}
	}

	private void OnPressed(GestureClick sender, GestureClick.PressedSignalArgs args)
	{
		// Report cancellation at the previous contact's location.
		contact.Cancel(Emit);
		x = args.X;
		y = args.Y;
		var button = sender.GetCurrentButton() switch
		{
			1 => SKMouseButton.Left,
			2 => SKMouseButton.Middle,
			3 => SKMouseButton.Right,
			_ => SKMouseButton.Unknown,
		};
		contact.Press(button, Emit);
	}

	private void OnReleased(GestureClick sender, GestureClick.ReleasedSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		contact.Release(Emit);
	}

	private void OnCancel(Gesture sender, Gesture.CancelSignalArgs args)
	{
		contact.Cancel(Emit);
	}

	private void OnEnter(EventControllerMotion sender, EventControllerMotion.EnterSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		Send(SKTouchAction.Entered, contact.IsPressed, contact.Button);
	}

	private void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		Send(SKTouchAction.Moved, contact.IsPressed, contact.Button);
	}

	private void OnLeave(EventControllerMotion sender, EventArgs args)
	{
		contact.Cancel(Emit);
		Send(SKTouchAction.Exited, false, contact.Button);
	}

	private bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args) =>
		Send(SKTouchAction.WheelChanged, contact.IsPressed, contact.Button, Math.Sign(-args.Dy));

	private void Emit(SKTouchAction action, bool inContact, SKMouseButton button) =>
		Send(action, inContact, button);

	private bool Send(SKTouchAction action, bool inContact, SKMouseButton button, int wheelDelta = 0)
	{
		var scale = getScale();
		var e = new SKTouchEventArgs(0, action, button, SKTouchDeviceType.Mouse,
			Gtk4Sizing.GetTouchLocation(x, y, scale, false), inContact, wheelDelta);
		onTouch(e);
		return e.Handled;
	}
}
