using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
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
}
