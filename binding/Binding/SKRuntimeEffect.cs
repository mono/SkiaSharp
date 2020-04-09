using System;

namespace SkiaSharp
{
	public unsafe class SKRuntimeEffect : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKRuntimeEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static SKRuntimeEffect Create (string sksl, out string errors)
		{
			using var s = new SKString (sksl);
			using var errorString = new SKString ();
			var effect = GetObject<SKRuntimeEffect> (SkiaApi.sk_runtimeeffect_make (s.Handle, errorString.Handle));
			errors = errorString?.ToString ();
			if (errors?.Length == 0)
				errors = null;
			return effect;
		}

		public int InputSize =>
			(int)SkiaApi.sk_runtimeeffect_get_input_size (Handle);

		public SKShader ToShader (bool isOpaque) =>
			ToShader (null, null, null, isOpaque);

		public SKShader ToShader (SKData inputs, bool isOpaque) =>
			ToShader (inputs, null, null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, bool isOpaque) =>
			ToShader (inputs, children, null, isOpaque);

		public SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix localMatrix, bool isOpaque) =>
			ToShader (inputs, children, (SKMatrix?)localMatrix, isOpaque);

		internal SKShader ToShader (SKData inputs, SKShader[] children, SKMatrix? localMatrix, bool isOpaque)
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
				if (localMatrix is SKMatrix m)
					return GetObject<SKShader> (SkiaApi.sk_runtimeeffect_make_shader (Handle, inputsHandle, ch, childLength, &m, isOpaque));
				else
					return GetObject<SKShader> (SkiaApi.sk_runtimeeffect_make_shader (Handle, inputsHandle, ch, childLength, null, isOpaque));
			}
		}

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
				return GetObject<SKColorFilter> (SkiaApi.sk_runtimeeffect_make_color_filter (Handle, inputsHandle, ch, (IntPtr)children?.Length));
			}
		}
	}
}
