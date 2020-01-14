using System;

namespace SkiaSharp
{
	public unsafe class SKPaint : SKObject
	{
		[Preserve]
		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPaint ()
			: this (SkiaApi.sk_paint_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_paint_delete (Handle);

		public void Reset ()
		{
			SkiaApi.sk_paint_reset (Handle);
		}

		public bool IsAntialias {
			get => SkiaApi.sk_paint_is_antialias (Handle);
			set => SkiaApi.sk_paint_set_antialias (Handle, value);
		}

		public bool IsDither {
			get => SkiaApi.sk_paint_is_dither (Handle);
			set => SkiaApi.sk_paint_set_dither (Handle, value);
		}

		public bool IsStroke {
			get => Style != SKPaintStyle.Fill;
			set => Style = value ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
		}

		public SKPaintStyle Style {
			get => SkiaApi.sk_paint_get_style (Handle);
			set => SkiaApi.sk_paint_set_style (Handle, value);
		}

		public SKColor Color {
			get => SkiaApi.sk_paint_get_color (Handle);
			set => SkiaApi.sk_paint_set_color (Handle, (uint)value);
		}

		public float StrokeWidth {
			get => SkiaApi.sk_paint_get_stroke_width (Handle);
			set => SkiaApi.sk_paint_set_stroke_width (Handle, value);
		}

		public float StrokeMiter {
			get => SkiaApi.sk_paint_get_stroke_miter (Handle);
			set => SkiaApi.sk_paint_set_stroke_miter (Handle, value);
		}

		public SKStrokeCap StrokeCap {
			get => SkiaApi.sk_paint_get_stroke_cap (Handle);
			set => SkiaApi.sk_paint_set_stroke_cap (Handle, value);
		}

		public SKStrokeJoin StrokeJoin {
			get => SkiaApi.sk_paint_get_stroke_join (Handle);
			set => SkiaApi.sk_paint_set_stroke_join (Handle, value);
		}

		public SKShader Shader {
			get => GetObject<SKShader> (SkiaApi.sk_paint_get_shader (Handle));
			set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKMaskFilter MaskFilter {
			get => GetObject<SKMaskFilter> (SkiaApi.sk_paint_get_maskfilter (Handle));
			set => SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKColorFilter ColorFilter {
			get => GetObject<SKColorFilter> (SkiaApi.sk_paint_get_colorfilter (Handle));
			set => SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKImageFilter ImageFilter {
			get => GetObject<SKImageFilter> (SkiaApi.sk_paint_get_imagefilter (Handle));
			set => SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKBlendMode BlendMode {
			get => SkiaApi.sk_paint_get_blendmode (Handle);
			set => SkiaApi.sk_paint_set_blendmode (Handle, value);
		}

		public SKFilterQuality FilterQuality {
			get => SkiaApi.sk_paint_get_filter_quality (Handle);
			set => SkiaApi.sk_paint_set_filter_quality (Handle, value);
		}

		public SKPathEffect PathEffect {
			get => GetObject<SKPathEffect> (SkiaApi.sk_paint_get_path_effect (Handle));
			set => SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKPaint Clone () =>
			GetObject<SKPaint> (SkiaApi.sk_paint_clone (Handle));

		// GetFillPath

		public SKPath GetFillPath (SKPath src)
			=> GetFillPath (src, 1f);

		public SKPath GetFillPath (SKPath src, float resScale)
		{
			var dst = new SKPath ();

			if (GetFillPath (src, dst, resScale)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public SKPath GetFillPath (SKPath src, SKRect cullRect)
			=> GetFillPath (src, cullRect, 1f);

		public SKPath GetFillPath (SKPath src, SKRect cullRect, float resScale)
		{
			var dst = new SKPath ();

			if (GetFillPath (src, dst, cullRect, resScale)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, null, resScale);
		}

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, cullRect, 1f);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, &cullRect, resScale);
		}
	}
}
