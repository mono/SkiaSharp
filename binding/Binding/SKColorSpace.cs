using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SKColorSpacePrimaries {
		private float fRX, fRY;
		private float fGX, fGY;
		private float fBX, fBY;
		private float fWX, fWY;

		public SKColorSpacePrimaries (float rx, float ry, float gx, float gy, float bx, float by, float wx, float wy)
		{
			fRX = rx;
			fRY = ry;
			fGX = gx;
			fGY = gy;
			fBX = bx;
			fBY = by;
			fWX = wx;
			fWY = wy;
		}

		public float RX { 
			get { return fRX; }
			set { fRX = value; }
		}
		public float RY { 
			get { return fRY; }
			set { fRY = value; }
		}
		public float GX { 
			get { return fGX; }
			set { fGX = value; }
		}
		public float GY { 
			get { return fGY; }
			set { fGY = value; }
		}
		public float BX { 
			get { return fBX; }
			set { fBX = value; }
		}
		public float BY { 
			get { return fBY; }
			set { fBY = value; }
		}
		public float WX { 
			get { return fWX; }
			set { fWX = value; }
		}
		public float WY { 
			get { return fWY; }
			set { fWY = value; }
		}

		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return SkiaApi.sk_colorspaceprimaries_to_xyzd50 (ref this, toXyzD50.Handle);
		}

		public SKMatrix44 ToXyzD50 ()
		{
			var xyzD50 = new SKMatrix44 ();
			if (!ToXyzD50 (xyzD50)) {
				xyzD50.Dispose ();
				xyzD50 = null;
			}
			return xyzD50;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKColorSpaceTransferFn {
		private float fG;
		private float fA;
		private float fB;
		private float fC;
		private float fD;
		private float fE;
		private float fF;

		public SKColorSpaceTransferFn (float g, float a, float b, float c, float d, float e, float f)
		{
			fG = g;
			fA = a;
			fB = b;
			fC = c;
			fD = d;
			fE = e;
			fF = f;
		}

		public float G { 
			get { return fG; }
			set { fG = value; }
		}
		public float A { 
			get { return fA; }
			set { fA = value; }
		}
		public float B { 
			get { return fB; }
			set { fB = value; }
		}
		public float C { 
			get { return fC; }
			set { fC = value; }
		}
		public float D { 
			get { return fD; }
			set { fD = value; }
		}
		public float E { 
			get { return fE; }
			set { fE = value; }
		}
		public float F { 
			get { return fF; }
			set { fF = value; }
		}

		public SKColorSpaceTransferFn Invert ()
		{
			SkiaApi.sk_colorspace_transfer_fn_invert (ref this, out var inverted);
			return inverted;
		}
	}

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
		
		public bool IsSrgb => SkiaApi.sk_colorspace_is_srgb (Handle);
		
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

		public static SKColorSpace CreateSrgb ()
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_srgb ());
		}

		public static SKColorSpace CreateSrgbLinear ()
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_srgb_linear ());
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

		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags = 0)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma (gamma, toXyzD50.Handle, flags));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags = 0)
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma_and_gamut (gamma, gamut, flags));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags = 0)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_coeffs (ref coeffs, toXyzD50.Handle, flags));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags = 0)
		{
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_coeffs_and_gamut (ref coeffs, gamut, flags));
		}

		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return SkiaApi.sk_colorspace_to_xyzd50 (Handle, toXyzD50.Handle);
		}

		public SKMatrix44 ToXyzD50 ()
		{
			var xyzD50 = new SKMatrix44 ();
			if (!ToXyzD50 (xyzD50)) {
				xyzD50.Dispose ();
				xyzD50 = null;
			}
			return xyzD50;
		}
	}
}
