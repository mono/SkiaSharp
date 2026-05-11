using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton WGL context, used by <see cref="GaneshGlRenderer"/>
	/// for headless GL on Windows.
	///
	/// <para>
	/// Bootstraps a message-only HWND (parented to <c>HWND_MESSAGE</c>), gets
	/// a DC, sets a minimal RGBA8/depth24/stencil8 pixel format, creates a
	/// dummy WGL context to query <c>wglCreateContextAttribsARB</c>, then
	/// creates a real GL 3.3 core profile context on the same DC. The window
	/// is never shown; Skia renders into its own FBO inside the context, so
	/// the DC's default framebuffer is never touched.
	/// </para>
	///
	/// <para>
	/// Mirrors <see cref="EglLoader"/> for Linux. The renderer
	/// (<see cref="GaneshGlRenderer"/>) picks one or the other based on
	/// <c>OperatingSystem.IsWindows()</c> / <c>IsLinux()</c>.
	/// </para>
	/// </summary>
	internal sealed unsafe class WglLoader
	{
		private static WglLoader instance;
		private static readonly object instanceLock = new object ();

		public IntPtr Hwnd  { get; private set; }
		public IntPtr Hdc   { get; private set; }
		public IntPtr Hglrc { get; private set; }
		public bool   IsAvailable   { get; private set; }
		public string FailureReason { get; private set; }

		public static WglLoader Shared {
			get {
				if (instance == null) {
					lock (instanceLock) {
						if (instance == null) {
							instance = new WglLoader ();
							instance.Initialize ();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Bind this loader's context to the calling thread. WGL contexts are
		/// thread-affine (one current context per thread); the renderer calls
		/// this once on its dedicated worker.
		/// </summary>
		public void MakeCurrent ()
		{
			if (!IsAvailable)
				throw new InvalidOperationException ($"WGL unavailable: {FailureReason}");
			if (!wglMakeCurrent (Hdc, Hglrc))
				throw new InvalidOperationException ($"wglMakeCurrent failed (err {Marshal.GetLastWin32Error ()})");
		}

		private IntPtr opengl32Handle;

		/// <summary>
		/// Procedure-address adapter for <see cref="GRGlInterface.CreateOpenGl"/>.
		/// <c>wglGetProcAddress</c> only returns pointers for GL functions ABOVE
		/// OpenGL 1.1 — legacy entry points (<c>glClear</c>, <c>glBegin</c>, …)
		/// have to come from <c>opengl32.dll</c> via <c>GetProcAddress</c>. We
		/// try wgl first and fall back to opengl32 on the documented sentinel
		/// returns (0, 1, 2, 3, -1).
		/// </summary>
		public IntPtr GetProc (string name)
		{
			var addr = wglGetProcAddress (name);
			var val = (long)addr;
			if (val != 0 && val != 1 && val != 2 && val != 3 && val != -1)
				return addr;

			if (opengl32Handle == IntPtr.Zero) {
				if (!NativeLibrary.TryLoad ("opengl32.dll", out opengl32Handle))
					return IntPtr.Zero;
			}
			return NativeLibrary.TryGetExport (opengl32Handle, name, out var sym) ? sym : IntPtr.Zero;
		}

		private void Initialize ()
		{
			try {
				if (!CreateWindowAndContext (out var failure)) {
					FailureReason = failure;
					return;
				}
				IsAvailable = true;
			} catch (DllNotFoundException ex) {
				FailureReason = $"Windows GL libraries not loadable: {ex.Message}";
			} catch (Exception ex) {
				FailureReason = $"WGL init failed: {ex.GetType ().Name}: {ex.Message}";
			}
		}

		private bool CreateWindowAndContext (out string failure)
		{
			failure = null;

			// HWND_MESSAGE = (HWND)-3: a special parent that makes the
			// resulting window message-only — never gets WM_PAINT, never
			// composited, never visible. We use the built-in "STATIC"
			// window class so we don't have to RegisterClass.
			var HWND_MESSAGE = new IntPtr (-3);
			Hwnd = CreateWindowExW (0, "STATIC", null, 0, 0, 0, 0, 0,
				HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (Hwnd == IntPtr.Zero) {
				failure = $"CreateWindowExW returned NULL (err {Marshal.GetLastWin32Error ()})";
				return false;
			}

			Hdc = GetDC (Hwnd);
			if (Hdc == IntPtr.Zero) {
				failure = "GetDC returned NULL";
				return false;
			}

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
			if (pixelFormat == 0) {
				failure = $"ChoosePixelFormat returned 0 (err {Marshal.GetLastWin32Error ()})";
				return false;
			}
			if (!SetPixelFormat (Hdc, pixelFormat, ref pfd)) {
				failure = $"SetPixelFormat failed (err {Marshal.GetLastWin32Error ()})";
				return false;
			}

			// Dummy context: wglCreateContext gives whatever GL the driver
			// defaults to (often GL 1.1 on Windows, since the modern
			// entry points live behind wglCreateContextAttribsARB).
			var dummyCtx = wglCreateContext (Hdc);
			if (dummyCtx == IntPtr.Zero) {
				failure = $"wglCreateContext (dummy) returned NULL (err {Marshal.GetLastWin32Error ()})";
				return false;
			}
			if (!wglMakeCurrent (Hdc, dummyCtx)) {
				failure = $"wglMakeCurrent (dummy) failed (err {Marshal.GetLastWin32Error ()})";
				wglDeleteContext (dummyCtx);
				return false;
			}

			// Query wglCreateContextAttribsARB through the dummy.
			var createAttribsAddr = wglGetProcAddress ("wglCreateContextAttribsARB");
			var createAttribsVal = (long)createAttribsAddr;
			if (createAttribsAddr == IntPtr.Zero || createAttribsVal == 1 || createAttribsVal == 2
				|| createAttribsVal == 3 || createAttribsVal == -1) {
				failure = "WGL_ARB_create_context not supported by this driver — install a recent GPU driver";
				wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
				wglDeleteContext (dummyCtx);
				return false;
			}
			var createAttribs = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB_t> (createAttribsAddr);

			// Request GL 3.3 core profile first. Fall back to a
			// compatibility profile if the driver rejects core.
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
				failure = $"wglCreateContextAttribsARB returned NULL (err {Marshal.GetLastWin32Error ()})";
				wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
				wglDeleteContext (dummyCtx);
				return false;
			}

			// Swap the dummy for the real context.
			wglMakeCurrent (IntPtr.Zero, IntPtr.Zero);
			wglDeleteContext (dummyCtx);
			Hglrc = realCtx;

			if (!wglMakeCurrent (Hdc, Hglrc)) {
				failure = $"wglMakeCurrent (real) failed (err {Marshal.GetLastWin32Error ()})";
				wglDeleteContext (Hglrc);
				Hglrc = IntPtr.Zero;
				return false;
			}
			return true;
		}

		// ---- Constants ----

		private const uint PFD_DRAW_TO_WINDOW              = 0x00000004;
		private const uint PFD_SUPPORT_OPENGL              = 0x00000020;
		private const uint PFD_DOUBLEBUFFER                = 0x00000001;
		private const byte PFD_TYPE_RGBA                   = 0;
		private const byte PFD_MAIN_PLANE                  = 0;
		private const int  WGL_CONTEXT_MAJOR_VERSION_ARB   = 0x2091;
		private const int  WGL_CONTEXT_MINOR_VERSION_ARB   = 0x2092;
		private const int  WGL_CONTEXT_PROFILE_MASK_ARB    = 0x9126;
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
	}
}
