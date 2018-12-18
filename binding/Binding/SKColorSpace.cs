using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	[StructLayout (LayoutKind.Sequential)]
	public struct SKColorSpacePrimaries
	{
		private float fRX;
		private float fRY;
		private float fGX;
		private float fGY;
		private float fBX;
		private float fBY;
		private float fWX;
		private float fWY;

		public SKColorSpacePrimaries (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 8)
				throw new ArgumentException ("The values must have exactly 8 items, one for each of [RX, RY, GX, GY, BX, BY, WX, WY].", nameof (values));

			fRX = values[0];
			fRY = values[1];
			fGX = values[2];
			fGY = values[3];
			fBX = values[4];
			fBY = values[5];
			fWX = values[6];
			fWY = values[7];
		}

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
			get => fRX;
			set => fRX = value;
		}
		public float RY {
			get => fRY;
			set => fRY = value;
		}
		public float GX {
			get => fGX;
			set => fGX = value;
		}
		public float GY {
			get => fGY;
			set => fGY = value;
		}
		public float BX {
			get => fBX;
			set => fBX = value;
		}
		public float BY {
			get => fBY;
			set => fBY = value;
		}
		public float WX {
			get => fWX;
			set => fWX = value;
		}
		public float WY {
			get => fWY;
			set => fWY = value;
		}

		public float[] Values => new[] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

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

	[StructLayout (LayoutKind.Sequential)]
	public struct SKColorSpaceTransferFn
	{
		private float fG;
		private float fA;
		private float fB;
		private float fC;
		private float fD;
		private float fE;
		private float fF;

		public SKColorSpaceTransferFn (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 7)
				throw new ArgumentException ("The values must have exactly 7 items, one for each of [G, A, B, C, D, E, F].", nameof (values));

			fG = values[0];
			fA = values[1];
			fB = values[2];
			fC = values[3];
			fD = values[4];
			fE = values[5];
			fF = values[6];
		}

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
			get => fG;
			set => fG = value;
		}
		public float A {
			get => fA;
			set => fA = value;
		}
		public float B {
			get => fB;
			set => fB = value;
		}
		public float C {
			get => fC;
			set => fC = value;
		}
		public float D {
			get => fD;
			set => fD = value;
		}
		public float E {
			get => fE;
			set => fE = value;
		}
		public float F {
			get => fF;
			set => fF = value;
		}

		public float[] Values => new[] { fG, fA, fB, fC, fD, fE, fF };

		public SKColorSpaceTransferFn Invert ()
		{
			SkiaApi.sk_colorspace_transfer_fn_invert (ref this, out var inverted);
			return inverted;
		}

		public float Transform (float x) =>
			SkiaApi.sk_colorspace_transfer_fn_transform (ref this, x);
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

		public SKColorSpaceType Type => SkiaApi.sk_colorspace_gamma_get_type (Handle);

		public SKNamedGamma NamedGamma => SkiaApi.sk_colorspace_gamma_get_gamma_named (Handle);

		public bool IsNumericalTransferFunction => GetNumericalTransferFunction (out _);

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

		public static SKColorSpace CreateSrgb () =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_srgb ());

		public static SKColorSpace CreateSrgbLinear () =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_srgb_linear ());

		public static SKColorSpace CreateIcc (IntPtr input, long length)
		{
			if (input == IntPtr.Zero)
				throw new ArgumentNullException (nameof (input));

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

		[Obsolete ("Use CreateRgb (SKColorSpaceRenderTargetGamma, SKMatrix44) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags) =>
			CreateRgb (gamma, toXyzD50);

		[Obsolete ("Use CreateRgb (SKColorSpaceRenderTargetGamma, SKColorSpaceGamut) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags) =>
			CreateRgb (gamma, gamut);

		[Obsolete ("Use CreateRgb (SKColorSpaceTransferFn, SKMatrix44) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags) =>
			CreateRgb (coeffs, toXyzD50);

		[Obsolete ("Use CreateRgb (SKColorSpaceTransferFn, SKColorSpaceGamut) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags) =>
			CreateRgb (coeffs, gamut);

		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma (gamma, toXyzD50.Handle));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut) =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma_and_gamut (gamma, gamut));

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_coeffs (ref coeffs, toXyzD50.Handle));
		}

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut) =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_coeffs_and_gamut (ref coeffs, gamut));

		public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma_named (gamma, toXyzD50.Handle));
		}

		public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut) =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_with_gamma_named_and_gamut (gamma, gamut));

		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return SkiaApi.sk_colorspace_to_xyzd50 (Handle, toXyzD50.Handle);
		}

		public bool GetNumericalTransferFunction (out SKColorSpaceTransferFn fn) =>
			SkiaApi.sk_colorspace_is_numerical_transfer_fn (Handle, out fn);

		public SKMatrix44 ToXyzD50 () =>
			GetObject<SKMatrix44> (SkiaApi.sk_colorspace_as_to_xyzd50 (Handle), false);

		public SKMatrix44 FromXyzD50 () =>
			GetObject<SKMatrix44> (SkiaApi.sk_colorspace_as_from_xyzd50 (Handle), false);
	}
}
