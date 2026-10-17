using System;
using System.Collections.Generic;
using SkiaSharp.Views.Maui.Platform.Gtk4;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class SKTouchHandlerTests
{
	private static SKTouchHandler Create(Action<SKTouchEventArgs> onTouch)
	{
		var handler = new SKTouchHandler(onTouch, (x, y) => new SKPoint((float)x, (float)y));
		handler.SetEnabled(true);
		return handler;
	}

	[Fact]
	public void RepeatedPressCancelsAtPreviousCoordinatesBeforeNewContact()
	{
		var events = new List<SKTouchEventArgs>();
		var handler = Create(events.Add);
		handler.ProcessPress(4, 7, SKMouseButton.Left);
		handler.ProcessPress(12, 15, SKMouseButton.Right);
		handler.ProcessCancel();

		Assert.Collection(events,
			e => Check(e, SKTouchAction.Pressed, SKMouseButton.Left, true, 4, 7),
			e => Check(e, SKTouchAction.Cancelled, SKMouseButton.Left, false, 4, 7),
			e => Check(e, SKTouchAction.Pressed, SKMouseButton.Right, true, 12, 15),
			e => Check(e, SKTouchAction.Cancelled, SKMouseButton.Right, false, 12, 15));
		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
	}

	[Fact]
	public void ReleaseRetainsPressedButtonAndHoverHasNoStaleButton()
	{
		var events = new List<SKTouchEventArgs>();
		var handler = Create(events.Add);
		handler.ProcessPress(1, 2, SKMouseButton.Middle);
		handler.ProcessRelease(3, 4);
		handler.ProcessCancel();
		handler.ProcessMotion(SKTouchAction.Moved, 5, 6);

		Assert.Collection(events,
			e => Check(e, SKTouchAction.Pressed, SKMouseButton.Middle, true, 1, 2),
			e => Check(e, SKTouchAction.Released, SKMouseButton.Middle, false, 3, 4),
			e => Check(e, SKTouchAction.Moved, SKMouseButton.Unknown, false, 5, 6));
		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
	}

	[Fact]
	public void LeaveCancelsBeforeReportingExit()
	{
		var events = new List<SKTouchEventArgs>();
		var handler = Create(events.Add);
		handler.ProcessPress(1, 2, SKMouseButton.Left);
		handler.ProcessLeave();
		handler.ProcessCancel();

		Assert.Collection(events,
			e => Check(e, SKTouchAction.Pressed, SKMouseButton.Left, true, 1, 2),
			e => Check(e, SKTouchAction.Cancelled, SKMouseButton.Left, false, 1, 2),
			e => Check(e, SKTouchAction.Exited, SKMouseButton.Unknown, false, 1, 2));
	}

	[Theory]
	[InlineData(SKTouchAction.Pressed)]
	[InlineData(SKTouchAction.Released)]
	[InlineData(SKTouchAction.Cancelled)]
	[InlineData(SKTouchAction.Moved)]
	[InlineData(SKTouchAction.WheelChanged)]
	public void CallbackFailureClearsContactAndDisablesEvents(SKTouchAction failingAction)
	{
		var error = new InvalidOperationException("Touch callback failed.");
		var events = new List<SKTouchEventArgs>();
		var shouldThrow = true;
		var handler = Create(e =>
		{
			events.Add(e);
			if (shouldThrow && e.ActionType == failingAction)
				throw error;
		});

		if (failingAction != SKTouchAction.Pressed)
			handler.ProcessPress(1, 2, SKMouseButton.Left);
		Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
		{
			switch (failingAction)
			{
				case SKTouchAction.Pressed: handler.ProcessPress(1, 2, SKMouseButton.Left); break;
				case SKTouchAction.Released: handler.ProcessRelease(3, 4); break;
				case SKTouchAction.Cancelled: handler.SetEnabled(false); break;
				case SKTouchAction.Moved: handler.ProcessMotion(SKTouchAction.Moved, 3, 4); break;
				case SKTouchAction.WheelChanged: handler.ProcessScroll(1); break;
			}
		}));

		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
		var count = events.Count;
		handler.SetEnabled(false);
		handler.ProcessPress(5, 6, SKMouseButton.Right);
		handler.ProcessMotion(SKTouchAction.Moved, 5, 6);
		Assert.False(handler.ProcessScroll(1));
		Assert.Equal(count, events.Count);
		shouldThrow = false;
		handler.SetEnabled(true);
		handler.ProcessPress(5, 6, SKMouseButton.Right);
		Assert.True(handler.IsPressed);
		Assert.Equal(SKMouseButton.Right, handler.Button);
	}

	[Fact]
	public void PressCallbackCanDisableInputWithoutLeavingAContact()
	{
		var events = new List<SKTouchEventArgs>();
		SKTouchHandler handler = null!;
		handler = Create(e =>
		{
			events.Add(e);
			if (e.ActionType == SKTouchAction.Pressed)
				handler.SetEnabled(false);
		});
		handler.ProcessPress(1, 2, SKMouseButton.Left);
		handler.ProcessRelease(3, 4);

		Assert.Equal(2, events.Count);
		Check(events[1], SKTouchAction.Cancelled, SKMouseButton.Left, false, 1, 2);
		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void RepeatedPressStopsWhenCancellationCallbackChangesEnabledState(bool reenable)
	{
		var events = new List<SKTouchEventArgs>();
		SKTouchHandler handler = null!;
		handler = Create(e =>
		{
			events.Add(e);
			if (e.ActionType == SKTouchAction.Cancelled)
			{
				handler.SetEnabled(false);
				if (reenable)
					handler.SetEnabled(true);
			}
		});
		handler.ProcessPress(1, 2, SKMouseButton.Left);
		handler.ProcessPress(3, 4, SKMouseButton.Right);

		Assert.Equal(2, events.Count);
		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
		if (reenable)
		{
			handler.ProcessMotion(SKTouchAction.Entered, 5, 6);
			Check(events[2], SKTouchAction.Entered, SKMouseButton.Unknown, false, 5, 6);
		}
	}

	[Fact]
	public void ReleaseCallbackCanStartANewContact()
	{
		SKTouchHandler handler = null!;
		handler = Create(e =>
		{
			if (e.ActionType == SKTouchAction.Released)
			{
				Assert.Equal(SKMouseButton.Left, handler.Button);
				handler.ProcessPress(5, 6, SKMouseButton.Right);
			}
		});
		handler.ProcessPress(1, 2, SKMouseButton.Left);
		handler.ProcessRelease(3, 4);

		Assert.True(handler.IsPressed);
		Assert.Equal(SKMouseButton.Right, handler.Button);
	}

	[Fact]
	public void DisableUsesLastScaledLocationAfterPlatformViewIsDisconnected()
	{
		var events = new List<SKTouchEventArgs>();
		var disconnected = false;
		var handler = new SKTouchHandler(events.Add, (x, y) =>
		{
			if (disconnected)
				throw new InvalidOperationException("The platform view is disconnected.");
			return new SKPoint((float)x * 2, (float)y * 2);
		});
		handler.SetEnabled(true);
		handler.ProcessPress(4, 7, SKMouseButton.Left);
		disconnected = true;
		handler.SetEnabled(false);

		Check(events[1], SKTouchAction.Cancelled, SKMouseButton.Left, false, 8, 14);
		Assert.False(handler.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, handler.Button);
	}

	[Fact]
	public void CancellationCallbackCanReplaceContactWithoutOuterPressOverwritingIt()
	{
		var events = new List<SKTouchEventArgs>();
		SKTouchHandler handler = null!;
		handler = Create(e =>
		{
			events.Add(e);
			if (e.ActionType == SKTouchAction.Cancelled)
				handler.ProcessPress(8, 9, SKMouseButton.Middle);
		});
		handler.ProcessPress(1, 2, SKMouseButton.Left);
		handler.ProcessPress(3, 4, SKMouseButton.Right);

		Assert.Equal(3, events.Count);
		Check(events[2], SKTouchAction.Pressed, SKMouseButton.Middle, true, 8, 9);
		Assert.True(handler.IsPressed);
		Assert.Equal(SKMouseButton.Middle, handler.Button);
	}

	[Theory]
	[InlineData(1, false, 4, 7)]
	[InlineData(2, false, 8, 14)]
	[InlineData(2, true, 4, 7)]
	[InlineData(3, false, 12, 21)]
	public void EventsUseTheHandlersCoordinateTransform(int scale, bool ignorePixelScaling, int x, int y)
	{
		var events = new List<SKTouchEventArgs>();
		var handler = new SKTouchHandler(events.Add,
			(px, py) => SKTouchHandler.GetTouchLocation(px, py, scale, ignorePixelScaling));
		handler.SetEnabled(true);
		handler.ProcessPress(4, 7, SKMouseButton.Left);
		Check(Assert.Single(events), SKTouchAction.Pressed, SKMouseButton.Left, true, x, y);
	}

	[Fact]
	public void InvalidDeviceScaleIsRejected() =>
		Assert.Throws<ArgumentOutOfRangeException>(() => SKTouchHandler.GetTouchLocation(1, 2, 0, false));

	[Theory]
	[InlineData(-2, true, 1)]
	[InlineData(2, false, -1)]
	[InlineData(0, true, 0)]
	public void WheelPropagatesHandledAndDirection(double dy, bool handled, int delta)
	{
		var events = new List<SKTouchEventArgs>();
		var handler = Create(e =>
		{
			events.Add(e);
			e.Handled = handled;
		});
		handler.ProcessMotion(SKTouchAction.Entered, 4, 7);
		Assert.Equal(handled, handler.ProcessScroll(dy));
		Check(events[1], SKTouchAction.WheelChanged, SKMouseButton.Unknown, false, 4, 7);
		Assert.Equal(delta, events[1].WheelDelta);
	}

	private static void Check(SKTouchEventArgs e, SKTouchAction action, SKMouseButton button,
		bool inContact, float x, float y)
	{
		Assert.Equal(action, e.ActionType);
		Assert.Equal(button, e.MouseButton);
		Assert.Equal(inContact, e.InContact);
		Assert.Equal(new SKPoint(x, y), e.Location);
		Assert.Equal(SKTouchDeviceType.Mouse, e.DeviceType);
	}
}
