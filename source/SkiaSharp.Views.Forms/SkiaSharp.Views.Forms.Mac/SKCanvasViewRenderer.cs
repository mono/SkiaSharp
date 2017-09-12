using System;
using AppKit;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Mac.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		protected override SKNativeView CreateNativeControl()
		{
			var view = new Test(HandleScrollWheel);
			return view;
		}
	}

	internal class Test : SKNativeView
	{
		private readonly Action<NSEvent> onScrollWheel;

		public Test(Action<NSEvent> onScrollWheel)
		{
			this.onScrollWheel = onScrollWheel;
		}

		public override void ScrollWheel(NSEvent theEvent)
		{
			base.ScrollWheel(theEvent);

			onScrollWheel?.Invoke(theEvent);
		}
	}
}
