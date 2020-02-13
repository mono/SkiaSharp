using System;

namespace SkiaSharp
{
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

		public readonly SKMatrix44 ToXyzD50 ()
		{
			var xyzD50 = new SKMatrix44 ();
			if (!ToXyzD50 (xyzD50)) {
				xyzD50.Dispose ();
				xyzD50 = null;
			}
			return xyzD50;
		}

		public readonly bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));

			fixed (SKColorSpacePrimaries* t = &this) {
				return SkiaApi.sk_colorspaceprimaries_to_xyzd50 (t, toXyzD50.Handle);
			}
		}
	}

	public unsafe partial struct SKColorSpaceTransferFn
	{
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
				return SkiaApi.sk_colorspace_transfer_fn_transform (t, x);
			}
		}
	}
}
