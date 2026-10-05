using System;
using Microsoft.Maui;

namespace SkiaSharp.Views.Maui.Controls.Gtk4;

internal sealed class SKTouchContact
{
	public bool IsPressed { get; private set; }

	public SKMouseButton Button { get; private set; } = SKMouseButton.Unknown;

	public void Press(SKMouseButton button, Action<SKTouchAction, bool, SKMouseButton> emit)
	{
		Cancel(emit);
		Button = button;
		IsPressed = true;
		emit(SKTouchAction.Pressed, true, button);
	}

	public void Release(Action<SKTouchAction, bool, SKMouseButton> emit) =>
		End(SKTouchAction.Released, emit);

	public void Cancel(Action<SKTouchAction, bool, SKMouseButton> emit) =>
		End(SKTouchAction.Cancelled, emit);

	private void End(SKTouchAction action, Action<SKTouchAction, bool, SKMouseButton> emit)
	{
		if (!IsPressed)
			return;

		IsPressed = false;
		try
		{
			emit(action, false, Button);
		}
		finally
		{
			if (!IsPressed)
				Button = SKMouseButton.Unknown;
		}
	}
}
