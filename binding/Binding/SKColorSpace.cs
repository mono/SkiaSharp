//
// Bindings for SKColorSpace
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKColorSpace : SKObject
	{
		[Preserve]
		internal SKColorSpace (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_colorspace_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public bool GammaIsCloseToSrgb => SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);
		
		public bool GammaIsLinear => SkiaApi.sk_colorspace_gamma_is_linear (Handle);
		
		public static bool Equal (SKColorSpace left, SKColorSpace right)
		{
			if (left == null) {
				throw new ArgumentNullException (nameof (left));
			}
			if (right == null) {
				throw new ArgumentNullException (nameof (right));
			}

			return SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
		}

		public static SKColorSpace CreateNamed (SKColorSpaceNamed name)
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_named (name));
		}

		public static SKColorSpace CreateIcc (IntPtr input, long length)
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (input, (IntPtr)length));
		}

		public static SKColorSpace CreateIcc (byte[] input, long length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (input, (IntPtr)length));
		}

		public static SKColorSpace CreateIcc (byte[] input)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (input, (IntPtr)input.Length));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma (gamma, toXyzD50.Handle));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_coeffs (ref coeffs, toXyzD50.Handle));
		}

		public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return SkiaApi.sk_colorspaceprimaries_to_xyzd50 (ref primaries, toXyzD50.Handle);
		}

		public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries)
		{
			var xyzD50 = new SKMatrix44 ();
			if (!ConvertPrimariesToXyzD50 (primaries, xyzD50)) {
				xyzD50.Dispose ();
				xyzD50 = null;
			}
			return xyzD50;
		}
	}
}
