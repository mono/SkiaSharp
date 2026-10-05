using System;
using System.Collections.Generic;
using Microsoft.Maui;
using SkiaSharp.Views.Maui.Controls.Gtk4;
using Xunit;

namespace SkiaSharp.Views.Maui.Gtk4.Tests;

public class SKTouchContactTests
{
	[Fact]
	public void RepeatedPressCancelsPreviousButtonBeforeNewContact()
	{
		var contact = new SKTouchContact();
		var events = new List<(SKTouchAction Action, SKMouseButton Button, bool InContact)>();
		void Record(SKTouchAction action, bool inContact, SKMouseButton button) =>
			events.Add((action, button, inContact));

		contact.Press(SKMouseButton.Left, Record);
		contact.Press(SKMouseButton.Right, Record);
		contact.Cancel(Record);

		Assert.Equal(
			new[]
			{
				(SKTouchAction.Pressed, SKMouseButton.Left, true),
				(SKTouchAction.Cancelled, SKMouseButton.Left, false),
				(SKTouchAction.Pressed, SKMouseButton.Right, true),
				(SKTouchAction.Cancelled, SKMouseButton.Right, false),
			},
			events);
		Assert.False(contact.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, contact.Button);
	}

	[Fact]
	public void ReleaseResetsButtonWithoutReportingAnotherCancellation()
	{
		var contact = new SKTouchContact();
		var actions = new List<SKTouchAction>();
		void Record(SKTouchAction action, bool inContact, SKMouseButton button) =>
			actions.Add(action);

		contact.Press(SKMouseButton.Middle, Record);
		contact.Release(Record);
		contact.Cancel(Record);

		Assert.Equal(new[] { SKTouchAction.Pressed, SKTouchAction.Released }, actions);
		Assert.False(contact.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, contact.Button);
	}

	[Fact]
	public void CallbackFailureDoesNotLeaveButtonPressed()
	{
		var contact = new SKTouchContact();
		contact.Press(SKMouseButton.Left, (action, inContact, button) => { });

		Assert.Throws<InvalidOperationException>(() =>
			contact.Cancel((action, inContact, button) => throw new InvalidOperationException()));

		Assert.False(contact.IsPressed);
		Assert.Equal(SKMouseButton.Unknown, contact.Button);
	}
}
