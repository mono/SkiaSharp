#nullable disable

using System;
using System.ComponentModel;
using System.Threading;

namespace SkiaSharp
{
	public unsafe class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted
	{
		private static SKColorSpace srgb;
		private static bool srgbInitialized;
		private static object srgbLock = new object ();

		private static SKColorSpace srgbLinear;
		private static bool srgbLinearInitialized;
		private static object srgbLinearLock = new object ();

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

		public static SKColorSpace CreateSrgb () =>
			LazyInitializer.EnsureInitialized (
				ref srgb, ref srgbInitialized, ref srgbLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb ()));

		// CreateSrgbLinear

		public static SKColorSpace CreateSrgbLinear () =>
			LazyInitializer.EnsureInitialized (
				ref srgbLinear, ref srgbLinearInitialized, ref srgbLinearLock,
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb_linear ()));

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

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50) =>
			GetObject (SkiaApi.sk_colorspace_new_rgb (&transferFn, &toXyzD50));

		// CreateCicp

		public static SKColorSpace CreateCicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics) =>
			GetObject (SkiaApi.sk_colorspace_new_cicp (colorPrimaries, transferCharacteristics));

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

		//

		internal static SKColorSpace GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKColorSpace (h, o));

		// Variant used by singleton accessors (CreateSrgb/CreateSrgbLinear). The returned wrapper has
		// IgnorePublicDispose set AND is latched immortal under HandleDictionary's critical section —
		// atomic with the HD lookup, so the process-global sRGB native object is never freed by this
		// wrapper's finalizer or DisposeInternal.
		internal static SKColorSpace GetDisposeProtectedObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddImmortalSingletonObject (handle, owns, unrefExisting, (h, o) => new SKColorSpace (h, o));
	}
}
