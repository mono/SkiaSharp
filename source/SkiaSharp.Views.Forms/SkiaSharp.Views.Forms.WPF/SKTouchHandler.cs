using System;
using System.Windows;
using System.Windows.Forms.Integration;

namespace SkiaSharp.Views.Forms
{
	internal class SKTouchHandler
	{
		private SKTouchHandlerElement element;
		private SKTouchHandlerWinForms winforms;

		public SKTouchHandler(Action<SKTouchEventArgs> onTouchAction, Func<double, double, SKPoint> scalePixels)
		{
			element = new SKTouchHandlerElement(onTouchAction, scalePixels);
			winforms = new SKTouchHandlerWinForms(onTouchAction, scalePixels);
		}

		public void SetEnabled(FrameworkElement view, bool enableTouchEvents)
		{
			if (view is WindowsFormsHost wfh)
				winforms.SetEnabled(wfh.Child, enableTouchEvents);
			else
				element.SetEnabled(view, enableTouchEvents);
		}

		public void Detach(FrameworkElement view)
		{
			if (view is WindowsFormsHost wfh)
				winforms.Detach(wfh.Child);
			else
				element.Detach(view);

			element = null;
			winforms = null;
		}
	}
}
