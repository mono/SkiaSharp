using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class GlxContext : GlContext
	{
		private IntPtr fDisplay;
		private IntPtr fPixmap;
		private IntPtr fGlxPixmap;
		private IntPtr fContext;

		public GlxContext()
		{
			fDisplay = Xlib.XOpenDisplay(null);
			if (fDisplay == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to open X display.");
			}

			var visualAttribs = new [] {
				Glx.GLX_X_RENDERABLE, Xlib.True,
				Glx.GLX_DRAWABLE_TYPE, Glx.GLX_PIXMAP_BIT,
				Glx.GLX_RENDER_TYPE, Glx.GLX_RGBA_BIT,
				// Glx.GLX_DOUBLEBUFFER, Xlib.True,
				Glx.GLX_RED_SIZE, 8,
				Glx.GLX_GREEN_SIZE, 8,
				Glx.GLX_BLUE_SIZE, 8,
				Glx.GLX_ALPHA_SIZE, 8,
				Glx.GLX_DEPTH_SIZE, 24,
				Glx.GLX_STENCIL_SIZE, 8,
				// Glx.GLX_SAMPLE_BUFFERS, 1,
				// Glx.GLX_SAMPLES, 4,
				Xlib.None
			};
			
			int glxMajor, glxMinor;

			if (!Glx.glXQueryVersion(fDisplay, out glxMajor, out glxMinor) ||
				(glxMajor < 1) ||
				(glxMajor == 1 && glxMinor < 3)) {
				Destroy();
				throw new Exception("GLX version 1.3 or higher required.");
			}

			var fbc = Glx.ChooseFBConfig(fDisplay, Xlib.XDefaultScreen(fDisplay), visualAttribs);
			if (fbc.Length == 0) {
				Destroy();
				throw new Exception("Failed to retrieve a framebuffer config.");
			}

			var bestFBC = IntPtr.Zero;
			var bestNumSamp = -1;
			for (int i = 0; i < fbc.Length; i++) {

				int sampleBuf, samples;
				Glx.glXGetFBConfigAttrib(fDisplay, fbc[i], Glx.GLX_SAMPLE_BUFFERS, out sampleBuf);
				Glx.glXGetFBConfigAttrib(fDisplay, fbc[i], Glx.GLX_SAMPLES, out samples);

				if (bestFBC == IntPtr.Zero || (sampleBuf > 0 && samples > bestNumSamp)) {
					bestFBC = fbc[i];
					bestNumSamp = samples;
				}
			}
			var vi = Glx.GetVisualFromFBConfig(fDisplay, bestFBC);

			fPixmap = Xlib.XCreatePixmap(fDisplay, Xlib.XRootWindow(fDisplay, vi.screen), 10, 10, (uint)vi.depth);
			if (fPixmap == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to create pixmap.");
			}
			
			fGlxPixmap = Glx.glXCreateGLXPixmap(fDisplay, ref vi, fPixmap);

			var glxExts = Glx.QueryExtensions(fDisplay, Xlib.XDefaultScreen(fDisplay));
			if (Array.IndexOf(glxExts, "GLX_ARB_create_context") == -1 ||
				Glx.glXCreateContextAttribsARB == null) {
				Console.WriteLine("OpenGL 3.0 doesn't seem to be available.");
				fContext = Glx.glXCreateNewContext(fDisplay, bestFBC, Glx.GLX_RGBA_TYPE, IntPtr.Zero, Xlib.True);
			} else {
				// Let's just use OpenGL 3.0, but we could try find the highest
				int major = 3, minor = 0;
				var flags = new List<int> {
					Glx.GLX_CONTEXT_MAJOR_VERSION_ARB, major,
					Glx.GLX_CONTEXT_MINOR_VERSION_ARB, minor,
				};
				if (major > 2) {
					flags.AddRange(new[] {
						Glx.GLX_CONTEXT_PROFILE_MASK_ARB, Glx.GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB,
					});
				}
				flags.Add(Xlib.None);

				fContext = Glx.glXCreateContextAttribsARB(fDisplay, bestFBC, IntPtr.Zero, Xlib.True, flags.ToArray());
			}
			if (fContext == IntPtr.Zero) {
				Destroy();
				throw new Exception("Failed to create an OpenGL context.");
			}

			if (!Glx.glXIsDirect(fDisplay, fContext)) {
				Console.WriteLine("Obtained indirect GLX rendering context.");
			}
		}

		public override void MakeCurrent()
		{
			if (!Glx.glXMakeCurrent(fDisplay, fGlxPixmap, fContext)) {
				Destroy();
				throw new Exception("Failed to set the context.");
			}
		}

        public override void SwapBuffers()
        {
            Glx.glXSwapBuffers(fDisplay, fGlxPixmap);
        }

		public override void Destroy()
		{
			if (fDisplay != IntPtr.Zero) {
				Glx.glXMakeCurrent(fDisplay, IntPtr.Zero, IntPtr.Zero);

				if (fContext != IntPtr.Zero) {
					Glx.glXDestroyContext(fDisplay, fContext);
					fContext = IntPtr.Zero;
				}

				if (fGlxPixmap != IntPtr.Zero) {
					Glx.glXDestroyGLXPixmap(fDisplay, fGlxPixmap);
					fGlxPixmap = IntPtr.Zero;
				}

				if (fPixmap != IntPtr.Zero) {
					Xlib.XFreePixmap(fDisplay, fPixmap);
					fPixmap = IntPtr.Zero;
				}
				
				fDisplay = IntPtr.Zero;
			}
		}
	}

	internal class Xlib
	{
		private const string libX11 = "libX11";

		public const int None = 0;
		public const int True = 1;
		public const int False = 0;

		[DllImport(libX11)]
		public extern static IntPtr XOpenDisplay(string display_name);
		[DllImport(libX11)]
		public extern static int XFree(IntPtr data);
		[DllImport(libX11)]
		public extern static int XDefaultScreen(IntPtr display);
		[DllImport(libX11)]
		public extern static IntPtr XRootWindow(IntPtr display, int screen);
		[DllImport(libX11)]
		public extern static IntPtr XCreatePixmap(IntPtr display, IntPtr d, uint width, uint height, uint depth);
		[DllImport(libX11)]
		public extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);

		[StructLayout(LayoutKind.Sequential)]
		public struct XVisualInfo
		{
			public IntPtr visual;
			public IntPtr visualid;
			public int screen;
			public int depth;
			public XVisualClass c_class;
			public ulong red_mask;
			public ulong green_mask;
			public ulong blue_mask;
			public int colormap_size;
			public int bits_per_rgb;

			public override string ToString()
			{
				return 
					"{XVisualInfo " + 
					$", visual={visual}" + 
					$", visualid={visualid}" + 
					$", screen={screen}" + 
					$", depth={depth}" + 
					$", c_class={c_class}" + 
					$", red_mask={red_mask}" + 
					$", green_mask={green_mask}" + 
					$", blue_mask={blue_mask}" + 
					$", colormap_size={colormap_size}" + 
					$", bits_per_rgb={bits_per_rgb}" + 
					"}";
			}
		}
	}

    internal enum XVisualClass : int {
        StaticGray = 0,
        GrayScale = 1,
        StaticColor = 2,
        PseudoColor = 3,
        TrueColor = 4,
        DirectColor = 5
    }

	internal class Glx
	{
		private const string libGL = "libGL";

		public const int GLX_USE_GL = 1;
		public const int GLX_BUFFER_SIZE = 2;
		public const int GLX_LEVEL = 3;
		public const int GLX_RGBA = 4;
		public const int GLX_DOUBLEBUFFER = 5;
		public const int GLX_STEREO = 6;
		public const int GLX_AUX_BUFFERS = 7;
		public const int GLX_RED_SIZE = 8;
		public const int GLX_GREEN_SIZE = 9;
		public const int GLX_BLUE_SIZE = 10;
		public const int GLX_ALPHA_SIZE = 11;
		public const int GLX_DEPTH_SIZE = 12;
		public const int GLX_STENCIL_SIZE = 13;
		public const int GLX_ACCUM_RED_SIZE = 14;
		public const int GLX_ACCUM_GREEN_SIZE = 15;
		public const int GLX_ACCUM_BLUE_SIZE = 16;
		public const int GLX_ACCUM_ALPHA_SIZE = 17;

		public const int GLX_DRAWABLE_TYPE = 0x8010;
		public const int GLX_RENDER_TYPE = 0x8011;
		public const int GLX_X_RENDERABLE = 0x8012;
		public const int GLX_RGBA_TYPE = 0x8014;
		public const int GLX_COLOR_INDEX_TYPE = 0x8015;

		public const int GLX_PIXMAP_BIT = 0x00000002;
		
		public const int GLX_RGBA_BIT = 0x00000001;

		public const int GLX_SAMPLE_BUFFERS = 0x186a0;
		public const int GLX_SAMPLES = 0x186a1;

		public const int GLX_CONTEXT_DEBUG_BIT_ARB = 0x00000001;
		public const int GLX_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x00000002;
		public const int GLX_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
		public const int GLX_CONTEXT_MINOR_VERSION_ARB = 0x2092;
		public const int GLX_CONTEXT_FLAGS_ARB = 0x2094;

		public const int GLX_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
		public const int GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB = 0x00000002;
		public const int GLX_CONTEXT_PROFILE_MASK_ARB = 0x9126;

		static Glx()
		{
			var ptr = glXGetProcAddressARB("glXCreateContextAttribsARB");
			if (ptr != IntPtr.Zero) {
				glXCreateContextAttribsARB = (glXCreateContextAttribsARBDelegate)Marshal.GetDelegateForFunctionPointer(ptr, typeof(glXCreateContextAttribsARBDelegate));
			}
		}

		[DllImport(libGL)]
		public extern static bool glXQueryVersion(IntPtr dpy, out int maj, out int min);
		[DllImport(libGL)]
		public extern static IntPtr glXChooseFBConfig(IntPtr dpy, int screen, [In] int[] attribList, out int nitems);
		public static IntPtr[] ChooseFBConfig(IntPtr dpy, int screen, int[] attribList)
		{
			int nitems;
			var fbcArrayPtr = glXChooseFBConfig(dpy, screen, attribList, out nitems);
			
			var fbcArray = new IntPtr[nitems];
			Marshal.Copy(fbcArrayPtr, fbcArray, 0, nitems);
			
			Xlib.XFree(fbcArrayPtr);

			return fbcArray;
		}
		[DllImport(libGL)]
		public extern static IntPtr glXGetVisualFromFBConfig(IntPtr dpy, IntPtr config);
		public static Xlib.XVisualInfo GetVisualFromFBConfig(IntPtr dpy, IntPtr config)
		{
			var visualPtr = glXGetVisualFromFBConfig(dpy, config);
			if (visualPtr == IntPtr.Zero) {
				throw new Exception("Failed to retrieve visual from framebuffer config.");
			}

			var visual = (Xlib.XVisualInfo) Marshal.PtrToStructure(visualPtr, typeof(Xlib.XVisualInfo));

			Xlib.XFree(visualPtr);

			return visual;
		}
		[DllImport(libGL)]
		public extern static bool glXMakeCurrent(IntPtr dpy, IntPtr drawable, IntPtr ctx);
		[DllImport(libGL)]
		public extern static bool glXSwapBuffers(IntPtr dpy, IntPtr drawable);
		[DllImport(libGL)]
		public extern static bool glXIsDirect(IntPtr dpy, IntPtr ctx);
		[DllImport(libGL)]
		public extern static int glXGetFBConfigAttrib(IntPtr dpy, IntPtr config, int attribute, out int value);
		[DllImport(libGL)]
		public extern static IntPtr glXCreateGLXPixmap(IntPtr dpy, ref Xlib.XVisualInfo visual, IntPtr pixmap);
		[DllImport(libGL)]
		public extern static void glXDestroyGLXPixmap(IntPtr dpy, IntPtr pixmap);
		[DllImport(libGL)]
		public extern static void glXDestroyContext(IntPtr dpy, IntPtr ctx);
		[DllImport(libGL)]
		public extern static IntPtr glXQueryExtensionsString(IntPtr dpy, int screen);
		public static string QueryExtensionsString(IntPtr dpy, int screen)
		{
			return Marshal.PtrToStringAnsi(glXQueryExtensionsString(dpy, screen));
		}
		public static string[] QueryExtensions(IntPtr dpy, int screen)
		{
			var str = QueryExtensionsString(dpy, screen);
			if (string.IsNullOrEmpty(str)) {
				return new string[0];
			}
			return str.Split(' ');
		}
		[DllImport(libGL)]
		public extern static IntPtr glXGetProcAddressARB(string procname);
		[DllImport(libGL)]
		public extern static IntPtr glXCreateNewContext(IntPtr dpy, IntPtr config, int renderType, IntPtr shareList, int direct);
		public static readonly glXCreateContextAttribsARBDelegate glXCreateContextAttribsARB;
		public delegate IntPtr glXCreateContextAttribsARBDelegate(IntPtr dpy, IntPtr config, IntPtr share_context, int direct, int[] attrib_list);
	}
}
