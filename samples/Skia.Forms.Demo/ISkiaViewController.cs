using System;
using Xamarin.Forms;
using SkiaSharp;

namespace Skia.Forms.Demo
{
	public interface ISkiaViewController : IViewController
	{
		void SendDraw (SKCanvas canvas);
		void SendTap ();
	}
}

