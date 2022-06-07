#if __MAUI__
using Microsoft.Maui.Controls;
using TForms = Microsoft.Maui.Controls.Compatibility.Forms;
using SKFormsView = SkiaSharp.Views.Maui.Controls.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Tizen.SKCanvasView;
#else
using Xamarin.Forms;

using TForms = Xamarin.Forms.Forms;

using SKFormsView = SkiaSharp.Views.Forms.SKCanvasView;
using SKNativeView = SkiaSharp.Views.Tizen.SKCanvasView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(SkiaSharp.Views.Forms.SKCanvasViewRenderer))]
#endif

#if __MAUI__
namespace SkiaSharp.Views.Maui.Controls.Compatibility
#else
namespace SkiaSharp.Views.Forms
#endif
{
#if __MAUI__ && __TIZEN__
	[System.Obsolete]
#endif
	public class SKCanvasViewRenderer : SKCanvasViewRendererBase<SKFormsView, SKNativeView>, IRegisterable
	{
		protected override SKNativeView CreateNativeControl() =>
			GetType() == typeof(SKCanvasViewRenderer)
				? new SKNativeView(TForms.NativeParent)
				: base.CreateNativeControl();
	}
}
