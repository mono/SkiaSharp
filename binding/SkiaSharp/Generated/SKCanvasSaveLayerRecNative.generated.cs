using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_canvas_savelayerrec_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKCanvasSaveLayerRecNative : IEquatable<SKCanvasSaveLayerRecNative> {
		// public sk_rect_t* fBounds
		public SKRect* fBounds;

		// public sk_paint_t* fPaint
		public IntPtr fPaint;

		// public sk_imagefilter_t* fBackdrop
		public IntPtr fBackdrop;

		// public sk_canvas_savelayerrec_flags_t fFlags
		public SKCanvasSaveLayerRecFlags fFlags;

		public readonly bool Equals (SKCanvasSaveLayerRecNative obj) =>
#pragma warning disable CS8909
			fBounds == obj.fBounds && fPaint == obj.fPaint && fBackdrop == obj.fBackdrop && fFlags == obj.fFlags;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKCanvasSaveLayerRecNative f && Equals (f);

		public static bool operator == (SKCanvasSaveLayerRecNative left, SKCanvasSaveLayerRecNative right) =>
			left.Equals (right);

		public static bool operator != (SKCanvasSaveLayerRecNative left, SKCanvasSaveLayerRecNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fBounds);
			hash.Add (fPaint);
			hash.Add (fBackdrop);
			hash.Add (fFlags);
			return hash.ToHashCode ();
		}

	}
}
