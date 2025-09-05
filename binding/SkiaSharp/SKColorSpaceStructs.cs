#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Describes a color gamut with primaries and a white point.
	/// </summary>
	public unsafe partial struct SKColorSpacePrimaries
	{
		public static readonly SKColorSpacePrimaries Empty;

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> instance.
		/// </summary>
		/// <param name="values">The values of the primaries and white point.</param>
		/// <remarks>There must be exactly 8 values in the array with the form [RX, RY, GX, GY, BX, BY, WX, WY].</remarks>
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

		/// <summary>
		/// Creates a new <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> instance.
		/// </summary>
		/// <param name="rx">The red X-coordinate.</param>
		/// <param name="ry">The red Y-coordinate.</param>
		/// <param name="gx">The green X-coordinate.</param>
		/// <param name="gy">The green Y-coordinate.</param>
		/// <param name="bx">The blue X-coordinate.</param>
		/// <param name="by">The blue Y-coordinate.</param>
		/// <param name="wx">The white X-coordinate.</param>
		/// <param name="wy">The white Y-coordinate.</param>
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

		/// <summary>
		/// Gets the values of the primaries and white as an array with the form [RX, RY, GX, GY, BX, BY, WX, WY].
		/// </summary>
		/// <remarks>]</remarks>
		public readonly float[] Values =>
			new[] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

		/// <param name="toXyzD50"></param>
		public readonly bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpacePrimaries* t = &this)
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_primaries_to_xyzd50 (t, xyz);
			}
		}

		public readonly SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;
	}

	/// <summary>
	/// Represents the coefficients for a common transfer function equation.
	/// </summary>
	/// <remarks><para>The coefficients are specified as a transformation from a curved space to linear. </para><para></para><para>LinearVal = C*InputVal + F;   (for 0.0f &lt;= InputVal &lt; D)</para><para>LinearVal = (A*InputVal + B)^G + E;   (for D &lt;= InputVal &lt;= 1.0f)</para><para></para><para>Function is undefined if InputVal is not in [ 0.0f, 1.0f ].</para><para>Resulting LinearVals must be in [ 0.0f, 1.0f ].</para><para>Function must be positive and increasing.</para></remarks>
	public unsafe partial struct SKColorSpaceTransferFn
	{
		public static SKColorSpaceTransferFn Srgb {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_srgb (&fn);
				return fn;
			}
		}

		public static SKColorSpaceTransferFn TwoDotTwo {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_2dot2 (&fn);
				return fn;
			}
		}

		public static SKColorSpaceTransferFn Linear {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_linear (&fn);
				return fn;
			}
		}

		public static SKColorSpaceTransferFn Rec2020 {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_rec2020 (&fn);
				return fn;
			}
		}

		public static SKColorSpaceTransferFn Pq {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_pq (&fn);
				return fn;
			}
		}

		public static SKColorSpaceTransferFn Hlg {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_hlg (&fn);
				return fn;
			}
		}

		public static readonly SKColorSpaceTransferFn Empty;

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />.
		/// </summary>
		/// <param name="values">The values of the coefficients.</param>
		/// <remarks>There must be exactly 7 values in the array with the form [G, A, B, C, D, E, F].</remarks>
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

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />.
		/// </summary>
		/// <param name="g">The G coefficient.</param>
		/// <param name="a">The A coefficient.</param>
		/// <param name="b">The B coefficient.</param>
		/// <param name="c">The C coefficient.</param>
		/// <param name="d">The D coefficient.</param>
		/// <param name="e">The E coefficient.</param>
		/// <param name="f">The F coefficient.</param>
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

		/// <summary>
		/// Gets the coefficients as an array with the form [G, A, B, C, D, E, F].
		/// </summary>
		public readonly float[] Values =>
			new[] { fG, fA, fB, fC, fD, fE, fF };

		/// <summary>
		/// Inverts coefficients for a common transfer function equation.
		/// </summary>
		/// <returns>Returns the mathematically inverted parametric transfer function equation.</returns>
		public readonly SKColorSpaceTransferFn Invert ()
		{
			SKColorSpaceTransferFn inverted;
			fixed (SKColorSpaceTransferFn* t = &this) {
				SkiaApi.sk_colorspace_transfer_fn_invert (t, &inverted);
			}
			return inverted;
		}

		/// <summary>
		/// Transform a single input by this transfer function.
		/// </summary>
		/// <param name="x">The input to transform.</param>
		/// <returns>Returns the transformed input.</returns>
		/// <remarks>For negative inputs, returns `-Transform(Math.Abs(x))`.</remarks>
		public readonly float Transform (float x)
		{
			fixed (SKColorSpaceTransferFn* t = &this) {
				return SkiaApi.sk_colorspace_transfer_fn_eval (t, x);
			}
		}
	}

	public unsafe partial struct SKColorSpaceXyz
	{
		public static SKColorSpaceXyz Srgb {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_srgb (&xyz);
				return xyz;
			}
		}

		public static SKColorSpaceXyz AdobeRgb {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_adobe_rgb (&xyz);
				return xyz;
			}
		}

		public static SKColorSpaceXyz DisplayP3 {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_display_p3 (&xyz);
				return xyz;
			}
		}

		public static SKColorSpaceXyz Rec2020 {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_rec2020 (&xyz);
				return xyz;
			}
		}

		public static SKColorSpaceXyz Xyz {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_xyz (&xyz);
				return xyz;
			}
		}

		public static readonly SKColorSpaceXyz Empty;

		public readonly static SKColorSpaceXyz Identity =
			new SKColorSpaceXyz(
				1, 0, 0,
				0, 1, 0,
				0, 0, 1);

		/// <param name="value"></param>
		public SKColorSpaceXyz (float value)
		{
			fM00 = value;
			fM01 = value;
			fM02 = value;

			fM10 = value;
			fM11 = value;
			fM12 = value;

			fM20 = value;
			fM21 = value;
			fM22 = value;
		}

		/// <param name="values"></param>
		public SKColorSpaceXyz (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 9)
				throw new ArgumentException ("The matrix array must have a length of 9.", nameof (values));

			fM00 = values[0];
			fM01 = values[1];
			fM02 = values[2];

			fM10 = values[3];
			fM11 = values[4];
			fM12 = values[5];

			fM20 = values[6];
			fM21 = values[7];
			fM22 = values[8];
		}

		/// <param name="m00"></param>
		/// <param name="m01"></param>
		/// <param name="m02"></param>
		/// <param name="m10"></param>
		/// <param name="m11"></param>
		/// <param name="m12"></param>
		/// <param name="m20"></param>
		/// <param name="m21"></param>
		/// <param name="m22"></param>
		public SKColorSpaceXyz (
			float m00, float m01, float m02,
			float m10, float m11, float m12,
			float m20, float m21, float m22)
		{
			fM00 = m00;
			fM01 = m01;
			fM02 = m02;

			fM10 = m10;
			fM11 = m11;
			fM12 = m12;

			fM20 = m20;
			fM21 = m21;
			fM22 = m22;
		}

		public float[] Values {
			readonly get => new float[9] {
				fM00, fM01, fM02,
				fM10, fM11, fM12,
				fM20, fM21, fM22,
			};
			set {
				if (value.Length != 9)
					throw new ArgumentException ("The matrix array must have a length of 9.", nameof (value));

				fM00 = value[0];
				fM01 = value[1];
				fM02 = value[2];

				fM10 = value[3];
				fM11 = value[4];
				fM12 = value[5];

				fM20 = value[6];
				fM21 = value[7];
				fM22 = value[8];
			}
		}

		/// <param name="x"></param>
		/// <param name="y"></param>
		public readonly float this[int x, int y] {
			get {
				if (x < 0 || x >= 3)
					throw new ArgumentOutOfRangeException (nameof (x));
				if (y < 0 || y >= 3)
					throw new ArgumentOutOfRangeException (nameof (y));

				var idx = x + (y * 3);
				return idx switch
				{
					0 => fM00,
					1 => fM01,
					2 => fM02,
					3 => fM10,
					4 => fM11,
					5 => fM12,
					6 => fM20,
					7 => fM21,
					8 => fM22,
					_ => throw new ArgumentOutOfRangeException ("index")
				};
			}
		}

		public readonly SKColorSpaceXyz Invert ()
		{
			SKColorSpaceXyz inverted;
			fixed (SKColorSpaceXyz* t = &this) {
				SkiaApi.sk_colorspace_xyz_invert (t, &inverted);
			}
			return inverted;
		}

		/// <param name="a"></param>
		/// <param name="b"></param>
		public static SKColorSpaceXyz Concat (SKColorSpaceXyz a, SKColorSpaceXyz b)
		{
			SKColorSpaceXyz result;
			SkiaApi.sk_colorspace_xyz_concat (&a, &b, &result);
			return result;
		}
	}

	public unsafe class SKColorSpaceIccProfile : SKObject
	{
		internal SKColorSpaceIccProfile (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKColorSpaceIccProfile ()
			: this (SkiaApi.sk_colorspace_icc_profile_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKColorSpaceIccProfile instance.");
		}

		protected override void DisposeNative () =>
			SkiaApi.sk_colorspace_icc_profile_delete (Handle);

		// properties

		public long Size {
			get {
				uint size;
				SkiaApi.sk_colorspace_icc_profile_get_buffer (Handle, &size);
				return size;
			}
		}

		public IntPtr Buffer =>
			(IntPtr)SkiaApi.sk_colorspace_icc_profile_get_buffer (Handle, null);

		// ToColorSpaceXyz

		/// <param name="toXyzD50"></param>
		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_icc_profile_get_to_xyzd50 (Handle, xyz);
			}
		}

		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// Create

		/// <param name="data"></param>
		public static SKColorSpaceIccProfile Create (byte[] data) =>
			Create (data.AsSpan ());

		/// <param name="data"></param>
		public static SKColorSpaceIccProfile Create (ReadOnlySpan<byte> data)
		{
			if (data.IsEmpty)
				return null;

			var skData = SKData.CreateCopy (data);
			var icc = Create (skData);
			if (icc == null)
				skData.Dispose ();
			return icc;
		}

		/// <param name="data"></param>
		public static SKColorSpaceIccProfile Create (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (data.IsEmpty)
				return null;

			return Referenced (Create (data.Data, data.Size), data);
		}

		/// <param name="data"></param>
		/// <param name="length"></param>
		public static SKColorSpaceIccProfile Create (IntPtr data, long length)
		{
			if (data == IntPtr.Zero)
				throw new ArgumentNullException (nameof (data));

			if (length <= 0)
				return null;

			var icc = new SKColorSpaceIccProfile ();
			if (!SkiaApi.sk_colorspace_icc_profile_parse ((void*)data, (IntPtr)length, icc.Handle)) {
				icc.Dispose ();
				icc = null;
			}
			return icc;
		}
	}
}
