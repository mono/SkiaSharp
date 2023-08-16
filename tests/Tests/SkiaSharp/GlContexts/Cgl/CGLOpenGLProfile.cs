using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal enum CGLOpenGLProfile {
		kCGLOGLPVersion_Legacy   = 0x1000,
		kCGLOGLPVersion_3_2_Core = 0x3200,
		kCGLOGLPVersion_GL3_Core = 0x3200,
		kCGLOGLPVersion_GL4_Core = 0x4100,
	}
}
