using System;

namespace HarfBuzzSharp
{
	public class UnicodeFunctions : NativeObject
	{
		public static UnicodeFunctions Default => new UnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_default ());

		public static UnicodeFunctions Empty => new UnicodeFunctions (HarfBuzzApi.hb_unicode_funcs_get_empty ());

		internal UnicodeFunctions (IntPtr handle)
			: base (handle)
		{

		}

		protected override void Dispose (bool disposing)
		{
			HarfBuzzApi.hb_unicode_funcs_destroy (Handle);

			base.Dispose (disposing);
		}
	}
}
