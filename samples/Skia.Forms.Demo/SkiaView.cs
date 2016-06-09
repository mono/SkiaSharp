using System;
using Xamarin.Forms;
using SkiaSharp;

namespace Skia.Forms.Demo
{
	public class SkiaView : View, ISkiaViewController
	{
		Demos.Sample sample;

		public SkiaView (Demos.Sample sample)
		{
			this.sample = sample;
		}

		void ISkiaViewController.SendDraw (SKCanvas canvas)
		{
			Draw (canvas);
		}

		void ISkiaViewController.SendTap ()
		{
			sample?.TapMethod?.Invoke ();
		}

		protected virtual void Draw (SKCanvas canvas)
		{
			sample?.Method?.Invoke (canvas, (int)Width, (int)Height);
		}
	}
}

