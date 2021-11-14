﻿using System;
using System.ComponentModel;

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

		internal SKColorSpace (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		void ISKNonVirtualReferenceCounted.ReferenceNative () =>
			SkiaApi.sk_colorspace_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () =>
			SkiaApi.sk_colorspace_unref (Handle);

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// properties

		public bool GammaIsCloseToSrgb =>
			SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);

		public bool GammaIsLinear =>
			SkiaApi.sk_colorspace_gamma_is_linear (Handle);

		public bool IsSrgb =>
			SkiaApi.sk_colorspace_is_srgb (Handle);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKColorSpaceType Type => SKColorSpaceType.Rgb;

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetNumericalTransferFunction() instead.")]
		public SKNamedGamma NamedGamma {
			get {
				var tf = GetNumericalTransferFunction ();
				return tf switch
				{
					_ when tf == SKColorSpaceTransferFn.Empty => SKNamedGamma.NonStandard,
					_ when tf == SKColorSpaceTransferFn.Linear => SKNamedGamma.Linear,
					_ when tf == SKColorSpaceTransferFn.Srgb => SKNamedGamma.Srgb,
					_ when tf == SKColorSpaceTransferFn.TwoDotTwo => SKNamedGamma.TwoDotTwoCurve,
					_ => SKNamedGamma.NonStandard,
				};
			}
		}

		public bool IsNumericalTransferFunction =>
			GetNumericalTransferFunction (out _);

		public static bool Equal (SKColorSpace left, SKColorSpace right)
		{
			if (left == null)
				throw new ArgumentNullException (nameof (left));
			if (right == null)
				throw new ArgumentNullException (nameof (right));

			return SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
		}

		// CreateSrgb

		public static SKColorSpace CreateSrgb () => srgb;

		// CreateSrgbLinear

		public static SKColorSpace CreateSrgbLinear () => srgbLinear;

		// CreateIcc

		public static SKColorSpace CreateIcc (IntPtr input, long length) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input, length));

		public static SKColorSpace CreateIcc (byte[] input, long length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			fixed (byte* i = input) {
				return CreateIcc (SKColorSpaceIccProfile.Create ((IntPtr)i, length));
			}
		}

		public static SKColorSpace CreateIcc (byte[] input) =>
			CreateIcc (input.AsSpan ());

		public static SKColorSpace CreateIcc (ReadOnlySpan<byte> input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		public static SKColorSpace CreateIcc (SKData input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		public static SKColorSpace CreateIcc (SKColorSpaceIccProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException (nameof (profile));

			return Referenced (GetObject (SkiaApi.sk_colorspace_new_icc (profile.Handle)), profile);
		}

		// CreateRgb

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags) =>
			CreateRgb (gamma, toXyzD50);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags) =>
			CreateRgb (gamma, gamut);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags) =>
			CreateRgb (coeffs, toXyzD50);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags) =>
			CreateRgb (coeffs, gamut);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			return CreateRgb (gamma.ToColorSpaceTransferFn (), toXyzD50.ToColorSpaceXyz ());
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut) =>
			CreateRgb (gamma.ToColorSpaceTransferFn (), gamut.ToColorSpaceXyz ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			return CreateRgb (coeffs, toXyzD50.ToColorSpaceXyz ());
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut) =>
			CreateRgb (coeffs, gamut.ToColorSpaceXyz ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			return CreateRgb (gamma.ToColorSpaceTransferFn (), toXyzD50.ToColorSpaceXyz ());
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
		public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut) =>
			CreateRgb (gamma.ToColorSpaceTransferFn (), gamut.ToColorSpaceXyz ());

		// CreateRgb

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50) =>
			GetObject (SkiaApi.sk_colorspace_new_rgb (&transferFn, &toXyzD50));

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

		// ToColorSpaceXyz

		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_to_xyzd50 (Handle, xyz);
			}
		}

		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// To*Gamma

		public SKColorSpace ToLinearGamma () =>
			GetObject (SkiaApi.sk_colorspace_make_linear_gamma (Handle));

		public SKColorSpace ToSrgbGamma () =>
			GetObject (SkiaApi.sk_colorspace_make_srgb_gamma (Handle));

		// *XyzD50

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ToColorSpaceXyz() instead.")]
		public SKMatrix44 ToXyzD50 () =>
			ToColorSpaceXyz ().ToMatrix44 ();

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			if (ToColorSpaceXyz (out var xyz) && xyz.ToMatrix44 () is SKMatrix44 m) {
				toXyzD50.SetColumnMajor (m.ToColumnMajor ());
				return true;
			}
			return false;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public SKMatrix44 FromXyzD50 () =>
			ToXyzD50 ()?.Invert ();

		//

		internal static SKColorSpace GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKColorSpace (h, o));

		private sealed class SKColorSpaceStatic : SKColorSpace
		{
			internal SKColorSpaceStatic (IntPtr x)
				: base (x, false)
			{
			}

			protected override void Dispose (bool disposing) { }
		}
	}
}
