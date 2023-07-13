using System;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	[Obsolete("View renderers are obsolete in .NET MAUI. Use the handlers instead.")]
	internal class SKCanvasViewRenderer
	{
		public SKCanvasViewRenderer()
		{
			throw new PlatformNotSupportedException("SKCanvasView is not yet supported on this platform.");
		}
	}
}
