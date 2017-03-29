//
// Bindings for Skia's SVG features
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKSvgCanvas
	{
		private SKSvgCanvas ()
		{
		}
		
		public static SKCanvas Create (SKRect bounds, SKXmlWriter writer)
		{
			if (writer == null) {
				throw new ArgumentNullException (nameof (writer));
			}

			return SKObject.GetObject<SKCanvas> (SkiaApi.sk_svgcanvas_create (ref bounds, writer.Handle));
		}
	}

	// internal as this is not stable

	internal class SKSvgDom : SKObject
	{
		[Preserve]
		internal SKSvgDom (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static SKSvgDom Create (SKStream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException (nameof (stream));
			}

			return GetObject<SKSvgDom> (SkiaApi.sk_svgdom_create_from_stream (stream.Handle));
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_svgdom_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public SKSize ContainerSize {
			get {
				SKSize size;
				SkiaApi.sk_svgdom_get_container_size (Handle, out size);
				return size;
			}
			set {
				SkiaApi.sk_svgdom_set_container_size (Handle, ref value);
			}
		}

		public void Render (SKCanvas canvas)
		{
			if (canvas == null) {
				throw new ArgumentNullException (nameof (canvas));
			}

			SkiaApi.sk_svgdom_render (Handle, canvas.Handle);
		}
	}
}
