#nullable disable

using System;
using System.Threading;

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
		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.", error: true)]
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
		private SKFont font;

		// Shared template that backs SKPaint()'s default font and SKPaint.Reset()'s
		// reset-to-default font. sk_compatpaint_new_with_font / sk_compatpaint_reset
		// both *copy* the font state into SkCompatPaint::fFont, so this singleton is
		// never mutated by callers.
		private static SKFont defaultFont;
		private static bool defaultFontInitialized;
		private static object defaultFontLock = new object ();

		private static SKFont DefaultFont =>
			LazyInitializer.EnsureInitialized (
				ref defaultFont, ref defaultFontInitialized, ref defaultFontLock,
				() => {
					var font = new SKFont (
						SkiaApi.sk_font_new_with_values (
							SKTypeface.Default.Handle,
							SKFont.DefaultSize,
							SKFont.DefaultScaleX,
							SKFont.DefaultSkewX),
						owns: true);
					// The PreventPublicDisposal call here doesn't suffer from the case of skia
					// giving us the same handle as a return value of another pinvoke call,
					// because sk_font_new_with_values creates a new object.
					font.PreventPublicDisposal ();
					return font;
				});

		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPaint ()
			: this (SkiaApi.sk_compatpaint_new_with_font (DefaultFont.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		[Obsolete ($"Use {nameof (SKFont)} instead.", error: true)]
		public SKPaint (SKFont font)
			: this (IntPtr.Zero, true)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			Handle = SkiaApi.sk_compatpaint_new_with_font (font.Handle);
			GC.KeepAlive (font);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			SkiaApi.sk_compatpaint_delete (Handle);
		}

		// Reset

		public void Reset ()
		{
			SkiaApi.sk_compatpaint_reset (Handle, DefaultFont.Handle);
		}

		// properties

		public bool IsAntialias {
			get {
				var r = SkiaApi.sk_paint_is_antialias (Handle);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_is_antialias (Handle, value);
			}
		}

		public bool IsDither {
			get {
				var r = SkiaApi.sk_paint_is_dither (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_dither (Handle, value);
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.LinearMetrics)} instead.", error: true)]
		public bool IsLinearText {
			get => GetFont ().LinearMetrics;
			set => GetFont ().LinearMetrics = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Subpixel)} instead.", error: true)]
		public bool SubpixelText {
			get => GetFont ().Subpixel;
			set => GetFont ().Subpixel = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Edging)} instead.", error: true)]
		public bool LcdRenderText {
			get {
				var r = SkiaApi.sk_compatpaint_get_lcd_render_text (Handle);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_lcd_render_text (Handle, value);
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.EmbeddedBitmaps)} instead.", error: true)]
		public bool IsEmbeddedBitmapText {
			get => GetFont ().EmbeddedBitmaps;
			set => GetFont ().EmbeddedBitmaps = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ForceAutoHinting)} instead.", error: true)]
		public bool IsAutohinted {
			get => GetFont ().ForceAutoHinting;
			set => GetFont ().ForceAutoHinting = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Hinting)} instead.", error: true)]
		public SKPaintHinting HintingLevel {
			get => (SKPaintHinting)GetFont ().Hinting;
			set => GetFont ().Hinting = (SKFontHinting)value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Embolden)} instead.", error: true)]
		public bool FakeBoldText {
			get => GetFont ().Embolden;
			set => GetFont ().Embolden = value;
		}

		public bool IsStroke {
			get => Style != SKPaintStyle.Fill;
			set => Style = value ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
		}

		public SKPaintStyle Style {
			get {
				var r = SkiaApi.sk_paint_get_style (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_style (Handle, value);
			}
		}

		public SKColor Color {
			get {
				var r = SkiaApi.sk_paint_get_color (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_color (Handle, (uint)value);
			}
		}

		public SKColorF ColorF {
			get {
				SKColorF color4f;
				SkiaApi.sk_paint_get_color4f (Handle, &color4f);
				return color4f;
			}
			set {
				SkiaApi.sk_paint_set_color4f (Handle, &value, IntPtr.Zero);
			}
		}

		public void SetColor (SKColorF color, SKColorSpace colorspace)
		{
			SkiaApi.sk_paint_set_color4f (Handle, &color, colorspace?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (colorspace);
		}

		public float StrokeWidth {
			get {
				var r = SkiaApi.sk_paint_get_stroke_width (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_width (Handle, value);
			}
		}

		public float StrokeMiter {
			get {
				var r = SkiaApi.sk_paint_get_stroke_miter (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_miter (Handle, value);
			}
		}

		public SKStrokeCap StrokeCap {
			get {
				var r = SkiaApi.sk_paint_get_stroke_cap (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_cap (Handle, value);
			}
		}

		public SKStrokeJoin StrokeJoin {
			get {
				var r = SkiaApi.sk_paint_get_stroke_join (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_join (Handle, value);
			}
		}

		public SKShader Shader {
			get {
				var r = SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		public SKMaskFilter MaskFilter {
			get {
				var r = SKMaskFilter.GetObject (SkiaApi.sk_paint_get_maskfilter (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		public SKColorFilter ColorFilter {
			get {
				var r = SKColorFilter.GetObject (SkiaApi.sk_paint_get_colorfilter (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		public SKImageFilter ImageFilter {
			get {
				var r = SKImageFilter.GetObject (SkiaApi.sk_paint_get_imagefilter (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		public SKBlendMode BlendMode {
			get {
				var r = SkiaApi.sk_paint_get_blendmode (Handle);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_blendmode (Handle, value);
			}
		}

		public SKBlender Blender {
			get {
				var r = SKBlender.GetObject (SkiaApi.sk_paint_get_blender (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_blender (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.", error: true)]
		public SKFilterQuality FilterQuality {
			get {
				var r = (SKFilterQuality)SkiaApi.sk_compatpaint_get_filter_quality (Handle);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_filter_quality (Handle, (int)value);
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Typeface)} instead.", error: true)]
		public SKTypeface Typeface {
			get => GetFont ().Typeface;
			set => GetFont ().Typeface = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Size)} instead.", error: true)]
		public float TextSize {
			get => GetFont ().Size;
			set => GetFont ().Size = value;
		}

		[Obsolete ($"Use {nameof (SKTextAlign)} method overloads instead.", error: true)]
		public SKTextAlign TextAlign {
			get {
				var r = SkiaApi.sk_compatpaint_get_text_align (Handle);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_text_align (Handle, value);
			}
		}

		[Obsolete ($"Use {nameof (SKTextEncoding)} method overloads instead.", error: true)]
		public SKTextEncoding TextEncoding {
			get {
				var r = SkiaApi.sk_compatpaint_get_text_encoding (Handle);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_text_encoding (Handle, value);
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ScaleX)} instead.", error: true)]
		public float TextScaleX {
			get => GetFont ().ScaleX;
			set => GetFont ().ScaleX = value;
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.SkewX)} instead.", error: true)]
		public float TextSkewX {
			get => GetFont ().SkewX;
			set => GetFont ().SkewX = value;
		}

		public SKPathEffect PathEffect {
			get {
				var r = SKPathEffect.GetObject (SkiaApi.sk_paint_get_path_effect (Handle));
				return r;
			}
			set {
				SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
			}
		}

		// FontSpacing

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Spacing)} instead.", error: true)]
		public float FontSpacing =>
			GetFont ().Spacing;

		// FontMetrics

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Metrics)} instead.", error: true)]
		public SKFontMetrics FontMetrics {
			get {
				return GetFont ().Metrics;
			}
		}

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetFontMetrics)}() instead.", error: true)]
		public float GetFontMetrics (out SKFontMetrics metrics) =>
			GetFont ().GetFontMetrics (out metrics);

		// Clone

		public SKPaint Clone ()
		{
			var r = GetObject (SkiaApi.sk_compatpaint_clone (Handle))!;
			return r;
		}

		// MeasureText

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (string text) =>
			GetFont ().MeasureText (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<char> text) =>
			GetFont ().MeasureText (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (byte[] text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<byte> text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, int length) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, IntPtr length) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (string text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<char> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (byte[] text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<byte> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, out bounds, this);

		// BreakText

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (string text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (string text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
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

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (byte[] text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (byte[] text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out measuredWidth, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out _, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);

		// GetTextPath

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (string text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<char> text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (byte[] text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, new SKPoint (x, y));

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (string text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (byte[] text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
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
			using var dst = new SKPathBuilder ();
			if (GetFillPath (src, dst, cullRect, matrix)) {
				return dst.Detach ();
			} else {
				return null;
			}
		}

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, SKMatrix matrix)
			=> GetFillPath (src, dst, &cullRect, matrix);

		private bool GetFillPath (SKPath src, SKPath dst, SKRect* cullRect, SKMatrix matrix)
		{
			_ = src ?? throw new ArgumentNullException (nameof (src));
			_ = dst ?? throw new ArgumentNullException (nameof (dst));

			using var builder = new SKPathBuilder ();
			if (!GetFillPath (src, builder, cullRect, matrix))
				return false;

			dst.ReplaceFromBuilder (builder);
			return true;
		}

		public bool GetFillPath (SKPath src, SKPathBuilder dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		public bool GetFillPath (SKPath src, SKPathBuilder dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, float resScale)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, SKMatrix matrix)
			=> GetFillPath (src, dst, &cullRect, matrix);

		private bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect* cullRect, SKMatrix matrix)
		{
			_ = src ?? throw new ArgumentNullException (nameof (src));
			_ = dst ?? throw new ArgumentNullException (nameof (dst));

			var result = SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, cullRect, &matrix);
			GC.KeepAlive (src);
			GC.KeepAlive (dst);
			return result;
		}

		// CountGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (string text) =>
			GetFont ().CountGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().CountGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (byte[] text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (IntPtr text, int length) =>
			GetFont ().CountGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().CountGlyphs (text, (int)length, TextEncoding);

		// GetGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (string text) =>
			GetFont ().GetGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (byte[] text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetFont ().GetGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphs (text, (int)length, TextEncoding);

		// ContainsGlyphs

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (byte[] text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (IntPtr text, int length) =>
			GetFont ().ContainsGlyphs (text, length, TextEncoding);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().ContainsGlyphs (text, (int)length, TextEncoding);

		// GetGlyphPositions

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (string text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<char> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<byte> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, TextEncoding, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, length, TextEncoding, origin);

		// GetGlyphOffsets

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (string text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (ReadOnlySpan<char> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (ReadOnlySpan<byte> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, TextEncoding, origin);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (IntPtr text, int length, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, length, TextEncoding, origin);

		// GetGlyphWidths

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (string text) =>
			GetFont ().GetGlyphWidths (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphWidths (text, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (byte[] text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (string text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, out bounds, this);

		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, out bounds, this);

		// GetTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.Create (text, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.Create (text, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (int)length, x, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, length, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetTextIntercepts (SKTextBlob)

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return text.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetPositionedTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreatePositioned (text, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreatePositioned (text, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, (int)length, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetHorizontalTextIntercepts

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreateHorizontal (text, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreateHorizontal (text, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, (int)length, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// Font

		[Obsolete ($"Use {nameof (SKFont)} instead.", error: true)]
		public SKFont ToFont ()
		{
			var r = SKFont.GetObject (SkiaApi.sk_compatpaint_make_font (Handle));
			return r;
		}

		[Obsolete ($"Use {nameof (SKFont)} instead.", error: true)]
		internal SKFont GetFont () =>
			font ??= OwnedBy (SKFont.GetObject (SkiaApi.sk_compatpaint_get_font (Handle), false), this);

		// Internal compat-paint bypass helpers used by the non-obsolete public APIs
		// in SKCanvas (DrawImage/DrawAtlas/DrawText) and SkiaSharp.HarfBuzz that
		// still respect the legacy paint.FilterQuality / paint.TextAlign /
		// paint.TextEncoding / paint.GetFont state. These mirror the obsolete
		// properties but avoid the CS0619 compile error and ref-assembly stripping
		// when called from a non-obsolete context. Exposed to SkiaSharp.HarfBuzz
		// via InternalsVisibleTo. Remove together with SkCompatPaint in Phase 2
		// of #3732.
		internal SKTextAlign GetLegacyTextAlign () =>
			SkiaApi.sk_compatpaint_get_text_align (Handle);

		internal SKTextEncoding GetLegacyTextEncoding () =>
			SkiaApi.sk_compatpaint_get_text_encoding (Handle);

		internal SKFont GetLegacyFont () =>
			font ??= OwnedBy (SKFont.GetObject (SkiaApi.sk_compatpaint_get_font (Handle), false), this);

		internal SKSamplingOptions GetLegacyFilterQualitySampling ()
		{
			var quality = SkiaApi.sk_compatpaint_get_filter_quality (Handle);
			return quality switch {
				0 => new SKSamplingOptions (SKFilterMode.Nearest, SKMipmapMode.None),
				1 => new SKSamplingOptions (SKFilterMode.Linear, SKMipmapMode.None),
				2 => new SKSamplingOptions (SKFilterMode.Linear, SKMipmapMode.Linear),
				3 => new SKSamplingOptions (SKCubicResampler.Mitchell),
				_ => throw new ArgumentOutOfRangeException (nameof (quality), $"Unknown filter quality: '{quality}'"),
			};
		}

		//

		internal static SKPaint GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKPaint (handle, true);
	}
}
