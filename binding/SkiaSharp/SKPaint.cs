#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Levels of hinting that can be performed.
	/// </summary>
	[Obsolete ($"Use {nameof (SKFontHinting)} instead.")]
	public enum SKPaintHinting
	{
		/// <summary>
		/// Don't perform hinting.
		/// </summary>
		NoHinting = 0,
		/// <summary>
		/// Use a lighter hinting level.
		/// </summary>
		Slight = 1,
		/// <summary>
		/// Use the default hinting level.
		/// </summary>
		Normal = 2,
		/// <summary>
		/// The same as <see cref="SKPaintHinting.Normal" />, unless we are rendering subpixel glyphs.
		/// </summary>
		Full = 3,
	}

	/// <summary>
	/// Filter quality settings.
	/// </summary>
	[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.")]
	public enum SKFilterQuality
	{
		/// <summary>
		/// Unspecified.
		/// </summary>
		None = 0,
		/// <summary>
		/// Low quality.
		/// </summary>
		Low = 1,
		/// <summary>
		/// Medium quality.
		/// </summary>
		Medium = 2,
		/// <summary>
		/// High quality.
		/// </summary>
		High = 3,
	}

	/// <summary>
	/// Convenience methods for <see cref="SKPixelGeometry" />.
	/// </summary>
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

	/// <summary>
	///   Holds the style and color information about how to draw geometries, text and bitmaps.
	/// </summary>
	/// <remarks>
	///   <para>
	///     Anytime you draw something in SkiaSharp, and want to specify what color it is,
	///     or how it blends with the background, or what style or font to draw it in, you
	///     specify those attributes in a paint.
	///   </para>
	///   <para>
	///     Unlike <see cref="SkiaSharp.SKCanvas" />, an paint object does not maintain an
	///     internal stack of state. That is, there is no save/restore on a paint.
	///     However, paint objects are relatively light-weight, so the client may create
	///     and maintain any number of paint objects, each set up for a particular use.
	///   </para>
	///   <para>
	///     Factoring all of these color and stylistic attributes out of the canvas state,
	///     and into (multiple) paint objects, allows the save and restore operations on
	///     the <see cref="SkiaSharp.SKCanvas" /> to be that much more efficient, as all they have
	///     to do is maintain the stack of matrix and clip settings.
	///   </para>
	///   <para>
	///     <b>Effects</b>
	///   </para>
	///   <para>
	///     Beyond simple attributes such as color, strokes, and text values, paints
	///     support effects. These are subclasses of different aspects of the drawing
	///     pipeline, that when referenced by a paint, are called to override some part
	///     of the drawing pipeline.
	///   </para>
	///   <para>
	///     There are five types of effects that can be assigned to an paint object:
	///   </para>
	///   <list type="table">
	///     <listheader>
	///       <term>Effect</term>
	///       <description>Details</description>
	///     </listheader>
	///     <item>
	///       <term>Blend Mode</term>
	///       <description>Blend modes and Duff-Porter transfer modes.</description>
	///     </item>
	///     <item>
	///       <term>Color Filter</term>
	///       <description>Modification of the source colors before applying the blend mode.</description>
	///     </item>
	///     <item>
	///       <term>Mask Filter</term>
	///       <description>Modification of the alpha mask before it is colorized and drawn (for example, blur).</description>
	///     </item>
	///     <item>
	///       <term>Path Effect</term>
	///       <description>Modification of the geometry (path) before the alpha mask is generated (for example, dashing).</description>
	///     </item>
	///     <item>
	///       <term>Shader</term>
	///       <description>Gradients and bitmap patterns.</description>
	///     </item>
	///   </list>
	/// </remarks>
	/// <example>
	///   <para>
	///     <b>Simple Example</b>
	///   </para>
	///   <para>
	///     The following example shows three different paints, each set up to draw in a
	///     different style. The caller can intermix these paints freely, either using
	///     them as is, or modifying them as the drawing proceeds.
	///   </para>
	///   <code language="csharp"><![CDATA[
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info))
	/// {
	/// 	SKCanvas canvas = surface.Canvas;
	/// 	canvas.Clear(SKColors.White);
	///
	/// 	var paint1 = new SKPaint
	/// 	{
	/// 		TextSize = 64.0f,
	/// 		IsAntialias = true,
	/// 		Color = new SKColor(255, 0, 0),
	/// 		Style = SKPaintStyle.Fill
	/// 	};
	///
	/// 	var paint2 = new SKPaint
	/// 	{
	/// 		TextSize = 64.0f,
	/// 		IsAntialias = true,
	/// 		Color = new SKColor(0, 136, 0),
	/// 		Style = SKPaintStyle.Stroke,
	/// 		StrokeWidth = 3
	/// 	};
	///
	/// 	var paint3 = new SKPaint
	/// 	{
	/// 		TextSize = 64.0f,
	/// 		IsAntialias = true,
	/// 		Color = new SKColor(136, 136, 136),
	/// 		TextScaleX = 1.5f
	/// 	};
	///
	/// 	var text = "Skia!";
	/// 	canvas.DrawText(text, 20.0f, 64.0f, paint1);
	/// 	canvas.DrawText(text, 20.0f, 144.0f, paint2);
	/// 	canvas.DrawText(text, 20.0f, 224.0f, paint3);
	/// }
	/// ]]></code>
	///   <para>
	///     The example above produces the following:
	///   </para>
	///   <para>
	///     <img src="~/docs-images/SKPaintText.png" alt="SKPaint and Text" />
	///   </para>
	/// </example>
	/// <example>
	///   <para>
	///     <b>Effects Example</b>
	///   </para>
	///   <para>
	///     The following example draws using a gradient instead of a single color. To do
	///     this a <see cref="SKShader" /> is assigned to the paint. Anything drawn with that paint
	///     will be drawn with the gradient specified in the call to
	///     SKShader.CreateLinearGradient.
	///   </para>
	///   <code language="csharp"><![CDATA[
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info))
	/// {
	/// 	SKCanvas canvas = surface.Canvas;
	/// 	canvas.Clear(SKColors.White);
	///
	/// 	// create a gradient
	/// 	var colors = new[]
	/// 	{
	/// 		SKColors.Blue,
	/// 		SKColors.Yellow
	/// 	};
	///
	/// 	var shader = SKShader.CreateLinearGradient(
	/// 		new SKPoint(0.0f, 0.0f),
	/// 		new SKPoint(256.0f, 256.0f),
	/// 		colors,
	/// 		null,
	/// 		SKShaderTileMode.Clamp);
	///
	/// 	// assign the gradient to the paint
	/// 	var paint = new SKPaint
	/// 	{
	/// 		Shader = shader
	/// 	};
	///
	/// 	canvas.DrawPaint(paint);
	/// }
	/// ]]></code>
	///   <para>
	///     The example above produces the following:
	///   </para>
	///   <para>
	///     <img src="~/docs-images/gradient.png" alt="SKPaint and SKShader" />
	///   </para>
	/// </example>
	public unsafe class SKPaint : SKObject, ISKSkipObjectRegistration
	{
		[Obsolete]
		private SKFont font;

		internal SKPaint (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new paint with the default settings.
		/// </summary>
		public SKPaint ()
			: this (SkiaApi.sk_compatpaint_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		/// <param name="font"></param>
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

		/// <summary>
		/// Resets all the paint properties to their defaults.
		/// </summary>
		public void Reset () =>
			SkiaApi.sk_compatpaint_reset (Handle);

		// properties

		/// <summary>
		/// Gets or sets a value indicating whether anti-aliasing is enabled.
		/// </summary>
		public bool IsAntialias {
			get => SkiaApi.sk_paint_is_antialias (Handle);
			set => SkiaApi.sk_compatpaint_set_is_antialias (Handle, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether dithering is enabled.
		/// </summary>
		public bool IsDither {
			get => SkiaApi.sk_paint_is_dither (Handle);
			set => SkiaApi.sk_paint_set_dither (Handle, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether text is linear.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.LinearMetrics)} instead.")]
		public bool IsLinearText {
			get => GetFont ().LinearMetrics;
			set => GetFont ().LinearMetrics = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use subpixel text positioning.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Subpixel)} instead.")]
		public bool SubpixelText {
			get => GetFont ().Subpixel;
			set => GetFont ().Subpixel = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether LCD text rendering is enabled.
		/// </summary>
		/// <remarks>
		/// <see cref="SKPaint.IsAntialias" /> must also be enabled for LCD rendering to be enabled.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Edging)} instead.")]
		public bool LcdRenderText {
			get => SkiaApi.sk_compatpaint_get_lcd_render_text (Handle);
			set => SkiaApi.sk_compatpaint_set_lcd_render_text (Handle, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether text is an embedded bitmap.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.EmbeddedBitmaps)} instead.")]
		public bool IsEmbeddedBitmapText {
			get => GetFont ().EmbeddedBitmaps;
			set => GetFont ().EmbeddedBitmaps = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether auto-hinting is enabled.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ForceAutoHinting)} instead.")]
		public bool IsAutohinted {
			get => GetFont ().ForceAutoHinting;
			set => GetFont ().ForceAutoHinting = value;
		}

		/// <summary>
		/// Gets or sets the level of hinting to be performed.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Hinting)} instead.")]
		public SKPaintHinting HintingLevel {
			get => (SKPaintHinting)GetFont ().Hinting;
			set => GetFont ().Hinting = (SKFontHinting)value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether fake bold text is enabled.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Embolden)} instead.")]
		public bool FakeBoldText {
			get => GetFont ().Embolden;
			set => GetFont ().Embolden = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether to paint a stroke or the fill.
		/// </summary>
		/// <remarks>
		/// This is a shortcut way to set <see cref="SKPaint.Style" /> to either <see cref="SKPaintStyle.Stroke" /> or <see cref="SKPaintStyle.Fill" />.
		/// </remarks>
		public bool IsStroke {
			get => Style != SKPaintStyle.Fill;
			set => Style = value ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
		}

		/// <summary>
		/// Gets or sets the painting style.
		/// </summary>
		/// <remarks>
		/// Can also be set using <see cref="SKPaint.IsStroke" />.
		/// </remarks>
		public SKPaintStyle Style {
			get => SkiaApi.sk_paint_get_style (Handle);
			set => SkiaApi.sk_paint_set_style (Handle, value);
		}

		/// <summary>
		/// Gets or sets the paint's foreground color.
		/// </summary>
		/// <remarks>
		/// The color is a 32-bit value containing ARGB. This 32-bit value is not premultiplied, meaning that its alpha can be any value, regardless of the values of R, G and B.
		/// </remarks>
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

		/// <param name="color"></param>
		/// <param name="colorspace"></param>
		public void SetColor (SKColorF color, SKColorSpace colorspace) =>
			SkiaApi.sk_paint_set_color4f (Handle, &color, colorspace?.Handle ?? IntPtr.Zero);

		/// <summary>
		/// Gets or sets the paint's stroke width.
		/// </summary>
		/// <remarks>
		/// This is used whenever the <see cref="SKPaint.Style" /> is <see cref="SKPaintStyle.Stroke" /> or <see cref="SKPaintStyle.StrokeAndFill" />. The value of zero is the special hairline mode.   Hairlines always draw with a width of 1 pixel, regardless of the transformation matrix.
		/// </remarks>
		public float StrokeWidth {
			get => SkiaApi.sk_paint_get_stroke_width (Handle);
			set => SkiaApi.sk_paint_set_stroke_width (Handle, value);
		}

		/// <summary>
		/// Gets or sets the paint's miter limit.
		/// </summary>
		/// <remarks>
		/// This is used whenever the <see cref="SKPaint.Style" /> is <see cref="SKPaintStyle.Stroke" /> or <see cref="SKPaintStyle.StrokeAndFill" /> to control the behavior of miter joins when the joins' angle is sharp.
		/// </remarks>
		public float StrokeMiter {
			get => SkiaApi.sk_paint_get_stroke_miter (Handle);
			set => SkiaApi.sk_paint_set_stroke_miter (Handle, value);
		}

		/// <summary>
		/// Gets or sets a value indicating how the start and end of stroked lines and paths are treated.
		/// </summary>
		public SKStrokeCap StrokeCap {
			get => SkiaApi.sk_paint_get_stroke_cap (Handle);
			set => SkiaApi.sk_paint_set_stroke_cap (Handle, value);
		}

		/// <summary>
		/// Gets or sets the path's join type.
		/// </summary>
		public SKStrokeJoin StrokeJoin {
			get => SkiaApi.sk_paint_get_stroke_join (Handle);
			set => SkiaApi.sk_paint_set_stroke_join (Handle, value);
		}

		/// <summary>
		/// Gets or sets the shader to use when painting.
		/// </summary>
		public SKShader Shader {
			get => SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
			set => SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Gets or sets the mask filter to use when painting.
		/// </summary>
		/// <remarks>
		/// Mask filters control the transformations on the alpha channel before primitives are drawn. Examples are blur or emboss.
		/// </remarks>
		public SKMaskFilter MaskFilter {
			get => SKMaskFilter.GetObject (SkiaApi.sk_paint_get_maskfilter (Handle));
			set => SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Gets or sets the paint's color filter.
		/// </summary>
		public SKColorFilter ColorFilter {
			get => SKColorFilter.GetObject (SkiaApi.sk_paint_get_colorfilter (Handle));
			set => SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Gets or sets the image filter.
		/// </summary>
		public SKImageFilter ImageFilter {
			get => SKImageFilter.GetObject (SkiaApi.sk_paint_get_imagefilter (Handle));
			set => SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Gets or sets the blend mode.
		/// </summary>
		public SKBlendMode BlendMode {
			get => SkiaApi.sk_paint_get_blendmode (Handle);
			set => SkiaApi.sk_paint_set_blendmode (Handle, value);
		}

		public SKBlender Blender {
			get => SKBlender.GetObject (SkiaApi.sk_paint_get_blender (Handle));
			set => SkiaApi.sk_paint_set_blender (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Gets or sets the filter quality of the current paint.
		/// </summary>
		/// <remarks>
		/// This affects the quality (and performance) of drawing scaled images.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.")]
		public SKFilterQuality FilterQuality {
			get => (SKFilterQuality)SkiaApi.sk_compatpaint_get_filter_quality (Handle);
			set => SkiaApi.sk_compatpaint_set_filter_quality (Handle, (int)value);
		}

		/// <summary>
		/// Gets or sets the typeface used when painting text. May be <see langword="null" />.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Typeface)} instead.")]
		public SKTypeface Typeface {
			get => GetFont ().Typeface;
			set => GetFont ().Typeface = value;
		}

		/// <summary>
		/// Gets or sets the text height in pixels.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Size)} instead.")]
		public float TextSize {
			get => GetFont ().Size;
			set => GetFont ().Size = value;
		}

		/// <summary>
		/// Gets or sets the path's align value.
		/// </summary>
		[Obsolete ($"Use {nameof (SKTextAlign)} method overloads instead.")]
		public SKTextAlign TextAlign {
			get => SkiaApi.sk_compatpaint_get_text_align (Handle);
			set => SkiaApi.sk_compatpaint_set_text_align (Handle, value);
		}

		/// <summary>
		/// Gets or sets the encoding used when drawing or measuring text.
		/// </summary>
		/// <remarks>
		/// This defaults to UTF-8 encoding.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextEncoding)} method overloads instead.")]
		public SKTextEncoding TextEncoding {
			get => SkiaApi.sk_compatpaint_get_text_encoding (Handle);
			set => SkiaApi.sk_compatpaint_set_text_encoding (Handle, value);
		}

		/// <summary>
		/// Gets or sets paint's horizontal scale factor for text.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ScaleX)} instead.")]
		public float TextScaleX {
			get => GetFont ().ScaleX;
			set => GetFont ().ScaleX = value;
		}

		/// <summary>
		/// Gets or sets paint's horizontal skew factor for text.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.SkewX)} instead.")]
		public float TextSkewX {
			get => GetFont ().SkewX;
			set => GetFont ().SkewX = value;
		}

		/// <summary>
		/// Gets or sets the path effect to use when painting.
		/// </summary>
		public SKPathEffect PathEffect {
			get => SKPathEffect.GetObject (SkiaApi.sk_paint_get_path_effect (Handle));
			set => SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
		}

		// FontSpacing

		/// <summary>
		/// Gets the recommend line spacing.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Spacing)} instead.")]
		public float FontSpacing =>
			GetFont ().Spacing;

		// FontMetrics

		/// <summary>
		/// Gets the font metrics for the current typeface.
		/// </summary>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Metrics)} instead.")]
		public SKFontMetrics FontMetrics {
			get {
				return GetFont ().Metrics;
			}
		}

		/// <param name="metrics"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetFontMetrics)}() instead.")]
		public float GetFontMetrics (out SKFontMetrics metrics) =>
			GetFont ().GetFontMetrics (out metrics);

		// Clone

		/// <summary>
		/// Creates a copy of the current paint.
		/// </summary>
		/// <returns>Returns the copy.</returns>
		/// <remarks>
		/// The copy is a shallow copy, all references will still point to the same objects.
		/// </remarks>
		public SKPaint Clone () =>
			GetObject (SkiaApi.sk_compatpaint_clone (Handle))!;

		// MeasureText

		/// <summary>
		/// Measures the specified text.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case the returned value should be treated has a height instead of a width.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (string text) =>
			GetFont ().MeasureText (text, this);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<char> text) =>
			GetFont ().MeasureText (text, this);

		/// <summary>
		/// Measures the specified text.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case the returned value should be treated has a height instead of a width.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (byte[] text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<byte> text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		/// <summary>
		/// Measures the specified UTF-8 encoded text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format.   This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, int length) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, this);

		/// <summary>
		/// Measures the specified UTF-8 encoded text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format.   This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, IntPtr length) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, this);

		/// <summary>
		/// Measures the specified text.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="bounds">The bounds of the text relative to (0, 0)</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case the returned value should be treated has a height instead of a width.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (string text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		/// <param name="text"></param>
		/// <param name="bounds"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<char> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		/// <summary>
		/// Measures the specified text.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="bounds">The bounds of the text relative to (0, 0)</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case the returned value should be treated has a height instead of a width.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (byte[] text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		/// <param name="text"></param>
		/// <param name="bounds"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (ReadOnlySpan<byte> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		/// <summary>
		/// Measures the specified UTF-8 encoded text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="bounds">The bounds of the text relative to (0, 0)</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case
		/// the returned value should be treated has a height instead of a width.
		/// The `buffer` parameter is a pointer to a region in memory that contains text
		/// encoded in the <see cref="SkiaSharp.SKPaint.TextEncoding" /> format. This only
		/// consumes up to `length` bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, out bounds, this);

		/// <summary>
		/// Measures the specified UTF-8 encoded text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="bounds">The bounds of the text relative to (0, 0)</param>
		/// <returns>Returns the width of the text.</returns>
		/// <remarks>
		/// This will return the vertical measure if this is vertical text, in which case
		/// the returned value should be treated has a height instead of a width.
		/// The `buffer` parameter is a pointer to a region in memory that contains text
		/// encoded in the <see cref="SkiaSharp.SKPaint.TextEncoding" /> format. This only
		/// consumes up to `length` bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.")]
		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, out bounds, this);

		// BreakText

		/// <summary>
		/// Measure the text, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= maxWidth are measured.</param>
		/// <returns>Returns the number of characters of text that were measured.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (string text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		/// <summary>
		/// Measure the text, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <param name="measuredWidth">The actual width of the measured text.</param>
		/// <returns>Returns the number of characters of text that were measured.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (string text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		/// <summary>
		/// Measure the text, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <param name="measuredWidth">The actual width of the measured text.</param>
		/// <param name="measuredText">The text that was measured.</param>
		/// <returns>Returns the number of characters of text that were measured.</returns>
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

		/// <param name="text"></param>
		/// <param name="maxWidth"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		/// <param name="text"></param>
		/// <param name="maxWidth"></param>
		/// <param name="measuredWidth"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		/// <summary>
		/// Measure the text, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= maxWidth are measured.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (byte[] text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		/// <summary>
		/// Measure the text, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="text">The text to be measured.</param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <param name="measuredWidth">The actual width of the measured text.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (byte[] text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		/// <param name="text"></param>
		/// <param name="maxWidth"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		/// <param name="text"></param>
		/// <param name="maxWidth"></param>
		/// <param name="measuredWidth"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		/// <summary>
		/// Measure the text buffer, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format. This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out _, this);

		/// <summary>
		/// Measure the text buffer, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <param name="measuredWidth">The actual width of the measured text.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format. This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out measuredWidth, this);

		/// <summary>
		/// Measure the text buffer, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format. This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out _, this);

		/// <summary>
		/// Measure the text buffer, stopping early if the measured width exceeds <paramref name="maxWidth" />.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="maxWidth">The maximum width. Only the subset of text whose accumulated widths are &lt;= <paramref name="maxWidth" /> are measured.</param>
		/// <param name="measuredWidth">The actual width of the measured text.</param>
		/// <returns>Returns the number of bytes of text that were measured.</returns>
		/// <remarks>
		/// The <paramref name="buffer" /> parameter is a pointer to a region in memory that contains text encoded in the <see cref="SKPaint.TextEncoding" /> format. This only consumes up to <paramref name="length" /> bytes from the buffer.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.")]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);

		// GetTextPath

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="text">The text to generate an outline for.</param>
		/// <param name="x">The x-coordinate of the first glyph in the text.</param>
		/// <param name="y">The y-coordinate of the first glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (string text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		/// <param name="text"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<char> text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="x">The x-coordinate of the first glyph in the text.</param>
		/// <param name="y">The y-coordinate of the first glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (byte[] text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		/// <param name="text"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="x">The x-coordinate of the first glyph in the text.</param>
		/// <param name="y">The y-coordinate of the first glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, new SKPoint (x, y));

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="x">The x-coordinate of the first glyph in the text.</param>
		/// <param name="y">The y-coordinate of the first glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, new SKPoint (x, y));

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="text">The text to generate an outline for.</param>
		/// <param name="points">The position to use for each glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (string text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, points);

		/// <param name="text"></param>
		/// <param name="points"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, points);

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="points">The position to use for each glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (byte[] text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		/// <param name="text"></param>
		/// <param name="points"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="points">The position to use for each glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		/// <param name="buffer"></param>
		/// <param name="length"></param>
		/// <param name="points"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, int length, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		/// <summary>
		/// Returns the path (outline) for the specified text.
		/// </summary>
		/// <param name="buffer">The pointer to a region holding text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The number of bytes to read from the <paramref name="buffer." /></param>
		/// <param name="points">The position to use for each glyph in the text.</param>
		/// <returns>Returns the <see cref="SKPath" /> containing the outline of the text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.")]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, points);

		// GetFillPath

		/// <summary>
		/// Creates a new path from the result of applying any and all effects to a source path.
		/// </summary>
		/// <param name="src">The source path.</param>
		/// <returns>Returns the resulting fill path, or null if the source path should be drawn with a hairline.</returns>
		public SKPath GetFillPath (SKPath src)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.Identity);

		/// <summary>
		/// Creates a new path from the result of applying any and all effects to a source path.
		/// </summary>
		/// <param name="src">The source path.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns the resulting fill path, or null if the source path should be drawn with a hairline.</returns>
		public SKPath GetFillPath (SKPath src, float resScale)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		public SKPath GetFillPath (SKPath src, SKMatrix matrix)
			=> GetFillPath (src, (SKRect*)null, matrix);

		/// <summary>
		/// Creates a new path from the result of applying any and all effects to a source path.
		/// </summary>
		/// <param name="src">The source path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <returns>Returns the resulting fill path, or null if the source path should be drawn with a hairline.</returns>
		public SKPath GetFillPath (SKPath src, SKRect cullRect)
			=> GetFillPath (src, &cullRect, SKMatrix.Identity);

		/// <summary>
		/// Creates a new path from the result of applying any and all effects to a source path.
		/// </summary>
		/// <param name="src">The source path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns the resulting fill path, or null if the source path should be drawn with a hairline.</returns>
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

		/// <summary>
		/// Applies any and all effects to a source path, returning the result in the destination.
		/// </summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <returns>Returns true if the path should be filled, or false if it should be drawn with a hairline.</returns>
		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		/// <summary>
		/// Applies any and all effects to a source path, returning the result in the destination.
		/// </summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns true if the path should be filled, or false if it should be drawn with a hairline.</returns>
		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		/// <summary>
		/// Applies any and all effects to a source path, returning the result in the destination.
		/// </summary>
		/// <param name="src">The source path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <returns>Returns true if the path should be filled, or false if it should be drawn with a hairline.</returns>
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		/// <summary>
		/// Applies any and all effects to a source path, returning the result in the destination.
		/// </summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="cullRect">The destination path may be culled to this rectangle.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns true if the path should be filled, or false if it should be drawn with a hairline.</returns>
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

		/// <summary>
		/// Returns the number of glyphs in text.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>Returns the number of glyphs in text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (string text) =>
			GetFont ().CountGlyphs (text);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().CountGlyphs (text);

		/// <summary>
		/// Returns the number of glyphs in text.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <returns>Returns the number of glyphs in text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (byte[] text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		/// <summary>
		/// Returns the number of glyphs in text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the number of glyphs in text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (IntPtr text, int length) =>
			GetFont ().CountGlyphs (text, length, TextEncoding);

		/// <summary>
		/// Returns the number of glyphs in text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the number of glyphs in text.</returns>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.")]
		public int CountGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().CountGlyphs (text, (int)length, TextEncoding);

		// GetGlyphs

		/// <summary>
		/// Converts text into glyph indices.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>Returns the glyph indices.</returns>
		/// <remarks>
		/// This method does not check the text for valid character codes or valid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (string text) =>
			GetFont ().GetGlyphs (text);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphs (text);

		/// <summary>
		/// Converts text into glyph indices.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <returns>Returns the glyph indices.</returns>
		/// <remarks>
		/// This method does not check the text for valid character codes or valid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (byte[] text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		/// <summary>
		/// Converts text into glyph indices.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the glyph indices.</returns>
		/// <remarks>
		/// This method does not check the text for valid character codes or valid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetFont ().GetGlyphs (text, length, TextEncoding);

		/// <summary>
		/// Converts text into glyph indices.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the glyph indices.</returns>
		/// <remarks>
		/// This method does not check the text for valid character codes or valid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.")]
		public ushort[] GetGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphs (text, (int)length, TextEncoding);

		// ContainsGlyphs

		/// <summary>
		/// Returns a value indicating whether or not all the characters corresponds to a non-zero glyph index.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>Returns true if all the characters corresponds to a non-zero glyph index, otherwise false if any characters in text are not supported in the typeface.</returns>
		/// <remarks>
		/// This method does not check to see if the text contains invalid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		/// <summary>
		/// Returns a value indicating whether or not all the characters corresponds to a non-zero glyph index.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <returns>Returns true if all the characters corresponds to a non-zero glyph index, otherwise false if any characters in text are not supported in the typeface.</returns>
		/// <remarks>
		/// This method does not check to see if the text contains invalid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (byte[] text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		/// <summary>
		/// Returns a value indicating whether or not all the characters corresponds to a non-zero glyph index.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns true if all the characters corresponds to a non-zero glyph index, otherwise false if any characters in text are not supported in the typeface.</returns>
		/// <remarks>
		/// This method does not check to see if the text contains invalid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (IntPtr text, int length) =>
			GetFont ().ContainsGlyphs (text, length, TextEncoding);

		/// <summary>
		/// Returns a value indicating whether or not all the characters corresponds to a non-zero glyph index.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns true if all the characters corresponds to a non-zero glyph index, otherwise false if any characters in text are not supported in the typeface.</returns>
		/// <remarks>
		/// This method does not check to see if the text contains invalid glyph indices.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.")]
		public bool ContainsGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().ContainsGlyphs (text, (int)length, TextEncoding);

		// GetGlyphPositions

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (string text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<char> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<byte> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, TextEncoding, origin);

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.")]
		public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, length, TextEncoding, origin);

		// GetGlyphOffsets

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (string text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (ReadOnlySpan<char> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		/// <param name="text"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (ReadOnlySpan<byte> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, TextEncoding, origin);

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="origin"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.")]
		public float[] GetGlyphOffsets (IntPtr text, int length, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, length, TextEncoding, origin);

		// GetGlyphWidths

		/// <summary>
		/// Retrieves the advance for each glyph in the text.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (string text) =>
			GetFont ().GetGlyphWidths (text, this);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphWidths (text, this);

		/// <summary>
		/// Retrieves the advance for each glyph in the text.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (byte[] text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		/// <param name="text"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		/// <summary>
		/// Retrieves the advance for each glyph in the text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, this);

		/// <summary>
		/// Retrieves the advance for each glyph in the text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, this);

		/// <summary>
		/// Retrieves the advance and bounds for each glyph in the text.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="bounds">The bounds for each glyph relative to (0, 0).</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths and bounds.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (string text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		/// <param name="text"></param>
		/// <param name="bounds"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		/// <summary>
		/// Retrieves the advance and bounds for each glyph in the text.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="bounds">The bounds for each glyph relative to (0, 0).</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths and bounds.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		/// <param name="text"></param>
		/// <param name="bounds"></param>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		/// <summary>
		/// Retrieves the advance and bounds for each glyph in the text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="bounds">The bounds for each glyph relative to (0, 0).</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths and bounds.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, out bounds, this);

		/// <summary>
		/// Retrieves the advance and bounds for each glyph in the text.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="bounds">The bounds for each glyph relative to (0, 0).</param>
		/// <returns>Returns the text advances for each glyph.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" /> to scale the widths and bounds.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.")]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, out bounds, this);

		// GetTextIntercepts

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="x">The x-coordinate of the origin to the text.</param>
		/// <param name="y">The y-coordinate of the origin to the text.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="x">The x-coordinate of the origin to the text.</param>
		/// <param name="y">The y-coordinate of the origin to the text.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="x">The x-coordinate of the origin to the text.</param>
		/// <param name="y">The y-coordinate of the origin to the text.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (int)length, x, y, upperBounds, lowerBounds);

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="x">The x-coordinate of the origin to the text.</param>
		/// <param name="y">The y-coordinate of the origin to the text.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, length, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetTextIntercepts (SKTextBlob)

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text blob.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return text.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetPositionedTextIntercepts

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="positions">The positions of each glyph.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="positions"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="positions">The positions of each glyph.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="positions"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="positions">The positions of each glyph.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="positions">The positions of each glyph.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, (int)length, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetHorizontalTextIntercepts

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="xpositions">The positions of each glyph in the horizontal direction.</param>
		/// <param name="y">The positions of all the glyphs along the y-coordinate.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="xpositions"></param>
		/// <param name="y"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="xpositions">The positions of each glyph in the horizontal direction.</param>
		/// <param name="y">The positions of all the glyphs along the y-coordinate.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		/// <param name="text"></param>
		/// <param name="xpositions"></param>
		/// <param name="y"></param>
		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="xpositions">The positions of each glyph in the horizontal direction.</param>
		/// <param name="y">The positions of all the glyphs along the y-coordinate.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.")]
		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		/// <summary>
		/// Calculate the intersections of two parallel lines and the glyphs.
		/// </summary>
		/// <param name="text">The text buffer encoded using the encoding specified in <see cref="SKPaint.TextEncoding" /> format.</param>
		/// <param name="length">The length of the text buffer.</param>
		/// <param name="xpositions">The positions of each glyph in the horizontal direction.</param>
		/// <param name="y">The positions of all the glyphs along the y-coordinate.</param>
		/// <param name="upperBounds">The upper line parallel to the advance.</param>
		/// <param name="lowerBounds">The lower line parallel to the advance.</param>
		/// <returns>Returns the intersections of two parallel lines and the glyphs.</returns>
		/// <remarks>
		/// Uses <see cref="SkiaSharp.SKPaint.TextEncoding" /> to decode text,
		/// <see cref="SkiaSharp.SKPaint.Typeface" /> to get the font metrics, and
		/// <see cref="SkiaSharp.SKPaint.TextSize" />, <see cref="SkiaSharp.SKPaint.FakeBoldText" />
		/// and <see cref="SkiaSharp.SKPaint.PathEffect" /> to scale and modify the glyph paths.
		/// </remarks>
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
