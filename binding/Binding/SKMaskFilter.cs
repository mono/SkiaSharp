using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Flags]
	[Obsolete]
	public enum SKBlurMaskFilterFlags
	{
		None = 0x00,
		IgnoreTransform = 0x01,
		HighQuality = 0x02,
		All = IgnoreTransform | HighQuality,
	}


	// TODO: `getFormat`
	// TODO: `computeFastBounds`

	public unsafe class SKMaskFilter : SKObject, ISKReferenceCounted
	{
		private const float BlurSigmaScale = 0.57735f;
		public const int TableMaxLength = 256;

		[Preserve]
		internal SKMaskFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static float ConvertRadiusToSigma (float radius)
		{
			return radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;
		}

		public static float ConvertSigmaToRadius (float sigma)
		{
			return sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags)
		{
			return CreateBlur (blurStyle, sigma, SKRect.Empty, true);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder)
		{
			return CreateBlur (blurStyle, sigma, occluder, true);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateBlur(SKBlurStyle, float, SKRect) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags)
		{
			return CreateBlur (blurStyle, sigma, occluder, true);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, &occluder, respectCTM));
		}

		public static SKMaskFilter CreateTable (byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException (nameof (table));
			if (table.Length != TableMaxLength)
				throw new ArgumentException ("Table must have a length of {SKColorTable.MaxLength}.", nameof (table));
			fixed (byte* t = table) {
				return GetObject (SkiaApi.sk_maskfilter_new_table (t));
			}
		}

		public static SKMaskFilter CreateGamma (float gamma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_gamma (gamma));
		}

		public static SKMaskFilter CreateClip (byte min, byte max)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_clip (min, max));
		}

		internal static SKMaskFilter GetObject (IntPtr handle) => GetOrAddObject (handle, (h, o) => new SKMaskFilter (h, o));
	}
}

