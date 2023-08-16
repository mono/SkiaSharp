using System;
using System.Runtime.InteropServices;

using EGLDisplay = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLSurface = System.IntPtr;
using EGLNativeDisplayType = System.IntPtr;
using EGLNativeWindowType = System.Object;
using glbool = System.Int32;

namespace SkiaSharp.Views.GlesInterop
{
	internal static class Egl
	{
		private const string libEGL = "libEGL.dll";

		// Out-of-band handle values
		public static readonly EGLNativeDisplayType EGL_DEFAULT_DISPLAY = IntPtr.Zero;
		public static readonly IntPtr EGL_NO_CONFIG = IntPtr.Zero;
		public static readonly IntPtr EGL_NO_DISPLAY = IntPtr.Zero;
		public static readonly IntPtr EGL_NO_CONTEXT = IntPtr.Zero;
		public static readonly IntPtr EGL_NO_SURFACE = IntPtr.Zero;

		public const glbool EGL_FALSE = 0;
		public const glbool EGL_TRUE = 1;
		public const int EGL_SUCCESS = 0x3000;

		// Config attributes
		public const int EGL_BUFFER_SIZE = 0x3020;
		public const int EGL_ALPHA_SIZE = 0x3021;
		public const int EGL_BLUE_SIZE = 0x3022;
		public const int EGL_GREEN_SIZE = 0x3023;
		public const int EGL_RED_SIZE = 0x3024;
		public const int EGL_DEPTH_SIZE = 0x3025;
		public const int EGL_STENCIL_SIZE = 0x3026;

		// QuerySurface / SurfaceAttrib / CreatePbufferSurface targets
		public const int EGL_HEIGHT = 0x3056;
		public const int EGL_WIDTH = 0x3057;

		// Attrib list terminator
		public const int EGL_NONE = 0x3038;

		// CreateContext attributes
		public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;

		// EGL_VERSION
		public const int EGL_SWAP_BEHAVIOR = 0x3093;
		public const int EGL_BUFFER_PRESERVED = 0x3094;
		public const int EGL_BUFFER_DESTROYED = 0x3095;
		public const int EGL_OPENGL_ES_API = 0x30A0;

		public const int EGL_RENDERABLE_TYPE = 0x3040;
		public const int EGL_OPENGL_ES2_BIT = 0x0004;
		public const int EGL_SURFACE_TYPE = 0x3033;
		public const int EGL_PBUFFER_BIT = 0x0001;
		public const int EGL_SWAP_BEHAVIOR_PRESERVED_BIT = 0x0400;

		// ANGLE
		public const int EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE = 0x33A4;
		public const int EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA;
		public const int EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE = 0x33AA;

		public const int EGL_PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
		public const int EGL_PLATFORM_ANGLE_MAX_VERSION_MAJOR_ANGLE = 0x3204;
		public const int EGL_PLATFORM_ANGLE_MAX_VERSION_MINOR_ANGLE = 0x3205;
		public const int EGL_PLATFORM_ANGLE_TYPE_DEFAULT_ANGLE = 0x3206;

		public const int EGL_PLATFORM_ANGLE_ANGLE = 0x3202;

		public const int EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE = 0x3207;
		public const int EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
		public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_ANGLE = 0x3209;
		public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_HARDWARE_ANGLE = 0x320A;
		public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_WARP_ANGLE = 0x320B;
		public const int EGL_PLATFORM_ANGLE_DEVICE_TYPE_D3D_REFERENCE_ANGLE = 0x320C;
		public const int EGL_PLATFORM_ANGLE_ENABLE_AUTOMATIC_TRIM_ANGLE = 0x320F;

		public const int EGL_FIXED_SIZE_ANGLE = 0x3201;

		public const string EGLNativeWindowTypeProperty = "EGLNativeWindowTypeProperty";
		public const string EGLRenderSurfaceSizeProperty = "EGLRenderSurfaceSizeProperty";
		public const string EGLRenderResolutionScaleProperty = "EGLRenderResolutionScaleProperty";

		[DllImport(libEGL)]
		private static extern IntPtr eglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);
		[DllImport(libEGL)]
		public static extern EGLDisplay eglGetPlatformDisplayEXT(uint platform, EGLNativeDisplayType native_display, int[] attrib_list);
		[DllImport(libEGL)]
		public static extern glbool eglInitialize(EGLDisplay dpy, out int major, out int minor);
		[DllImport(libEGL)]
		public static extern glbool eglChooseConfig(EGLDisplay dpy, int[] attrib_list, [In, Out] EGLConfig[] configs, int config_size, out int num_config);
		[DllImport(libEGL)]
		public static extern EGLContext eglCreateContext(EGLDisplay dpy, EGLConfig config, EGLContext share_context, int[] attrib_list);
		[DllImport(libEGL)]
		public static extern EGLSurface eglCreateWindowSurface(EGLDisplay dpy, EGLConfig config, [MarshalAs(UnmanagedType.IInspectable)] EGLNativeWindowType win, int[] attrib_list);
		[DllImport(libEGL)]
		public static extern glbool eglQuerySurface(EGLDisplay dpy, EGLSurface surface, int attribute, out int value);
		[DllImport(libEGL)]
		public static extern glbool eglDestroySurface(EGLDisplay dpy, EGLSurface surface);
		[DllImport(libEGL)]
		public static extern glbool eglMakeCurrent(EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);
		[DllImport(libEGL)]
		public static extern glbool eglSwapBuffers(EGLDisplay dpy, EGLSurface surface);
		[DllImport(libEGL)]
		public static extern glbool eglDestroyContext(EGLDisplay dpy, EGLContext ctx);
		[DllImport(libEGL)]
		public static extern glbool eglTerminate(EGLDisplay dpy);
		[DllImport(libEGL)]
		public static extern int eglGetError();
		[DllImport(libEGL)]
		public static extern glbool eglBindAPI(uint api);
		[DllImport(libEGL)]
		public static extern EGLSurface eglCreatePbufferSurface(EGLDisplay dpy, EGLConfig config, int[] attrib_list);
		[DllImport(libEGL)]
		public static extern glbool eglSurfaceAttrib(EGLDisplay dpy, EGLSurface surface, int attribute, int value);
	}
}
