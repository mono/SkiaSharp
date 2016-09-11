using System.Runtime.InteropServices;

namespace SkiaSharp.Views.GlesInterop
{
	internal static class Gles
	{
		private const string libGLESv2 = "libGLESv2.dll";

		public const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
		public const int GL_RENDERBUFFER_BINDING = 0x8CA7;

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

		[DllImport(libGLESv2)]
		public static extern void glGenRenderbuffers(int n, [In, Out] uint[] buffers);
		[DllImport(libGLESv2)]
		public static extern void glGetIntegerv(int pname, out int data);
		[DllImport(libGLESv2)]
		public static extern void glGetRenderbufferParameteriv(int target, int pname, out int param);
		[DllImport(libGLESv2)]
		public static extern void glBindRenderbuffer(int n, uint buffer);
		[DllImport(libGLESv2)]
		public static extern void glViewport(int x, int y, int width, int height);
		[DllImport(libGLESv2)]
		public static extern void glClearColor(float red, float green, float blue, float alpha);
		[DllImport(libGLESv2)]
		public static extern void glClear(int mask);
	}
}
