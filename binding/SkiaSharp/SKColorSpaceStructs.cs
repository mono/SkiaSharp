#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Describes a color gamut with primaries and a white point.</summary>
	/// <remarks />
	public unsafe partial struct SKColorSpacePrimaries
	{
		/// <summary>Represents an empty <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> with all values set to zero.</summary>
		/// <remarks />
		public static readonly SKColorSpacePrimaries Empty;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> structure with the specified values.</summary>
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpacePrimaries" /> structure with the specified coordinates.</summary>
		/// <param name="rx">The red X-coordinate.</param>
		/// <param name="ry">The red Y-coordinate.</param>
		/// <param name="gx">The green X-coordinate.</param>
		/// <param name="gy">The green Y-coordinate.</param>
		/// <param name="bx">The blue X-coordinate.</param>
		/// <param name="by">The blue Y-coordinate.</param>
		/// <param name="wx">The white X-coordinate.</param>
		/// <param name="wy">The white Y-coordinate.</param>
		/// <remarks />
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

		/// <summary>Gets the values of the primaries and white point as an array with the form [RX, RY, GX, GY, BX, BY, WX, WY].</summary>
		/// <value>An array containing the primaries and white point values.</value>
		/// <remarks />
		public readonly float[] Values =>
			new[] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

		/// <summary>Attempts to convert the primaries and white point to an <see cref="T:SkiaSharp.SKColorSpaceXyz" /> matrix.</summary>
		/// <param name="toXyzD50">When this method returns, contains the XYZ D50 matrix if the conversion succeeded.</param>
		/// <returns><see langword="true" /> if the conversion succeeded; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpacePrimaries* t = &this)
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_primaries_to_xyzd50 (t, xyz);
			}
		}

		/// <summary>Converts the primaries and white point to an <see cref="T:SkiaSharp.SKColorSpaceXyz" /> matrix.</summary>
		/// <returns>The XYZ D50 matrix, or <see cref="F:SkiaSharp.SKColorSpaceXyz.Empty" /> if the conversion is not possible.</returns>
		/// <remarks />
		public readonly SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;
	}

	/// <summary>Represents the coefficients for a common transfer function equation.</summary>
	/// <remarks><para>The coefficients are specified as a transformation from a curved space to linear.</para><para></para><para>LinearVal = C*InputVal + F;   (for 0.0f &lt;= InputVal &lt; D)</para><para>LinearVal = (A*InputVal + B)^G + E;   (for D &lt;= InputVal &lt;= 1.0f)</para><para></para><para>Function is undefined if InputVal is not in [ 0.0f, 1.0f ].</para><para>Resulting LinearVals must be in [ 0.0f, 1.0f ].</para><para>Function must be positive and increasing.</para></remarks>
	public unsafe partial struct SKColorSpaceTransferFn
	{
		/// <summary>Gets the transfer function for the sRGB color space.</summary>
		/// <value>The sRGB transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn Srgb {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_srgb (&fn);
				return fn;
			}
		}

		/// <summary>Gets the transfer function for a simple 2.2 gamma curve.</summary>
		/// <value>The 2.2 gamma transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn TwoDotTwo {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_2dot2 (&fn);
				return fn;
			}
		}

		/// <summary>Gets the transfer function for the linear color space.</summary>
		/// <value>The linear transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn Linear {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_linear (&fn);
				return fn;
			}
		}

		/// <summary>Gets the transfer function for the Rec. 2020 color space.</summary>
		/// <value>The Rec. 2020 transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn Rec2020 {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_rec2020 (&fn);
				return fn;
			}
		}

		/// <summary>Gets the transfer function for the PQ (Perceptual Quantizer) color space used in HDR content.</summary>
		/// <value>The PQ transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn Pq {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_pq (&fn);
				return fn;
			}
		}

		/// <summary>Gets the transfer function for the HLG (Hybrid Log-Gamma) color space.</summary>
		/// <value>The HLG transfer function coefficients.</value>
		/// <remarks />
		public static SKColorSpaceTransferFn Hlg {
			get {
				SKColorSpaceTransferFn fn;
				SkiaApi.sk_colorspace_transfer_fn_named_hlg (&fn);
				return fn;
			}
		}

		/// <summary>Represents an empty <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> with all coefficients set to zero.</summary>
		/// <remarks />
		public static readonly SKColorSpaceTransferFn Empty;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />.</summary>
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />.</summary>
		/// <param name="g">The G coefficient.</param>
		/// <param name="a">The A coefficient.</param>
		/// <param name="b">The B coefficient.</param>
		/// <param name="c">The C coefficient.</param>
		/// <param name="d">The D coefficient.</param>
		/// <param name="e">The E coefficient.</param>
		/// <param name="f">The F coefficient.</param>
		/// <remarks />
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

		/// <summary>Gets the coefficients as an array with the form [G, A, B, C, D, E, F].</summary>
		/// <value>An array of 7 float values containing the transfer function coefficients.</value>
		/// <remarks />
		public readonly float[] Values =>
			new[] { fG, fA, fB, fC, fD, fE, fF };

		/// <summary>Inverts coefficients for a common transfer function equation.</summary>
		/// <returns>Returns the mathematically inverted parametric transfer function equation.</returns>
		/// <remarks />
		public readonly SKColorSpaceTransferFn Invert ()
		{
			SKColorSpaceTransferFn inverted;
			fixed (SKColorSpaceTransferFn* t = &this) {
				SkiaApi.sk_colorspace_transfer_fn_invert (t, &inverted);
			}
			return inverted;
		}

		/// <summary>Transform a single input by this transfer function.</summary>
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

	/// <summary>Represents a 3x3 matrix used for color space transformation between RGB and XYZ coordinates.</summary>
	/// <remarks />
	public unsafe partial struct SKColorSpaceXyz
	{
		/// <summary>Gets the transformation matrix for the sRGB color space.</summary>
		/// <value>The sRGB color space transformation matrix.</value>
		/// <remarks />
		public static SKColorSpaceXyz Srgb {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_srgb (&xyz);
				return xyz;
			}
		}

		/// <summary>Gets the transformation matrix for the Adobe RGB color space.</summary>
		/// <value>The Adobe RGB color space transformation matrix.</value>
		/// <remarks />
		public static SKColorSpaceXyz AdobeRgb {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_adobe_rgb (&xyz);
				return xyz;
			}
		}

		/// <summary>Gets the transformation matrix for the Display P3 color space.</summary>
		/// <value>The Display P3 color space transformation matrix.</value>
		/// <remarks />
		public static SKColorSpaceXyz DisplayP3 {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_display_p3 (&xyz);
				return xyz;
			}
		}

		/// <summary>Gets the transformation matrix for the Rec. 2020 color space.</summary>
		/// <value>The Rec. 2020 color space transformation matrix.</value>
		/// <remarks />
		public static SKColorSpaceXyz Rec2020 {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_rec2020 (&xyz);
				return xyz;
			}
		}

		/// <summary>Gets the transformation matrix for the XYZ-D50 color space.</summary>
		/// <value>The XYZ-D50 color space transformation matrix.</value>
		/// <remarks />
		public static SKColorSpaceXyz Xyz {
			get {
				SKColorSpaceXyz xyz;
				SkiaApi.sk_colorspace_xyz_named_xyz (&xyz);
				return xyz;
			}
		}

		/// <summary>Represents an empty matrix with all values set to zero.</summary>
		/// <remarks />
		public static readonly SKColorSpaceXyz Empty;

		/// <summary>Represents the identity matrix.</summary>
		/// <remarks />
		public readonly static SKColorSpaceXyz Identity =
			new SKColorSpaceXyz(
				1, 0, 0,
				0, 1, 0,
				0, 0, 1);

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpaceXyz" /> struct with the specified value on the diagonal.</summary>
		/// <param name="value">The value to set for all diagonal elements of the matrix.</param>
		/// <remarks />
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpaceXyz" /> struct from an array of 9 values.</summary>
		/// <param name="values">An array of 9 float values representing the matrix in row-major order.</param>
		/// <remarks />
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpaceXyz" /> struct with the specified values.</summary>
		/// <param name="m00">The value at row 0, column 0.</param>
		/// <param name="m01">The value at row 0, column 1.</param>
		/// <param name="m02">The value at row 0, column 2.</param>
		/// <param name="m10">The value at row 1, column 0.</param>
		/// <param name="m11">The value at row 1, column 1.</param>
		/// <param name="m12">The value at row 1, column 2.</param>
		/// <param name="m20">The value at row 2, column 0.</param>
		/// <param name="m21">The value at row 2, column 1.</param>
		/// <param name="m22">The value at row 2, column 2.</param>
		/// <remarks />
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

		/// <summary>Gets or sets the matrix values as an array of 9 floats in row-major order.</summary>
		/// <value>The matrix values as a float array.</value>
		/// <remarks />
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

		/// <summary>Gets the value at the specified row and column.</summary>
		/// <param name="x">The row index (0-2).</param>
		/// <param name="y">The column index (0-2).</param>
		/// <value>The matrix value at the specified position.</value>
		/// <remarks />
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

		/// <summary>Computes the inverse of this matrix.</summary>
		/// <returns>The inverse of this matrix, or an empty matrix if the matrix is not invertible.</returns>
		/// <remarks />
		public readonly SKColorSpaceXyz Invert ()
		{
			SKColorSpaceXyz inverted;
			fixed (SKColorSpaceXyz* t = &this) {
				SkiaApi.sk_colorspace_xyz_invert (t, &inverted);
			}
			return inverted;
		}

		/// <summary>Concatenates two matrices by multiplying them together.</summary>
		/// <param name="a">The first matrix.</param>
		/// <param name="b">The second matrix.</param>
		/// <returns>The result of multiplying the two matrices.</returns>
		/// <remarks />
		public static SKColorSpaceXyz Concat (SKColorSpaceXyz a, SKColorSpaceXyz b)
		{
			SKColorSpaceXyz result;
			SkiaApi.sk_colorspace_xyz_concat (&a, &b, &result);
			return result;
		}
	}

	/// <summary>Represents an ICC color profile used to describe the color characteristics of a device or color space.</summary>
	/// <remarks />
	public unsafe class SKColorSpaceIccProfile : SKObject
	{
		internal SKColorSpaceIccProfile (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> class.</summary>
		/// <remarks />
		public SKColorSpaceIccProfile ()
			: this (SkiaApi.sk_colorspace_icc_profile_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKColorSpaceIccProfile instance.");
		}

		/// <summary>Releases the unmanaged resources associated with the ICC profile.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_colorspace_icc_profile_delete (Handle);

		// properties

		/// <summary>Gets the size of the ICC profile data in bytes.</summary>
		/// <value>The size of the ICC profile data in bytes.</value>
		/// <remarks />
		public long Size {
			get {
				uint size;
				SkiaApi.sk_colorspace_icc_profile_get_buffer (Handle, &size);
				return size;
			}
		}

		/// <summary>Gets a pointer to the raw ICC profile data buffer.</summary>
		/// <value>A pointer to the raw ICC profile data.</value>
		/// <remarks />
		public IntPtr Buffer =>
			(IntPtr)SkiaApi.sk_colorspace_icc_profile_get_buffer (Handle, null);

		// ToColorSpaceXyz

		/// <summary>Attempts to convert this ICC profile to an XYZ D50 color space matrix.</summary>
		/// <param name="toXyzD50">When this method returns, contains the XYZ D50 color space matrix.</param>
		/// <returns><see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_icc_profile_get_to_xyzd50 (Handle, xyz);
			}
		}

		/// <summary>Converts this ICC profile to an XYZ D50 color space matrix.</summary>
		/// <returns>The <see cref="T:SkiaSharp.SKColorSpaceXyz" /> matrix, or <see cref="F:SkiaSharp.SKColorSpaceXyz.Empty" /> if the conversion fails.</returns>
		/// <remarks />
		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// Create

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> from the specified byte array.</summary>
		/// <param name="data">The ICC profile data as a byte array.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" />, or <see langword="null" /> if the data is empty or invalid.</returns>
		/// <remarks />
		public static SKColorSpaceIccProfile Create (byte[] data) =>
			Create (data.AsSpan ());

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> from the specified span of bytes.</summary>
		/// <param name="data">The ICC profile data as a span of bytes.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" />, or <see langword="null" /> if the data is empty or invalid.</returns>
		/// <remarks />
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

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> from the specified data.</summary>
		/// <param name="data">The ICC profile data.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" />, or <see langword="null" /> if the data is empty or invalid.</returns>
		/// <remarks />
		public static SKColorSpaceIccProfile Create (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (data.IsEmpty)
				return null;

			return Referenced (Create (data.Data, data.Size), data);
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" /> from the specified pointer and length.</summary>
		/// <param name="data">A pointer to the ICC profile data.</param>
		/// <param name="length">The length of the data in bytes.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKColorSpaceIccProfile" />, or <see langword="null" /> if the data is invalid or length is less than or equal to zero.</returns>
		/// <remarks />
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
