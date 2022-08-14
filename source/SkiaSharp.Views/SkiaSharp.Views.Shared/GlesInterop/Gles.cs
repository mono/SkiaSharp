#if !__WATCHOS__ && !__WASM__ && (!UNO_REFERENCE_API || (NET6_0_OR_GREATER && (__IOS__ || __MACOS__)))
// Note that `(!UNO_REFERENCE_API || (NET6_0_OR_GREATER && (__IOS__ || __MACOS__)))` is required
// because of https://github.com/unoplatform/uno/issues/8814, where !UNO_REFERENCE_API should be enough.

using System.Runtime.InteropServices;

namespace SkiaSharp.Views.GlesInterop
{
	internal static class Gles
	{
#if __MACOS__
		private const string libGLESv2 = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif __IOS__ || __TVOS__
		private const string libGLESv2 = "/System/Library/Frameworks/OpenGLES.framework/OpenGLES";
#elif __DESKTOP__
		private const string libGLESv2 = "opengl32.dll";
#elif __ANDROID__
		private const string libGLESv2 = "libGLESv2.so";
#elif WINDOWS_UWP || WINDOWS
		private const string libGLESv2 = "libGLESv2.dll";
#elif __TIZEN__
		private const string libGLESv2 = "libGLESv2.so";
#endif

		public const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
		public const int GL_RENDERBUFFER_BINDING = 0x8CA7;

		public const int GL_BGRA8_EXT = 0x93A1;
		public const int GL_VERSION = 0x1F02;
		public const int GL_EXTENSIONS = 0x1F03;

		// GetPName
		public const int GL_SUBPIXEL_BITS = 0x0D50;
		public const int GL_RED_BITS = 0x0D52;
		public const int GL_GREEN_BITS = 0x0D53;
		public const int GL_BLUE_BITS = 0x0D54;
		public const int GL_ALPHA_BITS = 0x0D55;
		public const int GL_DEPTH_BITS = 0x0D56;
		public const int GL_STENCIL_BITS = 0x0D57;
		public const int GL_SAMPLES = 0x80A9;

		// ClearBufferMask
		public const int GL_DEPTH_BUFFER_BIT = 0x00000100;
		public const int GL_STENCIL_BUFFER_BIT = 0x00000400;
		public const int GL_COLOR_BUFFER_BIT = 0x00004000;

		public const int GL_NEAREST = 0x2600;

		public const int GL_READ_FRAMEBUFFER_ANGLE = 0x8CA8;
		public const int GL_DRAW_FRAMEBUFFER_ANGLE = 0x8CA9;
		public const int GL_DRAW_FRAMEBUFFER_BINDING_ANGLE = 0x8CA6;
		public const int GL_READ_FRAMEBUFFER_BINDING_ANGLE = 0x8CAA;

		// Framebuffer Object
		public const int GL_FRAMEBUFFER = 0x8D40;
		public const int GL_RENDERBUFFER = 0x8D41;

		public const int GL_RENDERBUFFER_WIDTH = 0x8D42;
		public const int GL_RENDERBUFFER_HEIGHT = 0x8D43;
		public const int GL_RENDERBUFFER_INTERNAL_FORMAT = 0x8D44;
		public const int GL_RENDERBUFFER_RED_SIZE = 0x8D50;
		public const int GL_RENDERBUFFER_GREEN_SIZE = 0x8D51;
		public const int GL_RENDERBUFFER_BLUE_SIZE = 0x8D52;
		public const int GL_RENDERBUFFER_ALPHA_SIZE = 0x8D53;
		public const int GL_RENDERBUFFER_DEPTH_SIZE = 0x8D54;
		public const int GL_RENDERBUFFER_STENCIL_SIZE = 0x8D55;
		public const int GL_COLOR_ATTACHMENT0 = 0x8CE0;
		public const int GL_DEPTH_ATTACHMENT = 0x8D00;
		public const int GL_STENCIL_ATTACHMENT = 0x8D20;

		public const int GL_DEPTH_COMPONENT16 = 0x81A5;
		public const int GL_DEPTH_STENCIL_OES = 0x84F9;
		public const int GL_UNSIGNED_INT_24_8_OES = 0x84FA;
		public const int GL_DEPTH24_STENCIL8_OES = 0x88F0;

		[DllImport(libGLESv2)]
		public static extern void glGenRenderbuffers(int n, [In, Out] uint[] buffers);
		[DllImport(libGLESv2)]
		public static extern void glGenRenderbuffers(int n, ref uint buffer);
		[DllImport(libGLESv2)]
		public static extern void glGenFramebuffers(int n, [In, Out] uint[] buffers);
		[DllImport(libGLESv2)]
		public static extern void glGenFramebuffers(int n, ref uint buffer);
		[DllImport(libGLESv2)]
		public static extern void glGetIntegerv(uint pname, out int data);
		[DllImport(libGLESv2)]
		public static extern void glGetRenderbufferParameteriv(uint target, int pname, out int param);
		[DllImport(libGLESv2)]
		public static extern void glBindRenderbuffer(uint target, uint buffer);
		[DllImport(libGLESv2)]
		public static extern void glViewport(int x, int y, int width, int height);
		[DllImport(libGLESv2)]
		public static extern void glClearColor(float red, float green, float blue, float alpha);
		[DllImport(libGLESv2)]
		public static extern void glClear(uint mask);
		[DllImport(libGLESv2)]
		public static extern void glBindFramebuffer(uint target, uint framebuffer);
		[DllImport(libGLESv2)]
		public static extern void glDeleteFramebuffers(int n, [In, Out] uint[] framebuffers);
		[DllImport(libGLESv2)]
		public static extern void glDeleteFramebuffers(int n, ref uint framebuffer);
		[DllImport(libGLESv2)]
		public static extern void glDeleteRenderbuffers(int n, [In, Out] uint[] renderbuffers);
		[DllImport(libGLESv2)]
		public static extern void glDeleteRenderbuffers(int n, ref uint renderbuffer);
		[DllImport(libGLESv2)]
		public static extern void glFramebufferRenderbuffer(uint target, uint attachment, uint renderbuffertarget, uint renderbuffer);
		[DllImport(libGLESv2)]
		public static extern System.IntPtr glGetString(uint value);
	}
}
#endif
