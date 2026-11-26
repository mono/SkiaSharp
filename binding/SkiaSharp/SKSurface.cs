#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Represents the backend/results of drawing to a canvas.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// The surface represents the backend/results of drawing to a canvas. For raster
	/// drawing, the surface will be pixels, but (for example) when drawing into a
	/// PDF or <xref:SkiaSharp.SKPicture> canvas, the surface stores the recorded
	/// commands.
	///
	/// The surface always has non-zero dimensions. If there is a request for a new
	/// surface, and either of the requested dimensions are zero, then <see langword="null" /> will
	/// be returned.
	///
	/// Once you create a surface with one of its <xref:SkiaSharp.SKSurface.Create%2A>
	/// methods, you can draw into the canvas returned by the
	/// <xref:SkiaSharp.SKSurface.Canvas> property. Once the drawing is complete, you
	/// can retrieve an <xref:SkiaSharp.SKImage> by calling the
	/// <xref:SkiaSharp.SKSurface.Snapshot%2A> method.
	///
	/// ## Examples
	///
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	///     SKCanvas canvas = surface.Canvas;
	///
	///     canvas.Clear(SKColors.White);
	///
	///     // configure our brush
	///     var redBrush = new SKPaint {
	///         Color = new SKColor(0xff, 0, 0),
	///         IsStroke = true
	///     };
	///     var blueBrush = new SKPaint {
	///         Color = new SKColor(0, 0, 0xff),
	///         IsStroke = true
	///     };
	///
	///     for (int i = 0; i < 64; i += 8) {
	///         var rect = new SKRect(i, i, 256 - i - 1, 256 - i - 1);
	///         canvas.DrawRect(rect, (i % 16 == 0) ? redBrush : blueBrush);
	///     }
	/// }
	/// ```
	///
	/// The example above produces the following:
	///
	/// ![SKSurface](~/images/surface-rects.png "SKSurface")
	/// ]]></format></remarks>
	public unsafe class SKSurface : SKObject, ISKReferenceCounted
	{
		// Skia's SkSurface lazily creates its SkCanvas once and returns the same raw
		// pointer for the surface's entire lifetime (SkSurface_Base::getCachedCanvas,
		// shared by all backends). The managed wrapper is therefore also stable, so we
		// cache it here to avoid a P/Invoke and a locked HandleDictionary lookup on every
		// access — a measurable win in draw-heavy render loops that fetch Canvas per draw.
		private SKCanvas canvas;

		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKSurface" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKSurface" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases managed resources.</summary>
		/// <remarks />
		protected override void DisposeManaged ()
		{
			base.DisposeManaged ();

			// Drop the cached wrapper; it is an owned child and was already torn down by
			// base disposal. A fresh surface gets its own canvas, so nothing to reuse.
			canvas = null;
		}

		// RASTER surface

		/// <summary>Creates a new surface with the specified image parameters.</summary>
		/// <param name="info">Contains the image configuration parameters.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />.</remarks>
		public static SKSurface Create (SKImageInfo info) =>
			Create (info, 0, null);

		/// <summary>Creates a new surface from the specified image parameters.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />.</remarks>
		public static SKSurface Create (SKImageInfo info, int rowBytes) =>
			Create (info, rowBytes, null);

		/// <summary>Creates a new surface from the specified image parameters and surface properties.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, SKSurfaceProperties props) =>
			Create (info, 0, props);

		/// <summary>Creates a new surface from the specified image parameters and surface properties.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var surface = GetObject (SkiaApi.sk_surface_new_raster (&cinfo, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (props);
			return surface;
		}

		// convenience RASTER DIRECT to use a SKPixmap instead of SKImageInfo and IntPtr

		/// <summary>Creates a new surface with the specified pixmap.</summary>
		/// <param name="pixmap">The pixmap.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (SKPixmap pixmap) =>
			Create (pixmap, null);

		/// <summary>Creates a new surface with the specified pixmap and surface properties.</summary>
		/// <param name="pixmap">The pixmap.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProperties props)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, null, null, props);
		}

		// RASTER DIRECT surface

		/// <summary>Creates a new surface with the specified image parameters using a provided buffer.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (SKImageInfo info, IntPtr pixels) =>
			Create (info, pixels, info.RowBytes, null, null, null);

		/// <summary>Creates a new surface with the specified image parameters using a provided buffer.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer that will be backend by the in-memory buffer provided in <paramref name="pixels" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes) =>
			Create (info, pixels, rowBytes, null, null, null);

		/// <summary>Creates a new surface with the specified image parameters using a provided buffer.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the surface is about to be disposed.</param>
		/// <param name="context">The user data to use when invoking the delegate.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context) =>
			Create (info, pixels, rowBytes, releaseProc, context, null);

		/// <summary>Creates a new surface from the specified image parameters and surface properties.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProperties props) =>
			Create (info, pixels, info.RowBytes, null, null, props);

		/// <summary>Creates a new surface from the specified image parameters, the provided buffer and surface properties.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer that will be backend by the in-memory buffer provided in <paramref name="pixels" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props) =>
			Create (info, pixels, rowBytes, null, null, props);

		/// <summary>Creates a new surface with the specified image parameters using a provided buffer.</summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the surface is about to be disposed.</param>
		/// <param name="context">The user data to use when invoking the delegate.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKSurfaceReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del != null ? DelegateProxies.SKSurfaceRasterReleaseProxy : null;
			var surface = GetObject (SkiaApi.sk_surface_new_raster_direct (&cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx, props?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (props);
			return surface;
		}

		// GPU BACKEND RENDER TARGET surface

		/// <summary>Wraps a pre-existing 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, colorType);

		/// <summary>Wraps a pre-existing backend 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType);

		/// <summary>Wraps a pre-existing backend 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace);

		/// <summary>Wraps a pre-existing backend 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, colorType, props);

		/// <summary>Wraps a pre-existing backend 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, props);

		/// <summary>Wraps a pre-existing backend 3D API render target as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace, props);

		/// <summary>Creates a surface that wraps a GPU backend render target.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, null);

		/// <summary>Creates a surface that wraps a GPU backend render target with the specified origin.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, renderTarget, origin, colorType, null, null);

		/// <summary>Creates a surface that wraps a GPU backend render target with color space.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, renderTarget, origin, colorType, colorspace, null);

		/// <summary>Creates a surface that wraps a GPU backend render target with surface properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, props);

		/// <summary>Creates a surface that wraps a GPU backend render target with origin and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, origin, colorType, null, props);

		/// <summary>Creates a surface that wraps a GPU backend render target with color space and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="renderTarget">The backend render target to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			var surface = GetObject (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (context);
			GC.KeepAlive (renderTarget);
			GC.KeepAlive (colorspace);
			GC.KeepAlive (props);
			return surface;
		}

		// GPU BACKEND TEXTURE surface

		/// <summary>Wraps a pre-existing 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, colorType);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, colorType);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, colorType, props);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, colorType, props);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, props);

		/// <summary>Wraps a pre-existing backend 3D API texture as a surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace, props);

		/// <summary>Creates a surface that wraps a GPU backend texture.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);

		/// <summary>Creates a surface that wraps a GPU backend texture with the specified origin.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, texture, origin, 0, colorType, null, null);

		/// <summary>Creates a surface that wraps a GPU backend texture with multisampling.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, texture, origin, sampleCount, colorType, null, null);

		/// <summary>Creates a surface that wraps a GPU backend texture with multisampling and color space.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, texture, origin, sampleCount, colorType, colorspace, null);

		/// <summary>Creates a surface that wraps a GPU backend texture with surface properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);

		/// <summary>Creates a surface that wraps a GPU backend texture with origin and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, 0, colorType, null, props);

		/// <summary>Creates a surface that wraps a GPU backend texture with multisampling and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, sampleCount, colorType, null, props);

		/// <summary>Creates a surface that wraps a GPU backend texture with all options.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="texture">The backend texture to wrap.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="colorType">The color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			var surface = GetObject (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (context);
			GC.KeepAlive (texture);
			GC.KeepAlive (colorspace);
			GC.KeepAlive (props);
			return surface;
		}

		// GPU NEW surface

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info) =>
			Create ((GRRecordingContext)context, budgeted, info);

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount);

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin);

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, props);

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, props);

		/// <summary>Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.</summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <param name="shouldCreateWithMips">A hint that the surface will host mip map images.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin, props, false);

		/// <summary>Creates a GPU-accelerated surface with the specified image info.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, null, false);

		/// <summary>Creates a GPU-accelerated surface with multisampling support.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, false);

		/// <summary>Creates a GPU-accelerated surface with multisampling and specified origin.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create (context, budgeted, info, sampleCount, origin, null, false);

		/// <summary>Creates a GPU-accelerated surface with the specified image info and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, false);

		/// <summary>Creates a GPU-accelerated surface with multisampling and properties.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="props">The surface properties.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, false);

		/// <summary>Creates a GPU-accelerated surface with all options including mipmap support.</summary>
		/// <param name="context">The GPU recording context.</param>
		/// <param name="budgeted">Whether the surface should count against the GPU resource budget.</param>
		/// <param name="info">The image info describing the surface dimensions and format.</param>
		/// <param name="sampleCount">The number of samples for MSAA rendering.</param>
		/// <param name="origin">The origin of the surface texture.</param>
		/// <param name="props">The surface properties.</param>
		/// <param name="shouldCreateWithMips">Whether to create mipmaps.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> on failure.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var surface = GetObject (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, &cinfo, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
			GC.KeepAlive (context);
			GC.KeepAlive (props);
			return surface;
		}

		// Graphite-backed render target

		/// <summary>Creates a Graphite-backed surface for the specified recorder.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="info">The image info describing the size and format of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info) =>
			Create (recorder, info, mipmapped: false, props: null);

		/// <summary>Creates a Graphite-backed surface for the specified recorder, optionally with mipmaps.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="info">The image info describing the size and format of the surface.</param>
		/// <param name="mipmapped"><see langword="true" /> to allocate the surface with mipmaps; otherwise, <see langword="false" />.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, bool mipmapped) =>
			Create (recorder, info, mipmapped, props: null);

		/// <summary>Creates a Graphite-backed surface for the specified recorder, using the specified surface properties.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="info">The image info describing the size and format of the surface.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, SKSurfaceProperties props) =>
			Create (recorder, info, mipmapped: false, props);

		/// <summary>Creates a Graphite-backed surface for the specified recorder, optionally with mipmaps and using the specified surface properties.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="info">The image info describing the size and format of the surface.</param>
		/// <param name="mipmapped"><see langword="true" /> to allocate the surface with mipmaps; otherwise, <see langword="false" />.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKImageInfo info, bool mipmapped, SKSurfaceProperties props)
		{
			if (recorder == null)
				throw new ArgumentNullException (nameof (recorder));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_graphite_surface_make_render_target (recorder.Handle, &cinfo, mipmapped, props?.Handle ?? IntPtr.Zero));
		}

		// Graphite-backed surface wrapping a caller-allocated GPU texture

		/// <summary>Creates a Graphite-backed surface that renders into an existing backend texture.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="backendTexture">The backend texture to render into.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the texture.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType) =>
			Create (recorder, backendTexture, colorType, colorSpace: null, props: null);

		/// <summary>Creates a Graphite-backed surface that renders into an existing backend texture, using the specified color space.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="backendTexture">The backend texture to render into.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the texture.</param>
		/// <param name="colorSpace">The color space of the texture, or <see langword="null" /> to use no color space.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace) =>
			Create (recorder, backendTexture, colorType, colorSpace, props: null);

		/// <summary>Creates a Graphite-backed surface that renders into an existing backend texture, using the specified color space and surface properties.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="backendTexture">The backend texture to render into.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the texture.</param>
		/// <param name="colorSpace">The color space of the texture, or <see langword="null" /> to use no color space.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace, SKSurfaceProperties props) =>
			Create (recorder, backendTexture, colorType, colorSpace, props, releaseProc: null);

		/// <summary>Creates a Graphite-backed surface that renders into an existing backend texture, invoking a callback when Skia no longer needs the texture.</summary>
		/// <param name="recorder">The recorder that the surface is created for.</param>
		/// <param name="backendTexture">The backend texture to render into.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the texture.</param>
		/// <param name="colorSpace">The color space of the texture, or <see langword="null" /> to use no color space.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <param name="releaseProc">The callback invoked when Skia is finished using the texture, or <see langword="null" /> for none.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, SKColorType colorType, SKColorSpace colorSpace, SKSurfaceProperties props, SKGraphiteReleaseDelegate releaseProc)
		{
			if (recorder == null)
				throw new ArgumentNullException (nameof (recorder));
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));

			DelegateProxies.Create (releaseProc, out _, out var ctx);
			var proxy = releaseProc != null ? DelegateProxies.SKGraphiteReleaseProxy : null;

			return GetObject (SkiaApi.sk_graphite_surface_wrap_backend_texture (
				recorder.Handle,
				backendTexture.Handle,
				colorType.ToNative (),
				colorSpace?.Handle ?? IntPtr.Zero,
				props?.Handle ?? IntPtr.Zero,
				proxy,
				(void*)ctx));
		}

#if __MACOS__ || __IOS__ || __TVOS__

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, out drawable);

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer using the specified color space.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, out drawable);

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer using the specified color space and surface properties.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, props, out drawable);

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, null, null, out drawable);

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer using the specified color space.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, colorspace, null, out drawable);

		/// <summary>Creates a GPU-backed surface that renders into a Metal layer using the specified color space and surface properties.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="layer">The Metal layer to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <param name="drawable">When this method returns, contains the Metal drawable associated with the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable)
		{
			void* drawablePtr;
			var surface = GetObject (SkiaApi.sk_surface_new_metal_layer (context.Handle, (void*)(IntPtr)layer.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero, &drawablePtr));
			GC.KeepAlive (context);
			GC.KeepAlive (colorspace);
			GC.KeepAlive (props);
			drawable = ObjCRuntime.Runtime.GetINativeObject<CoreAnimation.ICAMetalDrawable> ((IntPtr)drawablePtr, true);
			return surface;
		}

		/// <summary>Creates a GPU-backed surface that renders into a Metal view.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="view">The Metal view to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, view, origin, sampleCount, colorType, null, null);

		/// <summary>Creates a GPU-backed surface that renders into a Metal view using the specified color space.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="view">The Metal view to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, view, origin, sampleCount, colorType, colorspace, null);

		/// <summary>Creates a GPU-backed surface that renders into a Metal view using the specified color space and surface properties.</summary>
		/// <param name="context">The GPU context that the surface is created for.</param>
		/// <param name="view">The Metal view to render into.</param>
		/// <param name="origin">One of the enumeration values that specifies the surface origin.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">One of the enumeration values that specifies the color type of the surface.</param>
		/// <param name="colorspace">The color space of the surface, or <see langword="null" /> to use no color space.</param>
		/// <param name="props">The surface properties to use.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKSurface" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			var surface = GetObject (SkiaApi.sk_surface_new_metal_view (context.Handle, (void*)(IntPtr)view.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (context);
			GC.KeepAlive (colorspace);
			GC.KeepAlive (props);
			return surface;
		}

#endif

		// NULL surface

		/// <summary>Creates a new surface without any backing pixels.</summary>
		/// <param name="width">The desired width for the surface.</param>
		/// <param name="height">The desired height for the surface.</param>
		/// <returns>Returns the new surface if it could be created, otherwise <see langword="null" />.</returns>
		/// <remarks>Drawing to the <see cref="T:SkiaSharp.SKCanvas" /> returned from <see cref="P:SkiaSharp.SKSurface.Canvas" /> has no effect. Calling <see cref="M:SkiaSharp.SKSurface.Snapshot" /> on the returned <see cref="T:SkiaSharp.SKSurface" /> returns <see langword="null" />.</remarks>
		public static SKSurface CreateNull (int width, int height) =>
			GetObject (SkiaApi.sk_surface_new_null (width, height));

		//

		/// <summary>Gets the canvas for this surface which can be used for drawing into it.</summary>
		/// <value>The canvas for this surface.</value>
		/// <remarks />
		public SKCanvas Canvas {
			get {
				// Re-fetch if not yet cached, or if the cached wrapper was disposed out from
				// under us (e.g. a caller explicitly disposed the surface-owned canvas).
				if (canvas == null || canvas.Handle == IntPtr.Zero) {
					canvas = OwnedBy (SKCanvas.GetObject (SkiaApi.sk_surface_get_canvas (Handle), false, unrefExisting: false), this);
				}
				GC.KeepAlive (this);
				return canvas;
			}
		}

		/// <summary>Gets the surface property configuration.</summary>
		/// <value>The surface property configuration.</value>
		/// <remarks />
		public SKSurfaceProperties SurfaceProperties {
			get {
				var result = OwnedBy (SKSurfaceProperties.GetObject (SkiaApi.sk_surface_get_props (Handle), false), this);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the GPU recording context associated with this surface.</summary>
		/// <value>The GPU recording context, or <see langword="null" /> if not GPU-accelerated.</value>
		/// <remarks />
		public GRRecordingContext Context {
			get {
				var result = GRRecordingContext.GetObject (SkiaApi.sk_surface_get_recording_context (Handle), false, unrefExisting: false);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Takes a snapshot of the surface and returns it as an image.</summary>
		/// <returns>An <see cref="T:SkiaSharp.SKImage" /> that contains a snapshot of the current image.</returns>
		/// <remarks>You can use this method to take an <see cref="T:SkiaSharp.SKImage" /> snapshot of the current state of the surface.</remarks>
		public SKImage Snapshot ()
		{
			var result = SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot (Handle));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Creates an image from the specified area of the surface.</summary>
		/// <param name="bounds">The bounds of the area to capture.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKImage" /> containing the captured area.</returns>
		/// <remarks />
		public SKImage Snapshot (SKRectI bounds)
		{
			var result = SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot_with_crop (Handle, &bounds));
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Draws this surface onto the specified canvas at the given position.</summary>
		/// <param name="canvas">The canvas onto which this surface is drawn.</param>
		/// <param name="x">The x-coordinate of the destination position.</param>
		/// <param name="y">The y-coordinate of the destination position.</param>
		/// <param name="paint">The paint to apply when drawing, or <see langword="null" /> to use default paint settings.</param>
		/// <remarks />
		public void Draw (SKCanvas canvas, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_surface_draw (Handle, canvas.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
			GC.KeepAlive (this);
			GC.KeepAlive (canvas);
			GC.KeepAlive (paint);
		}

		/// <summary>Draws the surface onto the specified canvas at the given location using the specified sampling options and paint.</summary>
		/// <param name="canvas">The canvas onto which to draw the surface.</param>
		/// <param name="p">The location at which to draw the upper-left corner of the surface.</param>
		/// <param name="sampling">The sampling options to apply when scaling or filtering the surface.</param>
		/// <param name="paint">The paint to use when drawing the surface, or <see langword="null" /> for default rendering.</param>
		/// <remarks></remarks>
		public void Draw (SKCanvas canvas, SKPoint p, SKSamplingOptions sampling, SKPaint paint = null)
		{
			Draw (canvas, p.X, p.Y, sampling, paint);
		}

		/// <summary>Draws this surface onto the specified canvas at the given position.</summary>
		/// <param name="canvas">The canvas onto which this surface is drawn.</param>
		/// <param name="x">The x-coordinate of the destination position.</param>
		/// <param name="y">The y-coordinate of the destination position.</param>
		/// <param name="sampling">The sampling options to use when drawing.</param>
		/// <param name="paint">The paint to apply when drawing, or <see langword="null" /> to use default paint settings.</param>
		/// <remarks></remarks>
		public void Draw (SKCanvas canvas, float x, float y, SKSamplingOptions sampling, SKPaint paint = null)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_surface_draw_with_sampling (Handle, canvas.Handle, x, y, &sampling, paint == null ? IntPtr.Zero : paint.Handle);
			GC.KeepAlive (this);
			GC.KeepAlive (canvas);
			GC.KeepAlive (paint);
		}

		/// <summary>Returns the pixels, if they are available.</summary>
		/// <returns>Returns the pixels, if they are available, otherwise <see langword="null" />.</returns>
		/// <remarks>If the pixels are available, then the surface is only valid until the surface changes in any way, in which case the pixmap becomes invalid.</remarks>
		public SKPixmap PeekPixels ()
		{
			var pixmap = new SKPixmap ();
			var result = PeekPixels (pixmap);
			if (result) {
				return pixmap;
			} else {
				pixmap.Dispose ();
				return null;
			}
		}

		/// <summary>Returns the pixmap of the surface.</summary>
		/// <param name="pixmap">The pixmap to receive the pixel information.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the surface does not have access to pixel data.</returns>
		/// <remarks />
		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_surface_peek_pixels (Handle, pixmap.Handle);
			GC.KeepAlive (this);
			GC.KeepAlive (pixmap);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		/// <summary>Copies the pixels from the surface into the specified buffer.</summary>
		/// <param name="dstInfo">The image information describing the destination pixel buffer.</param>
		/// <param name="dstPixels">The pixel buffer to read the pixel data into.</param>
		/// <param name="dstRowBytes">The number of bytes in each row of in the destination buffer.</param>
		/// <param name="srcX">The source x-coordinate to start reading from.</param>
		/// <param name="srcY">The source y-coordinate to start reading from.</param>
		/// <returns>Returns <see langword="true" /> if the pixels were read, or <see langword="false" /> if there was an error.</returns>
		/// <remarks>This method may return <see langword="false" /> if the source rectangle [<paramref name="srcX" />, <paramref name="srcY" />, dstInfo.Width, dstInfo.Height] does not intersect the surface, or if the color type/alpha type could not be converted to the destination types.</remarks>
		public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var result = SkiaApi.sk_surface_read_pixels (Handle, &cinfo, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
			GC.KeepAlive (this);
			return result;
		}

		// RequestReadPixels

		/// <summary>Asynchronously reads pixels from the surface into a result delivered to a callback, using nearest sampling.</summary>
		/// <param name="info">The image info describing the desired size and format of the result.</param>
		/// <param name="srcRect">The rectangle of the surface to read, in pixels.</param>
		/// <param name="callback">The callback invoked with the read result, or <see langword="null" /> if the read fails; the result is valid only for the duration of the call.</param>
		/// <remarks />
		public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, Action<SKImageReadPixelsResult> callback) =>
			RequestReadPixels (info, srcRect, SKImageRescaleGamma.Src, SKImageRescaleMode.Nearest, callback);

		/// <summary>Asynchronously reads and rescales pixels from the surface into a result delivered to a callback.</summary>
		/// <param name="info">The image info describing the desired size and format of the result.</param>
		/// <param name="srcRect">The rectangle of the surface to read, in pixels.</param>
		/// <param name="rescaleGamma">One of the enumeration values that specifies the gamma space used for rescaling.</param>
		/// <param name="rescaleMode">One of the enumeration values that specifies the sampling algorithm used for rescaling.</param>
		/// <param name="callback">The callback invoked with the read result, or <see langword="null" /> if the read fails; the result is valid only for the duration of the call.</param>
		/// <remarks />
		public void RequestReadPixels (SKImageInfo info, SKRectI srcRect, SKImageRescaleGamma rescaleGamma, SKImageRescaleMode rescaleMode, Action<SKImageReadPixelsResult> callback)
		{
			if (callback == null)
				throw new ArgumentNullException (nameof (callback));

			Action<IntPtr> handler = raw => {
				using var result = raw == IntPtr.Zero ? null : new SKImageReadPixelsResult (raw, info);
				callback (result);
				// Keep this surface alive until the (possibly deferred) callback fires.
				GC.KeepAlive (this);
			};
			DelegateProxies.Create (handler, out _, out var ctx);

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			SkiaApi.sk_surface_async_rescale_and_read_pixels (Handle, &cinfo, &srcRect, rescaleGamma, rescaleMode, DelegateProxies.SKImageAsyncReadPixelsProxy, (void*)ctx);
			GC.KeepAlive (this);
		}

		/// <summary>Ensures all pending draw operations are submitted to the GPU.</summary>
		/// <remarks />
		public void Flush () => Flush (true);

		/// <summary>Ensures all pending draw operations are submitted to the GPU with specified options.</summary>
		/// <param name="submit">Whether to submit the commands to the GPU.</param>
		/// <param name="synchronous">Whether to wait for the GPU to finish processing.</param>
		/// <remarks />
		public void Flush (bool submit, bool synchronous = false)
		{
			if (Context is not GRContext grContext)
				return;

			if (submit)
				grContext.Flush (submit, synchronous);
			else
				grContext.Flush ();
		}

		internal static SKSurface GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKSurface (h, o));
	}
}
