using Microsoft.Maui;
using System;
using UIKit;

namespace SkiaSharp.Views.Maui.Platform;

internal class SKTouchHandlerProxy : SKEventProxy<IView, UIView>
{
	private SKTouchHandler? touchHandler;

	protected override void OnDisconnect(UIView platformView)
	{
		touchHandler?.Detach(platformView);
		touchHandler = null;
	}

	public void UpdateEnableTouchEvents(UIView platformView, bool enabled)
	{
		if (VirtualView is null)
			return;

		touchHandler ??= new SKTouchHandler(
			args => OnTouch(args),
			(x, y) => OnGetScaledCoord(x, y));

		touchHandler?.SetEnabled(platformView, enabled);
	}

	private void OnTouch(SKTouchEventArgs e)
	{
		if (VirtualView is ISKCanvasView canvasView)
			canvasView.OnTouch(e);
		else if (VirtualView is ISKGLView glView)
			glView.OnTouch(e);
	}

	private SKPoint OnGetScaledCoord(double x, double y)
	{
		var ignore = false;
		if (VirtualView is ISKCanvasView canvasView)
			ignore = canvasView.IgnorePixelScaling;
		else if (VirtualView is ISKGLView glView)
			ignore = glView.IgnorePixelScaling;

		if (ignore == false && touchHandler?.View is {} platformView)
		{
			var scale = platformView.ContentScaleFactor;

			x *= scale;
			y *= scale;
		}

		return new SKPoint((float)x, (float)y);
	}
}
