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

		void ISKNonVirtualReferenceCounted.ReferenceNative ()
		{
			SkiaApi.sk_colorspace_ref (Handle);
			GC.KeepAlive (this);
		}

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
		{
			SkiaApi.sk_colorspace_unref (Handle);
			GC.KeepAlive (this);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// properties

		public bool GammaIsCloseToSrgb {
			get {
				var result = SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool GammaIsLinear {
			get {
				var result = SkiaApi.sk_colorspace_gamma_is_linear (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		public bool IsSrgb {
			get {
				var result = SkiaApi.sk_colorspace_is_srgb (Handle);
				GC.KeepAlive (this);
				return result;
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

			var result = SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
			GC.KeepAlive (left);
			GC.KeepAlive (right);
			return result;
		}

		// CreateSrgb

		public static SKColorSpace CreateSrgb () =>
			LazyInitializer.EnsureInitialized (
				ref srgb, ref srgbInitialized, ref srgbLock,
				// Immortal Skia singleton (sk_srgb_singleton, function-local static) — never unref it.
				// See SKColorFilter.GetDisposeProtectedObject for the full teardown-crash rationale.
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb (), owns: false, unrefExisting: false));

		// CreateSrgbLinear

		public static SKColorSpace CreateSrgbLinear () =>
			LazyInitializer.EnsureInitialized (
				ref srgbLinear, ref srgbLinearInitialized, ref srgbLinearLock,
				// Immortal Skia singleton (sk_srgb_linear_singleton, function-local static) — never unref it.
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb_linear (), owns: false, unrefExisting: false));

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
				var result = SkiaApi.sk_colorspace_is_numerical_transfer_fn (Handle, f);
				GC.KeepAlive (this);
				return result;
			}
		}

		// ToProfile

		public SKColorSpaceIccProfile ToProfile ()
		{
			var profile = new SKColorSpaceIccProfile ();
			SkiaApi.sk_colorspace_to_profile (Handle, profile.Handle);
			GC.KeepAlive (this);
			return profile;
		}

		// ToColorSpaceXyz

		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				var result = SkiaApi.sk_colorspace_to_xyzd50 (Handle, xyz);
				GC.KeepAlive (this);
				return result;
			}
		}

		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// To*Gamma

		public SKColorSpace ToLinearGamma ()
		{
			var result = GetObject (SkiaApi.sk_colorspace_make_linear_gamma (Handle));
			GC.KeepAlive (this);
			return result;
		}

		public SKColorSpace ToSrgbGamma ()
		{
			var result = GetObject (SkiaApi.sk_colorspace_make_srgb_gamma (Handle));
			GC.KeepAlive (this);
			return result;
		}

		//

		internal static SKColorSpace GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKColorSpace (h, o));

		// Variant used by singleton accessors. The returned wrapper has IgnorePublicDispose
		// set under HandleDictionary's critical section — atomic with the HD lookup, so
		// no other thread can observe a non-dispose-protected state.
		internal static SKColorSpace GetDisposeProtectedObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddDisposeProtectedObject (handle, owns, unrefExisting, (h, o) => new SKColorSpace (h, o));
	}
}
