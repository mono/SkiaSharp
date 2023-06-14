#nullable enable
using System;
using Tizen.NUI;
using NView = Tizen.NUI.BaseComponents.View;

#if __MAUI__
namespace SkiaSharp.Views.Maui.Platform
#else
namespace SkiaSharp.Views.Forms
#endif
{
	internal class SKTouchHandler
	{
		private Action<SKTouchEventArgs>? onTouchAction;
		private Func<double, double, SKPoint>? scalePixels;
		private bool touchEnabled;
		private int currentId = 0;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;
		}

		public void SetEnabled(NView view, bool enableTouchEvents)
		{
			if (view != null)
			{
				if (touchEnabled != enableTouchEvents)
				{
					if (enableTouchEvents)
					{
						view.TouchEvent += OnTouchEvent;
					}
					else
					{
						view.TouchEvent -= OnTouchEvent;
					}
					touchEnabled = enableTouchEvents;
				}
			}
		}

		bool OnTouchEvent(object source, NView.TouchEventArgs e)
		{
			var pos = e.Touch.GetLocalPosition(0);
			var action = ToTouchAction(e.Touch.GetState(0));
			if (action == SKTouchAction.Pressed)
			{
				currentId++;
			}

			var coords = scalePixels?.Invoke(pos.X, pos.Y) ?? new SKPoint(pos.X, pos.Y);
			var inContact = (action == SKTouchAction.Pressed || action == SKTouchAction.Moved) ? true : false;
			onTouchAction?.Invoke(new SKTouchEventArgs(currentId, action, coords, inContact));
			return true;
		}

		public void Detach(NView view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		private static SKTouchAction ToTouchAction(PointStateType state) => state switch
		{
			PointStateType.Down => SKTouchAction.Pressed,
			PointStateType.Up => SKTouchAction.Released,
			PointStateType.Motion => SKTouchAction.Moved,
			PointStateType.Leave => SKTouchAction.Exited,
			PointStateType.Interrupted => SKTouchAction.Cancelled,
			_ => SKTouchAction.Cancelled,
		};
	}
}
