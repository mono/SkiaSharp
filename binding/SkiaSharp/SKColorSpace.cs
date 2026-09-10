#nullable disable

using System;
using System.ComponentModel;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents a color space.</summary>
	/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKColorSpace" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKColorSpace" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// properties

		/// <summary>Gets a value indicating whether or not the color space gamma is near enough to be approximated as sRGB.</summary>
		/// <value><see langword="true" /> if the gamma is close to sRGB; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool GammaIsCloseToSrgb {
			get {
				var result = SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether or not the color space gamma is linear.</summary>
		/// <value><see langword="true" /> if the gamma is linear; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool GammaIsLinear {
			get {
				var result = SkiaApi.sk_colorspace_gamma_is_linear (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether or not the color space is sRGB.</summary>
		/// <value><see langword="true" /> if the color space is sRGB; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsSrgb {
			get {
				var result = SkiaApi.sk_colorspace_is_srgb (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether the transfer function can be represented as coefficients to the standard equation.</summary>
		/// <value><see langword="true" /> if the transfer function is numerical; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsNumericalTransferFunction =>
			GetNumericalTransferFunction (out _);

		/// <summary>Compare two color spaces to determine if they are equivalent.</summary>
		/// <param name="left">The first color space.</param>
		/// <param name="right">The second color space.</param>
		/// <returns>Returns <see langword="true" /> if both color spaces are equivalent, otherwise <see langword="false" />.</returns>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> that represents the sRGB color space.</summary>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		/// <remarks />
		public static SKColorSpace CreateSrgb () =>
			LazyInitializer.EnsureInitialized (
				ref srgb, ref srgbInitialized, ref srgbLock,
				// Immortal Skia singleton (sk_srgb_singleton, function-local static) — never unref it.
				// See SKColorFilter.GetDisposeProtectedObject for the full teardown-crash rationale.
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb (), owns: false, unrefExisting: false));

		// CreateSrgbLinear

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> with the sRGB primaries, but a linear (1.0) gamma.</summary>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		/// <remarks />
		public static SKColorSpace CreateSrgbLinear () =>
			LazyInitializer.EnsureInitialized (
				ref srgbLinear, ref srgbLinearInitialized, ref srgbLinearLock,
				// Immortal Skia singleton (sk_srgb_linear_singleton, function-local static) — never unref it.
				() => GetDisposeProtectedObject (SkiaApi.sk_colorspace_new_srgb_linear (), owns: false, unrefExisting: false));

		// CreateIcc

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="input">The ICC profile data.</param>
		/// <param name="length">The size of the data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (IntPtr input, long length) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input, length));

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="input">The ICC profile data.</param>
		/// <param name="length">The size of the data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (byte[] input, long length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			fixed (byte* i = input) {
				return CreateIcc (SKColorSpaceIccProfile.Create ((IntPtr)i, length));
			}
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="input">The ICC profile data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (byte[] input) =>
			CreateIcc (input.AsSpan ());

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="input">The ICC profile data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" />, or <see langword="null" /> if the data is invalid.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (ReadOnlySpan<byte> input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="input">The ICC profile data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" />, or <see langword="null" /> if the data is invalid.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (SKData input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.</summary>
		/// <param name="profile">The ICC color profile.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" />, or <see langword="null" /> if the profile is invalid.</returns>
		/// <remarks />
		public static SKColorSpace CreateIcc (SKColorSpaceIccProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException (nameof (profile));

			return Referenced (GetObject (SkiaApi.sk_colorspace_new_icc (profile.Handle)), profile);
		}

		// CreateRgb

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from a transfer function and transformation matrix.</summary>
		/// <param name="transferFn">The transfer function for the color space.</param>
		/// <param name="toXyzD50">The transformation matrix to XYZ D50.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" />, or <see langword="null" /> if the parameters are invalid.</returns>
		/// <remarks />
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50) =>
			GetObject (SkiaApi.sk_colorspace_new_rgb (&transferFn, &toXyzD50));

		// CreateCicp

		/// <summary>Creates a new color space from the specified CICP color primaries and transfer function.</summary>
		/// <param name="colorPrimaries">One of the enumeration values that specifies the CICP color primaries.</param>
		/// <param name="transferCharacteristics">One of the enumeration values that specifies the CICP transfer function.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" />, or <see langword="null" /> if the specified CICP parameters do not define a supported color space.</returns>
		/// <remarks />
		public static SKColorSpace CreateCicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics) =>
			GetObject (SkiaApi.sk_colorspace_new_cicp (colorPrimaries, transferCharacteristics));

		// GetNumericalTransferFunction

		/// <summary>Returns the transfer function for this color space.</summary>
		/// <returns>The <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />, or <see cref="F:SkiaSharp.SKColorSpaceTransferFn.Empty" /> if the transfer function cannot be represented numerically.</returns>
		/// <remarks />
		public SKColorSpaceTransferFn GetNumericalTransferFunction () =>
			GetNumericalTransferFunction (out var fn) ? fn : SKColorSpaceTransferFn.Empty;

		/// <summary>Returns the values of the coefficients to the standard equation.</summary>
		/// <param name="fn">The values of the coefficients to the standard equation.</param>
		/// <returns>Returns <see langword="true" /> if transfer function can be represented as coefficients to the standard equation, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public bool GetNumericalTransferFunction (out SKColorSpaceTransferFn fn)
		{
			fixed (SKColorSpaceTransferFn* f = &fn) {
				var result = SkiaApi.sk_colorspace_is_numerical_transfer_fn (Handle, f);
				GC.KeepAlive (this);
				return result;
			}
		}

		// ToProfile

		/// <summary>Converts this color space to an ICC profile.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> representing this color space.</returns>
		/// <remarks />
		public SKColorSpaceIccProfile ToProfile ()
		{
			var profile = new SKColorSpaceIccProfile ();
			SkiaApi.sk_colorspace_to_profile (Handle, profile.Handle);
			GC.KeepAlive (this);
			return profile;
		}

		// ToColorSpaceXyz

		/// <summary>Attempts to get the XYZ D50 transformation matrix for this color space.</summary>
		/// <param name="toXyzD50">When this method returns, contains the XYZ D50 transformation matrix.</param>
		/// <returns><see langword="true" /> if the transformation matrix was retrieved successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				var result = SkiaApi.sk_colorspace_to_xyzd50 (Handle, xyz);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Returns the XYZ D50 transformation matrix for this color space.</summary>
		/// <returns>The <see cref="T:SkiaSharp.SKColorSpaceXyz" />, or <see cref="F:SkiaSharp.SKColorSpaceXyz.Empty" /> if the transformation cannot be computed.</returns>
		/// <remarks />
		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// To*Gamma

		/// <summary>Creates a new color space with the same gamut as this one, but with a linear gamma.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" /> with linear gamma.</returns>
		/// <remarks />
		public SKColorSpace ToLinearGamma ()
		{
			var result = GetObject (SkiaApi.sk_colorspace_make_linear_gamma (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Creates a new color space with the same gamut as this one, but with sRGB gamma.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpace" /> with sRGB gamma.</returns>
		/// <remarks />
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
