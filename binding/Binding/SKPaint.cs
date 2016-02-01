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
		internal SKPaint (IntPtr handle)
			: base (handle)
		{
		}
		
		public SKPaint ()
			: this (SkiaApi.sk_paint_new ())
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
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

		public bool IsStroke {
			get {
				return SkiaApi.sk_paint_is_stroke (Handle);
			}
			set {
				SkiaApi.sk_paint_set_stroke (Handle, value);
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
				return GetObject<SKShader>(SkiaApi.sk_paint_get_shader(Handle));
			}
			set {
				SkiaApi.sk_paint_set_shader(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public SKMaskFilter MaskFilter {
			get {
				return GetObject<SKMaskFilter>(SkiaApi.sk_paint_get_maskfilter(Handle));
			}
			set {
				SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public SKColorFilter ColorFilter {
			get {
				return GetObject<SKColorFilter>(SkiaApi.sk_paint_get_colorfilter(Handle));
			}
			set {
				SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}
		
		public SKImageFilter ImageFilter {
			get {
				return GetObject<SKImageFilter>(SkiaApi.sk_paint_get_imagefilter(Handle));
			}
			set {
				SkiaApi.sk_paint_set_imagefilter(Handle, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public SKXferMode XferMode {
			get {
				return SkiaApi.sk_paint_get_xfermode_mode(Handle);
			}
			set {
				SkiaApi.sk_paint_set_xfermode_mode (Handle, value);
			}
		}

		public SKTypeface Typeface {
			get {
				return GetObject<SKTypeface> (SkiaApi.sk_paint_get_typeface (Handle));
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

	}
}

