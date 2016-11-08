//
// Bindings for SKPaint
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKPaint : SKObject
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
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_paint_delete (Handle);
			}

			base.Dispose (disposing);
		}
		
		public bool IsAntialias {
			get {
				return SkiaApi.sk_paint_is_antialias (Handle);
			}
			set {
				SkiaApi.sk_paint_set_antialias (Handle, value);
			}
		}

		public bool IsDither {
			get {
				return SkiaApi.sk_paint_is_dither (Handle);
			}
			set {
				SkiaApi.sk_paint_set_dither (Handle, value);
			}
		}

		public bool IsVerticalText {
			get {
				return SkiaApi.sk_paint_is_verticaltext (Handle);
			}
			set {
				SkiaApi.sk_paint_set_verticaltext (Handle, value);
			}
		}

		public bool IsLinearText {
			get {
				return SkiaApi.sk_paint_is_linear_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_linear_text (Handle, value);
			}
		}

		public bool SubpixelText {
			get {
				return SkiaApi.sk_paint_is_subpixel_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_subpixel_text (Handle, value);
			}
		}

		public bool LcdRenderText {
			get {
				return SkiaApi.sk_paint_is_lcd_render_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_lcd_render_text (Handle, value);
			}
		}

		public bool IsEmbeddedBitmapText {
			get {
				return SkiaApi.sk_paint_is_embedded_bitmap_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_embedded_bitmap_text (Handle, value);
			}
		}

		public bool IsAutohinted {
			get {
				return SkiaApi.sk_paint_is_autohinted (Handle);
			}
			set {
				SkiaApi.sk_paint_set_autohinted (Handle, value);
			}
		}

		public SKPaintHinting HintingLevel {
			get {
				return SkiaApi.sk_paint_get_hinting (Handle);
			}
			set {
				SkiaApi.sk_paint_set_hinting (Handle, value);
			}
		}

		public bool UnderlineText {
			get {
				return SkiaApi.sk_paint_is_underline_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_underline_text (Handle, value);
			}
		}

		public bool StrikeThruText {
			get {
				return SkiaApi.sk_paint_is_strikethru_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_strikethru_text (Handle, value);
			}
		}

		public bool FakeBoldText {
			get {
				return SkiaApi.sk_paint_is_fake_bold_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_fake_bold_text (Handle, value);
			}
		}

		public bool DeviceKerningEnabled {
			get {
				return SkiaApi.sk_paint_is_dev_kern_text (Handle);
			}
			set {
				SkiaApi.sk_paint_set_dev_kern_text (Handle, value);
			}
		}

		public bool IsStroke {
			get {
				return SkiaApi.sk_paint_get_style (Handle) != SKPaintStyle.Fill;
			}
			set {
				SkiaApi.sk_paint_set_style (Handle, value ? SKPaintStyle.Stroke : SKPaintStyle.Fill);
			}
		}

		public SKPaintStyle Style {
			get {
				return SkiaApi.sk_paint_get_style (Handle);
			}
			set {
				SkiaApi.sk_paint_set_style (Handle, value);
			}
		}

		public SKColor Color {
			get {
				return SkiaApi.sk_paint_get_color (Handle);
			}
			set {
				SkiaApi.sk_paint_set_color (Handle, value);
			}
		}
		
		public float StrokeWidth {
			get {
				return SkiaApi.sk_paint_get_stroke_width (Handle);
			}
			set {
				SkiaApi.sk_paint_set_stroke_width (Handle, value);
			}
		}

		public float StrokeMiter {
			get {
				return SkiaApi.sk_paint_get_stroke_miter (Handle);
			}
			set {
				SkiaApi.sk_paint_set_stroke_miter (Handle, value);
			}
		}

		public SKStrokeCap StrokeCap {
			get {
				return SkiaApi.sk_paint_get_stroke_cap (Handle);
			}
			set {
				SkiaApi.sk_paint_set_stroke_cap (Handle, value);
			}
		}

		public SKStrokeJoin StrokeJoin {
			get {
				return SkiaApi.sk_paint_get_stroke_join (Handle);
			}
			set {
				SkiaApi.sk_paint_set_stroke_join (Handle, value);
			}
		}

		public SKShader Shader {
			get {
				return GetObject<SKShader>(SkiaApi.sk_paint_get_shader(Handle), false);
			}
			set {
				SkiaApi.sk_paint_set_shader(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public SKMaskFilter MaskFilter {
			get {
				return GetObject<SKMaskFilter>(SkiaApi.sk_paint_get_maskfilter(Handle), false);
			}
			set {
				SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public SKColorFilter ColorFilter {
			get {
				return GetObject<SKColorFilter>(SkiaApi.sk_paint_get_colorfilter(Handle), false);
			}
			set {
				SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}
		
		public SKImageFilter ImageFilter {
			get {
				return GetObject<SKImageFilter>(SkiaApi.sk_paint_get_imagefilter(Handle), false);
			}
			set {
				SkiaApi.sk_paint_set_imagefilter(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		[Obsolete ("Use BlendMode instead.")]
		public SKXferMode XferMode {
			get {
				return (SKXferMode)BlendMode;
			}
			set {
				BlendMode = (SKBlendMode)value;
			}
		}

		public SKBlendMode BlendMode {
			get {
				return SkiaApi.sk_paint_get_blendmode(Handle);
			}
			set {
				SkiaApi.sk_paint_set_blendmode (Handle, value);
			}
		}

		public SKFilterQuality FilterQuality
		{
			get
			{
				return SkiaApi.sk_paint_get_filter_quality(Handle);
			}
			set
			{
				SkiaApi.sk_paint_set_filter_quality(Handle, value);
			}
		}

		public SKTypeface Typeface {
			get {
				return GetObject<SKTypeface> (SkiaApi.sk_paint_get_typeface (Handle), false);
			}
			set {
				SkiaApi.sk_paint_set_typeface (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public float TextSize {
			get {
				return SkiaApi.sk_paint_get_textsize (Handle);
			}
			set {
				SkiaApi.sk_paint_set_textsize (Handle, value);
			}
		}

		public SKTextAlign TextAlign {
			get {
				return SkiaApi.sk_paint_get_text_align (Handle);
			}
			set {
				SkiaApi.sk_paint_set_text_align (Handle, value);
			}
		}

		public SKTextEncoding TextEncoding {
			get {
				return SkiaApi.sk_paint_get_text_encoding (Handle);
			}
			set {
				SkiaApi.sk_paint_set_text_encoding (Handle, value);
			}
		}

		public float TextScaleX {
			get {
				return SkiaApi.sk_paint_get_text_scale_x (Handle);
			}
			set {
				SkiaApi.sk_paint_set_text_scale_x (Handle, value);
			}
		}

		public float TextSkewX {
			get {
				return SkiaApi.sk_paint_get_text_skew_x (Handle);
			}
			set {
				SkiaApi.sk_paint_set_text_skew_x (Handle, value);
			}
		}

		public SKPathEffect PathEffect {
			get {
				return GetObject<SKPathEffect> (SkiaApi.sk_paint_get_path_effect (Handle));
			}
			set {
				SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public float MeasureText (string text)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = Util.GetEncodedText (text, TextEncoding);
			return SkiaApi.sk_paint_measure_text (Handle, bytes, (IntPtr) bytes.Length, IntPtr.Zero);
		}

		public float MeasureText (IntPtr buffer, IntPtr length)
		{
			if (buffer == IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return SkiaApi.sk_paint_measure_text (Handle, buffer, length, IntPtr.Zero);
		}

		public float MeasureText (string text, ref SKRect bounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = Util.GetEncodedText (text, TextEncoding);
			return SkiaApi.sk_paint_measure_text(Handle, bytes, (IntPtr) bytes.Length, ref bounds);
		}

		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds)
		{
			if (buffer == IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return SkiaApi.sk_paint_measure_text (Handle, buffer, length, ref bounds);
		}

		public long BreakText (string text, float maxWidth)
		{
			float measuredWidth;
			return BreakText (text, maxWidth, out measuredWidth);
		}

		public long BreakText (string text, float maxWidth, out float measuredWidth)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			var bytes = Util.GetEncodedText (text, TextEncoding);
			return (long) SkiaApi.sk_paint_break_text (Handle, bytes, (IntPtr) bytes.Length, maxWidth, out measuredWidth);
		}


		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth)
		{
			if (buffer == IntPtr.Zero)
				throw new ArgumentNullException (nameof (buffer));

			return (long)SkiaApi.sk_paint_break_text (Handle, buffer, length, maxWidth, out measuredWidth);
		}

		public SKPath GetTextPath(string text, float x, float y)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			var bytes = Util.GetEncodedText (text, TextEncoding);
			return GetObject<SKPath>(SkiaApi.sk_paint_get_text_path(Handle, bytes, (IntPtr) bytes.Length, x, y));
		}

		public SKPath GetTextPath(IntPtr buffer, IntPtr length, float x, float y)
		{
			if (buffer == IntPtr.Zero)
				throw new ArgumentNullException(nameof(buffer));
			return GetObject<SKPath>(SkiaApi.sk_paint_get_text_path(Handle, buffer, length, x, y));

		}

		public SKPath GetTextPath(string text, SKPoint[] points)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			var bytes = Util.GetEncodedText (text, TextEncoding);
			return GetObject<SKPath>(SkiaApi.sk_paint_get_pos_text_path(Handle, bytes, (IntPtr) bytes.Length, points));
		}

		public SKPath GetTextPath(IntPtr buffer, IntPtr length, SKPoint[] points)
		{
			if (buffer == IntPtr.Zero)
				throw new ArgumentNullException(nameof(buffer));
			return GetObject<SKPath>(SkiaApi.sk_paint_get_pos_text_path(Handle, buffer, length, points));
		}

		public bool GetFillPath(SKPath src, SKPath dst, SKRect cullRect, float resScale = 1)
		{
			if (src == null)
				throw new ArgumentNullException(nameof(src));
			if (dst == null)
				throw new ArgumentNullException(nameof(dst));
			return SkiaApi.sk_paint_get_fill_path(Handle, src.Handle, dst.Handle, ref cullRect, resScale);
		}

		public bool GetFillPath(SKPath src, SKPath dst, float resScale = 1)
		{
			if (src == null)
				throw new ArgumentNullException(nameof(src));
			if (dst == null)
				throw new ArgumentNullException(nameof(dst));
			return SkiaApi.sk_paint_get_fill_path(Handle, src.Handle, dst.Handle, IntPtr.Zero, resScale);
		}

		public SKFontMetrics FontMetrics
		{
			get
			{
				SKFontMetrics metrics;
				SkiaApi.sk_paint_get_fontmetrics(Handle, out metrics, 0f);
				return metrics;
			}
		}

		public float FontSpacing => SkiaApi.sk_paint_get_fontmetrics (Handle, IntPtr.Zero, 0);

		public float GetFontMetrics (out SKFontMetrics metrics, float scale = 0f)
		{
			return SkiaApi.sk_paint_get_fontmetrics (Handle, out metrics, scale);
		}

		public SKPaint Clone()
		{
			return GetObject<SKPaint>(SkiaApi.sk_paint_clone(Handle));
		}
	}
}

