using System;

namespace SkiaSharp
{
	public unsafe class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted
	{
		private static readonly SKColorSpace srgb;
		private static readonly SKColorSpace srgbLinear;

		static SKColorSpace ()
		{
			srgb = new SKColorSpaceStatic (SkiaApi.sk_colorspace_new_srgb ());
			srgbLinear = new SKColorSpaceStatic (SkiaApi.sk_colorspace_new_srgb_linear ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		[Preserve]
		internal SKColorSpace (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		void ISKNonVirtualReferenceCounted.ReferenceNative () =>
			SkiaApi.sk_colorspace_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () =>
			SkiaApi.sk_colorspace_unref (Handle);

		// properties

		public bool GammaIsCloseToSrgb => SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);

		public bool GammaIsLinear => SkiaApi.sk_colorspace_gamma_is_linear (Handle);

		public bool IsSrgb => SkiaApi.sk_colorspace_is_srgb (Handle);

		public bool IsNumericalTransferFunction => GetNumericalTransferFunction (out _);

		public static bool Equal (SKColorSpace left, SKColorSpace right)
		{
			if (left == null)
				throw new ArgumentNullException (nameof (left));
			if (right == null)
				throw new ArgumentNullException (nameof (right));

			return SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
		}

		// CreateSrgb*

		public static SKColorSpace CreateSrgb () => srgb;

		// CreateSrgbLinear

		public static SKColorSpace CreateSrgbLinear () => srgbLinear;

		// CreateRgb

		public static SKColorSpace CreateRgb (in SKColorSpaceTransferFn transferFn, SKMatrix44 toXyzD50)
		{
			var xyz = new SKColorSpaceXyz ();
			fixed (SKColorSpaceTransferFn* tf = &transferFn) {
				return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb (tf, &xyz));
			}
		}

		public static SKColorSpace CreateRgb (in SKColorSpaceTransferFn transferFn, in SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceTransferFn* tf = &transferFn)
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb (tf, xyz));
			}
		}

		// CreateIcc

		public static SKColorSpace CreateIcc (ReadOnlySpan<byte> data) =>
			CreateIcc (SKColorSpaceIccProfile.Create (data));

		public static SKColorSpace CreateIcc (SKData data) =>
			CreateIcc (SKColorSpaceIccProfile.Create (data));

		public static SKColorSpace CreateIcc (SKColorSpaceIccProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException (nameof (profile));

			var cs = GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (profile.Handle));
			if (cs != null)
				cs.KeepAlive (profile);

			return cs;
		}

		// GetNumericalTransferFunction

		public SKColorSpaceTransferFn GetNumericalTransferFunction () =>
			GetNumericalTransferFunction (out var fn) ? fn : SKColorSpaceTransferFn.Empty;

		public bool GetNumericalTransferFunction (out SKColorSpaceTransferFn fn)
		{
			fixed (SKColorSpaceTransferFn* f = &fn) {
				return SkiaApi.sk_colorspace_is_numerical_transfer_fn (Handle, f);
			}
		}

		// ToProfile

		public SKColorSpaceIccProfile ToProfile ()
		{
			var profile = new SKColorSpaceIccProfile ();
			SkiaApi.sk_colorspace_to_profile (Handle, profile.Handle);
			return profile;
		}

		// ToXyzD50

		public bool ToXyzD50 (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_to_xyzd50 (Handle, xyz);
			}
		}

		public SKColorSpaceXyz ToXyzD50 () =>
			ToXyzD50 (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// To*Gamma

		public SKColorSpace ToLinearGamma () =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_make_linear_gamma (Handle));

		public SKColorSpace ToSrgbGamma () =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_make_srgb_gamma (Handle));

		private sealed class SKColorSpaceStatic : SKColorSpace
		{
			internal SKColorSpaceStatic (IntPtr x)
				: base (x, false)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
