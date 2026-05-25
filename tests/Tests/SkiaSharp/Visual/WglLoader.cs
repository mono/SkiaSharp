using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Headless WGL context — owned by a single thread, end-to-end.
	///
	/// <para>
	/// Unlike <see cref="EglLoader"/> (which is a process-singleton and lets
	/// callers cross-thread <c>eglMakeCurrent</c> freely), WGL is strictly
	/// single-thread: the window class default DC's pixel format and the
	/// context's bind-state are both tied to the thread that <em>creates</em>
	/// them. Crossing the thread boundary surfaces as either ERROR_BUSY (170)
	/// or ERROR_INVALID_PIXEL_FORMAT (2004) depending on the driver.
	/// </para>
	///
	/// <para>
	/// Consequently this loader is NOT a static singleton: every instance is
	/// created, initialised, used, and disposed on exactly one thread —
	/// <see cref="GaneshGlRenderer"/>'s dedicated worker.
	/// </para>
	/// </summary>
	internal sealed unsafe class WglLoader : IDisposable
	{
		public IntPtr Hwnd  { get; private set; }
		public IntPtr Hdc   { get; private set; }
		public IntPtr Hglrc { get; private set; }

		/// <summary>Cheap process-wide probe for <see cref="GaneshGlRenderer.IsAvailable"/>:
		/// can we resolve opengl32.dll at all? Says nothing about whether
		/// <see cref="Initialize"/> will actually succeed.</summary>
		public static bool ProbeOpenGl32Loadable ()
		{
			var h = LoadLibraryW ("opengl32.dll");
			return h != IntPtr.Zero;
		}

		/// <summary>
		/// Brings the window + DC + pixel format + GL 3.3 core context up on
		/// the CALLING thread, and leaves the context current on it. Throws
		/// on failure with a descriptive message; caller (the worker thread)
		/// should catch and propagate via TaskCompletionSource.
		/// </summary>
		public void Initialize ()
		{
			// HWND_MESSAGE = (HWND)-3: message-only window — never composited,
			// never visible. Built-in "STATIC" class so we don't have to
			// RegisterClass.
			var HWND_MESSAGE = new IntPtr (-3);
			Hwnd = CreateWindowExW (0, "STATIC", null, 0, 0, 0, 0, 0,
				HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (Hwnd == IntPtr.Zero)
				throw new InvalidOperationException ($"CreateWindowExW returned NULL (err {Marshal.GetLastWin32Error ()})");

			Hdc = GetDC (Hwnd);
			if (Hdc == IntPtr.Zero)
				throw new InvalidOperationException ("GetDC returned NULL");

			var pfd = new PIXELFORMATDESCRIPTOR {
				nSize        = (ushort)sizeof (PIXELFORMATDESCRIPTOR),
				nVersion     = 1,
				dwFlags      = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
				iPixelType   = PFD_TYPE_RGBA,
				cColorBits   = 32,
				cAlphaBits   = 8,
				cDepthBits   = 24,
				cStencilBits = 8,
				iLayerType   = PFD_MAIN_PLANE,
			};
			var pixelFormat = ChoosePixelFormat (Hdc, ref pfd);
			if (pixelFormat == 0)
				throw new InvalidOperationException ($"ChoosePixelFormat returned 0 (err {Marshal.GetLastWin32Error ()})");
			if (!SetPixelFormat (Hdc, pixelFormat, ref pfd))
				throw new InvalidOperationException ($"SetPixelFormat failed (err {Marshal.GetLastWin32Error ()})");

			// Dummy context: wglCreateContext gives whatever GL the driver
			// defaults to (often GL 1.1 on Windows). We only need it long
			// enough to query wglCreateContextAttribsARB.
			var dummyCtx = wglCreateContext (Hdc);
			if (dummyCtx == IntPtr.Zero)
				throw new InvalidOperationException ($"wglCreateContext (dummy) returned NULL (err {Marshal.GetLastWin32Error ()})");
			if (!wglMakeCurrent (Hdc, dummyCtx)) {
				wglDeleteContext (dummyCtx);
				throw new InvalidOperationException ($"wglMakeCurrent (dummy) failed (err {Marshal.GetLastWin32Error ()})");
			}

			var createAttribsAddr = wglGetProcAddress ("wglCreateContextAttribsARB");
			var createAttribsVal = (long)createAttribsAddr;
			if (createAttribsAddr == IntPtr.Zero || createAttribsVal == 1 || createAttribsVal == 2
				|| createAttribsVal == 3 || createAttribsVal == -1) {
				wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
				wglDeleteContext (dummyCtx);
				throw new RendererUnavailableException (
					"WGL_ARB_create_context not supported by this driver — install a recent GPU driver");
			}
			var createAttribs = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB_t> (createAttribsAddr);

			// Request GL 3.3 core; fall back to plain GL 3.3 if the driver
			// rejects core profile.
			IntPtr realCtx;
			var coreAttribs = stackalloc int[] {
				WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
				WGL_CONTEXT_MINOR_VERSION_ARB, 3,
				WGL_CONTEXT_PROFILE_MASK_ARB,  WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
				0,
			};
			realCtx = createAttribs (Hdc, IntPtr.Zero, coreAttribs);
			if (realCtx == IntPtr.Zero) {
				var compatAttribs = stackalloc int[] {
					WGL_CONTEXT_MAJOR_VERSION_ARB, 3,
					WGL_CONTEXT_MINOR_VERSION_ARB, 3,
					0,
				};
				realCtx = createAttribs (Hdc, IntPtr.Zero, compatAttribs);
			}
			if (realCtx == IntPtr.Zero) {
				var err = Marshal.GetLastWin32Error ();
				wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
				wglDeleteContext (dummyCtx);
				throw new InvalidOperationException ($"wglCreateContextAttribsARB returned NULL (err {err})");
			}

			// Swap dummy for real, leave real current on this thread.
			wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
			wglDeleteContext (dummyCtx);
			Hglrc = realCtx;
			if (!wglMakeCurrent (Hdc, Hglrc)) {
				var err = Marshal.GetLastWin32Error ();
				wglDeleteContext (Hglrc);
				Hglrc = IntPtr.Zero;
				throw new InvalidOperationException ($"wglMakeCurrent (real) failed (err {err})");
			}
		}

		private IntPtr opengl32Handle;

		/// <summary>
		/// Procedure-address adapter for <see cref="GRGlInterface.CreateOpenGl"/>.
		/// <c>wglGetProcAddress</c> only returns pointers for GL functions above
		/// 1.1 — legacy entry points (glClear, glBegin, …) come from opengl32.dll
		/// directly. Try wgl first, fall back on the documented sentinel returns
		/// (0, 1, 2, 3, -1).
		/// </summary>
		public IntPtr GetProc (string name)
		{
			var addr = wglGetProcAddress (name);
			var val = (long)addr;
			if (val != 0 && val != 1 && val != 2 && val != 3 && val != -1)
				return addr;

			if (opengl32Handle == IntPtr.Zero)
				opengl32Handle = LoadLibraryW ("opengl32.dll");
			if (opengl32Handle == IntPtr.Zero)
				return IntPtr.Zero;
			return GetProcAddress (opengl32Handle, name);
		}

		public void Dispose ()
		{
			// MUST run on the same thread that called Initialize.
			if (Hglrc != IntPtr.Zero) {
				wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
				wglDeleteContext (Hglrc);
				Hglrc = IntPtr.Zero;
			}
			if (Hdc != IntPtr.Zero && Hwnd != IntPtr.Zero) {
				ReleaseDC (Hwnd, Hdc);
				Hdc = IntPtr.Zero;
			}
			if (Hwnd != IntPtr.Zero) {
				DestroyWindow (Hwnd);
				Hwnd = IntPtr.Zero;
			}
		}

		// ---- Constants ----

		private const uint PFD_DRAW_TO_WINDOW               = 0x00000004;
		private const uint PFD_SUPPORT_OPENGL               = 0x00000020;
		private const uint PFD_DOUBLEBUFFER                 = 0x00000001;
		private const byte PFD_TYPE_RGBA                    = 0;
		private const byte PFD_MAIN_PLANE                   = 0;
		private const int  WGL_CONTEXT_MAJOR_VERSION_ARB    = 0x2091;
		private const int  WGL_CONTEXT_MINOR_VERSION_ARB    = 0x2092;
		private const int  WGL_CONTEXT_PROFILE_MASK_ARB     = 0x9126;
		private const int  WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;

		[StructLayout (LayoutKind.Sequential)]
		private struct PIXELFORMATDESCRIPTOR
		{
			public ushort nSize, nVersion;
			public uint   dwFlags;
			public byte   iPixelType, cColorBits;
			public byte   cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift, cAlphaBits, cAlphaShift;
			public byte   cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
			public byte   cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
			public uint   dwLayerMask, dwVisibleMask, dwDamageMask;
		}

		[UnmanagedFunctionPointer (CallingConvention.StdCall)]
		private delegate IntPtr wglCreateContextAttribsARB_t (IntPtr hDC, IntPtr hShareContext, int* attribList);

		// ---- P/Invoke ----

		[DllImport ("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr CreateWindowExW (uint exStyle, string className, string windowName, uint style,
			int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

		[DllImport ("user32.dll", SetLastError = true)]
		private static extern IntPtr GetDC (IntPtr hwnd);

		[DllImport ("user32.dll", SetLastError = true)]
		private static extern int ReleaseDC (IntPtr hwnd, IntPtr hdc);

		[DllImport ("user32.dll", SetLastError = true)]
		[return: MarshalAs (UnmanagedType.Bool)]
		private static extern bool DestroyWindow (IntPtr hwnd);

		[DllImport ("gdi32.dll", SetLastError = true)]
		private static extern int ChoosePixelFormat (IntPtr hdc, ref PIXELFORMATDESCRIPTOR ppfd);

		[DllImport ("gdi32.dll", SetLastError = true)]
		[return: MarshalAs (UnmanagedType.Bool)]
		private static extern bool SetPixelFormat (IntPtr hdc, int pixelFormat, ref PIXELFORMATDESCRIPTOR ppfd);

		[DllImport ("opengl32.dll", SetLastError = true)]
		private static extern IntPtr wglCreateContext (IntPtr hdc);

		[DllImport ("opengl32.dll", SetLastError = true)]
		[return: MarshalAs (UnmanagedType.Bool)]
		private static extern bool wglMakeCurrent (IntPtr hdc, IntPtr hglrc);

		[DllImport ("opengl32.dll", SetLastError = true)]
		[return: MarshalAs (UnmanagedType.Bool)]
		private static extern bool wglDeleteContext (IntPtr hglrc);

		[DllImport ("opengl32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern IntPtr wglGetProcAddress (string name);

		[DllImport ("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibraryW (string lpLibFileName);

		[DllImport ("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
		private static extern IntPtr GetProcAddress (IntPtr hModule, string procName);
	}
}
