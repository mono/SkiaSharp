#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Represents the backend/results of drawing to a canvas.
	/// </summary>
	/// <remarks>The surface represents the backend/results of drawing to a canvas. For raster
	/// drawing, the surface will be pixels, but (for example) when drawing into a
	/// PDF or <see cref="SkiaSharp.SKPicture" /> canvas, the surface stores the recorded
	/// commands.
	/// The surface always has non-zero dimensions. If there is a request for a new
	/// surface, and either of the requested dimensions are zero, then <see langword="null" /> will
	/// be returned.
	/// Once you create a surface with one of its <see cref="SkiaSharp.SKSurface.Create%2A" />
	/// methods, you can draw into the canvas returned by the
	/// <see cref="SkiaSharp.SKSurface.Canvas" /> property. Once the drawing is complete, you
	/// can retrieve an <see cref="SkiaSharp.SKImage" /> by calling the
	/// <see cref="SkiaSharp.SKSurface.Snapshot%2A" /> method.
	/// ## Examples
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // configure our brush
	/// var redBrush = new SKPaint {
	/// Color = new SKColor(0xff, 0, 0),
	/// IsStroke = true
	/// };
	/// var blueBrush = new SKPaint {
	/// Color = new SKColor(0, 0, 0xff),
	/// IsStroke = true
	/// };
	/// for (int i = 0; i < 64; i += 8) {
	/// var rect = new SKRect(i, i, 256 - i - 1, 256 - i - 1);
	/// canvas.DrawRect(rect, (i % 16 == 0) ? redBrush : blueBrush);
	/// }
	/// }
	/// ```
	/// The example above produces the following:
	/// ![SKSurface](~/images/surface-rects.png "SKSurface")</remarks>
	public unsafe class SKSurface : SKObject, ISKReferenceCounted
	{
		internal SKSurface (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// RASTER surface

		/// <summary>
		/// Creates a new surface with the specified image parameters.
		/// </summary>
		/// <param name="info">Contains the image configuration parameters.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />.</remarks>
		public static SKSurface Create (SKImageInfo info) =>
			Create (info, 0, null);

		/// <summary>
		/// Creates a new surface from the specified image parameters.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info&amp;nbsp;" />.</remarks>
		public static SKSurface Create (SKImageInfo info, int rowBytes) =>
			Create (info, rowBytes, null);

		/// <summary>
		/// Creates a new surface from the specified image parameters and surface properties.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info&amp;nbsp;" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, SKSurfaceProperties props) =>
			Create (info, 0, props);

		/// <summary>
		/// Creates a new surface from the specified image parameters and surface properties.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_surface_new_raster (&cinfo, (IntPtr)rowBytes, props?.Handle ?? IntPtr.Zero));
		}

		// convenience RASTER DIRECT to use a SKPixmap instead of SKImageInfo and IntPtr

		/// <summary>
		/// Creates a new surface with the specified pixmap.
		/// </summary>
		/// <param name="pixmap">The pixmap.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (SKPixmap pixmap) =>
			Create (pixmap, null);

		/// <summary>
		/// Creates a new surface with the specified pixmap and surface properties.
		/// </summary>
		/// <param name="pixmap">The pixmap.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (SKPixmap pixmap, SKSurfaceProperties props)
		{
			if (pixmap == null) {
				throw new ArgumentNullException (nameof (pixmap));
			}
			return Create (pixmap.Info, pixmap.GetPixels (), pixmap.RowBytes, null, null, props);
		}

		// RASTER DIRECT surface

		/// <summary>
		/// Creates a new surface with the specified image parameters using a provided buffer.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels) =>
			Create (info, pixels, info.RowBytes, null, null, null);

		/// <summary>
		/// Creates a new surface with the specified image parameters using a provided buffer.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer that will be backend by the in-memory buffer provided in <paramref name="pixels" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes) =>
			Create (info, pixels, rowBytes, null, null, null);

		/// <summary>
		/// Creates a new surface with the specified image parameters using a provided buffer.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the surface is about to be disposed.</param>
		/// <param name="context">The user data to use when invoking the delegate.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context) =>
			Create (info, pixels, rowBytes, releaseProc, context, null);

		/// <summary>
		/// Creates a new surface from the specified image parameters and surface properties.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer with the parameters specified in <paramref name="info" />and the properties specified in <paramref name="props" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProperties props) =>
			Create (info, pixels, info.RowBytes, null, null, props);

		/// <summary>
		/// Creates a new surface from the specified image parameters, the provided buffer and surface properties.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		/// <remarks>This will create a buffer that will be backend by the in-memory buffer provided in <paramref name="pixels" />.</remarks>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props) =>
			Create (info, pixels, rowBytes, null, null, props);

		/// <summary>
		/// Creates a new surface with the specified image parameters using a provided buffer.
		/// </summary>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="pixels">The pointer to an in memory-buffer that can hold the image as specified.</param>
		/// <param name="rowBytes">The number of bytes per row in the pixel buffer.</param>
		/// <param name="releaseProc">The delegate to invoke when the surface is about to be disposed.</param>
		/// <param name="context">The user data to use when invoking the delegate.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props)
		{
			var cinfo = SKImageInfoNative.FromManaged (ref info);
			var del = releaseProc != null && context != null
				? new SKSurfaceReleaseDelegate ((addr, _) => releaseProc (addr, context))
				: releaseProc;
			DelegateProxies.Create (del, out _, out var ctx);
			var proxy = del != null ? DelegateProxies.SKSurfaceRasterReleaseProxy : null;
			return GetObject (SkiaApi.sk_surface_new_raster_direct (&cinfo, (void*)pixels, (IntPtr)rowBytes, proxy, (void*)ctx, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND RENDER TARGET surface

		/// <summary>
		/// Wraps a pre-existing 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, colorType);

		/// <summary>
		/// Wraps a pre-existing backend 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType);

		/// <summary>
		/// Wraps a pre-existing backend 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace);

		/// <summary>
		/// Wraps a pre-existing backend 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, colorType, props);

		/// <summary>
		/// Wraps a pre-existing backend 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, props);

		/// <summary>
		/// Wraps a pre-existing backend 3D API render target as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="renderTarget">The description of the existing render target.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, renderTarget, origin, colorType, colorspace, props);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="colorType"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, null);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, renderTarget, origin, colorType, null, null);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="colorspace"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, renderTarget, origin, colorType, colorspace, null);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="colorType"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType, null, props);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, renderTarget, origin, colorType, null, props);

		/// <param name="context"></param>
		/// <param name="renderTarget"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="colorspace"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (renderTarget == null)
				throw new ArgumentNullException (nameof (renderTarget));

			return GetObject (SkiaApi.sk_surface_new_backend_render_target (context.Handle, renderTarget.Handle, origin, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU BACKEND TEXTURE surface

		/// <summary>
		/// Wraps a pre-existing 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, colorType);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, colorType);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, colorType, props);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, colorType, props);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, props);

		/// <summary>
		/// Wraps a pre-existing backend 3D API texture as a surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="texture">The description of the existing texture.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="colorType">The color type to use for the surface.</param>
		/// <param name="colorspace">The colorspace to use for the surface.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, texture, origin, sampleCount, colorType, colorspace, props);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="colorType"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType) =>
			Create (context, texture, origin, 0, colorType, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="sampleCount"></param>
		/// <param name="colorType"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, texture, origin, sampleCount, colorType, null, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="sampleCount"></param>
		/// <param name="colorType"></param>
		/// <param name="colorspace"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, texture, origin, sampleCount, colorType, colorspace, null);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="colorType"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, GRSurfaceOrigin.BottomLeft, 0, colorType, null, props);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="colorType"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, 0, colorType, null, props);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="sampleCount"></param>
		/// <param name="colorType"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props) =>
			Create (context, texture, origin, sampleCount, colorType, null, props);

		/// <param name="context"></param>
		/// <param name="texture"></param>
		/// <param name="origin"></param>
		/// <param name="sampleCount"></param>
		/// <param name="colorType"></param>
		/// <param name="colorspace"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));
			if (texture == null)
				throw new ArgumentNullException (nameof (texture));

			return GetObject (SkiaApi.sk_surface_new_backend_texture (context.Handle, texture.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));
		}

		// GPU NEW surface

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info) =>
			Create ((GRRecordingContext)context, budgeted, info);

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount);

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin);

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, props);

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, props);

		/// <summary>
		/// Creates a new surface whose contents will be drawn to an offscreen render target, allocated by the surface.
		/// </summary>
		/// <param name="context">The graphics context.</param>
		/// <param name="budgeted">Whether an allocation should count against a cache budget.</param>
		/// <param name="info">The image configuration parameters.</param>
		/// <param name="sampleCount">The number of samples per pixel.</param>
		/// <param name="origin">The origin of the texture.</param>
		/// <param name="props">The surface property configuration.</param>
		/// <param name="shouldCreateWithMips">A hint that the surface will host mip map images.</param>
		/// <returns>Returns the new surface if it could be created and the configuration is supported, otherwise <see langword="null" />.</returns>
		public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips) =>
			Create ((GRRecordingContext)context, budgeted, info, sampleCount, origin, props, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, null, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		/// <param name="sampleCount"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, null, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		/// <param name="sampleCount"></param>
		/// <param name="origin"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin) =>
			Create (context, budgeted, info, sampleCount, origin, null, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props) =>
			Create (context, budgeted, info, 0, GRSurfaceOrigin.BottomLeft, props, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		/// <param name="sampleCount"></param>
		/// <param name="props"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props) =>
			Create (context, budgeted, info, sampleCount, GRSurfaceOrigin.BottomLeft, props, false);

		/// <param name="context"></param>
		/// <param name="budgeted"></param>
		/// <param name="info"></param>
		/// <param name="sampleCount"></param>
		/// <param name="origin"></param>
		/// <param name="props"></param>
		/// <param name="shouldCreateWithMips"></param>
		public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips)
		{
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			return GetObject (SkiaApi.sk_surface_new_render_target (context.Handle, budgeted, &cinfo, sampleCount, origin, props?.Handle ?? IntPtr.Zero, shouldCreateWithMips));
		}

		#if __MACOS__ || __IOS__
		
		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, out drawable);

		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, out drawable);

		public static SKSurface Create (GRContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create ((GRRecordingContext)context, layer, origin, sampleCount, colorType, colorspace, props, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, null, null, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, out CoreAnimation.ICAMetalDrawable drawable) =>
			Create (context, layer, origin, sampleCount, colorType, colorspace, null, out drawable);

		public static SKSurface Create (GRRecordingContext context, CoreAnimation.CAMetalLayer layer, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props, out CoreAnimation.ICAMetalDrawable drawable)
		{
			void* drawablePtr;
			var surface = GetObject (SkiaApi.sk_surface_new_metal_layer (context.Handle, (void*)(IntPtr)layer.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero, &drawablePtr));
			drawable = ObjCRuntime.Runtime.GetINativeObject<CoreAnimation.ICAMetalDrawable> ((IntPtr)drawablePtr, true);
			return surface;
		}

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType) =>
			Create (context, view, origin, sampleCount, colorType, null, null);

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace) =>
			Create (context, view, origin, sampleCount, colorType, colorspace, null);

		public static SKSurface Create (GRRecordingContext context, MetalKit.MTKView view, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props) =>
			GetObject (SkiaApi.sk_surface_new_metal_view (context.Handle, (void*)(IntPtr)view.Handle, origin, sampleCount, colorType.ToNative (), colorspace?.Handle ?? IntPtr.Zero, props?.Handle ?? IntPtr.Zero));

		#endif

		// NULL surface

		/// <summary>
		/// Creates a new surface without any backing pixels.
		/// </summary>
		/// <param name="width">The desired width for the surface.</param>
		/// <param name="height">The desired height for the surface.</param>
		/// <returns>Returns the new surface if it could be created, otherwise <see langword="null" />.</returns>
		/// <remarks>Drawing to the <see cref="SKCanvas" /> returned from <see cref="SKSurface.Canvas" /> has no effect. Calling <see cref="SKSurface.Snapshot" /> on the returned <see cref="SKSurface" /> returns <see langword="null" />.</remarks>
		public static SKSurface CreateNull (int width, int height) =>
			GetObject (SkiaApi.sk_surface_new_null (width, height));

		//

		/// <summary>
		/// Gets the canvas for this surface which can be used for drawing into it.
		/// </summary>
		public SKCanvas Canvas =>
			OwnedBy (SKCanvas.GetObject (SkiaApi.sk_surface_get_canvas (Handle), false, unrefExisting: false), this);

		/// <summary>
		/// Gets the surface property configuration.
		/// </summary>
		public SKSurfaceProperties SurfaceProperties =>
			OwnedBy (SKSurfaceProperties.GetObject (SkiaApi.sk_surface_get_props (Handle), false), this);

		public GRRecordingContext Context =>
			GRRecordingContext.GetObject (SkiaApi.sk_surface_get_recording_context (Handle), false, unrefExisting: false);

		/// <summary>
		/// Takes a snapshot of the surface and returns it as an image.
		/// </summary>
		/// <returns>An <see cref="SKImage" /> that contains a snapshot of the current image.</returns>
		/// <remarks>You can use this method to take an <see cref="SKImage" /> snapshot of the current state of the surface.</remarks>
		public SKImage Snapshot () =>
			SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot (Handle));

		/// <param name="bounds"></param>
		public SKImage Snapshot (SKRectI bounds) =>
			SKImage.GetObject (SkiaApi.sk_surface_new_image_snapshot_with_crop (Handle, &bounds));

		/// <summary>
		/// Draws the current surface on the specified canvas.
		/// </summary>
		/// <param name="canvas">The canvas to draw on.</param>
		/// <param name="x">The destination x-coordinate for the surface.</param>
		/// <param name="y">The destination y-coordinate for the surface.</param>
		/// <param name="paint">The paint to use when drawing the surface, or <see langword="null" />.</param>
		public void Draw (SKCanvas canvas, float x, float y, SKPaint paint)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_surface_draw (Handle, canvas.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
		}

		/// <summary>
		/// Returns the pixels, if they are available.
		/// </summary>
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

		/// <summary>
		/// Returns the pixmap of the surface.
		/// </summary>
		/// <param name="pixmap">The pixmap to receive the pixel information.</param>
		/// <returns>Returns <see langword="true" /> on success, or <see langword="false" /> if the surface does not have access to pixel data.</returns>
		public bool PeekPixels (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			var result = SkiaApi.sk_surface_peek_pixels (Handle, pixmap.Handle);
			if (result)
				pixmap.pixelSource = this;
			return result;
		}

		/// <summary>
		/// Copies the pixels from the surface into the specified buffer.
		/// </summary>
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

		public void Flush () => Flush (true);

		/// <param name="submit"></param>
		/// <param name="synchronous"></param>
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
