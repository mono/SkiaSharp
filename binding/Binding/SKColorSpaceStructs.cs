using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKColorSpaceTransferFn instead.")]
	public enum SKColorSpaceRenderTargetGamma
	{
		Linear = 0,
		Srgb = 1,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKColorSpaceXyz instead.")]
	public enum SKColorSpaceGamut
	{
		AdobeRgb = 1,
		Dcip3D65 = 2,
		Rec2020 = 3,
		Srgb = 0,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete]
	public enum SKColorSpaceType
	{
		Cmyk = 1,
		Gray = 2,
		Rgb = 0,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKColorSpaceTransferFn instead.")]
	public enum SKNamedGamma
	{
		Linear = 0,
		Srgb = 1,
		TwoDotTwoCurve = 2,
		NonStandard = 3,
	}

	public partial class SkiaExtensions
	{
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKColorSpaceRenderTargetGamma gamma) =>
			gamma switch {
				SKColorSpaceRenderTargetGamma.Linear => SKColorSpaceTransferFn.Linear,
				SKColorSpaceRenderTargetGamma.Srgb => SKColorSpaceTransferFn.Srgb,
				_ => throw new ArgumentOutOfRangeException (nameof (gamma)),
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKNamedGamma gamma) =>
			gamma switch {
				SKNamedGamma.Linear => SKColorSpaceTransferFn.Linear,
				SKNamedGamma.Srgb => SKColorSpaceTransferFn.Srgb,
				SKNamedGamma.TwoDotTwoCurve => SKColorSpaceTransferFn.TwoDotTwo,
				SKNamedGamma.NonStandard => SKColorSpaceTransferFn.Empty,
				_ => throw new ArgumentOutOfRangeException (nameof (gamma)),
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKColorSpaceXyz ToColorSpaceXyz (this SKColorSpaceGamut gamut) =>
			gamut switch {
				SKColorSpaceGamut.AdobeRgb => SKColorSpaceXyz.AdobeRgb,
				SKColorSpaceGamut.Dcip3D65 => SKColorSpaceXyz.Dcip3,
				SKColorSpaceGamut.Rec2020 => SKColorSpaceXyz.Rec2020,
				SKColorSpaceGamut.Srgb => SKColorSpaceXyz.Srgb,
				_ => throw new ArgumentOutOfRangeException (nameof (gamut)),
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKColorSpaceXyz ToColorSpaceXyz (this SKMatrix44 matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException (nameof (matrix));

			var values = matrix.ToRowMajor ();
			return new SKColorSpaceXyz (
				values[0], values[1], values[2],
				values[4], values[5], values[6],
				values[8], values[9], values[10]);
		}
	}

	public unsafe partial struct SKColorSpacePrimaries
	{
		public static readonly SKColorSpacePrimaries Empty;

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

		public readonly float[] Values =>
			new[] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ToColorSpaceXyz() instead.")]
		public readonly SKMatrix44 ToXyzD50 () =>
			ToMatrix44 ();

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
		public readonly bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			var xyz = ToMatrix44 ();
			if (xyz != null)
				toXyzD50.SetColumnMajor (xyz.ToColumnMajor ());
			return xyz != null;
		}

		[Obsolete]
		internal readonly SKMatrix44 ToMatrix44 () =>
			ToMatrix44 (out var toXYZ) ? toXYZ : null;

		[Obsolete]
		internal readonly bool ToMatrix44 (out SKMatrix44 toXyzD50)
		{
			if (!ToColorSpaceXyz (out var xyz)) {
				toXyzD50 = null;
				return false;
			}

			toXyzD50 = xyz.ToMatrix44 ();
			return true;
		}

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

		public readonly float[] Values =>
			new[] { fG, fA, fB, fC, fD, fE, fF };

		public readonly SKColorSpaceTransferFn Invert ()
		{
			SKColorSpaceTransferFn inverted;
			fixed (SKColorSpaceTransferFn* t = &this) {
				SkiaApi.sk_colorspace_transfer_fn_invert (t, &inverted);
			}
			return inverted;
		}

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

		[Obsolete ("Use DisplayP3 instead.")]
		public static SKColorSpaceXyz Dcip3 => DisplayP3;

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

		public SKColorSpaceXyz (float value)
		{
			m11 = value;
			m12 = value;
			m13 = value;

			m21 = value;
			m22 = value;
			m23 = value;

			m31 = value;
			m32 = value;
			m33 = value;
		}

		public SKColorSpaceXyz (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 9)
				throw new ArgumentException ("The matrix array must have a length of 9.", nameof (values));

			m11 = values[0];
			m12 = values[1];
			m13 = values[2];

			m21 = values[3];
			m22 = values[4];
			m23 = values[5];

			m31 = values[6];
			m32 = values[7];
			m33 = values[8];
		}

		public SKColorSpaceXyz (
			float m11, float m12, float m13,
			float m21, float m22, float m23,
			float m31, float m32, float m33)
		{
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;

			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;

			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
		}

		public float[] Values {
			readonly get => new float[9] {
				m11, m12, m13,
				m21, m22, m23,
				m31, m32, m33,
			};
			set {
				if (value.Length != 9)
					throw new ArgumentException ("The matrix array must have a length of 9.", nameof (value));

				m11 = value[0];
				m12 = value[1];
				m13 = value[2];

				m21 = value[3];
				m22 = value[4];
				m23 = value[5];

				m31 = value[6];
				m32 = value[7];
				m33 = value[8];
			}
		}

		public readonly float this[int x, int y] {
			get {
				if (x < 0 || x >= 3)
					throw new ArgumentOutOfRangeException (nameof (x));
				if (y < 0 || y >= 3)
					throw new ArgumentOutOfRangeException (nameof (y));

				var idx = x + (y * 3);
				return idx switch {
					0 => m11,
					1 => m12,
					2 => m13,
					3 => m21,
					4 => m22,
					5 => m23,
					6 => m31,
					7 => m32,
					8 => m33,
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

		public static SKColorSpaceXyz Concat (SKColorSpaceXyz a, SKColorSpaceXyz b)
		{
			SKColorSpaceXyz result;
			SkiaApi.sk_colorspace_xyz_concat (&a, &b, &result);
			return result;
		}

		public static SKColorSpaceXyz CreateIdentity() =>
			new (1, 0, 0,
				 0, 1, 0,
				 0, 0, 1);

		[Obsolete]
		internal readonly SKMatrix44 ToMatrix44 ()
		{
			var matrix = new SKMatrix44 ();
			matrix.Set3x3RowMajor (Values);
			return matrix;
		}

		internal readonly SKMatrix4x4 ToMatrix4x4 () =>
			new (m11, m12, m13, 0,
				 m21, m22, m23, 0,
				 m31, m32, m33, 0,
				 0, 0, 0, 1);
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

		public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50)
		{
			fixed (SKColorSpaceXyz* xyz = &toXyzD50) {
				return SkiaApi.sk_colorspace_icc_profile_get_to_xyzd50 (Handle, xyz);
			}
		}

		public SKColorSpaceXyz ToColorSpaceXyz () =>
			ToColorSpaceXyz (out var toXYZ) ? toXYZ : SKColorSpaceXyz.Empty;

		// Create

		public static SKColorSpaceIccProfile Create (byte[] data) =>
			Create (data.AsSpan ());

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

		public static SKColorSpaceIccProfile Create (SKData data)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (data.IsEmpty)
				return null;

			return Referenced (Create (data.Data, data.Size), data);
		}

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
