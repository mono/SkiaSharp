#nullable enable
using System;
using ElmSharp;

#if __MAUI__
namespace SkiaSharp.Views.Maui.Platform
#else
namespace SkiaSharp.Views.Forms
#endif
{
	internal class SKTouchHandler
	{
		private readonly MomentumHandler momentumHandler;
		private Action<SKTouchEventArgs>? onTouchAction;
		private Func<double, double, SKPoint>? scalePixels;
		private GestureLayer? gestureLayer;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			this.onTouchAction = onTouchAction;
			this.scalePixels = scalePixels;

			momentumHandler = new MomentumHandler(this);
		}

		public void SetEnabled(EvasObject view, bool enableTouchEvents)
		{
			if (view != null)
			{
				if (enableTouchEvents)
					CreateGestureLayer(view);
				else
					DestroyGestureLayer();
			}
		}

		public void Detach(EvasObject view)
		{
			// clean the view
			SetEnabled(view, false);

			// remove references
			onTouchAction = null;
			scalePixels = null;
		}

		private void CreateGestureLayer(EvasObject parent)
		{
			if (gestureLayer == null)
			{
				gestureLayer = new GestureLayer(parent);
				gestureLayer.Attach(parent);
				gestureLayer.Deleted += (s, e) =>
				{
					gestureLayer = null;
					DestroyGestureLayer();
				};
				gestureLayer.IsEnabled = true;

				gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Start, (data) => { momentumHandler.OnStarted(); });
				gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, (data) => { momentumHandler.OnMoved(); });
				gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, (data) => { momentumHandler.OnFinished(); });
				gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, (data) => { momentumHandler.OnAborted(); });
			}
		}

		private void DestroyGestureLayer()
		{
			if (gestureLayer != null)
			{
				gestureLayer.IsEnabled = false;
				gestureLayer.Unrealize();
				gestureLayer = null;
			}
		}

		private class MomentumHandler
		{
			private readonly SKTouchHandler handler;
			private int currentId = 0;

			public MomentumHandler(SKTouchHandler h)
			{
				handler = h;
			}

			public void OnStarted()
			{
				++currentId;
				PostEvent(SKTouchAction.Pressed);
			}

			public void OnMoved()
			{
				PostEvent(SKTouchAction.Moved);
			}

			public void OnFinished()
			{
				PostEvent(SKTouchAction.Released);
			}

			public void OnAborted()
			{
				PostEvent(SKTouchAction.Cancelled);
			}

			private void PostEvent(SKTouchAction action)
			{
				if (handler.onTouchAction == null || handler.scalePixels == null || handler.gestureLayer == null)
					return;

				var p = handler.gestureLayer.EvasCanvas.Pointer;
				var coords = handler.scalePixels(p.X, p.Y);
				var inContact = (action == SKTouchAction.Pressed || action == SKTouchAction.Moved) ? true : false;

				handler.onTouchAction(new SKTouchEventArgs(currentId, action, coords, inContact));
			}
		}
	}
}
