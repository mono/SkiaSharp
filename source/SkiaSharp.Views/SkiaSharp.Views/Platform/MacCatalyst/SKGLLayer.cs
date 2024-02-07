using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using CoreAnimation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using OpenGL;
using SkiaSharp.Views.GlesInterop;
using UIKit;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.iOS
{
	[SupportedOSPlatform("maccatalyst")]
	[ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	public class SKGLLayer : CAOpenGLLayer
	{
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext context;
		private GRGlFramebufferInfo glInfo;
		private GRBackendRenderTarget renderTarget;
		private SKSurface surface;
		private SKCanvas canvas;

		private SKSizeI lastSize;

		public SKGLLayer()
		{
			Opaque = true;
			NeedsDisplayOnBoundsChange = true;
		}

		public SKSize CanvasSize => lastSize;

		public GRContext GRContext => context;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
		private static extern CGLErrorCode CGLSetCurrentContext(NativeHandle ctx);

		public override void DrawInCGLContext(NativeHandle glContext, NativeHandle pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			CGLSetCurrentContext(glContext);

			if (context == null)
			{
				// get the bits for SkiaSharp
				var glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);
			}

			// manage the drawing surface
			var surfaceWidth = (int)(Bounds.Width * ContentsScale);
			var surfaceHeight = (int)(Bounds.Height * ContentsScale);
			var newSize = new SKSizeI(surfaceWidth, surfaceHeight);
			if (renderTarget == null || lastSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				lastSize = newSize;

				// read the info from the buffer
				Gles.glGetIntegerv(Gles.GL_FRAMEBUFFER_BINDING, out var framebuffer);
				Gles.glGetIntegerv(Gles.GL_STENCIL_BITS, out var stencil);
				Gles.glGetIntegerv(Gles.GL_SAMPLES, out var samples);
				var maxSamples = context.GetMaxSurfaceSampleCount(colorType);
				if (samples > maxSamples)
					samples = maxSamples;
				glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, samples, stencil, glInfo);
			}

			Gles.glClearColor(1, 0 ,0 , 1);
			Gles.glClear(Gles.GL_COLOR_BUFFER_BIT);

			// // create the surface
			// if (surface == null)
			// {
			// 	surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
			// 	canvas = surface.Canvas;
			// }

			// using (new SKAutoCanvasRestore(canvas, true))
			// {
			// 	// start drawing
			// 	var e = new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType);
			// 	OnPaintSurface(e);
			// }

			// // flush the SkiaSharp context to the GL context
			// canvas.Flush();
			context.Flush();

			base.DrawInCGLContext(glContext, pixelFormat, timeInterval, ref timeStamp);
		}

		public override void Release(NativeHandle glContext)
		{
			context.Dispose();

			base.Release(glContext);
		}
	}

	[SupportedOSPlatform("maccatalyst")]
	[ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	public class SKGLView : UIView
	{
		public SKGLView()
		{
			PaintSurface += OnPaintSurface;
		}

		public SKGLView(CGRect frame)
			: base(frame)
		{
			PaintSurface += OnPaintSurface;
		}

		public SKGLView(NativeHandle handle)
			: base(handle)
		{
			PaintSurface += OnPaintSurface;
		}

		public SKSize CanvasSize => ((SKGLLayer)Layer).CanvasSize;

		public GRContext GRContext => ((SKGLLayer)Layer).GRContext;

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface
		{
			add => ((SKGLLayer)Layer).PaintSurface += value;
			remove => ((SKGLLayer)Layer).PaintSurface -= value;
		}

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
		}

		[Export("layerClass")]
		public static Class LayerClass() => new Class(typeof(SKGLLayer));
	}
}
