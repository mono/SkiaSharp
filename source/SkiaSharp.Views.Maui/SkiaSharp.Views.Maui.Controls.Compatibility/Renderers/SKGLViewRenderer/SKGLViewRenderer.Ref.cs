using System;

namespace SkiaSharp.Views.Maui.Controls.Compatibility
{
	[Obsolete("View renderers are obsolete in .NET MAUI. Use the handlers instead.")]
	internal class SKGLViewRenderer
	{
		public SKGLViewRenderer()
		{
			throw new System.PlatformNotSupportedException("SKGLView is not yet supported on this platform.");
		}
	}
}
