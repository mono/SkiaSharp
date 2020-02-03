using System;

namespace SkiaSharp
{
	public unsafe class SKPathMeasure : SKObject
	{
		[Preserve]
		internal SKPathMeasure (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPathMeasure ()
			: this (SkiaApi.sk_pathmeasure_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
		}

		public SKPathMeasure (SKPath path, bool forceClosed = false, float resScale = 1)
			: this (SkiaApi.sk_pathmeasure_new_with_path (path?.Handle ?? IntPtr.Zero, forceClosed, resScale), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPathMeasure instance.");
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_pathmeasure_destroy (Handle);

		// properties

		public float Length =>
			SkiaApi.sk_pathmeasure_get_length (Handle);

		public bool IsClosed =>
			SkiaApi.sk_pathmeasure_is_closed (Handle);

		// SetPath

		public void SetPath (SKPath path, bool forceClosed = false) =>
			SkiaApi.sk_pathmeasure_set_path (Handle, path?.Handle ?? IntPtr.Zero, forceClosed);

		// GetPositionAndTangent

		public bool GetPositionAndTangent (float distance, out SKPoint position, out SKPoint tangent)
		{
			fixed (SKPoint* p = &position)
			fixed (SKPoint* t = &tangent) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, t);
			}
		}

		// GetPosition

		public bool GetPosition (float distance, out SKPoint position)
		{
			fixed (SKPoint* p = &position) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, p, null);
			}
		}

		// GetTangent

		public bool GetTangent (float distance, out SKPoint tangent)
		{
			fixed (SKPoint* t = &tangent) {
				return SkiaApi.sk_pathmeasure_get_pos_tan (Handle, distance, null, t);
			}
		}

		// GetMatrix

		public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags)
		{
			fixed (SKMatrix* m = &matrix) {
				return SkiaApi.sk_pathmeasure_get_matrix (Handle, distance, m, flags);
			}
		}

		// GetSegment

		public bool GetSegment (float start, float stop, SKPath dst, bool startWithMoveTo)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_pathmeasure_get_segment (Handle, start, stop, dst.Handle, startWithMoveTo);
		}

		public SKPath GetSegment (float start, float stop, bool startWithMoveTo)
		{
			var dst = new SKPath ();
			if (!SkiaApi.sk_pathmeasure_get_segment (Handle, start, stop, dst.Handle, startWithMoveTo)) {
				dst.Dispose ();
				dst = null;
			}
			return dst;
		}

		// NextContour

		public bool NextContour () =>
			SkiaApi.sk_pathmeasure_next_contour (Handle);
	}
}
