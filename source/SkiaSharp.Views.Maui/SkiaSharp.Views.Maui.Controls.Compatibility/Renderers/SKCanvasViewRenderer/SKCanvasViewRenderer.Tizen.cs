using System;
using Microsoft.Maui.Controls;

using TForms = Microsoft.Maui.Controls.Compatibility.Forms;
using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Tizen.SKCanvasView;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>, IRegisterable
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView(TForms.NativeParent)
				: base.CreateNativeControl();
	}
}
