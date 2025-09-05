#nullable disable

using System;

namespace SkiaSharp
{
	// TODO: carefully consider the `PeekPixels`, `ReadPixels`

	/// <summary>
	/// Encapsulates all of the state about drawing into a device (bitmap or surface).
	/// </summary>
	/// <remarks>
	/// A canvas encapsulates all of the state about drawing into a device (bitmap or
	/// surface).
	/// This includes a reference to the device itself, and a stack of matrix/clip
	/// values. For any given draw call (e.g. DrawRect), the geometry of the object
	/// being drawn is transformed by the concatenation of all the matrices in the
	/// stack. The transformed geometry is clipped by the intersection of all of the
	/// clips in the stack.
	/// While the canvas holds the state of the drawing device, the state (style) of
	/// the object being drawn is held by the paint, which is provided as a parameter
	/// to each of the "Draw" methods. The paint holds attributes such as color,
	/// typeface, the text size, the stroke width, the shader (for example, gradients,
	/// patterns), etc.
	/// The canvas is returned when accessing the
	/// <see cref="SkiaSharp.SKSurface.Canvas" /> property of a
	/// surface.
	/// ### Construction
	/// SkiaSharp has multiple backends which receive <see cref="SkiaSharp.SKCanvas" />
	/// drawing commands, including:
	/// * Raster Surface
	/// * GPU Surface
	/// * PDF Document
	/// * XPS Document _(experimental)_
	/// * SVG Canvas _(experimental)_
	/// * Picture
	/// * Null Canvas _(for testing)_
	/// #### Constructing a Raster Surface
	/// The raster backend draws to a block of memory. This memory can be managed by
	/// SkiaSharp or by the client.
	/// The recommended way of creating a canvas for the Raster and Ganesh backends is
	/// to use a <see cref="SkiaSharp.SKSurface" />, which is an object that manages the
	/// memory into which the canvas commands are drawn.
	/// ```csharp
	/// // define the surface properties
	/// var info = new SKImageInfo(256, 256);
	/// // construct a new surface
	/// var surface = SKSurface.Create(info);
	/// // get the canvas from the surface
	/// var canvas = surface.Canvas;
	/// // draw on the canvas ...
	/// ```
	/// Alternatively, we could have specified the memory for the surface explicitly,
	/// instead of asking SkiaSharp to manage it.
	/// ```csharp
	/// // define the surface properties
	/// var info = new SKImageInfo(256, 256);
	/// // allocate memory
	/// var memory = Marshal.AllocCoTaskMem(info.BytesSize);
	/// // construct a surface around the existing memory
	/// var surface = SKSurface.Create(info, memory, info.RowBytes);
	/// // get the canvas from the surface
	/// var canvas = surface.Canvas;
	/// // draw on the canvas ...
	/// ```
	/// #### Constructing a GPU Surface
	/// GPU surfaces must have a <see cref="SkiaSharp.GRContext" /> object which manages the
	/// GPU context, and related caches for textures and fonts.
	/// <see cref="SkiaSharp.GRContext" /> objects are matched one to one with OpenGL contexts
	/// or Vulkan devices. That is, all <see cref="SkiaSharp.SKSurface" /> instances that will
	/// be rendered to using the same OpenGL context or Vulkan device should share a
	/// <see cref="SkiaSharp.GRContext" />.
	/// SkiaSharp does not create an OpenGL context or a Vulkan device for you. In
	/// OpenGL mode it also assumes that the correct OpenGL context has been made
	/// current to the current thread when SkiaSharp calls are made.
	/// ```csharp
	/// // an OpenGL context must be created and set as current
	/// // define the surface properties
	/// var info = new SKImageInfo(256, 256);
	/// // create the surface
	/// var context = GRContext.CreateGl();
	/// var surface = SKSurface.Create(context, false, info);
	/// // get the canvas from the surface
	/// var canvas = surface.Canvas;
	/// // draw on the canvas ...
	/// ```
	/// #### Constructing a PDF Document
	/// The PDF backend uses <see cref="SkiaSharp.SKDocument" /> instead of
	/// <see cref="SkiaSharp.SKSurface" />, since a document must include multiple pages.
	/// ```csharp
	/// // create the document
	/// var stream = SKFileWStream.OpenStream("document.pdf");
	/// var document = SKDocument.CreatePdf(stream);
	/// // get the canvas from the page
	/// var canvas = document.BeginPage(256, 256);
	/// // draw on the canvas ...
	/// // end the page and document
	/// document.EndPage();
	/// document.Close();
	/// ```
	/// #### Constructing a XPS Document _(experimental)_
	/// The XPS backend uses <see cref="SkiaSharp.SKDocument" /> instead of
	/// <see cref="SkiaSharp.SKSurface" />, since a document must include multiple pages.
	/// ```csharp
	/// // create the document
	/// var stream = SKFileWStream.OpenStream("document.xps");
	/// var document = SKDocument.CreateXps(stream);
	/// // get the canvas from the page
	/// var canvas = document.BeginPage(256, 256);
	/// // draw on the canvas ...
	/// // end the page and document
	/// document.EndPage();
	/// document.Close();
	/// ```
	/// #### Constructing a SVG Canvas _(experimental)_
	/// The SVG backend uses <see cref="SkiaSharp.SKSvgCanvas" />.
	/// ```csharp
	/// // create the canvas
	/// var stream = SKFileWStream.OpenStream("image.svg");
	/// var writer = new SKXmlStreamWriter(stream);
	/// var canvas = SKSvgCanvas.Create(SKRect.Create(256, 256), writer);
	/// // draw on the canvas ...
	/// ```
	/// #### Constructing a Picture
	/// The XPS backend uses <see cref="SkiaSharp.SKPictureRecorder" /> instead of
	/// <see cref="SkiaSharp.SKSurface" />.
	/// ```csharp
	/// // create the picture recorder
	/// var recorder = new SKPictureRecorder();
	/// // get the canvas from the page
	/// var canvas = recorder.BeginRecording(SKRect.Create(256, 256));
	/// // draw on the canvas ...
	/// // finish recording
	/// var picture = recorder.EndRecording();
	/// ```
	/// #### Constructing a Null Canvas _(for testing)_
	/// The null canvas is a canvas that ignores all drawing commands and does
	/// nothing.
	/// ```csharp
	/// // create the dummy canvas
	/// var canvas = new SKNoDrawCanvas(256, 256);
	/// // draw on the canvas ...
	/// ```
	/// ### Transformations
	/// The canvas supports a number of 2D transformations. Unlike other 2D graphic
	/// systems like CoreGraphics or Cairo, SKCanvas extends the transformations to
	/// include perspectives.
	/// You can use the <see cref="SkiaSharp.SKCanvas.Scale" />,
	/// <see cref="SkiaSharp.SKCanvas.Skew" />, <see cref="SkiaSharp.SKCanvas.Translate" />,
	/// <see cref="SkiaSharp.SKCanvas.RotateDegrees" />,
	/// <see cref="SkiaSharp.SKCanvas.RotateRadians" /> to perform some of the most common
	/// 2D transformations.
	/// For more control you can use the <see cref="SkiaSharp.SKCanvas.SetMatrix" /> to set
	/// an arbitrary transformation using the <see cref="SkiaSharp.SKMatrix" /> and the
	/// <see cref="SkiaSharp.SKCanvas.Concat" /> to concatenate an <see cref="SkiaSharp.SKMatrix" />
	/// transformation to the current matrix in use.
	/// The <see cref="SkiaSharp.SKCanvas.ResetMatrix" /> can be used to reset the state of
	/// the matrix.
	/// ### Drawing
	/// The drawing operations can take a <see cref="SkiaSharp.SKPaint" /> parameter to affect
	/// their drawing. You use <see cref="SkiaSharp.SKPaint" /> objects to cache the style and
	/// color information to draw geometries, texts and bitmaps.
	/// ### Clipping and State
	/// It is possible to save the current transformations by calling the
	/// <see cref="SkiaSharp.SKCanvas.Save" /> method which preserves the current
	/// transformation matrix, you can then alter the matrix and restore the previous
	/// state by using the <see cref="SkiaSharp.SKCanvas.Restore" /> or
	/// <see cref="SkiaSharp.SKCanvas.RestoreToCount" /> methods.
	/// Additionally, it is possible to push a new state with
	/// <see cref="SkiaSharp.SKCanvas.SaveLayer" /> which will make an offscreen copy of a
	/// region, and once the drawing is completed, calling the
	/// <see cref="SkiaSharp.SKCanvas.Restore" /> method which copies the offscreen bitmap
	/// into this canvas.
	/// ## Examples
	/// ```csharp
	/// var info = new SKImageInfo(640, 480);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // set up drawing tools
	/// var paint = new SKPaint {
	/// IsAntialias = true,
	/// Color = new SKColor(0x2c, 0x3e, 0x50),
	/// StrokeCap = SKStrokeCap.Round
	/// };
	/// // create the Xamagon path
	/// var path = new SKPath();
	/// path.MoveTo(71.4311121f, 56f);
	/// path.CubicTo(68.6763107f, 56.0058575f, 65.9796704f, 57.5737917f, 64.5928855f, 59.965729f);
	/// path.LineTo(43.0238921f, 97.5342563f);
	/// path.CubicTo(41.6587026f, 99.9325978f, 41.6587026f, 103.067402f, 43.0238921f, 105.465744f);
	/// path.LineTo(64.5928855f, 143.034271f);
	/// path.CubicTo(65.9798162f, 145.426228f, 68.6763107f, 146.994582f, 71.4311121f, 147f);
	/// path.LineTo(114.568946f, 147f);
	/// path.CubicTo(117.323748f, 146.994143f, 120.020241f, 145.426228f, 121.407172f, 143.034271f);
	/// path.LineTo(142.976161f, 105.465744f);
	/// path.CubicTo(144.34135f, 103.067402f, 144.341209f, 99.9325978f, 142.976161f, 97.5342563f);
	/// path.LineTo(121.407172f, 59.965729f);
	/// path.CubicTo(120.020241f, 57.5737917f, 117.323748f, 56.0054182f, 114.568946f, 56f);
	/// path.LineTo(71.4311121f, 56f);
	/// path.Close();
	/// // draw the Xamagon path
	/// canvas.DrawPath(path, paint);
	/// }
	/// ```
	/// </remarks>
	public unsafe class SKCanvas : SKObject
	{
		private const int PatchCornerCount = 4;
		private const int PatchCubicsCount = 12;
		private const double RadiansCircle = 2.0 * Math.PI;
		private const double DegreesCircle = 360.0;

		internal SKCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a canvas with the specified bitmap to draw into.
		/// </summary>
		/// <param name="bitmap">The bitmap for the canvas to draw into.</param>
		/// <remarks>
		/// The structure of the bitmap is copied into the canvas.
		/// </remarks>
		public SKCanvas (SKBitmap bitmap)
			: this (IntPtr.Zero, true)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			Handle = SkiaApi.sk_canvas_new_from_bitmap (bitmap.Handle);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_canvas_destroy (Handle);

		/// <summary>
		/// Makes the canvas contents undefined.
		/// </summary>
		/// <remarks>
		/// Subsequent calls that read the canvas pixels, such as drawing with <see cref="SkiaSharp.SKBlendMode" />, return undefined
		/// results. Calling this method does not change clip or matrix and may do nothing, depending on the implementation
		/// of the underlying <see cref="SkiaSharp.SKSurface" />.
		/// <see cref="SkiaSharp.SKCanvas.Discard" /> allows optimized performance on subsequent draws by removing cached data associated
		/// with the underlying <see cref="SkiaSharp.SKSurface" />. It is not necessary to call <see cref="SkiaSharp.SKCanvas.Discard" /> once
		/// done with <see cref="SkiaSharp.SKCanvas" />; any cached data is deleted when the owning <see cref="SkiaSharp.SKSurface" /> is deleted.
		/// </remarks>
		public void Discard () =>
			SkiaApi.sk_canvas_discard (Handle);

		// QuickReject

		/// <summary>
		/// Checks to see if the specified rectangle, after being transformed by the current matrix, would lie completely outside of the current clip.
		/// </summary>
		/// <param name="rect">The rectangle to compare with the current clip.</param>
		/// <returns>Returns true if the rectangle (transformed by the canvas' matrix) does not intersect with the canvas' clip.</returns>
		/// <remarks>
		/// Call this to check if an area you intend to draw into is clipped out (and therefore you can skip making the draw calls).
		/// </remarks>
		public bool QuickReject (SKRect rect)
		{
			return SkiaApi.sk_canvas_quick_reject (Handle, &rect);
		}

		/// <summary>
		/// Checks to see if the specified path, after being transformed by the current matrix, would lie completely outside of the current clip.
		/// </summary>
		/// <param name="path">The path to compare with the current clip.</param>
		/// <returns>Returns true if the path (transformed by the canvas' matrix) does not intersect with the canvas' clip.</returns>
		/// <remarks>
		/// Call this to check if an area you intend to draw into is clipped out (and therefore you can skip making the draw calls).
		/// </remarks>
		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			return path.IsEmpty || QuickReject (path.Bounds);
		}

		// Save*

		#nullable enable
		/// <summary>
		/// Saves the canvas state.
		/// </summary>
		/// <returns>The value to pass to <see cref="SKCanvas.RestoreToCount(System.Int32)" /> to balance this save.</returns>
		/// <remarks>
		/// This call saves the current matrix, clip, and draw filter, and pushes a copy onto a private stack. Subsequent calls to translate, scale, rotate, skew, concatenate or clipping path or drawing filter all operate on this copy. When the balancing call to <see cref="SKCanvas.Restore" /> is made, the previous matrix, clipping, and drawing filters are restored.
		/// </remarks>
		public int Save ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SKCanvas");
			return SkiaApi.sk_canvas_save (Handle);
		}

		/// <summary>
		/// Saves the canvas state and allocates an offscreen bitmap.
		/// </summary>
		/// <param name="limit">This clipping rectangle hint to limit the size of the offscreen bitmap.</param>
		/// <param name="paint">This is copied, and is applied to the offscreen when <see cref="SKCanvas.Restore" /> is called.</param>
		/// <returns>The value to pass to <see cref="SKCanvas.RestoreToCount(System.Int32)" /> to balance this save.</returns>
		/// <remarks>
		/// This behaves the same as <see cref="SkiaSharp.SKCanvas.Save" /> but in addition it
		/// allocates an offscreen bitmap. All drawing calls are directed there, and only
		/// when the balancing call to <see cref="SkiaSharp.SKCanvas.Restore" /> is made is that
		/// offscreen transfered to the canvas (or the previous layer).
		/// The limit rectangle, is used as a hint to limit the size of the offscreen
		/// bitmap, and thus drawing may be clipped to it, though that clipping is not
		/// guaranteed to happen. If exact clipping is desired, use
		/// <see cref="SkiaSharp.SKCanvas.ClipRect(SkiaSharp.SKRect,SkiaSharp.SKClipOperation,System.Boolean)" />.
		/// </remarks>
		public int SaveLayer (SKRect limit, SKPaint? paint) =>
			SkiaApi.sk_canvas_save_layer (Handle, &limit, paint?.Handle ?? IntPtr.Zero);

		/// <summary>
		/// Saves the canvas state and allocates an offscreen bitmap.
		/// </summary>
		/// <param name="paint">This is copied, and is applied to the offscreen when <see cref="SKCanvas.Restore" /> is called.</param>
		/// <returns>The value to pass to <see cref="SKCanvas.RestoreToCount(System.Int32)" /> to balance this save.</returns>
		/// <remarks>
		/// This behaves the same as <see cref="SKCanvas.Save" /> but in addition it allocates an offscreen bitmap. All drawing calls are directed there, and only when the balancing call to <see cref="SKCanvas.Restore" /> is made is that offscreen transfered to the canvas (or the previous layer).
		/// </remarks>
		public int SaveLayer (SKPaint? paint) =>
			SkiaApi.sk_canvas_save_layer (Handle, null, paint?.Handle ?? IntPtr.Zero);

		public int SaveLayer (in SKCanvasSaveLayerRec rec)
		{
			var native = rec.ToNative ();
			return SkiaApi.sk_canvas_save_layer_rec (Handle, &native);
		}

		public int SaveLayer () =>
			SkiaApi.sk_canvas_save_layer (Handle, null, IntPtr.Zero);
		#nullable disable

		// DrawColor

		/// <summary>
		/// Fills the current clipping area with the specified color using the specified color and blend mode.
		/// </summary>
		/// <param name="color">The color to use to paint the clipping region.</param>
		/// <param name="mode">The blend mode for the color.</param>
		public void DrawColor (SKColor color, SKBlendMode mode = SKBlendMode.Src) =>
			SkiaApi.sk_canvas_draw_color (Handle, (uint)color, mode);

		/// <param name="color"></param>
		/// <param name="mode"></param>
		public void DrawColor (SKColorF color, SKBlendMode mode = SKBlendMode.Src) =>
			SkiaApi.sk_canvas_draw_color4f (Handle, color, mode);

		// DrawLine

		/// <summary>
		/// Draws a line on the canvas.
		/// </summary>
		/// <param name="p0">The first point coordinates.</param>
		/// <param name="p1">The second point coordinates.</param>
		/// <param name="paint">The paint to use when drawing the line.</param>
		public void DrawLine (SKPoint p0, SKPoint p1, SKPaint paint)
		{
			DrawLine (p0.X, p0.Y, p1.X, p1.Y, paint);
		}

		/// <summary>
		/// Draws a line on the canvas.
		/// </summary>
		/// <param name="x0">The first point x-coordinate.</param>
		/// <param name="y0">The first point y-coordinate.</param>
		/// <param name="x1">The second point x-coordinate.</param>
		/// <param name="y1">The second point y-coordinate.</param>
		/// <param name="paint">The paint to use when drawing the line.</param>
		public void DrawLine (float x0, float y0, float x1, float y1, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_line (Handle, x0, y0, x1, y1, paint.Handle);
		}

		// Clear

		/// <summary>
		/// Replaces all the pixels in the canvas' current clip with the <see cref="SKColors.Empty" /> color.
		/// </summary>
		public void Clear () =>
			Clear (SKColors.Empty);

		/// <summary>
		/// Replaces all the pixels in the canvas' current clip with the specified color.
		/// </summary>
		/// <param name="color">The color to use to replace the pixels in the current clipping region.</param>
		public void Clear (SKColor color) =>
			SkiaApi.sk_canvas_clear (Handle, (uint)color);

		/// <param name="color"></param>
		public void Clear (SKColorF color) =>
			SkiaApi.sk_canvas_clear_color4f (Handle, color);

		// Restore*

		/// <summary>
		/// Restore the canvas state.
		/// </summary>
		/// <remarks>
		/// This call balances a previous call to <see cref="SKCanvas.Save" />, and is used to remove all modifications to the matrix, clip and draw filter state since the last save call. It is an error to restore more times than was previously saved.
		/// </remarks>
		public void Restore ()
		{
			SkiaApi.sk_canvas_restore (Handle);
		}

		/// <summary>
		/// Efficiently restores the state to a specific level.
		/// </summary>
		/// <param name="count">The number of <see cref="SKCanvas.Save" /> levels to restore from, or -1 to restore all the way back to the initial value.</param>
		/// <remarks>
		/// Efficient way to pop any calls to <see cref="SKCanvas.Save" /> that happened after the save count reached <paramref name="count" />. It is an error for <paramref name="count" /> to be greater than <see cref="SKCanvas.SaveCount" />. To pop all the way back to the initial matrix/clip context set count to -1.
		/// </remarks>
		public void RestoreToCount (int count)
		{
			SkiaApi.sk_canvas_restore_to_count (Handle, count);
		}

		// Translate

		/// <summary>
		/// Pre-concatenates the current matrix with the specified translation.
		/// </summary>
		/// <param name="dx">The distance to translate in the x-direction</param>
		/// <param name="dy">The distance to translate in the y-direction.</param>
		public void Translate (float dx, float dy)
		{
			if (dx == 0 && dy == 0)
				return;

			SkiaApi.sk_canvas_translate (Handle, dx, dy);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified translation.
		/// </summary>
		/// <param name="point">The distance to translate.</param>
		public void Translate (SKPoint point)
		{
			if (point.IsEmpty)
				return;

			SkiaApi.sk_canvas_translate (Handle, point.X, point.Y);
		}

		// Scale

		/// <summary>
		/// Pre-concatenates the current matrix with the specified scale.
		/// </summary>
		/// <param name="s">The amount to scale.</param>
		public void Scale (float s)
		{
			if (s == 1)
				return;

			SkiaApi.sk_canvas_scale (Handle, s, s);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified scale.
		/// </summary>
		/// <param name="sx">The amount to scale in the x-direction.</param>
		/// <param name="sy">The amount to scale in the y-direction.</param>
		public void Scale (float sx, float sy)
		{
			if (sx == 1 && sy == 1)
				return;

			SkiaApi.sk_canvas_scale (Handle, sx, sy);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified scale.
		/// </summary>
		/// <param name="size">The amount to scale.</param>
		public void Scale (SKPoint size)
		{
			if (size.IsEmpty)
				return;

			SkiaApi.sk_canvas_scale (Handle, size.X, size.Y);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified scale, at the specific offset.
		/// </summary>
		/// <param name="sx">The amount to scale in the x-direction.</param>
		/// <param name="sy">The amount to scale in the y-direction.</param>
		/// <param name="px">The x-coordinate for the scaling center.</param>
		/// <param name="py">The y-coordinate for the scaling center.</param>
		public void Scale (float sx, float sy, float px, float py)
		{
			if (sx == 1 && sy == 1)
				return;

			Translate (px, py);
			Scale (sx, sy);
			Translate (-px, -py);
		}

		// Rotate*

		/// <summary>
		/// Pre-concatenates the current matrix with the specified rotation.
		/// </summary>
		/// <param name="degrees">The number of degrees to rotate.</param>
		public void RotateDegrees (float degrees)
		{
			if (degrees % DegreesCircle == 0)
				return;

			SkiaApi.sk_canvas_rotate_degrees (Handle, degrees);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified rotation.
		/// </summary>
		/// <param name="radians">The number of radians to rotate.</param>
		public void RotateRadians (float radians)
		{
			if (radians % RadiansCircle == 0)
				return;

			SkiaApi.sk_canvas_rotate_radians (Handle, radians);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified rotation, around the specified point.
		/// </summary>
		/// <param name="degrees">The number of degrees to rotate.</param>
		/// <param name="px">The x-coordinate of the point to rotate about.</param>
		/// <param name="py">The y-coordinate of the point to rotate about.</param>
		public void RotateDegrees (float degrees, float px, float py)
		{
			if (degrees % DegreesCircle == 0)
				return;

			Translate (px, py);
			RotateDegrees (degrees);
			Translate (-px, -py);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified rotation, around the specified point.
		/// </summary>
		/// <param name="radians">The number of radians to rotate.</param>
		/// <param name="px">The x-coordinate of the point to rotate about.</param>
		/// <param name="py">The y-coordinate of the point to rotate about.</param>
		public void RotateRadians (float radians, float px, float py)
		{
			if (radians % RadiansCircle == 0)
				return;

			Translate (px, py);
			RotateRadians (radians);
			Translate (-px, -py);
		}

		// Skew

		/// <summary>
		/// Pre-concatenates the current matrix with the specified skew.
		/// </summary>
		/// <param name="sx">The amount to skew in the x-direction.</param>
		/// <param name="sy">The amount to skew in the y-direction.</param>
		public void Skew (float sx, float sy)
		{
			if (sx == 0 && sy == 0)
				return;

			SkiaApi.sk_canvas_skew (Handle, sx, sy);
		}

		/// <summary>
		/// Pre-concatenates the current matrix with the specified skew.
		/// </summary>
		/// <param name="skew">The amount to skew.</param>
		public void Skew (SKPoint skew)
		{
			if (skew.IsEmpty)
				return;

			SkiaApi.sk_canvas_skew (Handle, skew.X, skew.Y);
		}

		// Concat

		/// <summary>
		/// Pre-concatenates the provided transformation matrix with the current transformation matrix.
		/// </summary>
		/// <param name="m">Transformation matrix to pre-concatenate.</param>
		public void Concat (in SKMatrix m) =>
			Concat ((SKMatrix44)m);

		public void Concat (in SKMatrix44 m)
		{
			fixed (SKMatrix44* ptr = &m) {
				SkiaApi.sk_canvas_concat (Handle, ptr);
			}
		}

		// Clip*

		/// <summary>
		/// Modify the current clip with the specified rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to combine with the current clip.</param>
		/// <param name="operation">The clip operator to apply to the current clip.</param>
		/// <param name="antialias">Whether or not the clip should be antialiased.</param>
		public void ClipRect (SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			SkiaApi.sk_canvas_clip_rect_with_operation (Handle, &rect, operation, antialias);
		}

		/// <summary>
		/// Modify the current clip with the specified rounded rectangle.
		/// </summary>
		/// <param name="rect">The rounded rectangle to combine with the current clip.</param>
		/// <param name="operation">The clip operator to apply to the current clip</param>
		/// <param name="antialias">Whether or not the clip should be antialiased.</param>
		public void ClipRoundRect (SKRoundRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));

			SkiaApi.sk_canvas_clip_rrect_with_operation (Handle, rect.Handle, operation, antialias);
		}

		/// <summary>
		/// Modify the current clip with the specified path.
		/// </summary>
		/// <param name="path">The path to combine with the current clip.</param>
		/// <param name="operation">The clip operator to apply to the current clip</param>
		/// <param name="antialias">Whether or not the clip should be antialiased.</param>
		public void ClipPath (SKPath path, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			SkiaApi.sk_canvas_clip_path_with_operation (Handle, path.Handle, operation, antialias);
		}

		/// <summary>
		/// Modify the current clip with the specified region.
		/// </summary>
		/// <param name="region">The region to combine with the current clip.</param>
		/// <param name="operation">The region operator to apply to the current clip.</param>
		public void ClipRegion (SKRegion region, SKClipOperation operation = SKClipOperation.Intersect)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			SkiaApi.sk_canvas_clip_region (Handle, region.Handle, operation);
		}

		/// <summary>
		/// Gets the bounds of the current clip (in local coordinates).
		/// </summary>
		public SKRect LocalClipBounds {
			get {
				GetLocalClipBounds (out var bounds);
				return bounds;
			}
		}

		/// <summary>
		/// Gets the bounds of the current clip (in device coordinates).
		/// </summary>
		public SKRectI DeviceClipBounds {
			get {
				GetDeviceClipBounds (out var bounds);
				return bounds;
			}
		}

		public bool IsClipEmpty => SkiaApi.sk_canvas_is_clip_empty (Handle);

		public bool IsClipRect => SkiaApi.sk_canvas_is_clip_rect (Handle);

		/// <summary>
		/// Returns the bounds of the current clip (in local coordinates).
		/// </summary>
		/// <param name="bounds">The resulting clip bounds.</param>
		/// <returns>Returns true if the clip bounds are non-empty, otherwise false.</returns>
		/// <remarks>
		/// This can be useful in that it tells you that drawing outside of these bounds will be clipped out.
		/// </remarks>
		public bool GetLocalClipBounds (out SKRect bounds)
		{
			fixed (SKRect* b = &bounds) {
				return SkiaApi.sk_canvas_get_local_clip_bounds (Handle, b);
			}
		}

		/// <summary>
		/// Returns the bounds of the current clip (in device coordinates).
		/// </summary>
		/// <param name="bounds">The resulting clip bounds.</param>
		/// <returns>Returns true if the clip bounds are non-empty, otherwise false.</returns>
		public bool GetDeviceClipBounds (out SKRectI bounds)
		{
			fixed (SKRectI* b = &bounds) {
				return SkiaApi.sk_canvas_get_device_clip_bounds (Handle, b);
			}
		}

		// DrawPaint

		/// <summary>
		/// Fills the current clipping path with the specified paint.
		/// </summary>
		/// <param name="paint">The paint used to fill the current clipping path.</param>
		public void DrawPaint (SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_paint (Handle, paint.Handle);
		}

		// DrawRegion

		/// <summary>
		/// Draws the outline of the specified region using the specified paint.
		/// </summary>
		/// <param name="region">The region to be drawn.</param>
		/// <param name="paint">The paint to use when drawing the region.</param>
		public void DrawRegion (SKRegion region, SKPaint paint)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_region (Handle, region.Handle, paint.Handle);
		}

		// DrawRect

		/// <summary>
		/// Draws a rectangle in the canvas.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="w">The rectangle width.</param>
		/// <param name="h">The rectangle height.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		public void DrawRect (float x, float y, float w, float h, SKPaint paint)
		{
			DrawRect (SKRect.Create (x, y, w, h), paint);
		}

		/// <summary>
		/// Draws a rectangle in the canvas.
		/// </summary>
		/// <param name="rect">The rectangle to draw.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		public void DrawRect (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_rect (Handle, &rect, paint.Handle);
		}

		// DrawRoundRect

		/// <summary>
		/// Draws a rounded rectangle in the canvas.
		/// </summary>
		/// <param name="rect">The rounded rectangle to draw.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		/// <remarks>
		/// The paint to use when drawing the rounded rectangle.
		/// </remarks>
		public void DrawRoundRect (SKRoundRect rect, SKPaint paint)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_rrect (Handle, rect.Handle, paint.Handle);
		}

		/// <summary>
		/// Draws a rounded rectangle in the canvas.
		/// </summary>
		/// <param name="x">The x-coordinate of the rectangle.</param>
		/// <param name="y">The y-coordinate of the rectangle.</param>
		/// <param name="w">The rectangle width.</param>
		/// <param name="h">The rectangle height.</param>
		/// <param name="rx">The x-radius of the oval used to round the corners.</param>
		/// <param name="ry">The y-radius of the oval used to round the corners.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		public void DrawRoundRect (float x, float y, float w, float h, float rx, float ry, SKPaint paint)
		{
			DrawRoundRect (SKRect.Create (x, y, w, h), rx, ry, paint);
		}

		/// <summary>
		/// Draws a rounded rectangle in the canvas.
		/// </summary>
		/// <param name="rect">The rectangle to draw.</param>
		/// <param name="rx">The x-radius of the oval used to round the corners.</param>
		/// <param name="ry">The y-radius of the oval used to round the corners.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		public void DrawRoundRect (SKRect rect, float rx, float ry, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_round_rect (Handle, &rect, rx, ry, paint.Handle);
		}

		/// <summary>
		/// Draws a rounded rectangle in the canvas.
		/// </summary>
		/// <param name="rect">The rectangle to draw.</param>
		/// <param name="r">The radius of the oval used to round the corners.</param>
		/// <param name="paint">The paint to use when drawing the rectangle.</param>
		/// <remarks>
		/// The paint to use when drawing the rectangle.
		/// </remarks>
		public void DrawRoundRect (SKRect rect, SKSize r, SKPaint paint)
		{
			DrawRoundRect (rect, r.Width, r.Height, paint);
		}

		// DrawOval

		/// <summary>
		/// Draws an oval on the canvas.
		/// </summary>
		/// <param name="cx">The center x-coordinate.</param>
		/// <param name="cy">The center y-coordinate.</param>
		/// <param name="rx">The vertical radius for the oval.</param>
		/// <param name="ry">The horizontal radius for the oval.</param>
		/// <param name="paint">The paint to use when drawing the oval.</param>
		public void DrawOval (float cx, float cy, float rx, float ry, SKPaint paint)
		{
			DrawOval (new SKRect (cx - rx, cy - ry, cx + rx, cy + ry), paint);
		}

		/// <summary>
		/// Draws an oval on the canvas.
		/// </summary>
		/// <param name="c">The center coordinates.</param>
		/// <param name="r">The radius for the oval.</param>
		/// <param name="paint">The paint to use when drawing the oval.</param>
		public void DrawOval (SKPoint c, SKSize r, SKPaint paint)
		{
			DrawOval (c.X, c.Y, r.Width, r.Height, paint);
		}

		/// <summary>
		/// Draws an oval on the canvas.
		/// </summary>
		/// <param name="rect">The bounding box for the oval.</param>
		/// <param name="paint">The paint to use when drawing the oval.</param>
		public void DrawOval (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_oval (Handle, &rect, paint.Handle);
		}

		// DrawCircle

		/// <summary>
		/// Draws a circle on the canvas.
		/// </summary>
		/// <param name="cx">The center x-coordinate.</param>
		/// <param name="cy">The center y-coordinate.</param>
		/// <param name="radius">The radius for the circle.</param>
		/// <param name="paint">The paint to use when drawing the circle.</param>
		public void DrawCircle (float cx, float cy, float radius, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_circle (Handle, cx, cy, radius, paint.Handle);
		}

		/// <summary>
		/// Draws a circle on the canvas.
		/// </summary>
		/// <param name="c">The center coordinates.</param>
		/// <param name="radius">The radius for the circle.</param>
		/// <param name="paint">The paint to use when drawing the circle.</param>
		public void DrawCircle (SKPoint c, float radius, SKPaint paint)
		{
			DrawCircle (c.X, c.Y, radius, paint);
		}

		// DrawPath

		/// <summary>
		/// Draws a path in the canvas.
		/// </summary>
		/// <param name="path">The path to draw.</param>
		/// <param name="paint">The paint to use when drawing the path.</param>
		public void DrawPath (SKPath path, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			SkiaApi.sk_canvas_draw_path (Handle, path.Handle, paint.Handle);
		}

		// DrawPoints

		/// <summary>
		/// Draws an array of points, lines or a polygon in the canvas, one at a time.
		/// </summary>
		/// <param name="mode">Determines how the points array will be interpreted: as points, as coordinates to draw lines, or as coordinates of a polygon.</param>
		/// <param name="points">The array of points to draw.</param>
		/// <param name="paint">The paint to use when drawing the points.</param>
		/// <remarks>
		/// For <see cref="SkiaSharp.SKPointMode.Points" />, each point is drawn centered at its
		/// coordinate, and its size is specified by the paint's stroke-width. It draws as
		/// a square, unless the paint's <see cref="SkiaSharp.SKPaint.StrokeCap" /> is
		/// <see cref="SkiaSharp.SKStrokeCap.Round" />, in which the points are drawn as circles.
		/// For <see cref="SkiaSharp.SKPointMode.Lines" />, each pair of points is drawn as a line
		/// segment, respecting the paint's settings for cap, join and width.
		/// For <see cref="SkiaSharp.SKPointMode.Polygon" />, the entire array is drawn as a
		/// series of connected line segments.
		/// Note that, while similar, the line and polygon modes draw slightly differently
		/// than the equivalent path built with a series of move to, line to calls, in
		/// that the path will draw all of its contours at once, with no interactions if
		/// contours intersect each other (think <see cref="SkiaSharp.SKBlendMode.Xor" />).
		/// </remarks>
		public void DrawPoints (SKPointMode mode, SKPoint[] points, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_canvas_draw_points (Handle, mode, (IntPtr)points.Length, p, paint.Handle);
			}
		}

		// DrawPoint

		/// <summary>
		/// Draws a point in the canvas with the specified color.
		/// </summary>
		/// <param name="p">The coordinates for the point to draw.</param>
		/// <param name="paint">The paint to use when drawing the point.</param>
		public void DrawPoint (SKPoint p, SKPaint paint)
		{
			DrawPoint (p.X, p.Y, paint);
		}

		/// <summary>
		/// Draws a point in the canvas with the specified color.
		/// </summary>
		/// <param name="x">The x-coordinate for the point to draw.</param>
		/// <param name="y">The y-coordinate for the point to draw.</param>
		/// <param name="paint">The paint to use when drawing the point.</param>
		public void DrawPoint (float x, float y, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_point (Handle, x, y, paint.Handle);
		}

		/// <summary>
		/// Draws a point in the canvas with the specified color.
		/// </summary>
		/// <param name="p">The coordinates for the point to draw.</param>
		/// <param name="color">The color to use.</param>
		public void DrawPoint (SKPoint p, SKColor color)
		{
			DrawPoint (p.X, p.Y, color);
		}

		/// <summary>
		/// Draws a point in the canvas with the specified color.
		/// </summary>
		/// <param name="x">The x-coordinate for the point to draw.</param>
		/// <param name="y">The y-coordinate for the point to draw.</param>
		/// <param name="color">The color to use.</param>
		public void DrawPoint (float x, float y, SKColor color)
		{
			using (var paint = new SKPaint { Color = color, BlendMode = SKBlendMode.Src }) {
				DrawPoint (x, y, paint);
			}
		}

		// DrawImage

		/// <summary>
		/// Draws an image on the canvas.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="p">The destination coordinates for the image.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImage (SKImage image, SKPoint p, SKPaint paint = null)
		{
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawImage (image, p.X, p.Y, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
		}

		public void DrawImage (SKImage image, SKPoint p, SKSamplingOptions sampling, SKPaint paint = null)
		{
			DrawImage (image, p.X, p.Y, sampling, paint);
		}

		/// <summary>
		/// Draws an image on the canvas.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="x">The destination x-coordinate for the image.</param>
		/// <param name="y">The destination y-coordinate for the image.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImage (SKImage image, float x, float y, SKPaint paint = null)
		{
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawImage (image, x, y, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
		}

		public void DrawImage (SKImage image, float x, float y, SKSamplingOptions sampling, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image (Handle, image.Handle, x, y, &sampling, paint?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Draws an image on the canvas.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="dest">The region to draw the image into.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImage (SKImage image, SKRect dest, SKPaint paint = null)
		{
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawImage (image, null, &dest, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
		}

		public void DrawImage (SKImage image, SKRect dest, SKSamplingOptions sampling, SKPaint paint = null)
		{
			DrawImage (image, null, &dest, sampling, paint);
		}

		/// <summary>
		/// Draws an image on the canvas.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="source">The source region to copy.</param>
		/// <param name="dest">The region to draw the image into.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImage (SKImage image, SKRect source, SKRect dest, SKPaint paint = null)
		{
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawImage (image, &source, &dest, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
		}

		public void DrawImage (SKImage image, SKRect source, SKRect dest, SKSamplingOptions sampling, SKPaint paint = null)
		{
			DrawImage (image, &source, &dest, sampling, paint);
		}

		private void DrawImage (SKImage image, SKRect* source, SKRect* dest, SKSamplingOptions sampling, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image_rect (Handle, image.Handle, source, dest, &sampling, paint?.Handle ?? IntPtr.Zero);
		}

		// DrawPicture

		/// <summary>
		/// Draws a picture on the canvas.
		/// </summary>
		/// <param name="picture">The picture to draw.</param>
		/// <param name="x">The destination x-coordinate for the picture.</param>
		/// <param name="y">The destination y-coordinate for the picture.</param>
		/// <param name="paint">The paint to use when drawing the picture, or <see langword="null" />.</param>
		public void DrawPicture (SKPicture picture, float x, float y, SKPaint paint = null)
		{
			var matrix = SKMatrix.CreateTranslation (x, y);
			DrawPicture (picture, matrix, paint);
		}

		/// <summary>
		/// Draws a picture on the canvas.
		/// </summary>
		/// <param name="picture">The picture to draw.</param>
		/// <param name="p">The destination coordinates for the picture.</param>
		/// <param name="paint">The paint to use when drawing the picture, or <see langword="null" />.</param>
		public void DrawPicture (SKPicture picture, SKPoint p, SKPaint paint = null)
		{
			DrawPicture (picture, p.X, p.Y, paint);
		}

		/// <summary>
		/// Draws a picture on the canvas.
		/// </summary>
		/// <param name="picture">The picture to draw.</param>
		/// <param name="matrix">The matrix to apply while painting.</param>
		/// <param name="paint">The paint to use when drawing the picture, or <see langword="null" />.</param>
		/// <remarks>
		/// This is equivalent to calling <see cref="SkiaSharp.SKCanvas.Save" />, followed by
		/// <see cref="SkiaSharp.SKCanvas.Concat(SkiaSharp.SKMatrix)" /> with the specified `matrix`,
		/// <see cref="SkiaSharp.SKCanvas.DrawPicture(SkiaSharp.SKPicture,SkiaSharp.SKPaint)" />
		/// and then <see cref="SkiaSharp.SKCanvas.Restore" />.
		/// If paint is non-null, the picture is drawn into a temporary buffer, and then
		/// the paint's alpha, color filter, image filter, blend mode are applied to that
		/// buffer as it is drawn to the canvas.
		/// </remarks>
		public void DrawPicture (SKPicture picture, in SKMatrix matrix, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, m, paint == null ? IntPtr.Zero : paint.Handle);
		}

		/// <summary>
		/// Draws a picture on the canvas.
		/// </summary>
		/// <param name="picture">The picture to draw.</param>
		/// <param name="paint">The paint to use when drawing the picture, or <see langword="null" />.</param>
		public void DrawPicture (SKPicture picture, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));
			SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, null, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// DrawDrawable

		/// <summary>
		/// Draws a drawable on the canvas.
		/// </summary>
		/// <param name="drawable">The drawable to draw.</param>
		/// <param name="matrix">The matrix to apply while painting.</param>
		public void DrawDrawable (SKDrawable drawable, in SKMatrix matrix)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_canvas_draw_drawable (Handle, drawable.Handle, m);
		}

		/// <summary>
		/// Draws a drawable on the canvas.
		/// </summary>
		/// <param name="drawable">The drawable to draw.</param>
		/// <param name="x">The destination x-coordinate for the drawable.</param>
		/// <param name="y">The destination y-coordinate for the drawable.</param>
		public void DrawDrawable (SKDrawable drawable, float x, float y)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			var matrix = SKMatrix.CreateTranslation (x, y);
			DrawDrawable (drawable, matrix);
		}

		/// <summary>
		/// Draws a drawable on the canvas.
		/// </summary>
		/// <param name="drawable">The drawable to draw.</param>
		/// <param name="p">The destination coordinates for the drawable.</param>
		public void DrawDrawable (SKDrawable drawable, SKPoint p)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			var matrix = SKMatrix.CreateTranslation (p.X, p.Y);
			DrawDrawable (drawable, matrix);
		}

		// DrawBitmap

		/// <summary>
		/// Draws a bitmap on the canvas.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="p">The destination coordinates for the bitmap.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmap (SKBitmap bitmap, SKPoint p, SKPaint paint = null) =>
			DrawBitmap (bitmap, p.X, p.Y, paint);

		/// <summary>
		/// Draws a bitmap on the canvas.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="x">The destination x-coordinate for the bitmap.</param>
		/// <param name="y">The destination y-coordinate for the bitmap.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmap (SKBitmap bitmap, float x, float y, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImage (image, x, y, paint);
		}

		/// <summary>
		/// Draws a bitmap on the canvas.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="dest">The region to draw the bitmap into.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmap (SKBitmap bitmap, SKRect dest, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImage (image, dest, paint);
		}

		/// <summary>
		/// Draws a bitmap on the canvas.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="source">The source region to copy.</param>
		/// <param name="dest">The region to draw the bitmap into.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmap (SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImage (image, source, dest, paint);
		}

		// DrawSurface

		/// <summary>
		/// Draws a surface on the canvas.
		/// </summary>
		/// <param name="surface">The surface to draw.</param>
		/// <param name="p">The destination coordinates for the surface.</param>
		/// <param name="paint">The paint to use when drawing the surface, or <see langword="null" />.</param>
		public void DrawSurface (SKSurface surface, SKPoint p, SKPaint paint = null)
		{
			DrawSurface (surface, p.X, p.Y, paint);
		}

		/// <summary>
		/// Draws a surface on the canvas.
		/// </summary>
		/// <param name="surface">The surface to draw.</param>
		/// <param name="x">The destination x-coordinate for the surface.</param>
		/// <param name="y">The destination y-coordinate for the surface.</param>
		/// <param name="paint">The paint to use when drawing the surface, or <see langword="null" />.</param>
		public void DrawSurface (SKSurface surface, float x, float y, SKPaint paint = null)
		{
			if (surface == null)
				throw new ArgumentNullException (nameof (surface));

			surface.Draw (this, x, y, paint);
		}

		// DrawText (SKTextBlob)

		/// <summary>
		/// Draws a text blob on the canvas at the specified coordinates.
		/// </summary>
		/// <param name="text">The text blob to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		public void DrawText (SKTextBlob text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text_blob (Handle, text.Handle, x, y, paint.Handle);
		}

		// DrawText

		/// <summary>
		/// Draws text on the canvas at the specified coordinates.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="p">The coordinates of the origin of the text being drawn.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		[Obsolete ("Use DrawText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public void DrawText (string text, SKPoint p, SKPaint paint) =>
			DrawText (text, p, paint.TextAlign, paint.GetFont (), paint);

		/// <summary>
		/// Draws text on the canvas at the specified coordinates.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="x">The x-coordinate of the origin of the text being drawn.</param>
		/// <param name="y">The y-coordinate of the origin of the text being drawn.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		[Obsolete ("Use DrawText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public void DrawText (string text, float x, float y, SKPaint paint) =>
			DrawText (text, x, y, paint.TextAlign, paint.GetFont (), paint);

		public void DrawText (string text, SKPoint p, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			DrawText (text, p, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public void DrawText (string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			DrawText (text, p.X, p.Y, textAlign, font, paint);

		/// <param name="text"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="font"></param>
		/// <param name="paint"></param>
		public void DrawText (string text, float x, float y, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			DrawText (text, x, y, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public void DrawText (string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			if (textAlign != SKTextAlign.Left) {
				var width = font.MeasureText (text);
				if (textAlign == SKTextAlign.Center)
					width *= 0.5f;
				x -= width;
			}

			using var blob = SKTextBlob.Create (text, font);
			if (blob == null)
				return;

			DrawText (blob, x, y, paint);
		}

		// DrawTextOnPath

		/// <summary>
		/// Draws text on the canvas following a path.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="path">The path the text should follow for its baseline.</param>
		/// <param name="offset">The offset to the path to add to the text's starting position.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		[Obsolete ("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint) =>
			DrawTextOnPath (text, path, offset, true, paint);

		/// <summary>
		/// Draws text on the canvas following a path.
		/// </summary>
		/// <param name="text">The text to draw.</param>
		/// <param name="path">The path the text should follow for its baseline.</param>
		/// <param name="hOffset">The distance along the path to add to the text's starting position.</param>
		/// <param name="vOffset">The distance above (-) or below (+) the path to position the text.</param>
		/// <param name="paint">The paint to use when drawing the text.</param>
		[Obsolete ("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint) =>
			DrawTextOnPath (text, path, new SKPoint (hOffset, vOffset), true, paint);

		/// <param name="text"></param>
		/// <param name="path"></param>
		/// <param name="offset"></param>
		/// <param name="warpGlyphs"></param>
		/// <param name="paint"></param>
		[Obsolete ("Use DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint) =>
			DrawTextOnPath (text, path, offset, warpGlyphs, paint.GetFont (), paint);

		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			DrawTextOnPath (text, path, offset, true, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			DrawTextOnPath (text, path, offset, true, textAlign, font, paint);

		public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			DrawTextOnPath (text, path, new SKPoint (hOffset, vOffset), true, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			DrawTextOnPath (text, path, new SKPoint (hOffset, vOffset), true, textAlign, font, paint);

		/// <param name="text"></param>
		/// <param name="path"></param>
		/// <param name="offset"></param>
		/// <param name="warpGlyphs"></param>
		/// <param name="font"></param>
		/// <param name="paint"></param>
		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			DrawTextOnPath (text, path, offset, warpGlyphs, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			if (warpGlyphs) {
				using var textPath = font.GetTextPathOnPath (text, path, textAlign, offset);
				DrawPath (textPath, paint);
			} else {
				using var blob = SKTextBlob.CreatePathPositioned (text, font, path, textAlign, offset);
				if (blob != null)
					DrawText (blob, 0, 0, paint);
			}
		}

		// Surface

#nullable enable
		public SKSurface? Surface =>
			SKSurface.GetObject (SkiaApi.sk_get_surface (Handle), owns: false, unrefExisting: false);
#nullable disable

		// Context

#nullable enable
		public GRRecordingContext? Context =>
			GRRecordingContext.GetObject (SkiaApi.sk_get_recording_context (Handle), owns: false, unrefExisting: false);
		#nullable disable

		// Flush

		/// <summary>
		/// Triggers the immediate execution of all pending draw operations.
		/// </summary>
		/// <remarks>
		/// For the GPU backend this will resolve all rendering to the GPU surface backing the surface that owns this canvas.
		/// </remarks>
		public void Flush ()
		{
			(Context as GRContext)?.Flush ();
		}

		// Draw*Annotation

		/// <summary>
		/// Send an key/value pair "annotation" to the canvas.
		/// </summary>
		/// <param name="rect">The bounds of the annotation.</param>
		/// <param name="key">The name of the annotation.</param>
		/// <param name="value">The blob of data to attach to the annotation.</param>
		/// <remarks>
		/// The caller still retains its ownership of the data (if any).
		/// Note: on may canvas types, this information is ignored, but some
		/// canvases (e.g. recording a picture or drawing to a PDF document) will pass on
		/// this information.
		/// </remarks>
		public void DrawAnnotation (SKRect rect, string key, SKData value)
		{
			var bytes = StringUtilities.GetEncodedText (key, SKTextEncoding.Utf8, true);
			fixed (byte* b = bytes) {
				SkiaApi.sk_canvas_draw_annotation (base.Handle, &rect, b, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		/// <summary>
		/// Annotates the canvas by associating the specified URL with the specified rectangle (in local coordinates).
		/// </summary>
		/// <param name="rect">The bounds of the annotation.</param>
		/// <param name="value">The data that specifies the URL.</param>
		/// <remarks>
		/// The caller is responsible for managing its ownership of the data.
		/// If the backend of this canvas does not support annotations, this call is
		/// safely ignored.
		/// </remarks>
		public void DrawUrlAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_url_annotation (Handle, &rect, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Annotates the canvas by associating the specified URL with the specified rectangle (in local coordinates).
		/// </summary>
		/// <param name="rect">The bounds of the annotation.</param>
		/// <param name="value">The URL.</param>
		/// <returns>Returns the actual data object that was attached to the canvas.</returns>
		/// <remarks>
		/// If the backend of this canvas does not support annotations, this call is safely ignored.
		/// </remarks>
		public SKData DrawUrlAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawUrlAnnotation (rect, data);
			return data;
		}

		/// <summary>
		/// Annotates the canvas by associating a name with the specified point (see <see cref="SKCanvas.DrawLinkDestinationAnnotation(SkiaSharp.SKRect,SkiaSharp.SKData)" />).
		/// </summary>
		/// <param name="point">The location of the destination.</param>
		/// <param name="value">The data that specifies the name of the destination.</param>
		/// <remarks>
		/// The caller is responsible for managing its ownership of the data.
		/// If the backend of this canvas does not support annotations, this call is
		/// safely ignored.
		/// </remarks>
		public void DrawNamedDestinationAnnotation (SKPoint point, SKData value)
		{
			SkiaApi.sk_canvas_draw_named_destination_annotation (Handle, &point, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Annotates the canvas by associating a name with the specified point (see <see cref="SKCanvas.DrawLinkDestinationAnnotation(SkiaSharp.SKRect,System.String)" />).
		/// </summary>
		/// <param name="point">The location of the destination.</param>
		/// <param name="value">The name of the destination.</param>
		/// <returns>Returns the actual data object that was attached to the canvas.</returns>
		/// <remarks>
		/// If the backend of this canvas does not support annotations, this call is safely ignored.
		/// </remarks>
		public SKData DrawNamedDestinationAnnotation (SKPoint point, string value)
		{
			var data = SKData.FromCString (value);
			DrawNamedDestinationAnnotation (point, data);
			return data;
		}

		/// <summary>
		/// Annotates the canvas by making the specified rectangle link to a named destination (see <see cref="SKCanvas.DrawNamedDestinationAnnotation(SkiaSharp.SKPoint,SkiaSharp.SKData)" />).
		/// </summary>
		/// <param name="rect">The bounds of the annotation.</param>
		/// <param name="value">The data that specifies the name of the link's destination.</param>
		/// <remarks>
		/// The caller is responsible for managing its ownership of the data.
		/// If the backend of this canvas does not support annotations, this call is
		/// safely ignored.
		/// </remarks>
		public void DrawLinkDestinationAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_link_destination_annotation (Handle, &rect, value == null ? IntPtr.Zero : value.Handle);
		}

		/// <summary>
		/// Annotates the canvas by making the specified rectangle link to a named destination (see <see cref="SKCanvas.DrawNamedDestinationAnnotation(SkiaSharp.SKPoint,System.String)" />).
		/// </summary>
		/// <param name="rect">The bounds of the annotation.</param>
		/// <param name="value">The name of the link's destination.</param>
		/// <returns>Returns the actual data object that was attached to the canvas.</returns>
		/// <remarks>
		/// If the backend of this canvas does not support annotations, this call is safely ignored.
		/// </remarks>
		public SKData DrawLinkDestinationAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawLinkDestinationAnnotation (rect, data);
			return data;
		}

		// Draw*NinePatch

		/// <summary>
		/// Draws the bitmap, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="center">The center region within the bitmap to logically divide the bitmap into 9 sections (3x3).</param>
		/// <param name="dst">The region to draw the bitmap into.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null) =>
			DrawBitmapNinePatch (bitmap, center, dst, SKFilterMode.Nearest, paint);

		public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImageNinePatch (image, center, dst, filterMode, paint);
		}

		/// <summary>
		/// Draws the image, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="center">The center region within the image to logically divide the image into 9 sections (3x3).</param>
		/// <param name="dst">The region to draw the image into.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKPaint paint = null) =>
			DrawImageNinePatch (image, center, dst, SKFilterMode.Nearest, paint);

		public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKFilterMode filterMode = SKFilterMode.Nearest, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			// the "center" rect must fit inside the image "rect"
			if (!SKRect.Create (image.Width, image.Height).Contains (center))
				throw new ArgumentException ("Center rectangle must be contained inside the image bounds.", nameof (center));

			SkiaApi.sk_canvas_draw_image_nine (Handle, image.Handle, &center, &dst, filterMode, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// Draw*Lattice

		/// <summary>
		/// Draws the bitmap, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="xDivs">The x-coordinates that divide the bitmap vertically, describing the areas to stretch or shrink.</param>
		/// <param name="yDivs">The y-coordinates that divide the bitmap horizontally, describing the areas to stretch or shrink.</param>
		/// <param name="dst">The region to draw the bitmap into.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null) =>
			DrawBitmapLattice (bitmap, xDivs, yDivs, dst, SKFilterMode.Nearest, paint);

		public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImageLattice (image, xDivs, yDivs, dst, filterMode, paint);
		}

		/// <summary>
		/// Draws the image, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="xDivs">The x-coordinates that divide the image vertically, describing the areas to stretch or shrink.</param>
		/// <param name="yDivs">The Y-coordinates that divide the image horizontally, describing the areas to stretch or shrink.</param>
		/// <param name="dst">The region to draw the image into.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null) =>
			DrawImageLattice (image, xDivs, yDivs, dst, SKFilterMode.Nearest, paint);

		public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
		{
			var lattice = new SKLattice {
				XDivs = xDivs,
				YDivs = yDivs
			};
			DrawImageLattice (image, lattice, dst, filterMode, paint);
		}

		/// <summary>
		/// Draws the bitmap, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="bitmap">The bitmap to draw.</param>
		/// <param name="lattice">The lattice that describes the areas of the bitmap to stretch or shrink.</param>
		/// <param name="dst">The region to draw the bitmap into.</param>
		/// <param name="paint">The paint to use when drawing the bitmap.</param>
		public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint = null) =>
			DrawBitmapLattice (bitmap, lattice, dst, SKFilterMode.Nearest, paint);

		public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
		{
			using var image = SKImage.FromBitmap (bitmap);
			DrawImageLattice (image, lattice, dst, filterMode, paint);
		}

		/// <summary>
		/// Draws the image, stretched or shrunk differentially to fit into the destination rectangle.
		/// </summary>
		/// <param name="image">The image to draw.</param>
		/// <param name="lattice">The lattice that describes the areas of the image to stretch or shrink.</param>
		/// <param name="dst">The region to draw the image into.</param>
		/// <param name="paint">The paint to use when drawing the image, or <see langword="null" />.</param>
		public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKPaint paint = null) =>
			DrawImageLattice (image, lattice, dst, SKFilterMode.Nearest, paint);

		public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			if (lattice.XDivs == null)
				throw new ArgumentNullException (nameof (lattice.XDivs));
			if (lattice.YDivs == null)
				throw new ArgumentNullException (nameof (lattice.YDivs));

			fixed (int* x = lattice.XDivs)
			fixed (int* y = lattice.YDivs)
			fixed (SKLatticeRectType* r = lattice.RectTypes)
			fixed (SKColor* c = lattice.Colors) {
				var nativeLattice = new SKLatticeInternal {
					fBounds = null,
					fRectTypes = r,
					fXCount = lattice.XDivs.Length,
					fXDivs = x,
					fYCount = lattice.YDivs.Length,
					fYDivs = y,
					fColors = (uint*)c,
				};
				if (lattice.Bounds != null) {
					var bounds = lattice.Bounds.Value;
					nativeLattice.fBounds = &bounds;
				}
				SkiaApi.sk_canvas_draw_image_lattice (Handle, image.Handle, &nativeLattice, &dst, filterMode, paint == null ? IntPtr.Zero : paint.Handle);
			}
		}

		// *Matrix

		/// <summary>
		/// Sets the current matrix to identity.
		/// </summary>
		public void ResetMatrix ()
		{
			SkiaApi.sk_canvas_reset_matrix (Handle);
		}

		public void SetMatrix (in SKMatrix matrix) =>
			SetMatrix ((SKMatrix44)matrix);

		/// <summary>
		/// Replaces the current matrix with a copy of the specified matrix.
		/// </summary>
		/// <param name="matrix">The matrix that will be copied into the current matrix.</param>
		[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]
		public void SetMatrix (SKMatrix matrix) =>
			SetMatrix (in matrix);

		public void SetMatrix (in SKMatrix44 matrix)
		{
			fixed (SKMatrix44* ptr = &matrix) {
				SkiaApi.sk_canvas_set_matrix (Handle, ptr);
			}
		}

		/// <summary>
		/// Gets the current matrix on the canvas.
		/// </summary>
		/// <remarks>
		/// This does not account for the translate in any of the devices.
		/// </remarks>
		public SKMatrix TotalMatrix => TotalMatrix44.Matrix;

		public SKMatrix44 TotalMatrix44 {
			get {
				SKMatrix44 matrix;
				SkiaApi.sk_canvas_get_matrix (Handle, &matrix);
				return matrix;
			}
		}

		// SaveCount

		/// <summary>
		/// Gets the number of matrix/clip states on the canvas' private stack.
		/// </summary>
		/// <remarks>
		/// This will equal the number of <see cref="SKCanvas.Save" /> calls minus <see cref="SKCanvas.Restore" /> calls + 1. The save count on a new canvas is 1.
		/// </remarks>
		public int SaveCount => SkiaApi.sk_canvas_get_save_count (Handle);

		// DrawVertices

		/// <summary>
		/// Draws an array of vertices, interpreted as triangles (based on mode).
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="vertices">The array of vertices for the mesh.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <param name="paint">The shader/texture.</param>
		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, colors);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		/// <summary>
		/// Draws an array of vertices, interpreted as triangles (based on mode).
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="vertices">The array of vertices for the mesh.</param>
		/// <param name="texs">The coordinates in texture space (not UV space) for each vertex. May be <see langword="null" />.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <param name="paint">The shader/texture.</param>
		/// <remarks>
		/// If both textures and vertex-colors are <see langword="null" />, it strokes hairlines with the paint's color. This behavior is a useful debugging mode to visualize the mesh.
		/// </remarks>
		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		/// <summary>
		/// Draws an array of vertices, interpreted as triangles (based on mode).
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="vertices">The array of vertices for the mesh.</param>
		/// <param name="texs">The coordinates in texture space (not UV space) for each vertex. May be <see langword="null" />.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <param name="indices">The array of indices to reference into the vertex (texture coordinates, colors) array.</param>
		/// <param name="paint">The shader/texture.</param>
		/// <remarks>
		/// If both textures and vertex-colors are <see langword="null" />, it strokes hairlines with the paint's color. This behavior is a useful debugging mode to visualize the mesh.
		/// </remarks>
		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, UInt16[] indices, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors, indices);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		/// <summary>
		/// Draws an array of vertices, interpreted as triangles (based on mode).
		/// </summary>
		/// <param name="vmode">How to interpret the array of vertices.</param>
		/// <param name="vertices">The array of vertices for the mesh.</param>
		/// <param name="texs">The coordinates in texture space (not UV space) for each vertex. May be <see langword="null" />.</param>
		/// <param name="colors">The color for each vertex, to be interpolated across the triangle. May be <see langword="null" />.</param>
		/// <param name="mode">The blend mode to use to combine the colors with the texture, before being drawn using the paint. Used if both texture coordinates and colors are present.</param>
		/// <param name="indices">The array of indices to reference into the vertex (texture coordinates, colors) array.</param>
		/// <param name="paint">The shader/texture.</param>
		/// <remarks>
		/// If both textures and vertex-colors are <see langword="null" />, it strokes hairlines with the paint's color. This behavior is a useful debugging mode to visualize the mesh.
		/// </remarks>
		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, UInt16[] indices, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors, indices);
			DrawVertices (vert, mode, paint);
		}

		/// <summary>
		/// Draws a set of vertices.
		/// </summary>
		/// <param name="vertices">The mesh to draw.</param>
		/// <param name="mode">The blend mode to use to combine the colors with the texture, before being drawn using the paint. Used if both texture coordinates and colors are present.</param>
		/// <param name="paint">The shader/texture.</param>
		/// <remarks>
		/// If both textures and vertex-colors are <see langword="null" />, it strokes hairlines with the paint's color. This behavior is a useful debugging mode to visualize the mesh.
		/// </remarks>
		public void DrawVertices (SKVertices vertices, SKBlendMode mode, SKPaint paint)
		{
			if (vertices == null)
				throw new ArgumentNullException (nameof (vertices));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_vertices (Handle, vertices.Handle, mode, paint.Handle);
		}

		// DrawArc

		/// <param name="oval"></param>
		/// <param name="startAngle"></param>
		/// <param name="sweepAngle"></param>
		/// <param name="useCenter"></param>
		/// <param name="paint"></param>
		public void DrawArc (SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_arc (Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
		}

		// DrawRoundRectDifference

		/// <param name="outer"></param>
		/// <param name="inner"></param>
		/// <param name="paint"></param>
		public void DrawRoundRectDifference (SKRoundRect outer, SKRoundRect inner, SKPaint paint)
		{
			if (outer == null)
				throw new ArgumentNullException (nameof (outer));
			if (inner == null)
				throw new ArgumentNullException (nameof (inner));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_drrect (Handle, outer.Handle, inner.Handle, paint.Handle);
		}

		// DrawAtlas

		/// <param name="atlas"></param>
		/// <param name="sprites"></param>
		/// <param name="transforms"></param>
		/// <param name="paint"></param>
		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint = null) =>
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawAtlas (atlas, sprites, transforms, null, SKBlendMode.Dst, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, null, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKSamplingOptions sampling, SKPaint paint = null) =>
			DrawAtlas (atlas, sprites, transforms, null, SKBlendMode.Dst, sampling, null, paint);

		/// <param name="atlas"></param>
		/// <param name="sprites"></param>
		/// <param name="transforms"></param>
		/// <param name="colors"></param>
		/// <param name="mode"></param>
		/// <param name="paint"></param>
		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint = null) =>
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawAtlas (atlas, sprites, transforms, colors, mode, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, null, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKPaint paint = null) =>
			DrawAtlas (atlas, sprites, transforms, colors, mode, sampling, null, paint);

		/// <param name="atlas"></param>
		/// <param name="sprites"></param>
		/// <param name="transforms"></param>
		/// <param name="colors"></param>
		/// <param name="mode"></param>
		/// <param name="cullRect"></param>
		/// <param name="paint"></param>
		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint = null) =>
#pragma warning disable CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'
			DrawAtlas (atlas, sprites, transforms, colors, mode, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, &cullRect, paint);
#pragma warning restore CS0618 // 'SKPaint.FilterQuality' is obsolete: 'Use SKSamplingOptions instead.'

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect cullRect, SKPaint paint = null) =>
			DrawAtlas (atlas, sprites, transforms, colors, mode, sampling, &cullRect, paint);

		private void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect* cullRect, SKPaint paint = null)
		{
			if (atlas == null)
				throw new ArgumentNullException (nameof (atlas));
			if (sprites == null)
				throw new ArgumentNullException (nameof (sprites));
			if (transforms == null)
				throw new ArgumentNullException (nameof (transforms));

			if (transforms.Length != sprites.Length)
				throw new ArgumentException ("The number of transforms must match the number of sprites.", nameof (transforms));
			if (colors != null && colors.Length != sprites.Length)
				throw new ArgumentException ("The number of colors must match the number of sprites.", nameof (colors));

			fixed (SKRect* s = sprites)
			fixed (SKRotationScaleMatrix* t = transforms)
			fixed (SKColor* c = colors) {
				SkiaApi.sk_canvas_draw_atlas (Handle, atlas.Handle, t, s, (uint*)c, transforms.Length, mode, &sampling, cullRect, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// DrawPatch

		/// <param name="cubics"></param>
		/// <param name="colors"></param>
		/// <param name="texCoords"></param>
		/// <param name="paint"></param>
		public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint) =>
			DrawPatch (cubics, colors, texCoords, SKBlendMode.Modulate, paint);

		/// <param name="cubics"></param>
		/// <param name="colors"></param>
		/// <param name="texCoords"></param>
		/// <param name="mode"></param>
		/// <param name="paint"></param>
		public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint)
		{
			if (cubics == null)
				throw new ArgumentNullException (nameof (cubics));
			if (cubics.Length != PatchCubicsCount)
				throw new ArgumentException ($"Cubics must have a length of {PatchCubicsCount}.", nameof (cubics));

			if (colors != null && colors.Length != PatchCornerCount)
				throw new ArgumentException ($"Colors must have a length of {PatchCornerCount}.", nameof (colors));

			if (texCoords != null && texCoords.Length != PatchCornerCount)
				throw new ArgumentException ($"Texture coordinates must have a length of {PatchCornerCount}.", nameof (texCoords));

			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			fixed (SKPoint* cubes = cubics)
			fixed (SKColor* cols = colors)
			fixed (SKPoint* coords = texCoords) {
				SkiaApi.sk_canvas_draw_patch (Handle, cubes, (uint*)cols, coords, mode, paint.Handle);
			}
		}

		internal static SKCanvas GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKCanvas (h, o));
	}

	/// <summary>
	/// Convenience class used to restore the canvas state in a using statement.
	/// </summary>
	/// <remarks>
	/// This class can be used in a using statement to save the state of the canvas
	/// (matrix, clip and draw filter) allowing you to change these components and have
	/// them automatically undone by virtue of having the
	/// <see cref="SkiaSharp.SKAutoCanvasRestore.Dispose" /> method restore the canvas state to
	/// the state it was when this instance was created.
	/// ## Examples
	/// ```csharp
	/// SKCanvas canvas = ...;
	/// using (new SKAutoCanvasRestore(canvas)) {
	/// // perform some transform
	/// canvas.RotateDegrees(45);
	/// // draw as usual
	/// var paint = new SKPaint ();
	/// canavs.DrawRect (10, 10, 100, 100, paint);
	/// // automatically restore to original transform
	/// }
	/// ```
	/// </remarks>
	public class SKAutoCanvasRestore : IDisposable
	{
		private SKCanvas canvas;
		private readonly int saveCount;

		/// <summary>
		/// Creates a canvas restore point, invoking the <see cref="SKCanvas.Save" /> method.
		/// </summary>
		/// <param name="canvas">The canvas whose state will be preserved.</param>
		public SKAutoCanvasRestore (SKCanvas canvas)
			: this (canvas, true)
		{
		}

		/// <summary>
		/// Creates a canvas restore point.
		/// </summary>
		/// <param name="canvas">The canvas whose state will be preserved.</param>
		/// <param name="doSave">Whether or not to invoke <see cref="SKCanvas.Save" /> method at this point.</param>
		public SKAutoCanvasRestore (SKCanvas canvas, bool doSave)
		{
			this.canvas = canvas;
			this.saveCount = 0;

			if (canvas != null) {
				saveCount = canvas.SaveCount;
				if (doSave) {
					canvas.Save ();
				}
			}
		}

		/// <summary>
		/// <para>Disposes the canvas restore point, restoring the state of the canvas (matrix, clip and draw filter) to the state it was when the object was created.</para><para>This operation will not do anything if you had previously manually called the <see cref="SKAutoCanvasRestore.Restore" /> method.</para>
		/// </summary>
		public void Dispose ()
		{
			Restore ();
		}

		/// <summary>
		/// Restores the canvas restore point, restoring the state of the canvas (matrix, clip and draw filter) to the state it was when the object was creatd.
		/// </summary>
		public void Restore ()
		{
			// canvas can be GC-ed before us
			if (canvas != null && canvas.Handle != IntPtr.Zero) {
				canvas.RestoreToCount (saveCount);
			}
			canvas = null;
		}
	}

#nullable enable
	public unsafe struct SKCanvasSaveLayerRec
	{
		public SKRect? Bounds { readonly get; set; }

		public SKPaint? Paint { readonly get; set; }

		public SKImageFilter? Backdrop { readonly get; set; }

		public SKCanvasSaveLayerRecFlags Flags { readonly get; set; }

		internal readonly SKCanvasSaveLayerRecNative ToNative () =>
			new SKCanvasSaveLayerRecNative {
				fBounds = Bounds is { } bounds ? &bounds : (SKRect*)null,
				fPaint = Paint?.Handle ?? IntPtr.Zero,
				fBackdrop = Backdrop?.Handle ?? IntPtr.Zero,
				fFlags = Flags
			};
	}
#nullable disable
}
