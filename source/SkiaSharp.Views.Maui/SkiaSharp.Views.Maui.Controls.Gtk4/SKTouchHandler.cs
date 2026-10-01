using System;
using Gtk;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

internal sealed class SKTouchHandler
{
	private readonly Widget widget;
	private readonly Action<SKTouchEventArgs> onTouch;
	private readonly Func<int> getScale;
	private GestureClick? click;
	private EventControllerMotion? motion;
	private EventControllerScroll? scroll;
	private bool pressed;
	private double x;
	private double y;
	private SKMouseButton button = SKMouseButton.Unknown;

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
		Cancel();

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
		Cancel();
		x = args.X;
		y = args.Y;
		button = sender.GetCurrentButton() switch
		{
			1 => SKMouseButton.Left,
			2 => SKMouseButton.Middle,
			3 => SKMouseButton.Right,
			_ => SKMouseButton.Unknown,
		};
		pressed = true;
		Send(SKTouchAction.Pressed, true);
	}

	private void OnReleased(GestureClick sender, GestureClick.ReleasedSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		if (pressed)
		{
			pressed = false;
			Send(SKTouchAction.Released, false);
			button = SKMouseButton.Unknown;
		}
	}

	private void OnCancel(Gesture sender, Gesture.CancelSignalArgs args)
	{
		Cancel();
	}

	private void OnEnter(EventControllerMotion sender, EventControllerMotion.EnterSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		Send(SKTouchAction.Entered, pressed);
	}

	private void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args)
	{
		x = args.X;
		y = args.Y;
		Send(SKTouchAction.Moved, pressed);
	}

	private void OnLeave(EventControllerMotion sender, EventArgs args)
	{
		Cancel();
		Send(SKTouchAction.Exited, false);
	}

	private void Cancel()
	{
		if (!pressed)
			return;
		pressed = false;
		Send(SKTouchAction.Cancelled, false);
		button = SKMouseButton.Unknown;
	}

	private bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args) =>
		Send(SKTouchAction.WheelChanged, pressed, Math.Sign(-args.Dy));

	private bool Send(SKTouchAction action, bool inContact, int wheelDelta = 0)
	{
		var scale = getScale();
		var e = new SKTouchEventArgs(0, action, button, SKTouchDeviceType.Mouse,
			Gtk4Sizing.GetTouchLocation(x, y, scale, false), inContact, wheelDelta);
		onTouch(e);
		return e.Handled;
	}
}
