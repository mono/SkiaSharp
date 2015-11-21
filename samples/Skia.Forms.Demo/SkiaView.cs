using System;
using Xamarin.Forms;
using SkiaSharp;

namespace Skia.Forms.Demo
{
	public class SkiaView : View, ISkiaViewController
	{
		Action <SKCanvas, int, int> onDrawCallback;

		public SkiaView (Action <SKCanvas, int, int> onDrawCallback)
		{
			this.onDrawCallback = onDrawCallback;
		}

		void ISkiaViewController.SendDraw (SKCanvas canvas)
		{
			Draw (canvas);
		}

		protected virtual void Draw (SKCanvas canvas)
		{
			onDrawCallback (canvas, (int)Width, (int)Height);
		}
	}
}

