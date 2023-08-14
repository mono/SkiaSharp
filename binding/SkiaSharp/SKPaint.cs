#nullable disable

using System;

namespace SkiaSharp
{
	[Obsolete ($"Use {nameof (SKFontHinting)} instead.")]
	public enum SKPaintHinting
	{
		NoHinting = 0,
		Slight = 1,
		Normal = 2,
		Full = 3,
	}

	[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.")]
	public enum SKFilterQuality
	{
		None = 0,
		Low = 1,
		Medium = 2,
		High = 3,
	}

	public static partial class SkiaExtensions
	{
		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.")]
		public static SKSamplingOptions ToSamplingOptions (this SKFilterQuality quality) =>
			quality switch {
				SKFilterQuality.None => new SKSamplingOptions (SKFilterMode.Nearest, SKMipmapMode.None),
				SKFilterQuality.Low => new SKSamplingOptions (SKFilterMode.Linear, SKMipmapMode.None),
				SKFilterQuality.Medium => new SKSamplingOptions (SKFilterMode.Linear, SKMipmapMode.Linear),
				SKFilterQuality.High => new SKSamplingOptions (SKCubicResampler.Mitchell),
				_ => throw new ArgumentOutOfRangeException (nameof (quality), $"Unknown filter quality: '{quality}'"),
			};
	}

	public unsafe class SKPaint : SKObject, ISKSkipObjectRegistration
	{
		[Obsolete]
		private SKFont font;

		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPaint ()
			: this (SkiaApi.sk_compatpaint_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		[Obsolete ($"Use {nameof (SKFont)} instead.")]
		public SKPaint (SKFont font)
			: this (IntPtr.Zero, true)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			Handle = SkiaApi.sk_compatpaint_new_with_font (font.Handle);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_compatpaint_delete (Handle);

		// Reset

		public void Reset () =>
			SkiaApi.sk_compatpaint_reset (Handle);

		// properties

		public bool IsAntialias {
			get => SkiaApi.sk_paint_is_antialias (Handle);
			set => SkiaApi.sk_compatpaint_set_is_antialias (Handle, value);
		}

		public bool IsDither {
			get => SkiaApi.sk_paint_is_dither (Handle);
			set => SkiaApi.sk_paint_set_dither (Handle, value);
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.LinearMetrics)} instead.")]
		public bool IsLinearText {
			get => GetFont ().LinearMetrics;
			set => GetFont ().LinearMetrics = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Subpixel)} instead.")]
		public bool SubpixelText {
			get => GetFont ().Subpixel;
			set => GetFont ().Subpixel = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Edging)} instead.")]
		public bool LcdRenderText {
			get => SkiaApi.sk_compatpaint_get_lcd_render_text (Handle);
			set => SkiaApi.sk_compatpaint_set_lcd_render_text (Handle, value);
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.EmbeddedBitmaps)} instead.")]
		public bool IsEmbeddedBitmapText {
			get => GetFont ().EmbeddedBitmaps;
			set => GetFont ().EmbeddedBitmaps = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ForceAutoHinting)} instead.")]
		public bool IsAutohinted {
			get => GetFont ().ForceAutoHinting;
			set => GetFont ().ForceAutoHinting = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Hinting)} instead.")]
		public SKPaintHinting HintingLevel {
			get => (SKPaintHinting)GetFont ().Hinting;
			set => GetFont ().Hinting = (SKFontHinting)value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Embolden)} instead.")]
		public bool FakeBoldText {
			get => GetFont ().Embolden;
			set => GetFont ().Embolden = value;
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

		public SKColorF ColorF {
			get {
				SKColorF color4f;
				SkiaApi.sk_paint_get_color4f (Handle, &color4f);
				return color4f;
			}
			set => SkiaApi.sk_paint_set_color4f (Handle, &value, IntPtr.Zero);
		}

		public void SetColor (SKColorF color, SKColorSpace colorspace) =>
			SkiaApi.sk_paint_set_color4f (Handle, &color, colorspace?.Handle ?? IntPtr.Zero);

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
			get => SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
			set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKMaskFilter MaskFilter {
			get => SKMaskFilter.GetObject (SkiaApi.sk_paint_get_maskfilter (Handle));
			set => SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKColorFilter ColorFilter {
			get => SKColorFilter.GetObject (SkiaApi.sk_paint_get_colorfilter (Handle));
			set => SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKImageFilter ImageFilter {
			get => SKImageFilter.GetObject (SkiaApi.sk_paint_get_imagefilter (Handle));
			set => SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKBlendMode BlendMode {
			get => SkiaApi.sk_paint_get_blendmode (Handle);
			set => SkiaApi.sk_paint_set_blendmode (Handle, value);
		}

		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.")]
		public SKFilterQuality FilterQuality {
			get => (SKFilterQuality)SkiaApi.sk_compatpaint_get_filter_quality (Handle);
			set => SkiaApi.sk_compatpaint_set_filter_quality (Handle, (int)value);
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Typeface)} instead.")]
		public SKTypeface Typeface {
			get => GetFont ().Typeface;
			set => GetFont ().Typeface = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Size)} instead.")]
		public float TextSize {
			get => GetFont ().Size;
			set => GetFont ().Size = value;
		}

		[Obsolete ($"Use {nameof (SKTextAlign)} method overloads instead.")]
		public SKTextAlign TextAlign {
			get => SkiaApi.sk_compatpaint_get_text_align (Handle);
			set => SkiaApi.sk_compatpaint_set_text_align (Handle, value);
		}

		[Obsolete ($"Use {nameof (SKTextEncoding)} method overloads instead.")]
		public SKTextEncoding TextEncoding {
			get => SkiaApi.sk_compatpaint_get_text_encoding (Handle);
			set => SkiaApi.sk_compatpaint_set_text_encoding (Handle, value);
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ScaleX)} instead.")]
		public float TextScaleX {
			get => GetFont ().ScaleX;
			set => GetFont ().ScaleX = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.SkewX)} instead.")]
		public float TextSkewX {
			get => GetFont ().SkewX;
			set => GetFont ().SkewX = value;
		}

		public SKPathEffect PathEffect {
			get => SKPathEffect.GetObject (SkiaApi.sk_paint_get_path_effect (Handle));
			set => SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		// FontSpacing

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Spacing)} instead.")]
		public float FontSpacing =>
			GetFont ().Spacing;

		// FontMetrics

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Metrics)} instead.")]
		public SKFontMetrics FontMetrics {
			get {
				return GetFont ().Metrics;
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetFontMetrics)}() instead.")]
		public float GetFontMetrics (out SKFontMetrics metrics) =>
			GetFont ().GetFontMetrics (out metrics);

		// Clone

		public SKPaint Clone () =>
			GetObject (SkiaApi.sk_paint_clone (Handle))!;

		// MeasureText

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (string text) =>
			GetFont ().MeasureText (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<char> text) =>
			GetFont ().MeasureText (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (byte[] text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<byte> text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, int length) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, IntPtr length) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (string text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<char> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (byte[] text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<byte> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, out bounds, this);

		// BreakText

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (string text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (string text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (string text, float maxWidth, out float measuredWidth, out string measuredText)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var charsRead = GetFont ().BreakText (text, maxWidth, out measuredWidth, this);
			if (charsRead == 0) {
				measuredText = string.Empty;
				return 0;
			}
			if (charsRead == text.Length) {
				measuredText = text;
				return text.Length;
			}
			measuredText = text.Substring (0, charsRead);
			return charsRead;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (byte[] text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (byte[] text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);

		// GetTextPath

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (string text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<char> text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (byte[] text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (string text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (byte[] text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, points);

		// GetFillPath

		public SKPath GetFillPath (SKPath src)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.Identity);

		public SKPath GetFillPath (SKPath src, float resScale)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		public SKPath GetFillPath (SKPath src, SKMatrix matrix)
			=> GetFillPath (src, (SKRect*)null, matrix);

		public SKPath GetFillPath (SKPath src, SKRect cullRect)
			=> GetFillPath (src, &cullRect, SKMatrix.Identity);

		public SKPath GetFillPath (SKPath src, SKRect cullRect, float resScale)
			=> GetFillPath (src, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		public SKPath GetFillPath (SKPath src, SKRect cullRect, SKMatrix matrix)
			=> GetFillPath (src, &cullRect, matrix);

		private SKPath GetFillPath (SKPath src, SKRect* cullRect, SKMatrix matrix)
		{
			var dst = new SKPath ();
			if (GetFillPath (src, dst, cullRect, matrix)) {
				return dst;
			} else {
				dst.Dispose ();
				return null;
			}
		}

		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, SKMatrix matrix)
			=> GetFillPath (src, dst, &cullRect, matrix);

		private bool GetFillPath (SKPath src, SKPath dst, SKRect* cullRect, SKMatrix matrix)
		{
			_ = src ?? throw new ArgumentNullException (nameof (src));
			_ = dst ?? throw new ArgumentNullException (nameof (dst));

			return SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, cullRect, &matrix);
		}

		// CountGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (string text) =>
			GetFont ().CountGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().CountGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (byte[] text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (IntPtr text, int length) =>
			GetFont ().CountGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().CountGlyphs (text, (int)length, TextEncoding);

		// GetGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (string text) =>
			GetFont ().GetGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (byte[] text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetFont ().GetGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphs (text, (int)length, TextEncoding);

		// ContainsGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (byte[] text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (IntPtr text, int length) =>
			GetFont ().ContainsGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().ContainsGlyphs (text, (int)length, TextEncoding);

		// GetGlyphPositions

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (string text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<char> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<byte> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, TextEncoding, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, length, TextEncoding, origin);

		// GetGlyphOffsets

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (string text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (ReadOnlySpan<char> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (ReadOnlySpan<byte> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, TextEncoding, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (IntPtr text, int length, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, length, TextEncoding, origin);

		// GetGlyphWidths

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (string text) =>
			GetFont ().GetGlyphWidths (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphWidths (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (byte[] text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (string text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, out bounds, this);

		// GetTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (int)length, x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, length, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetTextIntercepts (SKTextBlob)

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return text.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetPositionedTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, (int)length, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetHorizontalTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, (int)length, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// Font

		[Obsolete ($"Use {nameof (SKFont)} instead.")]
		public SKFont ToFont () =>
			SKFont.GetObject (SkiaApi.sk_compatpaint_make_font (Handle));

		[Obsolete ($"Use {nameof (SKFont)} instead.")]
		internal SKFont GetFont () =>
			font ??= OwnedBy (SKFont.GetObject (SkiaApi.sk_compatpaint_get_font (Handle), false), this);

		//

		internal static SKPaint GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKPaint (handle, true);
	}
}
