using System;

namespace SkiaSharp
{
	public unsafe class SKRuntimeEffect : SKObject, ISKReferenceCounted
	{
		internal SKRuntimeEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Create

		public static SKRuntimeEffect Create (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject (SkiaApi.sk_runtimeeffect_make (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		// properties

		public int InputSize =>
			(int)SkiaApi.sk_runtimeeffect_get_input_size (Handle);

		// ToShader

		public SKShader ToShader (bool isOpaque) =>
			ToShader (null, null, null, isOpaque);

		public SKShader ToShader (SKData inputs, bool isOpaque) =>
			ToShader (inputs, null, null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, bool isOpaque) =>
			ToShader (inputs, children, null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs, children, &localMatrix, isOpaque);

		internal unsafe SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix* localMatrix, bool isOpaque)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;

			IntPtr[] childrenHandles = null;
			var childLength = IntPtr.Zero;
			if (children != null && children.Length > 0) {
				childrenHandles = new IntPtr[children.Length];
				for (int i = 0; i < children.Length; i++) {
					childrenHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
				}
				childLength = (IntPtr)children.Length;
			}

			fixed (IntPtr* ch = childrenHandles) {
				return SKShader.GetObject (SkiaApi.sk_runtimeeffect_make_shader (Handle, inputsHandle, ch, childLength, localMatrix, isOpaque));
			}
		}

		// ToColorFilter

		public SKColorFilter ToColorFilter () =>
			ToColorFilter (null, null);

		public SKColorFilter ToColorFilter (SKData inputs) =>
			ToColorFilter (inputs, null);

		internal SKColorFilter ToColorFilter (SKData inputs, SKShader[] children)
		{
			var inputsHandle = inputs?.Handle ?? IntPtr.Zero;

			IntPtr[] childrenHandles = null;
			if (children != null && children.Length > 0) {
				childrenHandles = new IntPtr[children.Length];
				for (int i = 0; i < children.Length; i++) {
					childrenHandles[i] = children[i]?.Handle ?? IntPtr.Zero;
				}
			}

			fixed (IntPtr* ch = childrenHandles) {
				return SKColorFilter.GetObject (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, inputsHandle, ch, (IntPtr)children?.Length));
			}
		}

		//

		internal static SKRuntimeEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKRuntimeEffect (h, o));
	}
}
