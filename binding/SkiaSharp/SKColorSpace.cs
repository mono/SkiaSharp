#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a color space.
	/// </summary>
	public unsafe class SKColorSpace : SKObject, ISKNonVirtualReferenceCounted
	{
		private static readonly SKColorSpace srgb;
		private static readonly SKColorSpace srgbLinear;

		static SKColorSpace ()
		{
			// TODO: This is not the best way to do this as it will create a lot of objects that
			//       might not be needed, but it is the only way to ensure that the static
			//       instances are created before any access is made to them.
			//       See more info: SKObject.EnsureStaticInstanceAreInitialized()

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

		/// <summary>
		/// Gets a value indicating whether or not the color space gamma is near enough to be approximated as sRGB.
		/// </summary>
		public bool GammaIsCloseToSrgb =>
			SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);

		/// <summary>
		/// Gets a value indicating whether or not the color space gamma is linear.
		/// </summary>
		public bool GammaIsLinear =>
			SkiaApi.sk_colorspace_gamma_is_linear (Handle);

		/// <summary>
		/// Gets a value indicating whether or not the color space is sRGB.
		/// </summary>
		public bool IsSrgb =>
			SkiaApi.sk_colorspace_is_srgb (Handle);

		/// <summary>
		/// Gets a value indicating whether the transfer function can be represented as coefficients to the standard equation.
		/// </summary>
		public bool IsNumericalTransferFunction =>
			GetNumericalTransferFunction (out _);

		/// <summary>
		/// Compare two color spaces to determine if they are equivalent.
		/// </summary>
		/// <param name="left">The first color space.</param>
		/// <param name="right">The second color space.</param>
		/// <returns>Returns <see langword="true" /> if both color spaces are equivalent, otherwise <see langword="false" />.</returns>
		public static bool Equal (SKColorSpace left, SKColorSpace right)
		{
			if (left == null)
				throw new ArgumentNullException (nameof (left));
			if (right == null)
				throw new ArgumentNullException (nameof (right));

			return SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
		}

		// CreateSrgb

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> that represents the sRGB color space.
		/// </summary>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		public static SKColorSpace CreateSrgb () => srgb;

		// CreateSrgbLinear

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> with the sRGB primaries, but a linear (1.0) gamma
		/// </summary>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		public static SKColorSpace CreateSrgbLinear () => srgbLinear;

		// CreateIcc

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.
		/// </summary>
		/// <param name="input">The ICC profile data.</param>
		/// <param name="length">The size of the data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		public static SKColorSpace CreateIcc (IntPtr input, long length) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input, length));

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.
		/// </summary>
		/// <param name="input">The ICC profile data.</param>
		/// <param name="length">The size of the data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		public static SKColorSpace CreateIcc (byte[] input, long length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			fixed (byte* i = input) {
				return CreateIcc (SKColorSpaceIccProfile.Create ((IntPtr)i, length));
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpace" /> from an ICC profile.
		/// </summary>
		/// <param name="input">The ICC profile data.</param>
		/// <returns>Returns the new instance of <see cref="T:SkiaSharp.SKColorSpace" />.</returns>
		public static SKColorSpace CreateIcc (byte[] input) =>
			CreateIcc (input.AsSpan ());

		/// <param name="input"></param>
		public static SKColorSpace CreateIcc (ReadOnlySpan<byte> input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		/// <param name="input"></param>
		public static SKColorSpace CreateIcc (SKData input) =>
			CreateIcc (SKColorSpaceIccProfile.Create (input));

		/// <param name="profile"></param>
		public static SKColorSpace CreateIcc (SKColorSpaceIccProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException (nameof (profile));

			return Referenced (GetObject (SkiaApi.sk_colorspace_new_icc (profile.Handle)), profile);
		}

		// CreateRgb

		/// <param name="transferFn"></param>
		/// <param name="toXyzD50"></param>
		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50) =>
			GetObject (SkiaApi.sk_colorspace_new_rgb (&transferFn, &toXyzD50));

		// GetNumericalTransferFunction

		public SKColorSpaceTransferFn GetNumericalTransferFunction () =>
			GetNumericalTransferFunction (out var fn) ? fn : SKColorSpaceTransferFn.Empty;

		/// <summary>
		/// Returns the values of the coefficients to the standard equation.
		/// </summary>
		/// <param name="fn">The values of the coefficients to the standard equation.</param>
		/// <returns>Returns <see langword="true" /> if transfer function can be represented as coefficients to the standard equation, otherwise <see langword="false" />.</returns>
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

		/// <param name="toXyzD50"></param>
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
