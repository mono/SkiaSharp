using System;
using System.ComponentModel;
using Android.Content;
using Xamarin.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Android.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]

namespace SkiaSharp.Views.Forms
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>
	{
		public SKCanvasViewRenderer(Context context)
			: base(context)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("This constructor is obsolete as of version 2.5. Please use SKCanvasViewRenderer(Context) instead.")]
		public SKCanvasViewRenderer()
			: base()
		{
		}

		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView(Context)
				: base.CreateNativeControl();
	}
}
