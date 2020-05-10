using System;

namespace SkiaSharp
{
	public unsafe class SK3dView : SKObject, ISKSkipObjectRegistration
	{
		internal SK3dView (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SK3dView ()
			: this (SkiaApi.sk_3dview_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SK3dView instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_3dview_destroy (Handle);

		// Matrix

		public SKMatrix Matrix {
			get {
				var matrix = SKMatrix.MakeIdentity ();
				GetMatrix (ref matrix);
				return matrix;
			}
		}

		public void GetMatrix (ref SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_3dview_get_matrix (Handle, m);
			}
		}

		// Save

		public void Save () =>
			SkiaApi.sk_3dview_save (Handle);

		// Restore

		public void Restore () =>
			SkiaApi.sk_3dview_restore (Handle);

		// Translate

		public void Translate (float x, float y, float z) =>
			SkiaApi.sk_3dview_translate (Handle, x, y, z);

		public void TranslateX (float x) =>
			Translate (x, 0, 0);

		public void TranslateY (float y) =>
			Translate (0, y, 0);

		public void TranslateZ (float z) =>
			Translate (0, 0, z);

		// Rotate*Degrees

		public void RotateXDegrees (float degrees) =>
			SkiaApi.sk_3dview_rotate_x_degrees (Handle, degrees);

		public void RotateYDegrees (float degrees) =>
			SkiaApi.sk_3dview_rotate_y_degrees (Handle, degrees);

		public void RotateZDegrees (float degrees) =>
			SkiaApi.sk_3dview_rotate_z_degrees (Handle, degrees);

		// Rotate*Radians

		public void RotateXRadians (float radians) =>
			SkiaApi.sk_3dview_rotate_x_radians (Handle, radians);

		public void RotateYRadians (float radians) =>
			SkiaApi.sk_3dview_rotate_y_radians (Handle, radians);

		public void RotateZRadians (float radians) =>
			SkiaApi.sk_3dview_rotate_z_radians (Handle, radians);

		// DotWithNormal

		public float DotWithNormal (float dx, float dy, float dz) =>
			SkiaApi.sk_3dview_dot_with_normal (Handle, dx, dy, dz);

		// Apply

		public void ApplyToCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_3dview_apply_to_canvas (Handle, canvas.Handle);
		}
	}
}
