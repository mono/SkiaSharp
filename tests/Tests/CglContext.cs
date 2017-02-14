using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class CglContext : GlContext
	{
		private IntPtr fContext;

		public CglContext()
		{
			var attributes = new [] {
				CGLPixelFormatAttribute.kCGLPFAOpenGLProfile, (CGLPixelFormatAttribute)CGLOpenGLProfile.kCGLOGLPVersion_3_2_Core,
				CGLPixelFormatAttribute.kCGLPFADoubleBuffer, 
				CGLPixelFormatAttribute.kCGLPFANone
			};

			IntPtr pixFormat;
			int npix;

			Cgl.CGLChoosePixelFormat(attributes, out pixFormat, out npix);

			if (pixFormat == IntPtr.Zero) {
				throw new Exception("CGLChoosePixelFormat failed.");
			}

			Cgl.CGLCreateContext(pixFormat, IntPtr.Zero, out fContext);
			Cgl.CGLReleasePixelFormat(pixFormat);

			if (fContext == IntPtr.Zero) {
				throw new Exception("CGLCreateContext failed.");
			}
		}

		public override void MakeCurrent()
		{
			Cgl.CGLSetCurrentContext(fContext);
		}

		public override void SwapBuffers()
		{
			Cgl.CGLFlushDrawable(fContext);
		}

		public override void Destroy()
		{
			if (fContext != IntPtr.Zero) {
				Cgl.CGLReleaseContext(fContext);
				fContext = IntPtr.Zero;
			}
		}
	}

	internal enum CGLOpenGLProfile {
		kCGLOGLPVersion_Legacy   = 0x1000,
		kCGLOGLPVersion_3_2_Core = 0x3200,
		kCGLOGLPVersion_GL3_Core = 0x3200,
		kCGLOGLPVersion_GL4_Core = 0x4100,
	}

	internal enum CGLPixelFormatAttribute {
		kCGLPFANone                                =   0,
		kCGLPFAAllRenderers                        =   1,
		kCGLPFATripleBuffer                        =   3,
		kCGLPFADoubleBuffer                        =   5,
		kCGLPFAColorSize                           =   8,
		kCGLPFAAlphaSize                           =  11,
		kCGLPFADepthSize                           =  12,
		kCGLPFAStencilSize                         =  13,
		kCGLPFAMinimumPolicy                       =  51,
		kCGLPFAMaximumPolicy                       =  52,
		kCGLPFASampleBuffers                       =  55,
		kCGLPFASamples                             =  56,
		kCGLPFAColorFloat                          =  58,
		kCGLPFAMultisample                         =  59,
		kCGLPFASupersample                         =  60,
		kCGLPFASampleAlpha                         =  61,
		kCGLPFARendererID                          =  70,
		kCGLPFANoRecovery                          =  72,
		kCGLPFAAccelerated                         =  73,
		kCGLPFAClosestPolicy                       =  74,
		kCGLPFABackingStore                        =  76,
		kCGLPFABackingVolatile                     =  77,
		kCGLPFADisplayMask                         =  84,
		kCGLPFAAllowOfflineRenderers               =  96,
		kCGLPFAAcceleratedCompute                  =  97,
		kCGLPFAOpenGLProfile                       =  99,
		kCGLPFASupportsAutomaticGraphicsSwitching  = 101,
		kCGLPFAVirtualScreenCount                  = 128,
	}

	internal enum CGLError {
		kCGLNoError            =     0,
		kCGLBadAttribute       = 10000,
		kCGLBadProperty        = 10001,
		kCGLBadPixelFormat     = 10002,
		kCGLBadRendererInfo    = 10003,
		kCGLBadContext         = 10004,
		kCGLBadDrawable        = 10005,
		kCGLBadDisplay         = 10006,
		kCGLBadState           = 10007,
		kCGLBadValue           = 10008,
		kCGLBadMatch           = 10009,
		kCGLBadEnumeration     = 10010,
		kCGLBadOffScreen       = 10011,
		kCGLBadFullScreen      = 10012,
		kCGLBadWindow          = 10013,
		kCGLBadAddress         = 10014,
		kCGLBadCodeModule      = 10015,
		kCGLBadAlloc           = 10016,
		kCGLBadConnection      = 10017,
	}

	internal class Cgl
	{
		private const string libGL = "/System/Library/Frameworks/OpenGL.framework/Versions/A/OpenGL";

		[DllImport(libGL)]
		public extern static void CGLGetVersion(out int majorvers, out int minorvers);
		[DllImport(libGL)]
		public extern static CGLError CGLChoosePixelFormat([In] CGLPixelFormatAttribute[] attribs, out IntPtr pix, out int npix);
		[DllImport(libGL)]
		public extern static CGLError CGLCreateContext(IntPtr pix, IntPtr share, out IntPtr ctx);
		[DllImport(libGL)]
		public extern static CGLError CGLReleasePixelFormat(IntPtr pix);
		[DllImport(libGL)]
		public extern static CGLError CGLSetCurrentContext(IntPtr ctx);
		[DllImport(libGL)]
		public extern static void CGLReleaseContext(IntPtr ctx);
		[DllImport(libGL)]
		public extern static CGLError CGLFlushDrawable(IntPtr ctx);
	}
}
