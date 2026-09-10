#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Levels of hinting that can be performed.</summary>
	/// <remarks />
	[Obsolete ($"Use {nameof (SKFontHinting)} instead.", error: true)]
	public enum SKPaintHinting
	{
		/// <summary>Don't perform hinting.</summary>
		NoHinting = 0,
		/// <summary>Use a lighter hinting level.</summary>
		Slight = 1,
		/// <summary>Use the default hinting level.</summary>
		Normal = 2,
		/// <summary>The same as <see cref="F:SkiaSharp.SKPaintHinting.Normal" />, unless we are rendering subpixel glyphs.</summary>
		Full = 3,
	}

	/// <summary>Filter quality settings.</summary>
	/// <remarks />
	[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.", error: true)]
	public enum SKFilterQuality
	{
		/// <summary>Unspecified.</summary>
		None = 0,
		/// <summary>Low quality.</summary>
		Low = 1,
		/// <summary>Medium quality.</summary>
		Medium = 2,
		/// <summary>High quality.</summary>
		High = 3,
	}

	public static partial class SkiaExtensions
	{
		/// <summary>Converts the legacy filtering quality to sampling options.</summary>
		/// <param name="quality">The legacy filtering quality.</param>
		/// <returns>The sampling options corresponding to <paramref name="quality" />.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="quality" /> is not a defined value.</exception>
		/// <remarks />
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

	/// <summary>Holds the style and color information about how to draw geometries, text and bitmaps.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Anytime you draw something in SkiaSharp, and want to specify what color it is,
	/// or how it blends with the background, or what style or font to draw it in, you
	/// specify those attributes in a paint.
	///
	/// Unlike <xref:SkiaSharp.SKCanvas>, an paint object does not maintain an
	/// internal stack of state. That is, there is no save/restore on a paint.
	/// However, paint objects are relatively light-weight, so the client may create
	/// and maintain any number of paint objects, each set up for a particular use.
	///
	/// Factoring all of these color and stylistic attributes out of the canvas state,
	/// and into (multiple) paint objects, allows the save and restore operations on
	/// the <xref:SkiaSharp.SKCanvas> to be that much more efficient, as all they have
	/// to do is maintain the stack of matrix and clip settings.
	///
	/// ### Effects
	///
	/// Beyond simple attributes such as color, strokes, and text values, paints
	/// support effects. These are subclasses of different aspects of the drawing
	/// pipeline, that when referenced by a paint, are called to override some part
	/// of the drawing pipeline.
	///
	/// There are five types of effects that can be assigned to an paint object:
	///
	/// | Effect        | Details                                                                                        |
	/// |---------------|------------------------------------------------------------------------------------------------|
	/// | Blend Mode    | Blend modes and Duff-Porter transfer modes.                                                    |
	/// | Color Filter  | Modification of the source colors before applying the blend mode.                              |
	/// | Mask Filter   | Modification of the alpha mask before it is colorized and drawn (for example, blur).           |
	/// | Path Effect   | Modification of the geometry (path) before the alpha mask is generated (for example, dashing). |
	/// | Shader        | Gradients and bitmap patterns.                                                                 |
	///
	/// ## Examples
	///
	/// ### Simple Example
	///
	/// The following example shows three different paints, each set up to draw in a
	/// different style. The caller can intermix these paints freely, either using
	/// them as is, or modifying them as the drawing proceeds.
	///
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	///     SKCanvas canvas = surface.Canvas;
	///
	///     canvas.Clear(SKColors.White);
	///
	///     using var font = new SKFont { Size = 64.0f };
	///     using var scaledFont = new SKFont { Size = 64.0f, ScaleX = 1.5f };
	///
	///     using var paint1 = new SKPaint {
	///         IsAntialias = true,
	///         Color = new SKColor(255, 0, 0),
	///         Style = SKPaintStyle.Fill
	///     };
	///
	///     using var paint2 = new SKPaint {
	///         IsAntialias = true,
	///         Color = new SKColor(0, 136, 0),
	///         Style = SKPaintStyle.Stroke,
	///         StrokeWidth = 3
	///     };
	///
	///     using var paint3 = new SKPaint {
	///         IsAntialias = true,
	///         Color = new SKColor(136, 136, 136)
	///     };
	///
	///     var text = "Skia!";
	///     canvas.DrawText(text, 20.0f, 64.0f, SKTextAlign.Left, font, paint1);
	///     canvas.DrawText(text, 20.0f, 144.0f, SKTextAlign.Left, font, paint2);
	///     canvas.DrawText(text, 20.0f, 224.0f, SKTextAlign.Left, scaledFont, paint3);
	/// }
	/// ```
	///
	/// The example above produces the following:
	///
	/// ![SKPaint and Text](~/images/SKPaintText.png "SKPaint and Text")
	///
	/// ### Effects Example
	///
	/// The following example draws using a gradient instead of a single color. To do,
	/// this a `SKShader` is assigned to the paint. Anything drawn with that paint
	/// will be drawn with the gradient specified in the call to
	/// `SKShader.CreateLinearGradient`.
	///
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	///     SKCanvas canvas = surface.Canvas;
	///
	///     canvas.Clear(SKColors.White);
	///
	///     // create a gradient
	///     var colors = new[] {
	///         SKColors.Blue,
	///         SKColors.Yellow
	///     };
	///     var shader = SKShader.CreateLinearGradient(
	///         new SKPoint(0.0f, 0.0f),
	///         new SKPoint(256.0f, 256.0f),
	///         colors,
	///         null,
	///         SKShaderTileMode.Clamp);
	///
	///     // assign the gradient to the paint
	///     var paint = new SKPaint {
	///         Shader = shader
	///     };
	///
	///     canvas.DrawPaint(paint);
	/// }
	/// ```
	///
	/// The example above produces the following:
	///
	/// ![SKPaint and SKShader](~/images/gradient.png "SKPaint and SKShader")
	/// ]]></format></remarks>
	public unsafe class SKPaint : SKObject, ISKSkipObjectRegistration
	{
		[Obsolete]
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

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPaint" /> class with the default settings.</summary>
		/// <remarks />
		public SKPaint ()
			: this (SkiaApi.sk_compatpaint_new_with_font (DefaultFont.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPaint instance.");
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPaint" /> class from the specified font.</summary>
		/// <param name="font">The font to use.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="font" /> is <see langword="null" />.</exception>
		/// <remarks />
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPaint" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPaint" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			SkiaApi.sk_compatpaint_delete (Handle);
			GC.KeepAlive (this);
		}

		// Reset

		/// <summary>Resets all the paint properties to their defaults.</summary>
		/// <remarks />
		public void Reset ()
		{
			SkiaApi.sk_compatpaint_reset (Handle, DefaultFont.Handle);
			GC.KeepAlive (this);
		}

		// properties

		/// <summary>Gets or sets a value indicating whether anti-aliasing is enabled.</summary>
		/// <value><see langword="true" /> if anti-aliasing is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsAntialias {
			get {
				var r = SkiaApi.sk_paint_is_antialias (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_is_antialias (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets a value indicating whether dithering is enabled.</summary>
		/// <value><see langword="true" /> if dithering is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsDither {
			get {
				var r = SkiaApi.sk_paint_is_dither (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_dither (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets a value indicating whether linear text metrics are used.</summary>
		/// <value><see langword="true" /> if linear text metrics are used; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.LinearMetrics)} instead.", error: true)]
		public bool IsLinearText {
			get => GetFont ().LinearMetrics;
			set => GetFont ().LinearMetrics = value;
		}

		/// <summary>Gets or sets a value indicating whether subpixel text positioning is enabled.</summary>
		/// <value><see langword="true" /> if subpixel text positioning is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Subpixel)} instead.", error: true)]
		public bool SubpixelText {
			get => GetFont ().Subpixel;
			set => GetFont ().Subpixel = value;
		}

		/// <summary>Gets or sets a value indicating whether LCD text rendering is enabled.</summary>
		/// <value><see langword="true" /> if LCD text rendering is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Edging)} instead.", error: true)]
		public bool LcdRenderText {
			get {
				var r = SkiaApi.sk_compatpaint_get_lcd_render_text (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_lcd_render_text (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets a value indicating whether embedded bitmap glyphs are used.</summary>
		/// <value><see langword="true" /> if embedded bitmap glyphs are used; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.EmbeddedBitmaps)} instead.", error: true)]
		public bool IsEmbeddedBitmapText {
			get => GetFont ().EmbeddedBitmaps;
			set => GetFont ().EmbeddedBitmaps = value;
		}

		/// <summary>Gets or sets a value indicating whether automatic font hinting is forced.</summary>
		/// <value><see langword="true" /> if automatic hinting is forced; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ForceAutoHinting)} instead.", error: true)]
		public bool IsAutohinted {
			get => GetFont ().ForceAutoHinting;
			set => GetFont ().ForceAutoHinting = value;
		}

		/// <summary>Gets or sets the font hinting level.</summary>
		/// <value>One of the enumeration values that specifies the font hinting level.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Hinting)} instead.", error: true)]
		public SKPaintHinting HintingLevel {
			get => (SKPaintHinting)GetFont ().Hinting;
			set => GetFont ().Hinting = (SKFontHinting)value;
		}

		/// <summary>Gets or sets a value indicating whether glyphs are emboldened.</summary>
		/// <value><see langword="true" /> if glyphs are emboldened; otherwise, <see langword="false" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Embolden)} instead.", error: true)]
		public bool FakeBoldText {
			get => GetFont ().Embolden;
			set => GetFont ().Embolden = value;
		}

		/// <summary>Gets or sets a value indicating whether to paint a stroke or the fill.</summary>
		/// <value><see langword="true" /> to stroke; <see langword="false" /> to fill.</value>
		/// <remarks>This is a shortcut way to set <see cref="P:SkiaSharp.SKPaint.Style" /> to either <see cref="F:SkiaSharp.SKPaintStyle.Stroke" /> or <see cref="F:SkiaSharp.SKPaintStyle.Fill" />.</remarks>
		public bool IsStroke {
			get => Style != SKPaintStyle.Fill;
			set => Style = value ? SKPaintStyle.Stroke : SKPaintStyle.Fill;
		}

		/// <summary>Gets or sets the painting style.</summary>
		/// <value>One of the enumeration values that specifies the painting style.</value>
		/// <remarks>Can also be set using <see cref="P:SkiaSharp.SKPaint.IsStroke" />.</remarks>
		public SKPaintStyle Style {
			get {
				var r = SkiaApi.sk_paint_get_style (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_style (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the paint's foreground color.</summary>
		/// <value>The paint's foreground color as a 32-bit ARGB value.</value>
		/// <remarks>The color is a 32-bit value containing ARGB. This 32-bit value is not premultiplied, meaning that its alpha can be any value, regardless of the values of R, G and B.</remarks>
		public SKColor Color {
			get {
				var r = SkiaApi.sk_paint_get_color (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_color (Handle, (uint)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the paint's color as an <see cref="T:SkiaSharp.SKColorF" /> (floating-point RGBA).</summary>
		/// <value>The color in floating-point representation.</value>
		/// <remarks>This property provides higher precision color values than the <see cref="P:SkiaSharp.SKPaint.Color" /> property.</remarks>
		public SKColorF ColorF {
			get {
				SKColorF color4f;
				SkiaApi.sk_paint_get_color4f (Handle, &color4f);
				GC.KeepAlive (this);
				return color4f;
			}
			set {
				SkiaApi.sk_paint_set_color4f (Handle, &value, IntPtr.Zero);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Sets the paint's color using a floating-point color value in the specified color space.</summary>
		/// <param name="color">The color value as an <see cref="T:SkiaSharp.SKColorF" />.</param>
		/// <param name="colorspace">The <see cref="T:SkiaSharp.SKColorSpace" /> for interpreting the color.</param>
		/// <remarks />
		public void SetColor (SKColorF color, SKColorSpace colorspace)
		{
			SkiaApi.sk_paint_set_color4f (Handle, &color, colorspace?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (colorspace);
			GC.KeepAlive (this);
		}

		/// <summary>Gets or sets the paint's stroke width.</summary>
		/// <value>The stroke width in pixels.</value>
		/// <remarks>This is used whenever the <see cref="P:SkiaSharp.SKPaint.Style" /> is <see cref="F:SkiaSharp.SKPaintStyle.Stroke" /> or <see cref="F:SkiaSharp.SKPaintStyle.StrokeAndFill" />. The value of zero is the special hairline mode. Hairlines always draw with a width of 1 pixel, regardless of the transformation matrix.</remarks>
		public float StrokeWidth {
			get {
				var r = SkiaApi.sk_paint_get_stroke_width (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_width (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the paint's miter limit.</summary>
		/// <value>The miter limit value.</value>
		/// <remarks>This is used whenever the <see cref="P:SkiaSharp.SKPaint.Style" /> is <see cref="F:SkiaSharp.SKPaintStyle.Stroke" /> or <see cref="F:SkiaSharp.SKPaintStyle.StrokeAndFill" /> to control the behavior of miter joins when the joins' angle is sharp.</remarks>
		public float StrokeMiter {
			get {
				var r = SkiaApi.sk_paint_get_stroke_miter (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_miter (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets a value indicating how the start and end of stroked lines and paths are treated.</summary>
		/// <value>One of the enumeration values that specifies how the start and end of stroked lines and paths are treated.</value>
		/// <remarks />
		public SKStrokeCap StrokeCap {
			get {
				var r = SkiaApi.sk_paint_get_stroke_cap (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_cap (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the path's join type.</summary>
		/// <value>One of the enumeration values that specifies the join type.</value>
		/// <remarks />
		public SKStrokeJoin StrokeJoin {
			get {
				var r = SkiaApi.sk_paint_get_stroke_join (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_stroke_join (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the shader to use when painting.</summary>
		/// <value>The shader, or <see langword="null" /> if none is set.</value>
		/// <remarks />
		public SKShader Shader {
			get {
				var r = SKShader.GetObject (SkiaApi.sk_paint_get_shader (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_shader (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the mask filter to use when painting.</summary>
		/// <value>The mask filter, or <see langword="null" /> if none is set.</value>
		/// <remarks>Mask filters control the transformations on the alpha channel before primitives are drawn. Examples are blur or emboss.</remarks>
		public SKMaskFilter MaskFilter {
			get {
				var r = SKMaskFilter.GetObject (SkiaApi.sk_paint_get_maskfilter (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_maskfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the paint's color filter.</summary>
		/// <value>The color filter applied to source colors before drawing, or <see langword="null" /> if no filter is applied.</value>
		/// <remarks />
		public SKColorFilter ColorFilter {
			get {
				var r = SKColorFilter.GetObject (SkiaApi.sk_paint_get_colorfilter (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_colorfilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the image filter.</summary>
		/// <value>The image filter, or <see langword="null" /> if none is set.</value>
		/// <remarks />
		public SKImageFilter ImageFilter {
			get {
				var r = SKImageFilter.GetObject (SkiaApi.sk_paint_get_imagefilter (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_imagefilter (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the blend mode.</summary>
		/// <value>The blend mode used for combining source and destination colors during drawing operations.</value>
		/// <remarks />
		public SKBlendMode BlendMode {
			get {
				var r = SkiaApi.sk_paint_get_blendmode (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_blendmode (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the custom <see cref="T:SkiaSharp.SKBlender" /> for compositing.</summary>
		/// <value>The custom blender, or <see langword="null" /> to use the default <see cref="P:SkiaSharp.SKPaint.BlendMode" />.</value>
		/// <remarks>When set, the blender overrides the <see cref="P:SkiaSharp.SKPaint.BlendMode" /> property.</remarks>
		public SKBlender Blender {
			get {
				var r = SKBlender.GetObject (SkiaApi.sk_paint_get_blender (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_blender (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the legacy filtering quality.</summary>
		/// <value>One of the enumeration values that specifies the filtering quality.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKSamplingOptions)} instead.", error: true)]
		public SKFilterQuality FilterQuality {
			get {
				var r = (SKFilterQuality)SkiaApi.sk_compatpaint_get_filter_quality (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_filter_quality (Handle, (int)value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the typeface.</summary>
		/// <value>The typeface, or <see langword="null" />.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Typeface)} instead.", error: true)]
		public SKTypeface Typeface {
			get => GetFont ().Typeface;
			set => GetFont ().Typeface = value;
		}

		/// <summary>Gets or sets the text size.</summary>
		/// <value>The text size.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Size)} instead.", error: true)]
		public float TextSize {
			get => GetFont ().Size;
			set => GetFont ().Size = value;
		}

		/// <summary>Gets or sets the horizontal text alignment.</summary>
		/// <value>One of the enumeration values that specifies the text alignment.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextAlign)} method overloads instead.", error: true)]
		public SKTextAlign TextAlign {
			get {
				var r = SkiaApi.sk_compatpaint_get_text_align (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_text_align (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the encoding used for byte text.</summary>
		/// <value>One of the enumeration values that specifies the text encoding.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextEncoding)} method overloads instead.", error: true)]
		public SKTextEncoding TextEncoding {
			get {
				var r = SkiaApi.sk_compatpaint_get_text_encoding (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_compatpaint_set_text_encoding (Handle, value);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Gets or sets the horizontal text scale.</summary>
		/// <value>The horizontal text scale.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ScaleX)} instead.", error: true)]
		public float TextScaleX {
			get => GetFont ().ScaleX;
			set => GetFont ().ScaleX = value;
		}

		/// <summary>Gets or sets the text skew.</summary>
		/// <value>The text skew.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.SkewX)} instead.", error: true)]
		public float TextSkewX {
			get => GetFont ().SkewX;
			set => GetFont ().SkewX = value;
		}

		/// <summary>Gets or sets the path effect to use when painting.</summary>
		/// <value>The path effect, or <see langword="null" /> if none is set.</value>
		/// <remarks />
		public SKPathEffect PathEffect {
			get {
				var r = SKPathEffect.GetObject (SkiaApi.sk_paint_get_path_effect (Handle));
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_paint_set_path_effect (Handle, value == null ? IntPtr.Zero : value.Handle);
				GC.KeepAlive (value);
				GC.KeepAlive (this);
			}
		}

		// FontSpacing

		/// <summary>Gets the recommended spacing between text baselines.</summary>
		/// <value>The recommended baseline spacing.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Spacing)} instead.", error: true)]
		public float FontSpacing =>
			GetFont ().Spacing;

		// FontMetrics

		/// <summary>Gets the font metrics.</summary>
		/// <value>The font metrics.</value>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.Metrics)} instead.", error: true)]
		public SKFontMetrics FontMetrics {
			get {
				return GetFont ().Metrics;
			}
		}

		/// <summary>Gets the font metrics.</summary>
		/// <param name="metrics">When this method returns, contains the font metrics.</param>
		/// <returns>The recommended spacing between text baselines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetFontMetrics)}() instead.", error: true)]
		public float GetFontMetrics (out SKFontMetrics metrics) =>
			GetFont ().GetFontMetrics (out metrics);

		// Clone

		/// <summary>Creates a copy of the current paint.</summary>
		/// <returns>Returns the copy.</returns>
		/// <remarks>The copy is a shallow copy, all references will still point to the same objects.</remarks>
		public SKPaint Clone ()
		{
			var r = GetObject (SkiaApi.sk_compatpaint_clone (Handle))!;
			GC.KeepAlive (this);
			return r;
		}

		// MeasureText

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (string text) =>
			GetFont ().MeasureText (text, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<char> text) =>
			GetFont ().MeasureText (text, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (byte[] text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<byte> text) =>
			GetFont ().MeasureText (text, TextEncoding, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, int length) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, IntPtr length) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (string text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<char> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, out bounds, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (byte[] text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (ReadOnlySpan<byte> text, ref SKRect bounds) =>
			GetFont ().MeasureText (text, TextEncoding, out bounds, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, int length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, length, TextEncoding, out bounds, this);

		/// <summary>Measures the width of the text using this paint's font.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The measured width of the text.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.MeasureText)}() instead.", error: true)]
		public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds) =>
			GetFont ().MeasureText (buffer, (int)length, TextEncoding, out bounds, this);

		// BreakText

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (string text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (string text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <param name="measuredText">When this method returns, contains the measured text prefix.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
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

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth) =>
			GetFont ().BreakText (text, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<char> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, maxWidth, out measuredWidth, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (byte[] text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (byte[] text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (text, TextEncoding, maxWidth, out measuredWidth, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, int length, float maxWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, length, TextEncoding, maxWidth, out measuredWidth, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out _, this);

		/// <summary>Measures the longest text prefix that fits within the specified width.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="maxWidth">The maximum width of the measured text.</param>
		/// <param name="measuredWidth">When this method returns, contains the width of the measured text.</param>
		/// <returns>The length of the measured text prefix.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.BreakText)}() instead.", error: true)]
		public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth) =>
			GetFont ().BreakText (buffer, (int)length, TextEncoding, maxWidth, out measuredWidth, this);

		// GetTextPath

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (string text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<char> text, float x, float y) =>
			GetFont ().GetTextPath (text, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (byte[] text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, float x, float y) =>
			GetFont ().GetTextPath (text, TextEncoding, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, float x, float y) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, new SKPoint (x, y));

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (string text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (byte[] text, SKPoint[] points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (text, TextEncoding, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, int length, ReadOnlySpan<SKPoint> points) =>
			GetFont ().GetTextPath (buffer, length, TextEncoding, points);

		/// <summary>Creates a path containing the outlines of the text.</summary>
		/// <param name="buffer">The address of the text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="points">The positions of the glyphs.</param>
		/// <returns>A path containing the text outlines.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetTextPath)}() instead.", error: true)]
		public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points) =>
			GetFont ().GetTextPath (buffer, (int)length, TextEncoding, points);

		// GetFillPath

		/// <summary>Creates a new path from the result of applying any and all effects to a source path.</summary>
		/// <param name="src">The source path.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
		public SKPath GetFillPath (SKPath src)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.Identity);

		/// <summary>Creates a new path from the result of applying any and all effects to a source path.</summary>
		/// <param name="src">The source path.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
		public SKPath GetFillPath (SKPath src, float resScale)
			=> GetFillPath (src, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Creates a new path from the result of applying any and all effects to a source path with a transformation matrix.</summary>
		/// <param name="src">The source path to transform.</param>
		/// <param name="matrix">The transformation matrix to apply.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
		public SKPath GetFillPath (SKPath src, SKMatrix matrix)
			=> GetFillPath (src, (SKRect*)null, matrix);

		/// <summary>Creates a new path from the result of applying any and all effects to a source path.</summary>
		/// <param name="src">The source path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
		public SKPath GetFillPath (SKPath src, SKRect cullRect)
			=> GetFillPath (src, &cullRect, SKMatrix.Identity);

		/// <summary>Creates a new path from the result of applying any and all effects to a source path.</summary>
		/// <param name="src">The source path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
		public SKPath GetFillPath (SKPath src, SKRect cullRect, float resScale)
			=> GetFillPath (src, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Creates a new path from the result of applying effects to a source path with culling and transformation.</summary>
		/// <param name="src">The source path to transform.</param>
		/// <param name="cullRect">The culling rectangle to limit the path effect.</param>
		/// <param name="matrix">The transformation matrix to apply.</param>
		/// <returns>Returns the resulting fill path, or <see langword="null" /> if the source path should be drawn with a hairline.</returns>
		/// <remarks />
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

		/// <summary>Applies any and all effects to a source path, returning the result in the destination.</summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <returns><see langword="true" /> if the path should be filled, or false if it should be drawn with a hairline.</returns>
		/// <remarks />
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		/// <summary>Applies any and all effects to a source path, returning the result in the destination.</summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns><see langword="true" /> if the path should be filled, or false if it should be drawn with a hairline.</returns>
		/// <remarks />
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Applies any and all effects to a source path with a transformation matrix, writing the result to the destination.</summary>
		/// <param name="src">The source path to transform.</param>
		/// <param name="dst">The destination path to receive the result.</param>
		/// <param name="matrix">The transformation matrix to apply.</param>
		/// <returns><see langword="true" /> if the path should be filled; <see langword="false" /> if it should be drawn with a hairline.</returns>
		/// <remarks />
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		/// <summary>Applies any and all effects to a source path, returning the result in the destination.</summary>
		/// <param name="src">The source path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="cullRect">The limit to be passed to the path effect.</param>
		/// <returns><see langword="true" /> if the path should be filled, or false if it should be drawn with a hairline.</returns>
		/// <remarks />
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		/// <summary>Applies any and all effects to a source path, returning the result in the destination.</summary>
		/// <param name="src">The input path.</param>
		/// <param name="dst">The output path.</param>
		/// <param name="cullRect">The destination path may be culled to this rectangle.</param>
		/// <param name="resScale">If &gt; 1, increase precision, else if (0 &lt; res &lt; 1) reduce precision in favor of speed/size.</param>
		/// <returns><see langword="true" /> if the path should be filled, or false if it should be drawn with a hairline.</returns>
		/// <remarks />
		[Obsolete ("Use the SKPathBuilder overload instead.")]
		public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Applies any and all effects to a source path with culling and transformation, writing the result to the destination.</summary>
		/// <param name="src">The source path to transform.</param>
		/// <param name="dst">The destination path to receive the result.</param>
		/// <param name="cullRect">The culling rectangle to limit the path effect.</param>
		/// <param name="matrix">The transformation matrix to apply.</param>
		/// <returns><see langword="true" /> if the path should be filled; <see langword="false" /> if it should be drawn with a hairline.</returns>
		/// <remarks />
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

		/// <summary>Computes the filled geometry of the source path using this paint's style and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.Identity);

		/// <summary>Computes the filled geometry of the source path at the specified resolution scale and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <param name="resScale">The scale factor used to determine resolution-dependent path simplification.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst, float resScale)
			=> GetFillPath (src, dst, (SKRect*)null, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Computes the filled geometry of the source path after applying a transformation matrix and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <param name="matrix">The transformation matrix to apply before computing the fill.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKMatrix matrix)
			=> GetFillPath (src, dst, (SKRect*)null, matrix);

		/// <summary>Computes the filled geometry of the source path within the cull rectangle and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <param name="cullRect">A rectangle used to cull path elements that lie entirely outside its bounds.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.Identity);

		/// <summary>Computes the filled geometry of the source path within the cull rectangle at the specified resolution scale, and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <param name="cullRect">A rectangle used to cull path elements that lie entirely outside its bounds.</param>
		/// <param name="resScale">The scale factor used to determine resolution-dependent path simplification.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, float resScale)
			=> GetFillPath (src, dst, &cullRect, SKMatrix.CreateScale (resScale, resScale));

		/// <summary>Computes the filled geometry of the source path within the cull rectangle after applying a transformation matrix, and writes the result to the destination path builder.</summary>
		/// <param name="src">The source path to fill.</param>
		/// <param name="dst">The <see cref="T:SkiaSharp.SKPathBuilder" /> to which the filled path is written.</param>
		/// <param name="cullRect">A rectangle used to cull path elements that lie entirely outside its bounds.</param>
		/// <param name="matrix">The transformation matrix to apply before computing the fill.</param>
		/// <returns><see langword="true" /> if the fill path was successfully computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, SKMatrix matrix)
			=> GetFillPath (src, dst, &cullRect, matrix);

		private bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect* cullRect, SKMatrix matrix)
		{
			_ = src ?? throw new ArgumentNullException (nameof (src));
			_ = dst ?? throw new ArgumentNullException (nameof (dst));

			var result = SkiaApi.sk_paint_get_fill_path (Handle, src.Handle, dst.Handle, cullRect, &matrix);
			GC.KeepAlive (src);
			GC.KeepAlive (dst);
			GC.KeepAlive (this);
			return result;
		}

		// GetFastBounds

		/// <summary>Attempts to compute a conservative bounding rectangle that contains the result of drawing the supplied bounds with this paint.</summary>
		/// <param name="bounds">The original bounds of the geometry to be drawn, before any paint effects are considered.</param>
		/// <param name="fastBounds">When this method returns, contains a conservative bounding rectangle that includes any stroke width, mask filter, image filter, or other effects on this paint applied to <paramref name="bounds" />; otherwise, <see cref="F:SkiaSharp.SKRect.Empty" /> if the bounds cannot be computed. This parameter is treated as uninitialized.</param>
		/// <returns><see langword="true" /> if a conservative bounding rectangle could be computed and was written to <paramref name="fastBounds" />; otherwise, <see langword="false" />.</returns>
		/// <remarks><para>The returned bounds are conservative: they may be larger than the actual affected pixels, but they are guaranteed to enclose them. This is useful for quickly culling draws that fall outside a clip without performing the full draw.</para></remarks>
		public bool GetFastBounds (SKRect bounds, out SKRect fastBounds)
		{
			if (!SkiaApi.sk_paint_can_compute_fast_bounds (Handle)) {
				GC.KeepAlive (this);
				fastBounds = SKRect.Empty;
				return false;
			}

			fixed (SKRect* storage = &fastBounds) {
				SkiaApi.sk_paint_compute_fast_bounds (Handle, &bounds, storage);
			}
			GC.KeepAlive (this);
			return true;
		}

		// CountGlyphs

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (string text) =>
			GetFont ().CountGlyphs (text);

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().CountGlyphs (text);

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (byte[] text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().CountGlyphs (text, TextEncoding);

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (IntPtr text, int length) =>
			GetFont ().CountGlyphs (text, length, TextEncoding);

		/// <summary>Counts the glyphs in the text.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The number of glyphs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.CountGlyphs)}() instead.", error: true)]
		public int CountGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().CountGlyphs (text, (int)length, TextEncoding);

		// GetGlyphs

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (string text) =>
			GetFont ().GetGlyphs (text);

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphs (text);

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (byte[] text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphs (text, TextEncoding);

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (IntPtr text, int length) =>
			GetFont ().GetGlyphs (text, length, TextEncoding);

		/// <summary>Converts the text to glyph IDs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The glyph IDs.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphs)}() instead.", error: true)]
		public ushort[] GetGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphs (text, (int)length, TextEncoding);

		// ContainsGlyphs

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (string text) =>
			GetFont ().ContainsGlyphs (text);

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			GetFont ().ContainsGlyphs (text);

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (byte[] text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (ReadOnlySpan<byte> text) =>
			GetFont ().ContainsGlyphs (text, TextEncoding);

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (IntPtr text, int length) =>
			GetFont ().ContainsGlyphs (text, length, TextEncoding);

		/// <summary>Determines whether the text can be represented by glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns><see langword="true" /> if every character has a glyph; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.ContainsGlyphs)}() instead.", error: true)]
		public bool ContainsGlyphs (IntPtr text, IntPtr length) =>
			GetFont ().ContainsGlyphs (text, (int)length, TextEncoding);

		// GetGlyphPositions

		/// <summary>Gets the positions of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph positions.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (string text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		/// <summary>Gets the positions of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph positions.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<char> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, origin);

		/// <summary>Gets the positions of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph positions.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (ReadOnlySpan<byte> text, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, TextEncoding, origin);

		/// <summary>Gets the positions of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph positions.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphPositions)}() instead.", error: true)]
		public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin = default) =>
			GetFont ().GetGlyphPositions (text, length, TextEncoding, origin);

		// GetGlyphOffsets

		/// <summary>Gets the horizontal offsets of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph offsets.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (string text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		/// <summary>Gets the horizontal offsets of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph offsets.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (ReadOnlySpan<char> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, origin);

		/// <summary>Gets the horizontal offsets of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph offsets.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (ReadOnlySpan<byte> text, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, TextEncoding, origin);

		/// <summary>Gets the horizontal offsets of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="origin">The origin of the text.</param>
		/// <returns>The glyph offsets.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphOffsets)}() instead.", error: true)]
		public float[] GetGlyphOffsets (IntPtr text, int length, float origin = 0f) =>
			GetFont ().GetGlyphOffsets (text, length, TextEncoding, origin);

		// GetGlyphWidths

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (string text) =>
			GetFont ().GetGlyphWidths (text, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text) =>
			GetFont ().GetGlyphWidths (text, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (byte[] text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, int length) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (string text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<char> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, out bounds, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (ReadOnlySpan<byte> text, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, TextEncoding, out bounds, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, length, TextEncoding, out bounds, this);

		/// <summary>Gets the widths of the text glyphs.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="bounds">When this method returns, contains the text bounds.</param>
		/// <returns>The glyph widths.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)}.{nameof (SKFont.GetGlyphWidths)}() instead.", error: true)]
		public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds) =>
			GetFont ().GetGlyphWidths (text, (int)length, TextEncoding, out bounds, this);

		// GetTextIntercepts

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.Create (text, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text.AsSpan (), x, y, upperBounds, lowerBounds);

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.Create (text, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds) =>
			GetTextIntercepts (text, (int)length, x, y, upperBounds, lowerBounds);

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.Create (text, length, TextEncoding, GetFont (), new SKPoint (x, y));
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetTextIntercepts (SKTextBlob)

		/// <summary>Gets the text intercept intervals within the specified bounds.</summary>
		/// <param name="text">The text blob.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			return text.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetPositionedTextIntercepts

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreatePositioned (text, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text.AsSpan (), positions, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreatePositioned (text, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds) =>
			GetPositionedTextIntercepts (text, (IntPtr)length, positions, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="positions">The positions of the glyphs.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreatePositioned (text, (int)length, TextEncoding, GetFont (), positions);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// GetHorizontalTextIntercepts

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<char> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreateHorizontal (text, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text.AsSpan (), xpositions, y, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (ReadOnlySpan<byte> text, ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds)
		{
			using var blob = SKTextBlob.CreateHorizontal (text, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds) =>
			GetHorizontalTextIntercepts (text, (IntPtr)length, xpositions, y, upperBounds, lowerBounds);

		/// <summary>Gets the intercept intervals of horizontally positioned text within the specified bounds.</summary>
		/// <param name="text">The text to process.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="xpositions">The horizontal positions of the glyphs.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="upperBounds">The upper intercept bound.</param>
		/// <param name="lowerBounds">The lower intercept bound.</param>
		/// <returns>The intercept intervals.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKTextBlob)}.{nameof (SKTextBlob.GetIntercepts)}() instead.", error: true)]
		public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds)
		{
			if (text == IntPtr.Zero && length != IntPtr.Zero)
				throw new ArgumentNullException (nameof (text));

			using var blob = SKTextBlob.CreateHorizontal (text, (int)length, TextEncoding, GetFont (), xpositions, y);
			return blob.GetIntercepts (upperBounds, lowerBounds, this);
		}

		// Font

		/// <summary>Creates a font from the current paint.</summary>
		/// <returns>A font that represents the current paint's font settings.</returns>
		/// <remarks />
		[Obsolete ($"Use {nameof (SKFont)} instead.", error: true)]
		public SKFont ToFont ()
		{
			var r = SKFont.GetObject (SkiaApi.sk_compatpaint_make_font (Handle));
			GC.KeepAlive (this);
			return r;
		}

		[Obsolete ($"Use {nameof (SKFont)} instead.", error: true)]
		private SKFont GetFont () =>
			font ??= OwnedBy (SKFont.GetObject (SkiaApi.sk_compatpaint_get_font (Handle), false), this);

		// Internal compat-paint bypass helpers used by the non-obsolete public APIs
		// in SKCanvas (DrawImage/DrawAtlas/DrawText) and SkiaSharp.HarfBuzz that
		// still respect the legacy paint.FilterQuality / paint.TextAlign /
		// paint.TextEncoding / paint.GetFont state. These mirror the obsolete
		// properties but avoid the CS0619 compile error and ref-assembly stripping
		// when called from a non-obsolete context. Exposed to SkiaSharp.HarfBuzz
		// via InternalsVisibleTo. Remove together with SkCompatPaint in Phase 2
		// of #3732.
		[Obsolete ("Use SKFont directly instead.")]
		internal SKTextAlign GetLegacyTextAlign () =>
			SkiaApi.sk_compatpaint_get_text_align (Handle);

		[Obsolete ("Use SKFont directly instead.")]
		internal SKTextEncoding GetLegacyTextEncoding () =>
			SkiaApi.sk_compatpaint_get_text_encoding (Handle);

		[Obsolete ("Use SKFont directly instead.")]
		internal SKFont GetLegacyFont () =>
			GetFont ();

		[Obsolete ("Use SKSamplingOptions directly instead.")]
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
