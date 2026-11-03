using System;
using Gtk;

namespace SkiaSharp.Views.Maui.Platform.Gtk4;

internal sealed class SKTouchHandler
{
	private readonly Action<SKTouchEventArgs> onTouchAction;
	private readonly Func<double, double, SKPoint> scalePixels;
	private Widget? widget;
	private GestureClick? click;
	private EventControllerMotion? motion;
	private EventControllerScroll? scroll;
	private bool enabled;
	private int stateRevision;
	private double x;
	private double y;
	private SKPoint lastLocation;

	internal bool IsPressed { get; private set; }
	internal SKMouseButton Button { get; private set; } = SKMouseButton.Unknown;

	public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
	{
		this.onTouchAction = onTouchAction;
		this.scalePixels = scalePixels;
	}

	internal static SKPoint GetTouchLocation(double x, double y, int scale, bool ignorePixelScaling)
	{
		if (scale < 1)
			throw new ArgumentOutOfRangeException(nameof(scale));

		var factor = ignorePixelScaling ? 1 : scale;
		return new SKPoint((float)(x * factor), (float)(y * factor));
	}

	public void SetEnabled(Widget platformView, bool enableTouchEvents)
	{
		if (!enableTouchEvents)
		{
			Detach(platformView);
			return;
		}

		if (click is not null)
			return;

		SetEnabled(true);
		widget = platformView;
		try
		{
			click = GestureClick.New();
			click.SetButton(0);
			click.OnPressed += OnPressed;
			click.OnReleased += OnReleased;
			click.OnCancel += OnCancel;
			platformView.AddController(click);

			motion = EventControllerMotion.New();
			motion.OnEnter += OnEnter;
			motion.OnMotion += OnMotion;
			motion.OnLeave += OnLeave;
			platformView.AddController(motion);

			scroll = EventControllerScroll.New(EventControllerScrollFlags.Vertical);
			scroll.OnScroll += OnScroll;
			platformView.AddController(scroll);
		}
		catch
		{
			SetEnabled(false);
			throw;
		}
	}

	public void Detach(Widget platformView) => SetEnabled(false);

	internal void SetEnabled(bool enableTouchEvents)
	{
		if (enabled != enableTouchEvents)
		{
			enabled = enableTouchEvents;
			stateRevision++;
		}

		if (!enabled)
		{
			// Remove native subscriptions before notifying user code, which may re-enter.
			try
			{
				RemoveControllers();
			}
			finally
			{
				EndContact(SKTouchAction.Cancelled);
			}
		}
	}

	private void RemoveControllers()
	{
		var previousWidget = widget;
		var previousClick = click;
		var previousMotion = motion;
		var previousScroll = scroll;
		widget = null;
		click = null;
		motion = null;
		scroll = null;
		try
		{
			if (previousClick is not null)
			{
				previousClick.OnPressed -= OnPressed;
				previousClick.OnReleased -= OnReleased;
				previousClick.OnCancel -= OnCancel;
				previousWidget?.RemoveController(previousClick);
			}
		}
		finally
		{
			try
			{
				if (previousMotion is not null)
				{
					previousMotion.OnEnter -= OnEnter;
					previousMotion.OnMotion -= OnMotion;
					previousMotion.OnLeave -= OnLeave;
					previousWidget?.RemoveController(previousMotion);
				}
			}
			finally
			{
				if (previousScroll is not null)
				{
					previousScroll.OnScroll -= OnScroll;
					previousWidget?.RemoveController(previousScroll);
				}
			}
		}
	}

	private void OnPressed(GestureClick sender, GestureClick.PressedSignalArgs args) =>
		ProcessPress(args.X, args.Y, sender.GetCurrentButton() switch
		{
			1 => SKMouseButton.Left,
			2 => SKMouseButton.Middle,
			3 => SKMouseButton.Right,
			_ => SKMouseButton.Unknown,
		});

	private void OnReleased(GestureClick sender, GestureClick.ReleasedSignalArgs args) =>
		ProcessRelease(args.X, args.Y);

	private void OnCancel(Gesture sender, Gesture.CancelSignalArgs args) => ProcessCancel();

	private void OnEnter(EventControllerMotion sender, EventControllerMotion.EnterSignalArgs args) =>
		ProcessMotion(SKTouchAction.Entered, args.X, args.Y);

	private void OnMotion(EventControllerMotion sender, EventControllerMotion.MotionSignalArgs args) =>
		ProcessMotion(SKTouchAction.Moved, args.X, args.Y);

	private void OnLeave(EventControllerMotion sender, EventArgs args) => ProcessLeave();

	private bool OnScroll(EventControllerScroll sender, EventControllerScroll.ScrollSignalArgs args) =>
		ProcessScroll(args.Dy);

	internal void ProcessPress(double newX, double newY, SKMouseButton button)
	{
		if (!enabled)
			return;

		// Cancellation retains the previous coordinates and may disable or replace the contact.
		if (!EndContact(SKTouchAction.Cancelled) || !enabled)
			return;

		stateRevision++;
		x = newX;
		y = newY;
		Button = button;
		IsPressed = true;
		Send(SKTouchAction.Pressed, true, button);
	}

	internal void ProcessRelease(double newX, double newY)
	{
		if (!enabled)
			return;

		x = newX;
		y = newY;
		EndContact(SKTouchAction.Released);
	}

	internal void ProcessCancel() => EndContact(SKTouchAction.Cancelled);

	internal void ProcessMotion(SKTouchAction action, double newX, double newY)
	{
		if (!enabled)
			return;

		x = newX;
		y = newY;
		Send(action, IsPressed, IsPressed ? Button : SKMouseButton.Unknown);
	}

	internal void ProcessLeave()
	{
		if (enabled && EndContact(SKTouchAction.Cancelled) && enabled)
			Send(SKTouchAction.Exited, false, SKMouseButton.Unknown);
	}

	internal bool ProcessScroll(double dy) =>
		enabled && Send(SKTouchAction.WheelChanged, IsPressed,
			IsPressed ? Button : SKMouseButton.Unknown, Math.Sign(-dy));

	private bool EndContact(SKTouchAction action)
	{
		if (!IsPressed)
			return true;

		var revision = ++stateRevision;
		var button = Button;
		IsPressed = false;
		try
		{
			Send(action, false, button);
		}
		finally
		{
			if (!IsPressed)
				Button = SKMouseButton.Unknown;
		}
		return stateRevision == revision;
	}

	private bool Send(SKTouchAction action, bool inContact, SKMouseButton button, int wheelDelta = 0)
	{
		try
		{
			// Disconnect may already have cleared the platform view used for scaling.
			if (action != SKTouchAction.Cancelled && action != SKTouchAction.Exited)
				lastLocation = scalePixels(x, y);
			var e = new SKTouchEventArgs(0, action, button, SKTouchDeviceType.Mouse,
				lastLocation, inContact, wheelDelta);
			onTouchAction(e);
			return e.Handled;
		}
		catch
		{
			enabled = false;
			stateRevision++;
			IsPressed = false;
			Button = SKMouseButton.Unknown;
			RemoveControllers();
			throw;
		}
	}
}
